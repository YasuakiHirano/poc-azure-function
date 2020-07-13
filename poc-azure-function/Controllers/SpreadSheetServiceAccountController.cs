using System;
using System.Collections.Generic;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Data = Google.Apis.Sheets.v4.Data;

namespace poc_azure_function.Controllers
{
    public class SpreadSheetServiceAccountController
    {
        private SheetsService _sheetsService;
        private DriveService _driveService;
        public string _editSpreadsheetId = "{spread_sheets_id}";
        public string _parentFolderId = "{parent_folder_id}";

        /// <summary>
        /// Service AccountでGoogle Spread SheetにアクセスするためのSheetsServiceを返す
        /// </summary>
        /// <returns></returns>
        private SheetsService ConnectSpreadSheet()
        {
            GoogleCredential credential;
            string[] scopes = { SheetsService.Scope.Spreadsheets }; 
            using (var stream = new FileStream("service-account-key.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(scopes);
            }
            return new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "codelike spread seets connect",
            });
        }

        /// <summary>
        /// Service AccountでGoogle Drive にアクセスするためのDriveServiceを返す。
        /// </summary>
        /// <returns></returns>
        private DriveService ConnectDriveService()
        {
            GoogleCredential credential;
            string[] scopes = { DriveService.Scope.Drive };
            using (var stream = new FileStream("service-account-key.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(scopes);
            }
            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "codelike drive connect",
            });
        }
        /// <summary>
        /// Google Drive Apiフォルダ作成
        /// </summary>
        [FunctionName("SaCreateFolder")]
        public IActionResult CreateFolder(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sa_create_folder")] HttpRequest req, ILogger log)
        {
            _driveService = ConnectDriveService();

            Google.Apis.Drive.v3.Data.File meta = new Google.Apis.Drive.v3.Data.File();
            meta.Name = "folderName";
            meta.Description = "folder description";
            meta.MimeType = "application/vnd.google-apps.folder";

            // 親ディレクトリはService Accountのメールアドレスを入れて共有する
            meta.Parents = new List<string> { _parentFolderId };

            var request = _driveService.Files.Create(meta);
            request.Fields = "id";
            var response = request.Execute();

            return new ObjectResult(JsonConvert.SerializeObject(response.Id));
        }

        /// <summary>
        /// Service AccountでSpread Sheetを作成する
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("SaSpreadCreate")]
        public IActionResult SpreadCreate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sa_spread_create")] HttpRequest req, ILogger log)
        {
            _driveService = ConnectDriveService();

            Google.Apis.Drive.v3.Data.File meta = new Google.Apis.Drive.v3.Data.File
            {
                Name = "SheetName",
                Description = "SpreadSheetDescription",
                MimeType = "application/vnd.google-apps.spreadsheet",
                Parents = new List<string> { _parentFolderId }
            };
            var request = _driveService.Files.Create(meta);
            var response = request.Execute();

            return new ObjectResult(JsonConvert.SerializeObject(response.Id));
        }

        /// <summary>
        /// Service AccountでSpread Sheetを編集する
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("SaSpreadEdit")]
        public IActionResult SpreadEdit(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sa_spread_edit")] HttpRequest req, ILogger log)
        {
            _sheetsService = ConnectSpreadSheet();
            String range = "シート1!B2";
            string valueInputOption = "USER_ENTERED";
            List<Data.ValueRange> updateData = new List<Data.ValueRange>();
            var dataValueRange = new Data.ValueRange();

            List<IList<object>> data = new List<IList<object>>();
            data.Add(GetRandomList());
            data.Add(GetRandomList());
            data.Add(GetRandomList());
            data.Add(GetRandomList());
            data.Add(GetRandomList());
            dataValueRange.Range = range;
            dataValueRange.Values = data;
            updateData.Add(dataValueRange);

            Data.BatchUpdateValuesRequest requestBody = new Data.BatchUpdateValuesRequest();
            requestBody.ValueInputOption = valueInputOption;
            requestBody.Data = updateData;
            var request = _sheetsService.Spreadsheets.Values.BatchUpdate(requestBody, _editSpreadsheetId);

            Data.BatchUpdateValuesResponse response = request.Execute();

            return new ObjectResult(JsonConvert.SerializeObject(response));
        }

        /// <summary>
        /// Service AccountでSpread Sheetに追記処理
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("SaSpreadAppend")]
        public IActionResult SpreadAppend(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sa_spread_append")] HttpRequest req, ILogger log)
        {
            _sheetsService = ConnectSpreadSheet();
            string range = "シート1!B2";
            var valueInputOption = (SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum)2;
            var insertDataOption = (SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum)1;

            Data.ValueRange requestBody = new Data.ValueRange();
            List<IList<object>> data = new List<IList<object>>();
            data.Add(GetAppendList());
            data.Add(GetAppendList());
            data.Add(GetAppendList());
            data.Add(GetAppendList());
            data.Add(GetAppendList());
            requestBody.Values = data;
            SpreadsheetsResource.ValuesResource.AppendRequest request = _sheetsService.Spreadsheets.Values.Append(requestBody, _editSpreadsheetId, range);
            request.ValueInputOption = valueInputOption;
            request.InsertDataOption = insertDataOption;

            Data.AppendValuesResponse response = request.Execute();

            return new ObjectResult(JsonConvert.SerializeObject(response));
        }

        private List<object> GetRandomList()
        {
            var list = new List<object>
            {
                Guid.NewGuid().ToString("N").Substring(0, 6),
                Guid.NewGuid().ToString("N").Substring(0, 6),
                Guid.NewGuid().ToString("N").Substring(0, 6),
                Guid.NewGuid().ToString("N").Substring(0, 6),
                Guid.NewGuid().ToString("N").Substring(0, 6)
            };
            return list;
        }

        private List<object> GetAppendList()
        {
            var list = new List<object>
            {
                "append1",
                "append2",
                "append3",
                "append4",
                "append5",
            };
            return list;
        }        
    }
}
