using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using Barracuda.OpenApi.Interfaces;
using Barracuda.OpenApi.Attributes;
using Demo.Azure.Functions.Models;

namespace Demo.Azure.Functions
{
    [OpenAPIVersion("3.0.1")]
    [APIInfo("Demo API", "v1")]

    public class OpenApiReaderFunctions
    {
        public readonly IOpenApiBuilder _builder;
      
        public OpenApiReaderFunctions(
            IOpenApiBuilder builder
            )
        {
            _builder = builder;
        }

        [FunctionName("GetDemo")]
        /*
           open api reader attributes 
        */
        [OpenAPI]
        [Summary("Get products register")]
        [QueryStringParameter("up-tenant-id", "header", "Tenant id", typeof(string), Required = true)]
        [QueryStringParameter("filter", "query", "Filter", typeof(string))]
        [QueryStringParameter("sort", "query", "Sort", typeof(string))]
        [QueryStringParameter("page", "query", "Page", typeof(int))]
        [QueryStringParameter("productGroup", "query", "Product Group", typeof(string))]
        [QueryStringParameter("productUnit", "query", "Product Unit", typeof(string))]
        [OpenAPIProducesResponse(typeof(List<DemoModels>))]
        [HttpGet]
        [Route("/api/demos/GetAll")]
        public async Task<IActionResult> GetDemo(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "demos/GetDemo")] HttpRequest req,
            ILogger log)
        {
            return await Task.FromResult(new OkObjectResult(new List<DemoModels>()));
        }


        [FunctionName("GetWithId")]
        [OpenAPI]
        [Summary("Get a product register")]
        [QueryStringParameter("up-tenant-id", "header", "Tenant id", typeof(string), Required = true)]
        [QueryStringParameter("id", "path", "Item Code", typeof(string), Required = true)]
        [OpenAPIProducesResponse(typeof(DemoModels))]
        [HttpGet]
        [Route("/api/demos/Get/{id}")]
        public async Task<IActionResult> Get(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "demos/Get/{id}")] HttpRequest req, string id)
        {
            return await Task.FromResult(new OkObjectResult(new DemoModels()));
        }

        [FunctionName("Post")]
        [OpenAPI]
        [Summary("Add, update or delete a product register")]
        [QueryStringParameter("up-tenant-id", "header", "Tenant id", typeof(string), Required = true)]
        [RequestBodyType(typeof(DemoModels), "Product register")]
        [OpenAPIProducesResponse(typeof(String))]
        [HttpPost]
        [Route("/api/demos/POST")]
        public async Task<IActionResult> Post(
         [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "demos/POST")] HttpRequestMessage req)
        {

            DemoModels data = await req.Content.ReadAsAsync<DemoModels>();

            return await Task.FromResult(new OkObjectResult("demo post"));
        }

        [FunctionName("OpenAPI")]
        public async Task<IActionResult> OpenAPI([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "demos/openapi/v1")] HttpRequestMessage request)
        {
            
            return await Task.FromResult(_builder.Build(Assembly.GetExecutingAssembly(), this.GetType().Name, request));
        }


        [FunctionName("OpenAPIUI")]
        public async Task<IActionResult> OpenAPIUI([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "demos/openapi/ui")] HttpRequestMessage request)
        {
            return await Task.FromResult(_builder.OpenAPIUI());
        }

        [FunctionName("OpenAPIAuth")]
        public async Task<IActionResult> OpenAPIAuth([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "demos/openapi/auth")] HttpRequestMessage request)
        {
            return await Task.FromResult(_builder.OpenAPIAuth());
        }
    }
}
