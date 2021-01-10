
using Barracuda.OpenApi.Interfaces;
using Barracuda.OpenApi.Services;
using Demo.Azure.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(StartupAzure))]

namespace Demo.Azure.Functions
{
    public class StartupAzure : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            builder.Services.AddScoped<IOpenApiBuilder, OpenApiBuilder>();
            builder.Services.AddScoped<IOpenApiReader, OpenApiReader>();
            builder.Services.AddScoped<ISettingsOpenApi, SettingsOpenApi>();
        }
    }
}

