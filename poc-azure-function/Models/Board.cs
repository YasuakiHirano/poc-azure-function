using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace poc_azure_function.Model
{
    [Table("boards")]
    public class Board
    {
        public Board()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("user_name")]
        public string UserName { get; set; }

        [Column("about_text")]
        public string AboutText { get; set; }

        [Column("password")]
        public string Password { get; set; }
    }
}
