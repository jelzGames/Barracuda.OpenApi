using System;
using System.Collections.Generic;
using System.Text;

namespace Barracuda.OpenApi.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method)]
    public class OpenAPIProducesResponse : System.Attribute
    {
        public Type Type;
        public string DictionaryName;
        public string DictionaryArg0;
        public string DictionaryArg1;


        public OpenAPIProducesResponse(Type Type, string DictionaryName = "", string DictionaryArg0 = "", string DictionaryArg1 = "")
        {
            this.Type = Type;
            this.DictionaryName = DictionaryName;
            this.DictionaryArg0 = DictionaryArg0;
            this.DictionaryArg1 = DictionaryArg1;
        }
    }
}
