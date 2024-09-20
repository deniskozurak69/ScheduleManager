using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryWebApplication1.Models
{
    public partial class Article
    {
        [Key]
        public int ArticleId { get; set; }
        public int? AuthorId { get; set; }

        [Display(Name = "Article Name")]
        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public string? ArticleName { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public int? CategoryId { get; set; }

        [Display(Name = "Publish Date")]
        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public DateOnly? PublishDate { get; set; }

        [ForeignKey("AuthorId")]
        public virtual User? Author { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? CategoryNavigation { get; set; }

        [Required(ErrorMessage = "Файл не додано")]
        public string? Text { get; set; }
    }
}