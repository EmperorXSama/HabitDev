using System.Reflection;
using HabitDev.Configurations;
using HabitDev.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace HabitDev;

public static  class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment) =>
        services
            .AddConfigurationOptions(configuration)
            .AddOpenTelemetry(configuration, environment)
            .AddDatabase(configuration);


    private static IServiceCollection AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        OtlpConfig otlpConfig = configuration.GetSection(OtlpConfig.SectionName)
            .Get<OtlpConfig>()!;
        // service version 
        string serviceVersion = Assembly
            .GetExecutingAssembly()
            .GetName()
            .Version?.ToString() ?? "1.0.0";
        // machine name
        string instanceId = Environment.GetEnvironmentVariable("HOSTNAME") ?? Environment.MachineName;


        ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: "HabitDev.Api",
                serviceVersion: serviceVersion,
                serviceInstanceId: instanceId
            )
            .AddAttributes(new Dictionary<string, object>()
            {
                ["deployment:environment"] = environment.EnvironmentName,
                ["host:name"] = instanceId
            });
        
        
       services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("OpenT.Api"))
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)

                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                        opts.Filter = ctx =>
                            !ctx.Request.Path.StartsWithSegments("/health");
                    })

                    .AddHttpClientInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                    })

                    .AddEntityFrameworkCoreInstrumentation()
                    .AddNpgsql()
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(otlpConfig.Endpoint);
                        opts.Protocol =
                            OpenTelemetry.Exporter
                                .OtlpExportProtocol.Grpc;
                    });
            })

            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()

                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(otlpConfig.Endpoint);
                    });
            });

        services.AddLogging(logging =>
        {
            logging.ClearProviders();

          
            logging.AddConsole();

            logging.AddOpenTelemetry(opts =>
            {
                opts.IncludeFormattedMessage = true;
                opts.IncludeScopes = true;
                opts.ParseStateValues = true;

                opts.SetResourceBuilder(resourceBuilder);

                opts.AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri(otlpConfig.Endpoint);
                });
            });
        });

        
        return services;
    }


    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName))
        );
        return services;
    }
    
    private  static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OtlpConfig>()
            .Bind(configuration.GetSection(OtlpConfig.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
}
