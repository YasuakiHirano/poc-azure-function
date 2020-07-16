using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace poc_azure_function.Controllers
{
    public static class ImageController
    {
        [FunctionName("ImageToBase64")]
        public static IActionResult ImageToBase64(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "image_to_base64")] HttpRequest req,
            ILogger log)
        {
            var stream = new StreamReader(req.Body).BaseStream;
            byte[] bytes;
            using (BinaryReader br = new BinaryReader(stream))
            {
                bytes = br.ReadBytes((int)stream.Length);
            }
            var base64Image = Convert.ToBase64String(bytes);
            return new OkObjectResult(base64Image);
        }

        [FunctionName("Base64ToImage")]
        public static async System.Threading.Tasks.Task<IActionResult> Base64ToImageAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "base64_to_image")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            var base64Image = data?.image;

            var bytes = Convert.FromBase64String(base64Image.Value);

            return new FileContentResult(bytes, "image/png");
        }
    }
}
