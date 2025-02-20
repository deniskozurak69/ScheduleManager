using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryWebApplication1.Models
{
    public partial class SchedulePart
    {
        [Key]
        public int PartId { get; set; }

        public string? DayOfWeek { get; set; }

        public int? LessonNumber { get; set; }
        public TimeSpan? BeginTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? SubjectId { get; set; }
        public int? AuditoryId { get; set; }
        [ForeignKey("AuditoryId")]
        public virtual Auditory? Auditory { get; set; }
        public int? TeacherSubjectId { get; set; }
        [ForeignKey("TeacherSubjectId")]
        public virtual TeacherSubject? TeacherSubject { get; set; }
        public int? ScheduleId { get; set; }
        public int? ActiveSchedule { get; set; }
        public int? LectureId { get; set; }
        [ForeignKey("LectureId")]
        public virtual Lecture? Lecture { get; set; }
        public int? GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual Group? Group { get; set; }

    }
}
