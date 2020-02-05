using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace poc_azure_function.Model
{
    [Table("messages")]
    public class Message
    {
        public Message()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Column("board_id")]
        public long BoardId { get; set; }

        [Column("user_name")]
        public string UserName { get; set; }

        [Column("message")]
        public string MessageText { get; set; }
    }
}
