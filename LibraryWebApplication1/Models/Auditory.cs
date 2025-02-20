using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryWebApplication1.Models
{
    public partial class Auditory
    {
        [Key]
        public int AuditoryId { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public string? AuditoryName { get; set; }
        public virtual ICollection<SchedulePart>? ScheduleParts { get; set; } = new List<SchedulePart>();
    }
}
