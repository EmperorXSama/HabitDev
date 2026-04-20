using System.Reflection;
using FluentValidation;
using HabitDev.Configurations;
using HabitDev.Database;
using HabitDev.Database.Entities;
using HabitDev.DTOs.Habits;
using HabitDev.Services.Sorting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using OpenTelemetry.Exporter;
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
            .AddDatabase(configuration)
            .AddServices()
            .AddValidators();


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
            .ConfigureResource(r => r
                .AddService(
                    serviceName: "HabitDev.Api",
                    serviceVersion: serviceVersion,
                    serviceInstanceId: instanceId)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environment.EnvironmentName,
                    ["host.name"] = instanceId
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                        opts.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation(opts => opts.RecordException = true)
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(otlpConfig.Endpoint);
                        opts.Protocol = OtlpExportProtocol.Grpc;
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
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
                    exporter.Protocol = OtlpExportProtocol.Grpc;
                });
            });
        });

        
        return services;
    }

    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        
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
    private  static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<HabitDto, Habit>>(_ =>
            HabitMappings.SortMapping);

        services.AddTransient<SortMappingProvider>();
        return services;
    }
}
