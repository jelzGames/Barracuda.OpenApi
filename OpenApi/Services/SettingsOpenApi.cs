using Barracuda.OpenApi.Interfaces;
using System;

namespace Barracuda.OpenApi.Services
{
    public class SettingsOpenApi : ISettingsOpenApi
    {
        public SettingsOpenApi()
        {
            authUrl = System.Environment.GetEnvironmentVariable("authUrl", EnvironmentVariableTarget.Process);
            tokenUrl = System.Environment.GetEnvironmentVariable("tokenUrl", EnvironmentVariableTarget.Process);
            OpenApiUrl = System.Environment.GetEnvironmentVariable("OpenApiUrl", EnvironmentVariableTarget.Process);
            OpenApiOauth2RedirectUrl = System.Environment.GetEnvironmentVariable("OpenApiOauth2RedirectUrl", EnvironmentVariableTarget.Process);
            OpenApiClientId = System.Environment.GetEnvironmentVariable("OpenApiClientId", EnvironmentVariableTarget.Process);
            OpenApiClientSecret = System.Environment.GetEnvironmentVariable("OpenApiClientSecret", EnvironmentVariableTarget.Process);
            BarracudaAuthUrl = System.Environment.GetEnvironmentVariable("BarracudaAuthUrl", EnvironmentVariableTarget.Process);
            BarracudaRefreshTokenUrl = System.Environment.GetEnvironmentVariable("BarracudaRefreshTokenUrl", EnvironmentVariableTarget.Process);
            BarracudaRefreshUrl = System.Environment.GetEnvironmentVariable("BarracudaRefreshUrl", EnvironmentVariableTarget.Process);
            CookieToken = System.Environment.GetEnvironmentVariable("CookieToken", EnvironmentVariableTarget.Process);
            CookieTokenPath = System.Environment.GetEnvironmentVariable("CookieTokenPath", EnvironmentVariableTarget.Process);
            CookieRefreshToken = System.Environment.GetEnvironmentVariable("CookieRefreshToken", EnvironmentVariableTarget.Process);
            CookieRefreshTokenPath = System.Environment.GetEnvironmentVariable("CookieRefreshTokenPath", EnvironmentVariableTarget.Process);
        }

        public string authUrl { get; private set; }
        public string tokenUrl { get; private set; }
        public string OpenApiUrl { get; private set; }
        public string OpenApiOauth2RedirectUrl { get; private set; }
        public string OpenApiClientId { get; private set; }
        public string OpenApiClientSecret { get; private set; }
        public string BarracudaAuthUrl { get; private set; }
        public string BarracudaRefreshTokenUrl { get; private set; }
        public string BarracudaRefreshUrl { get; private set; }
        public string CookieToken { get; private set; }
        public string CookieTokenPath { get; private set; }
        public string CookieRefreshToken { get; private set; }
        public string CookieRefreshTokenPath { get; private set; }
    }
}
