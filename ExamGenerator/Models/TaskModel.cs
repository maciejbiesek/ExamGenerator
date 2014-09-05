using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExamGenerator.Models
{
    public class TaskModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string tags { get; set; }
        public List<int> TagIdList { get; set; }
    }


}