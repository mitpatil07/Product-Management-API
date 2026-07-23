using Microsoft.Extensions.Configuration;
using Serilog;

namespace ProductManagement.Infrastructure.Logging;

public static class SerilogConfiguration
{
    public static LoggerConfiguration ConfigureSerilog(IConfiguration configuration)
    {
        return new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    }
}
