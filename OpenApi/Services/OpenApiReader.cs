using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using System.Dynamic;
using Newtonsoft.Json;
using Barracuda.OpenApi.Interfaces;
using Barracuda.OpenApi.Attributes;

namespace Barracuda.OpenApi.Services
{
    public class OpenApiReader : IOpenApiReader
    {
        public readonly ISettingsOpenApi _settings;
        public string pathSchema = "#/components/schemas/";
        Dictionary<string, IDictionary<string, Object>> componentsType;
        Dictionary<string, Type> types;
        List<Type> typeSearch;
        Type attrSystemresponse;
        

        public OpenApiReader(
            ISettingsOpenApi settings)
        {
            componentsType = new Dictionary<string, IDictionary<string, object>>();
            types = new Dictionary<string, Type>();
            typeSearch = new List<Type>();
            _settings = settings;

        }

        public string Read(Assembly assembly, string controller, string server, string authUrl, string tokenUrl)
        {
            var methods = (from type in assembly.GetTypes()
                           from method in type.GetMethods(
                             BindingFlags.Public |
                             BindingFlags.Instance)
                           select type).Where(c => c.Name == controller).Distinct().ToList();

            
            var model = new ExpandoObject() as IDictionary<string, Object>;
          
            object[] attributes = methods[0].GetCustomAttributes(typeof(OpenAPIVersion), false);

            if (attributes.Length > 0)
            {
                OpenAPIVersion attribute = attributes[0] as OpenAPIVersion;
                model.Add("openapi", attribute.Version);
            }
            


            var info = new ExpandoObject() as IDictionary<string, Object>;

            attributes = methods[0].GetCustomAttributes(typeof(APIInfo), false);

            if (attributes.Length > 0)
            {
                APIInfo attribute = attributes[0] as APIInfo;
                info.Add("title", attribute.Title);
                info.Add("version", attribute.Version);
                model.Add("info", info);
            }

            List<IDictionary<string, Object>> serverList = new List<IDictionary<string, Object>>();
            var servers = new ExpandoObject() as IDictionary<string, Object>;
            var endPoint = new ExpandoObject() as IDictionary<string, Object>;

            endPoint.Add("url", server);
            serverList.Add(endPoint);
            model.Add("servers", serverList);

            var paths = new ExpandoObject() as IDictionary<string, Object>;
            var components = new ExpandoObject() as IDictionary<string, Object>;
            var schemas = new ExpandoObject() as IDictionary<string, Object>;
            var securitySchemes = new ExpandoObject() as IDictionary<string, Object>;

            GetPaths(methods[0].GetMethods(), paths, schemas);
            GetSecuritySchemas(securitySchemes, authUrl, tokenUrl);
           
            model.Add("paths", paths);
            components.Add("schemas", schemas);
            components.Add("securitySchemes", securitySchemes);
            model.Add("components", components);

            GetSecurity(model);

            schemas = new ExpandoObject() as IDictionary<string, Object>;
            schemas.Add("type", "apiKey");
            schemas.Add("in", "cookie");
            schemas.Add("name", _settings.CookieToken);
            securitySchemes.Add("cookieAuth", schemas);

            var json = JsonConvert.SerializeObject(model, Formatting.Indented);

            return json;
        }

        void GetPaths(MethodInfo[] methods, IDictionary<string, object> paths, IDictionary<string, object> components)
        {
            foreach (var method in methods)
            {
                object[] attributes = method.GetCustomAttributes(typeof(OpenAPI), false);

                if (attributes.Length > 0)
                {
                    attributes = method.GetCustomAttributes(typeof(OpenAPIProducesResponse), false);

                    if (attributes.Length > 0)
                    {
                        OpenAPIProducesResponse attribute = attributes[0] as OpenAPIProducesResponse;

                        string componentName = attribute.Type.Name.Replace("`1", String.Empty);

                        attrSystemresponse = null;

                        if (attribute.Type.FullName.StartsWith("System."))
                        {
                            var arguments = attribute.Type.GetGenericArguments();
                            if (arguments.Length == 0)
                            {
                                attrSystemresponse = attribute.Type;
                            }
                        }
                     
                        GetComponentName(attribute.Type, ref componentName, attribute.Type.AssemblyQualifiedName);

                        var name = attribute.Type.Name.Replace("`1", String.Empty);

                        var typeObject = attribute.Type;

                        var response = new ExpandoObject() as IDictionary<string, Object>;
                        var properties = new ExpandoObject() as IDictionary<string, Object>;
                       
                        if (name.Contains("IQueryable") ||
                                    name.Contains("IEnumerable") ||
                                    name.Contains("List"))
                        {
                            var argument = attribute.Type.GetGenericArguments()[0];
                            componentName = argument.Name + componentName;

                            var items = new ExpandoObject() as IDictionary<string, Object>;

                            response.Add("type", "array");
                            response.Add("items", items);
                            items.Add("$ref", pathSchema + argument.Name);

                            if (Nullable.GetUnderlyingType(typeObject) != null)
                            {
                                response.Add("nullable", true);
                            }

                            if (!types.ContainsKey(argument.Name))
                            {
                                types.Add(argument.Name, argument);
                                typeSearch.Add(argument);
                            }
                        }
                        else if (name.Contains("Dictionary"))
                        {
                            componentName = attribute.DictionaryName;

                            Type[] arguments = attribute.Type.GetGenericArguments();
                            Type keyType = arguments[0];
                            Type valueType = arguments[1];

                            var nameType = keyType.Name;

                            if (keyType.IsClass
                                 && !keyType.FullName.StartsWith("System."))
                            {
                                if (!types.ContainsKey(keyType.Name))
                                {
                                    types.Add(keyType.Name, keyType);
                                    typeSearch.Add(keyType);
                                }
                            }
                            else if (IsNumericType(keyType) && IsintegerType(keyType))
                            {
                                nameType = "Integer";
                            }

                            var items = new ExpandoObject() as IDictionary<string, Object>;
                            items.Add("type", ToLowerFirstChar(nameType));
                            properties.Add(ToLowerFirstChar(attribute.DictionaryArg0), items);

                            nameType = valueType.Name;

                            if (valueType.IsClass
                                && !valueType.FullName.StartsWith("System."))
                            {
                                if (!types.ContainsKey(valueType.Name))
                                {
                                    types.Add(keyType.Name, valueType);
                                    typeSearch.Add(valueType);
                                }
                            }
                            else if (IsNumericType(valueType) && IsintegerType(valueType))
                            {
                                nameType = "Integer";
                            }


                            items = new ExpandoObject() as IDictionary<string, Object>;
                            items.Add("type", ToLowerFirstChar(nameType));
                            properties.Add(ToLowerFirstChar(attribute.DictionaryArg1), items);
                            response.Add("type", "object");
                            response.Add("properties", properties);

                        }
                        else
                        {
                            response.Add("type", "object");
                            response.Add("additionalProperties", false);
                            response.Add("properties", properties);

                            if (attrSystemresponse == null)
                            {
                                if (!types.ContainsKey(typeObject.Name) && attribute.Type.GetGenericArguments().Length == 0)
                                {
                                    types.Add(typeObject.Name, typeObject);
                                    typeSearch.Add(typeObject);
                                }
                            }
                        }

                        if (attrSystemresponse == null)
                        {
                            if (!components.ContainsKey(componentName))
                            {
                                components.Add(componentName, response);
                                componentsType.Add(componentName, response);
                            }

                        }
                        GetModels(typeObject, properties);

                        GetPath(method, paths, componentName, components);


                    }

                }
            }

            typeSearch = types.Values.ToList();
            for (var x = 0; x < typeSearch.Count; x++)
            {
               
                var response = new ExpandoObject() as IDictionary<string, Object>;
               
                if (!components.ContainsKey(typeSearch[x].Name))
                {
                    components.Add(typeSearch[x].Name, response);
                }

                if (typeSearch[x].IsEnum)
                {
                    var value = new ExpandoObject() as IDictionary<string, Object>;
                    
                    List<string> enums = new List<string>();
                    response.Add("enum", enums);
                    response.Add("type", "string");

                    foreach (var item in typeSearch[x].GetEnumValues())
                    {
                        var valEnum = Enum.GetName(typeSearch[x], item);
                        enums.Add(valEnum);
                    }

                    if (Nullable.GetUnderlyingType(typeSearch[x]) != null)
                    {
                        value.Add("nullable", true);
                    }
                }
                else
                {
                    var properties = new ExpandoObject() as IDictionary<string, Object>;
                    response.Add("type", "object");
                    response.Add("properties", properties);
                    response.Add("additionalProperties", false);

                    GetModels(typeSearch[x], properties);
                }
            }

        }

        void GetPath(MethodInfo method, IDictionary<string, Object> paths, string componentName, IDictionary<string, object> components)
        {
            object[] attributes = method.GetCustomAttributes(typeof(RouteAttribute), false);

            if (attributes.Length > 0)
            {
                RouteAttribute attribute = attributes[0] as RouteAttribute;

                var http = new ExpandoObject() as IDictionary<string, Object>;
               
                GetHttp(method, http, componentName, components);

                paths.Add(attribute.Template, http);

            }
        }

        void GetHttp (MethodInfo method, IDictionary<string, Object> http, string componentName, IDictionary<string, object> components)
        {
            object[] attributes = method.GetCustomAttributes(typeof(HttpGetAttribute), false);

            if (attributes.Length > 0)
            {
                http.Add("get", new ExpandoObject() as IDictionary<string, Object>);
                GetTags(method, http["get"] as IDictionary<string, Object>);
                GetSummary(method, http["get"] as IDictionary<string, Object>);
                GetOperation(method, http["get"] as IDictionary<string, Object>);
                GetParameters(method, http["get"] as IDictionary<string, Object>);
                GetResponsesOK(method, http["get"] as IDictionary<string, Object>, componentName);
            }
            else
            {
                attributes = method.GetCustomAttributes(typeof(HttpPostAttribute), false);

                if (attributes.Length > 0)
                {
                    http.Add("post", new ExpandoObject() as IDictionary<string, Object>);
                    GetTags(method, http["post"] as IDictionary<string, Object>);
                    GetSummary(method, http["post"] as IDictionary<string, Object>);
                    GetOperation(method, http["post"] as IDictionary<string, Object>);
                    GetParameters(method, http["post"] as IDictionary<string, Object>);
                    GetRequestBody(method, http["post"] as IDictionary<string, Object>, components);
                    GetResponsesOK(method, http["post"] as IDictionary<string, Object>, componentName);

                }
            }
        }

        void GetTags(MethodInfo method, IDictionary<string, Object> http)
        {
            http.Add("tags", new List<string>());
        }

        void GetSummary(MethodInfo method, IDictionary<string, Object> http)
        {
            object[] attributes = method.GetCustomAttributes(typeof(Summary), false);

            if (attributes.Length > 0)
            {
                Summary attribute = attributes[0] as Summary;
                http.Add("summary", attribute.description);
            }
        }

        void GetOperation(MethodInfo method, IDictionary<string, Object> http)
        {
            object[] attributes = method.GetCustomAttributes(typeof(FunctionNameAttribute), false);

            if (attributes.Length > 0)
            {
                FunctionNameAttribute attribute = attributes[0] as FunctionNameAttribute;
                http.Add("operationId", attribute.Name);
            }
        }

        void GetParameters(MethodInfo method, IDictionary<string, Object> http)
        {
            var parameters = new List<IDictionary<string, Object>>(); 

            object[] attributes = method.GetCustomAttributes(typeof(QueryStringParameter), false);

            foreach (var item in attributes)
            {
                QueryStringParameter attribute = item as QueryStringParameter;

                var parameter = new ExpandoObject() as IDictionary<string, Object>;
                parameter.Add("name", attribute.Name);
                parameter.Add("in", attribute.In);
                parameter.Add("description", attribute.Description);
                
                var schema = new ExpandoObject() as IDictionary<string, Object>;
                if (attribute.DataType.Name == "Guid")
                {
                    schema.Add("type", "string");
                    schema.Add("format", "uuid");
                    schema.Add("default", Guid.Empty.ToString());

                }
                else
                {
                    if (IsNumericType(attribute.DataType) && IsintegerType(attribute.DataType))
                    {
                        schema.Add("type", "integer");
                 
                    }
                    else
                    {
                        schema.Add("type", ToLowerFirstChar(attribute.DataType.Name));
                    }
                }

                parameter.Add("required", attribute.Required);

                parameter.Add("schema", schema);

                parameters.Add(parameter);

            }

            http.Add("parameters", parameters);
        }

        void GetResponsesOK(MethodInfo method, IDictionary<string, Object> http, string componentName)
        {

            var responses = new ExpandoObject() as IDictionary<string, Object>;
            var status = new ExpandoObject() as IDictionary<string, Object>;
            http.Add("responses", responses);
            responses.Add("200", status);
            status.Add("description", "Success");
            var content = new ExpandoObject() as IDictionary<string, Object>;
            status.Add("content", content);
            
            var typeContent = new ExpandoObject() as IDictionary<string, Object>;
            content.Add("text/plain", typeContent);
            var schema = new ExpandoObject() as IDictionary<string, Object>;
            if (attrSystemresponse != null)
            {
                if (attrSystemresponse.Name == "Guid")
                {
                    schema.Add("type", "string");
                    schema.Add("format", "uuid");
                    schema.Add("default", Guid.Empty.ToString());

                }
                else if (IsNumericType(attrSystemresponse) && IsintegerType(attrSystemresponse))
                {
                    schema.Add("type", "integer");
                }
                else
                {
                    schema.Add("type", ToLowerFirstChar(attrSystemresponse.Name));

                }
            }
            else
            {
                schema.Add("$ref", pathSchema + componentName);
            }
            typeContent.Add("schema", schema);

            typeContent = new ExpandoObject() as IDictionary<string, Object>;
            content.Add("application/json", typeContent);
            schema = new ExpandoObject() as IDictionary<string, Object>;
            if (attrSystemresponse != null)
            {
                if (attrSystemresponse.Name == "Guid")
                {
                    schema.Add("type", "string");
                    schema.Add("format", "uuid");
                    schema.Add("default", Guid.Empty.ToString());

                }
                else if (IsNumericType(attrSystemresponse) && IsintegerType(attrSystemresponse))
                {
                    schema.Add("type", "integer");
                }
                else
                {
                    schema.Add("type", ToLowerFirstChar(attrSystemresponse.Name));

                }
            }
            else
            {
                schema.Add("$ref", pathSchema + componentName);
            }
            typeContent.Add("schema", schema);

            typeContent = new ExpandoObject() as IDictionary<string, Object>;
            content.Add("text/json", typeContent);
            schema = new ExpandoObject() as IDictionary<string, Object>;
            if (attrSystemresponse != null)
            {
                if (attrSystemresponse.Name == "Guid")
                {
                    schema.Add("type", "string");
                    schema.Add("format", "uuid");
                    schema.Add("default", Guid.Empty.ToString());

                }
                else if (IsNumericType(attrSystemresponse) && IsintegerType(attrSystemresponse))
                {
                    schema.Add("type", "integer");
                }
                else
                {
                    schema.Add("type", ToLowerFirstChar(attrSystemresponse.Name));

                }
            }
            else
            {
                schema.Add("$ref", pathSchema + componentName);
            }
            typeContent.Add("schema", schema);
        }

        void GetRequestBody(MethodInfo method, IDictionary<string, Object> http, IDictionary<string, object> components)
        {
            object[] attributes = method.GetCustomAttributes(typeof(RequestBodyType), false);

            if (attributes.Length > 0)
            {
                RequestBodyType attribute = attributes[0] as RequestBodyType;

                string componentName = attribute.DataType.Name.Replace("`1", String.Empty);

                GetComponentName(attribute.DataType, ref componentName, attribute.DataType.AssemblyQualifiedName);

                var name = attribute.DataType.Name.Replace("`1", String.Empty);

                var typeObject = attribute.DataType;

                var response = new ExpandoObject() as IDictionary<string, Object>;
                var properties = new ExpandoObject() as IDictionary<string, Object>;

                if (name.Contains("IQueryable") ||
                            name.Contains("IEnumerable") ||
                            name.Contains("List"))
                {
                    var argument = attribute.DataType.GetGenericArguments()[0];


                    componentName = argument.Name + componentName;

                    var items = new ExpandoObject() as IDictionary<string, Object>;

                    response.Add("type", "array");
                    response.Add("items", items);
                    if (argument.FullName.StartsWith("System."))
                    {
                        items.Add("type", ToLowerFirstChar(argument.Name));
                    }
                    else
                    {
                        items.Add("$ref", pathSchema + argument.Name);

                        if (!types.ContainsKey(argument.Name))
                        {
                            types.Add(argument.Name, argument);
                            typeSearch.Add(argument);
                        }
                    }

                    if (Nullable.GetUnderlyingType(typeObject) != null)
                    {
                        response.Add("nullable", true);
                    }

                    
                }
                else if (name.Contains("Dictionary"))
                {
                    componentName = attribute.DictionaryName;

                    Type[] arguments = attribute.DataType.GetGenericArguments();
                    Type keyType = arguments[0];
                    Type valueType = arguments[1];

                    var nameType = keyType.Name;

                    if (keyType.IsClass
                         && !keyType.FullName.StartsWith("System."))
                    {
                        if (!types.ContainsKey(keyType.Name))
                        {
                            types.Add(keyType.Name, keyType);
                            typeSearch.Add(keyType);
                        }
                    }
                    else if (IsNumericType(keyType) && IsintegerType(keyType))
                    {
                        nameType = "Integer";
                    }

                    var items = new ExpandoObject() as IDictionary<string, Object>;
                    items.Add("type", ToLowerFirstChar(nameType));
                    properties.Add(ToLowerFirstChar(attribute.DictionaryArg0), items);

                    nameType = valueType.Name;

                    if (valueType.IsClass
                        && !valueType.FullName.StartsWith("System."))
                    {
                        if (!types.ContainsKey(valueType.Name))
                        {
                            types.Add(keyType.Name, valueType);
                            typeSearch.Add(valueType);
                        }
                    }
                    else if (IsNumericType(valueType) && IsintegerType(valueType))
                    {
                        nameType = "Integer";
                    }


                    items = new ExpandoObject() as IDictionary<string, Object>;
                    items.Add("type", ToLowerFirstChar(nameType));
                    properties.Add(ToLowerFirstChar(attribute.DictionaryArg1), items);
                    response.Add("type", "object");
                    response.Add("properties", properties);

                   

                }
                else
                {
                    response.Add("type", "object");
                    response.Add("additionalProperties", false);
                    response.Add("properties", properties);

                    if (!types.ContainsKey(typeObject.Name) && attribute.DataType.GetGenericArguments().Length == 0)
                    {
                        types.Add(typeObject.Name, typeObject);
                        typeSearch.Add(typeObject);
                    }
                }

                if (!components.ContainsKey(componentName))
                {
                    components.Add(componentName, response);
                    componentsType.Add(componentName, response);
                }

                GetModels(typeObject, properties);

                var responses = new ExpandoObject() as IDictionary<string, Object>;
                var content = new ExpandoObject() as IDictionary<string, Object>;
                http.Add("requestBody", responses);
                responses.Add("description", attribute.Description);
                responses.Add("content", content);

                var typeContent = new ExpandoObject() as IDictionary<string, Object>;
                var schema = new ExpandoObject() as IDictionary<string, Object>;

                typeContent = new ExpandoObject() as IDictionary<string, Object>;
                content.Add("application/json", typeContent);
                schema = new ExpandoObject() as IDictionary<string, Object>;
                schema.Add("$ref", pathSchema + componentName);
                typeContent.Add("schema", schema);

                typeContent = new ExpandoObject() as IDictionary<string, Object>;
                content.Add("text/json", typeContent);
                schema = new ExpandoObject() as IDictionary<string, Object>;
                schema.Add("$ref", pathSchema + componentName);
                typeContent.Add("schema", schema);


                /*
                if (!types.ContainsKey(attribute.DataType.Name))
                {
                    types.Add(attribute.DataType.Name, attribute.DataType);
                    typeSearch.Add(attribute.DataType);
                }*/
            }
        }


        void GetComponentName(Type type, ref string componentName, string qualifyName)
        {
          
            foreach (var property in type.GetProperties())
            {
                var name = property.PropertyType.Name.Replace("`1", String.Empty); 

                if (qualifyName.Contains(name))
                {
                    if (name.Contains("IQueryable") ||
                        name.Contains("IEnumerable") ||
                        name.Contains("List"))
                    {
                        componentName = name + componentName;

                        var argument = property.PropertyType.GetGenericArguments()[0];

                        componentName = argument.Name + componentName;

                        break;
                    }
                    else
                    {
                        componentName = name + componentName;
                    }
                }
                else
                {
                    break;
                }

            }
        }

        void GetModels(Type type, IDictionary<string, Object> properties)
        {
            
            foreach (var property in type.GetProperties())
            {
                var value = new ExpandoObject() as IDictionary<string, Object>;
                var items = new ExpandoObject() as IDictionary<string, Object>;


                if (property.PropertyType.Name.Contains("IQueryable") ||
                    property.PropertyType.Name.Contains("IEnumerable") ||
                    property.PropertyType.Name.Contains("List"))
                {
                    var argument = property.PropertyType.GetGenericArguments()[0];

                    value.Add("type", "array");
                    value.Add("items", items);
                    if (argument.FullName.StartsWith("System."))
                    {
                        if (argument.Name == "Guid")
                        {
                            items.Add("type", "string");
                            items.Add("format", "uuid");
                            items.Add("default", Guid.Empty.ToString());
                        }
                        else
                        {
                            items.Add("type", ToLowerFirstChar(argument.Name));
                        }
                    }
                    else 
                    {
                        items.Add("$ref", pathSchema + argument.Name);

                        if (!types.ContainsKey(argument.Name))
                        {
                            types.Add(argument.Name, argument);
                            typeSearch.Add(argument);
                        }
                    }

                    if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                    {
                        value.Add("nullable", true);
                    }

                    properties.Add(ToLowerFirstChar(property.Name), value);

                    
                }
                else if (property.PropertyType.IsEnum)
                {
                    items.Add("$ref", pathSchema + property.PropertyType.Name);
                    properties.Add(ToLowerFirstChar(property.Name), items);

                    if (!types.ContainsKey(property.PropertyType.Name))
                    {
                        types.Add(property.PropertyType.Name, property.PropertyType);
                        typeSearch.Add(property.PropertyType);
                    }

                    
                }
                else if (property.PropertyType.IsClass
                         && !property.PropertyType.FullName.StartsWith("System."))
                {
                    items.Add("$ref", pathSchema + property.PropertyType.Name);
                   
                    properties.Add(ToLowerFirstChar(property.Name), items);

                    if (!types.ContainsKey(property.PropertyType.Name))
                    {
                        types.Add(property.PropertyType.Name, property.PropertyType);
                        typeSearch.Add(property.PropertyType);
                    }
                }
                else if (!type.Name.Contains("Dictionary"))
                {
                    var prop = property.PropertyType;
                    var nullable = false;
                    if (Nullable.GetUnderlyingType(prop) != null) {
                        
                        prop = prop.GetGenericArguments()[0];
                        nullable = true;
                    }

                    if (prop.Name == "Guid")
                    {
                        value.Add("type", "string");
                        value.Add("format", "uuid");
                        value.Add("default", Guid.Empty.ToString());
                   
                    }
                    else if (IsNumericType(prop))
                    {
                        var nameType = property.PropertyType.Name;

                        if (IsintegerType(prop))
                        {
                            nameType = "Integer";
                        }
                        value.Add("type", "number");
                        value.Add("format", nameType.ToLower());

                    }
                    else if (Type.GetTypeCode(prop) == TypeCode.DateTime)
                    {
                        value.Add("type", "string");
                        value.Add("format", "date-time");
                    }
                    else
                    {
                        value.Add("type", prop.Name.ToLower());
                    }

                    if (nullable)
                    {
                        value.Add("nullable", true);
                    }
                    properties.Add(ToLowerFirstChar(property.Name), value);

                }
            }
        }

        string GetTypeShort(String type)
        {
            var index = type.LastIndexOf(".");
            return type.Substring(index + 1, type.Length - (index + 1));

        }

        void GetSecuritySchemas(IDictionary<string, Object> securitySchemes, string authUrl, string tokenUrl)
        {
            var securityName = new ExpandoObject() as IDictionary<string, Object>;
            var flows = new ExpandoObject() as IDictionary<string, Object>;
            var implicitSecurity = new ExpandoObject() as IDictionary<string, Object>;
            var scopes = new ExpandoObject() as IDictionary<string, Object>;


            implicitSecurity.Add("authorizationUrl", authUrl);
            implicitSecurity.Add("tokenUrl", tokenUrl);
            implicitSecurity.Add("scopes", scopes);
            flows.Add("authorizationCode", implicitSecurity);
            securityName.Add("type", "oauth2");
            securityName.Add("flows", flows);
            securitySchemes.Add("oAuthNoScopes", securityName);
        }

        void GetSecurity(IDictionary<string, Object> model)
        {
            var security = new ExpandoObject() as IDictionary<string, Object>;
            var type = new ExpandoObject() as IDictionary<string, Object>;

            var securityList = new List<object>();
            var typeList = new List<object>();

            type.Add("oAuthNoScopes", typeList);
            type.Add("cookieAuth", typeList);
            securityList.Add(type);
            model.Add("security", securityList);

        }

        public static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsintegerType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }

        private string ToLowerFirstChar(string input)
        {
            string newString = input;
            if (!String.IsNullOrEmpty(newString) && Char.IsUpper(newString[0]))
                newString = Char.ToLower(newString[0]) + newString.Substring(1);
            return newString;
        }
    }
}
