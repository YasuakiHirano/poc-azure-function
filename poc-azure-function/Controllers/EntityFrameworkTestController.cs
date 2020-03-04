using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using poc_azure_function.Model;

namespace poc_azure_function.Controllers
{
    public class EntityFrameworkTestController
    {
        private static void OutputBoard(Board board) {
            Console.WriteLine("Board Table[ Id:" + board.Id + " Title:" + board.Title + " UserName:" + board.UserName + " AboutText:" + board.AboutText + " ]");
        }

        private static void OutputMessage(Message message)
        {
            Console.WriteLine("Message Table[ Id:" + message.Id + " BoardId:" + message.BoardId + " UserName:" + message.UserName + " MessageText:" + message.MessageText + " ]");
        }

        [FunctionName("EntityFrameworkTestController_Select")]
        public static IActionResult Select(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "efcore_test_select")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("start EntityFrameworkTestController_Select");

            using (var _dbContext = new DBContextFactory().CreateDbContext())
            {
                // 主キーを検索
                Console.WriteLine("----- find test -----");
                var board = _dbContext.Boards.Find((long)2);
                OutputBoard(board);

                // リストで取得
                Console.WriteLine("----- toList test -----");
                var boards = _dbContext.Boards.ToList();
                foreach (var row in boards)
                {
                    OutputBoard(row);
                }

                // 条件指定で取得する
                Console.WriteLine("----- where test -----");
                board = _dbContext.Boards
                    .Where(e => e.AboutText.Contains("説明３"))
                    .FirstOrDefault();
                OutputBoard(board);

                // 外部結合
                Console.WriteLine("----- left join test -----");
                var leftJoinObjects =_dbContext.Boards
                    .GroupJoin(_dbContext.Messages,
                        b => b.Id,
                        m => m.BoardId,
                        (joinBoard, joinMessage) => new
                        {
                            Board = joinBoard,
                            Message = joinMessage
                        }
                    );

                foreach (var leftJoinObject in leftJoinObjects)
                {
                    OutputBoard(leftJoinObject.Board);
                    foreach (var message in leftJoinObject.Message.ToList()) {
                        OutputMessage(message);
                    }
                }

                // 内部結合
                Console.WriteLine("----- join test -----");
                var joinObjects = _dbContext.Boards
                    .Join(_dbContext.Messages,
                        b => b.Id,
                        m => m.BoardId,
                        (joinBoard, joinMessage) => new
                        {
                            Board = joinBoard,
                            Message = joinMessage
                        }
                    );

                foreach (var joinObject in joinObjects)
                {
                    OutputBoard(joinObject.Board);
                    OutputMessage(joinObject.Message);
                }
            }

            return new OkResult();
        }


        [FunctionName("EntityFrameworkTestController_Insert")]
        public static async System.Threading.Tasks.Task<IActionResult> Insert(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "efcore_test_insert")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("start EntityFrameworkTestController_Insert");

            // jsonリクエストを取得
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // jsonリクエストで掲示板登録
            using (var _dbContext = new DBContextFactory().CreateDbContext())
            {
                var board = new Board
                {
                    Title = data?.Title,
                    UserName = data?.UserName,
                    AboutText = data?.AboutText,
                    Password = data?.Password
                };

                _dbContext.Boards.Add(board);
                _dbContext.SaveChanges();
            }

            return new OkResult();
        }

        [FunctionName("EntityFrameworkTestController_Update")]
        public static async System.Threading.Tasks.Task<IActionResult> UpdateAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "efcore_test_update")] HttpRequest req,
            ILogger log
        )
        {
            log.LogInformation("start EntityFrameworkTestController_Update");

            // jsonリクエストを取得
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // jsonリクエストで掲示板更新
            using (var _dbContext = new DBContextFactory().CreateDbContext())
            {
                var board = _dbContext.Boards.Find((long)data?.Id);
                if (board != null)
                {
                    board.Title = data?.Title;
                    board.UserName = data?.UserName;
                    board.AboutText = data?.AboutText;
                    board.Password = data?.Password;
                    _dbContext.SaveChanges();
                }
            }

            return new OkResult();
        }

    }
}
