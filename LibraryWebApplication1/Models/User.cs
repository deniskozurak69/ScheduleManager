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
        public string? Name { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public string? Surname { get; set; }
        public string? Password { get; set; }
        public int IsLogged { get; set; }
        public int Priority { get; set; }
        public virtual ICollection<Subject>? Subjects { get; set; } = new List<Subject>();
        public virtual ICollection<Request>? Requests { get; set; } = new List<Request>();
        public virtual ICollection<TeacherSubject>? TeacherSubjects { get; set; } = new List<TeacherSubject>();
    }
}
