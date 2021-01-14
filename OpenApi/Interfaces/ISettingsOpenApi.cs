using System;
using System.Collections.Generic;
using System.Text;

namespace Barracuda.OpenApi.Interfaces
{
    public interface ISettingsOpenApi
    {
        public string authUrl { get; }
        public string tokenUrl { get; }
        public string OpenApiUrl { get; }
        public string OpenApiOauth2RedirectUrl { get; }
        public string OpenApiClientId { get; }
        public string OpenApiClientSecret { get; }
        public string BarracudaAuthUrl { get; }
        public string BarracudaRefreshTokenUrl { get; }
        public string BarracudaRefreshUrl { get; }
        public string CookieToken { get; }
        public string CookieTokenPath { get; }
        public string CookieRefreshToken { get; }
        public string CookieRefreshTokenPath { get; }
    }
}
