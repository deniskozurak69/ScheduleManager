using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace LibraryWebApplication1.Models
{
    public partial class Lecture
    {
        [Key]
        public int LectureId { get; set; }

        public string? DayOfWeek { get; set; }

        public int? LessonNumber { get; set; }
        public int? TotalWeeks { get; set; }
        public TimeSpan? BeginTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public virtual ICollection<Request>? Requests { get; set; } = new List<Request>();
        public virtual ICollection<SchedulePart>? ScheduleParts { get; set; } = new List<SchedulePart>();

    }
}
