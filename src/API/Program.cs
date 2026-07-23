using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProductManagement.API.Filters;
using ProductManagement.API.Middleware;
using ProductManagement.API.Services;
using ProductManagement.Application.Common;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Mappings;
using ProductManagement.Application.Validators;
using ProductManagement.Domain.Interfaces;
using ProductManagement.Infrastructure.Persistence;
using ProductManagement.Infrastructure.Persistence.Repositories;
using ProductManagement.Infrastructure.Security;
using Serilog;

namespace ProductManagement.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==================================================
            // 1. SERILOG STRUCTURED LOGGING CONFIGURATION
            // ==================================================
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            try
            {
                Log.Information("Starting Web API host...");

                // ==================================================
                // 2. DATABASE CONFIGURATION
                // ==================================================
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));

                // ==================================================
                // 3. CORE SERVICES REGISTER (DEPENDENCY INJECTION)
                // ==================================================
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
                builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
                builder.Services.AddScoped<ITokenService, TokenService>();

                // Register MediatR
                builder.Services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssembly(typeof(Result).Assembly));

                // Register AutoMapper
                builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

                // Register FluentValidation Validators
                builder.Services.AddValidatorsFromAssembly(typeof(CreateProductCommandValidator).Assembly);

                // ==================================================
                // 4. API CONTROLLERS AND VALIDATION FILTER
                // ==================================================
                builder.Services.AddControllers(options =>
                {
                    options.Filters.Add<ValidationFilter>(); // Apply global validation action filter
                }).AddNewtonsoftJson();

                // Suppress default validation filter so our custom ValidationFilter intercepts
                builder.Services.Configure<ApiBehaviorOptions>(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });

                // ==================================================
                // 5. JWT BEARER AUTHENTICATION
                // ==================================================
                var jwtSettings = new JwtSettings();
                builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
                builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false; // Set to true in Production
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                        ClockSkew = TimeSpan.Zero // Remove standard 5 minutes clock skew delay
                    };
                });

                // ==================================================
                // 6. API VERSIONING
                // ==================================================
                builder.Services.AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true;
                })
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });

                // ==================================================
                // 7. SWAGGER GENERATION WITH JWT SECURITY DEFINITIONS
                // ==================================================
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Product Management API",
                        Version = "v1",
                        Description = "Enterprise-grade RESTful API for Product & Item management."
                    });

                    // Add JWT Bearer Security Definition
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Input your JWT token in the format: Bearer {your_token}"
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });

                    // Enable XML comments documentation
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                    {
                        options.IncludeXmlComments(xmlPath);
                    }
                });

                // ==================================================
                // 8. SECURITY & UTILITIES (RATE LIMITING, HEALTH CHECKS, CORS)
                // ==================================================
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy", policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
                });

                // Native Fixed Window Rate Limiting (100 requests per 60s per client)
                builder.Services.AddRateLimiter(options =>
                {
                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                    options.AddFixedWindowLimiter("fixed", opt =>
                    {
                        opt.Window = TimeSpan.FromSeconds(60);
                        opt.PermitLimit = 100;
                        opt.QueueLimit = 2;
                        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    });
                });

                builder.Services.AddHealthChecks();

                // Build application
                var app = builder.Build();

                // ==================================================
                // 9. CONFIGURE REQUEST PIPELINE (MIDDLEWARES)
                // ==================================================
                app.UseMiddleware<ExceptionMiddleware>(); // Custom exception handling middleware

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(options =>
                    {
                        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Management API v1");
                    });
                }
                else
                {
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseSerilogRequestLogging(); // serilog request tracking middleware

                app.UseCors("CorsPolicy");
                app.UseRateLimiter();

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapHealthChecks("/health");
                app.MapControllers();

                // ==================================================
                // 10. AUTO RUN MIGRATIONS & SEED DATA
                // ==================================================
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<ApplicationDbContext>();
                        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

                        // Apply pending migrations automatically on startup
                        if (context.Database.IsRelational())
                        {
                            await context.Database.MigrateAsync();
                        }
                        
                        // Seed database demo records
                        await DbContextSeed.SeedAsync(context, passwordHasher);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "An error occurred during database migration or seeding.");
                    }
                }

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
