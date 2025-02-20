using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryWebApplication1.Models
{
    public partial class Request
    {
        [Key]
        public int RequestId { get; set; }
        public int? TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public virtual User? Teacher { get; set; }
        public int? LectureId { get; set; }
        [ForeignKey("LectureId")]
        public virtual Lecture? Lecture { get; set; }
    }
}
