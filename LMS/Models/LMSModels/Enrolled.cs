using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Enrolled
    {
        public string Grade { get; set; }
        public string UId { get; set; }
        public uint ClassId { get; set; }
        public uint CourseId { get; set; }
        public string Subject { get; set; }
        public uint EnrolledId { get; set; }

        public virtual Classes Class { get; set; }
        public virtual Courses Course { get; set; }
        public virtual Departments SubjectNavigation { get; set; }
        public virtual Students U { get; set; }
    }
}
