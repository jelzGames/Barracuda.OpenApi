using System;
using System.Collections.Generic;
using System.Text;

namespace Barracuda.OpenApi.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method)]
    public class OpenAPIVersion : System.Attribute
    {
        public string Version { get; set; }

        public OpenAPIVersion(string Version)
        {
            this.Version = Version;
        }
    }
}
