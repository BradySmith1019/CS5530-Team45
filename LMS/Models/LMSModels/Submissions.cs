using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submissions
    {
        public uint Score { get; set; }
        public string Contents { get; set; }
        public DateTime Time { get; set; }
        public int SubmissionId { get; set; }
        public string UId { get; set; }
        public uint AssignId { get; set; }
        public uint CatId { get; set; }
        public uint ClassId { get; set; }
        public uint CourseId { get; set; }
        public string Subject { get; set; }

        public virtual Assignments Assign { get; set; }
        public virtual AssignmentCategories Cat { get; set; }
        public virtual Classes Class { get; set; }
        public virtual Courses Course { get; set; }
        public virtual Departments SubjectNavigation { get; set; }
        public virtual Students U { get; set; }
    }
}
