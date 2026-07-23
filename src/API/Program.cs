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
using ProductManagement.Infrastructure.Security;
using Serilog;

namespace ProductManagement.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(Result).Assembly));

            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            builder.Services.AddValidatorsFromAssembly(typeof(CreateProductCommandValidator).Assembly);

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
            }).AddNewtonsoftJson();

            // Suppress default validation filter so our custom ValidationFilter intercepts
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

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
                options.RequireHttpsMetadata = false;
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
                    ClockSkew = TimeSpan.Zero
                };
            });

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

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Product Management API",
                    Version = "v1",
                    Description = "RESTful API for Product & Item management."
                });

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

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

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

            var app = builder.Build();

            app.UseMiddleware<ExceptionMiddleware>();

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

            app.UseSerilogRequestLogging();

            app.UseCors("CorsPolicy");
            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHealthChecks("/health");
            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var passwordHasher = services.GetRequiredService<IPasswordHasher>();

                    if (context.Database.IsRelational())
                    {
                        await context.Database.MigrateAsync();
                    }
                    
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

