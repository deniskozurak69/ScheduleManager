using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryWebApplication1.Models
{
    public partial class Subject
    {
        [Key]
        public int SubjectId { get; set; }
        public int? UserId { get; set; }

        public int? TotalHours { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Файл не додано")]
        public string? Description { get; set; }
        public virtual ICollection<GroupSubject>? GroupSubjects { get; set; } = new List<GroupSubject>();
        public virtual ICollection<TeacherSubject>? TeacherSubjects { get; set; } = new List<TeacherSubject>();
    }
}
