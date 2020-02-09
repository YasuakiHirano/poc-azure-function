using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    }
}
