using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace poc_azure_function.Controllers
{
    public class SpreadSheetOAuthController
    {
        private readonly string[] _scopes = { SheetsService.Scope.Spreadsheets };
        private SheetsService _sheetsService;

        /// <summary>
        /// AzureFunctionでSpread Sheetを作成
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("SpreadCreate")]
        public IActionResult SpreadCreate(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "spread_create")] HttpRequest req, ILogger log)
        {
            // Create Google Sheets API service.
            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = ConnectSpreadSheetOAuth(),
                ApplicationName = "TestCreateSheet",
            });


            var requestBody = new Spreadsheet
            {
                Properties = new SpreadsheetProperties()
            };

            requestBody.Properties.Title = "CreateMySheet";

            var request = _sheetsService.Spreadsheets.Create(requestBody);
            var response = request.Execute();

            return new ObjectResult(JsonConvert.SerializeObject(response));
        }

        /// <summary>
        /// Spread Sheetにアクセストークンを取得して接続
        /// </summary>
        /// <returns></returns>
        private UserCredential ConnectSpreadSheetOAuth()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("oauth_desctop_appkey.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    _scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            return credential;
        }
    }
}
