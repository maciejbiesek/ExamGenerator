//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ExamGenerator.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TAGS
    {
        public TAGS()
        {
            this.EXAMS = new HashSet<EXAMS>();
            this.TASKS = new HashSet<TASKS>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<EXAMS> EXAMS { get; set; }
        public virtual ICollection<TASKS> TASKS { get; set; }
    }
}
