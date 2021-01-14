
using Barracuda.Indentity.Provider.Interfaces;
using Barracuda.Indentity.Provider.Services;
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

            builder.Services.AddScoped<IUsersSecretsApplication, UsersSecretsApplication>();
            builder.Services.AddScoped<IUsersSecretsRepository, UsersSecretsRepository>();
            builder.Services.AddScoped<IUsersSecretsDomain, UsersSecretsDomain>();
            builder.Services.AddScoped<IUserInfo, OpenIdUserInfo>();
            builder.Services.AddScoped<ISettingsUserSecrests, SettingsUserSecrets>();
            builder.Services.AddScoped<ITokens, Tokens>();
            builder.Services.AddScoped<ISocial, Social>();
            builder.Services.AddScoped<IRedisCache, RedisCache>();
            builder.Services.AddScoped<ICryptograhic, Cryptograhic>();
            builder.Services.AddScoped<IResult, Result>();
            builder.Services.AddScoped<IErrorMessages, ErrorMessages>();
            builder.Services.AddScoped<ISettingsRedis, SettingsRedis>();
            builder.Services.AddScoped<ISettingsTokens, SettingsTokens>();
            builder.Services.AddScoped<ISettingsCosmos, SettingsCosmos>();

        }
    }
}

