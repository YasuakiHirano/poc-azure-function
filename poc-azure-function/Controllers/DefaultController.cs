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
using System.Threading;

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

        [FunctionName("CastTestController_Run")]
        public static IActionResult CastTestRun(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cast/default")] HttpRequest req
        )
        {
            var idText = req.Query["userId"];
            var ageText = req.Query["age"];

            Guid id;
            int age;

            try
            {
                id = Guid.Parse(idText);
                age = int.Parse(ageText);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new Dictionary<string, object> { { "エラーが発生しました。パラメータを確認してください。", e } });
            }


            var result = new Dictionary<string, string>
            {
                { "ID", id.ToString() },
                { "年齢", age.ToString() }
            };

            return new OkObjectResult(result);
        }

        [FunctionName("CastTestController_Try")]
        public static IActionResult CastTestTry(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cast/try")] HttpRequest req
        )
        {
            string idText = req.Query["userId"];
            string ageText = req.Query["age"];

            if (!Guid.TryParse(idText, out Guid id))
            {
                return new BadRequestObjectResult(new Dictionary<string, object> { { "ユーザーIDでエラーが発生しました。パラメータを確認してください。", idText } });
            }

            if (!int.TryParse(ageText, out int age))
            {
                return new BadRequestObjectResult(new Dictionary<string, object> { { "年齢でエラーが発生しました。パラメータを確認してください。", ageText } });
            }

            var result = new Dictionary<string, string>
            {
                { "ID", id.ToString() },
                { "年齢", age.ToString() }
            };

            return new OkObjectResult(result);
        }

        [FunctionName("ThreadTest")]
        public static IActionResult ThreadTest(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "thead/test")] HttpRequest req
        )
        {
            Task.Run(() => {
                Thread.Sleep(3000);
                var name = req.Query["name"];
                Console.WriteLine($"名前は{name}です。");
            });
            return new OkObjectResult("thread test!!");
        }

        [FunctionName("ThreadTest2")]
        public static IActionResult ThreadTest2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "thead/test2")] HttpRequest req
        )
        {
            var name = req.Query["name"];
            Task.Run(() => {
                var name = req.Query["name"];
                Thread.Sleep(3000);
                Console.WriteLine($"名前は{name}です。");
            });
            return new OkObjectResult("thread test!!");
        }

        [FunctionName("ThreadTest3")]
        public static IActionResult ThreadTest3(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "thead/test3")] HttpRequest req
        )
        {
            Task.Run(() => {
                Thread.Sleep(1000);
                Console.WriteLine($"thread test1");
            });
            Task.Run(() => {
                Thread.Sleep(1000);
                Console.WriteLine($"thread test2");
            });
            Task.Run(() => {
                Thread.Sleep(1000);
                Console.WriteLine($"thread test3");
            });

            var newTask = new Task(TaskMethod);
            newTask.Start();

            Console.WriteLine($"thread test4");
            return new OkObjectResult("thread test!!");
        }

        private static void TaskMethod() {
            Thread.Sleep(1000);
            Console.WriteLine($"task method!!");
        }
    }
}
