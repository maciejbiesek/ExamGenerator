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
    
    public partial class TASKS
    {
        public TASKS()
        {
            this.TAGS = new HashSet<TAGS>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
    
        public virtual ICollection<TAGS> TAGS { get; set; }
    }
}
