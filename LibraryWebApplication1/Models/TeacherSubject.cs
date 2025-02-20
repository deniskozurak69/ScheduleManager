using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryWebApplication1.Models
{
    public partial class TeacherSubject
    {
        [Key]
        public int ItemId { get; set; }

        public int? SubjectId { get; set; }
        [ForeignKey("SubjectId")]
        public virtual Subject? Subject { get; set; }
        public string? Name { get; set; }
        public int? TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public virtual User? Teacher { get; set; }
        public virtual ICollection<SchedulePart>? ScheduleParts { get; set; } = new List<SchedulePart>();

    }
}