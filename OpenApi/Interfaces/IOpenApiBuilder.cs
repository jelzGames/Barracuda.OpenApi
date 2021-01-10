using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace Barracuda.OpenApi.Interfaces
{
    public interface IOpenApiBuilder
    {
        public IActionResult Build(Assembly assembly, string name, HttpRequestMessage request);
        public IActionResult OpenAPIUI();
        public IActionResult OpenAPIAuth();
    }
}
