using System.Reflection;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ImageMagick;

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

        [FunctionName("ImageResize")]
        public static IActionResult ImageToBase64Resize(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "image_resize")] HttpRequest req,
            ILogger log)
        {
            // requestの画像を取得
            var stream = new StreamReader(req.Body).BaseStream;

            // streamからmemorystreamに変換
            var ms = new MemoryStream();
            StreamToMemoryStream(stream, ms);

            var settings = new MagickReadSettings();

            ms.Position = 0;
            var image = new MagickImage(ms, settings);

            // オリジナル画像の幅を保持
            int originalImageWith = image.Width;

            // 画像を縮小する
            var reducedImageMemory = new MemoryStream();
            image.Resize(originalImageWith / 2, 0);
            image.Write(reducedImageMemory);

            // 元のサイズに戻す
            reducedImageMemory.Position = 0;
            var outputImageMemory = new MemoryStream();

            settings.Width = originalImageWith;
            var resizeImageMemory = new MagickImage(reducedImageMemory, settings);
            resizeImageMemory.Strip();
            resizeImageMemory.Resize(originalImageWith, 0);
            resizeImageMemory.Write(outputImageMemory);

            var bytes = Convert.FromBase64String(Convert.ToBase64String(outputImageMemory.ToArray()));
            return new FileContentResult(bytes, "image/png");
        }

        public static void StreamToMemoryStream(Stream input, MemoryStream output)
        {
            byte[] buffer = new byte[32*1024];
            int read;
            while((read = input.Read (buffer, 0, buffer.Length)) > 0)
            {
                output.Write (buffer, 0, read);
            }
        }
    }
}
