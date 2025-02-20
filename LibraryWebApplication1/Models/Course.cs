using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryWebApplication1.Models
{
    public partial class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public string? CourseName { get; set; }
        public virtual ICollection<Group>? Groups { get; set; } = new List<Group>();
    }
}
