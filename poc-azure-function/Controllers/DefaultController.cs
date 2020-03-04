using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using poc_azure_function.Request;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace poc_azure_function.Controllers
{
    public static class DefaultController
    {
        [FunctionName("DefaultController_Run")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "default")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        [FunctionName("CSharpValidationTest")]
        public static IActionResult CSharpValidationTest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "csharp_test_validation")] HttpRequest req,
            ILogger log
            )
        {
            TestRequest request = new TestRequest
            {
                Name = req.Query["name"].ToString() ?? "",
                Age = req.Query["age"].ToString() ?? "",
                ZipCode = req.Query["zip_code"].ToString() ?? "",
                BirthDay = string.IsNullOrEmpty(req.Query["birth"].ToString()) ? DateTime.Now : DateTime.Parse(req.Query["birth"])
            };

            List<ValidationResult> results = new List<ValidationResult>();
            Validator.TryValidateObject(request, new ValidationContext(request, null, null), results, true);

            if (results.Count != 0)
            {
                foreach (ValidationResult result in results)
                {
                    Console.WriteLine(result.ErrorMessage);
                }
                return new BadRequestResult();
            }

            return new CreatedResult("", request);
        }        
    }
}
