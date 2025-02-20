using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryWebApplication1.Models
{
    public partial class Group
    {
        [Key]
        public int GroupId { get; set; }

        public string? GroupName { get; set; }
        public int? CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
        public int? SpecialtyId { get; set; }
        [ForeignKey("SpecialtyId")]
        public virtual Specialty? Specialty { get; set; }

        public virtual ICollection<GroupSubject>? GroupSubjects { get; set; } = new List<GroupSubject>();
        public virtual ICollection<SchedulePart>? ScheduleParts { get; set; } = new List<SchedulePart>();

    }
}
