using Barracuda.OpenApi.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace Barracuda.OpenApi.Services
{
    public class OpenApiBuilder : IOpenApiBuilder
    {
        public readonly IOpenApiReader _reader;
        public readonly ISettingsOpenApi _settings;

        public OpenApiBuilder(
            IOpenApiReader reader,
            ISettingsOpenApi settings
        )
        {
            _reader = reader;
            _settings = settings;
        }

        public IActionResult Build(Assembly assembly, string typeName, HttpRequestMessage request)
        {
            var serverPath = request.RequestUri.Scheme + "://" + request.RequestUri.Authority;

            var json = _reader.Read(assembly, typeName, serverPath, _settings.authUrl, _settings.tokenUrl);

            return new OkObjectResult(json);
        }

        public IActionResult OpenAPIAuth()
        {
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string path = fileInfo.Directory.Parent.FullName;

            var content = File.ReadAllText(path + "/Templates/Auth.html");

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = content
            };
        }

        public IActionResult OpenAPIUI()
        {
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string path = fileInfo.Directory.Parent.FullName;

            var content = File.ReadAllText(path + "/Templates/OpenApi.html");

            var i = content.IndexOf("url: ''");
            if (i >= 0)
            {
                content = content.Remove(i, "url: ''".Length);
                content = content.Insert(i, "url: '" + _settings.OpenApiUrl + "'");
            }

            i = content.IndexOf("oauth2RedirectUrl: ''");
            if (i >= 0)
            {
                content = content.Remove(i, "oauth2RedirectUrl: ''".Length);
                content = content.Insert(i, "oauth2RedirectUrl: '" + _settings.OpenApiOauth2RedirectUrl + "'");
            }

            i = content.IndexOf("clientId: ''");
            if (i >= 0)
            {
                content = content.Remove(i, "clientId: ''".Length);
                content = content.Insert(i, "clientId: '" + _settings.OpenApiClientId + "'");
            }

            i = content.IndexOf("clientSecret: ''");
            if (i >= 0)
            {
                content = content.Remove(i, "clientSecret: ''".Length);
                content = content.Insert(i, "clientSecret: '" + _settings.OpenApiClientSecret + "'");
            }

            i = content.IndexOf("BarracudaAuthUrl");
            if (i >= 0)
            {
                content = content.Remove(i, "BarracudaAuthUrl".Length);
                content = content.Insert(i, _settings.BarracudaAuthUrl + "'");
            }

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = content
            };
        }
    }
}
