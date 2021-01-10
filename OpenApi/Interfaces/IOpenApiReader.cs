using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Barracuda.OpenApi.Interfaces
{
    public interface IOpenApiReader
    {
        public string Read(Assembly assembly, string controller, string server, string authUrl, string tokenUrl);
    }
}
