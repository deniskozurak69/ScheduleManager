using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryWebApplication1.Models;
public partial class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CategoryId { get; set; }
    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    public string? Name { get; set; }
    [Required(ErrorMessage = "Файл не додано")]
    public string? Description { get; set; }
    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
