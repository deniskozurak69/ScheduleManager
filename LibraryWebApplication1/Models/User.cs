using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace LibraryWebApplication1.Models
{
    public partial class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Файл не додано")]
        public string? ProfilePhoto { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public double Longtitude { get; set; }
        public virtual ICollection<Article>? Articles { get; set; } = new List<Article>();
    }
}
