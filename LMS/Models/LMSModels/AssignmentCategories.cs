using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class AssignmentCategories
    {
        public AssignmentCategories()
        {
            Assignments = new HashSet<Assignments>();
            Submissions = new HashSet<Submissions>();
        }

        public uint Weight { get; set; }
        public string Name { get; set; }
        public uint CatId { get; set; }
        public uint ClassId { get; set; }

        public virtual Classes Class { get; set; }
        public virtual ICollection<Assignments> Assignments { get; set; }
        public virtual ICollection<Submissions> Submissions { get; set; }
    }
}
