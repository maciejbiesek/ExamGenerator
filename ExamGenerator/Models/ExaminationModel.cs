using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExamGenerator.Models
{
    public class ExaminationModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> TagIdList { get; set; }
        public int NumberOfQuestions { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public int Version { get; set; }
    }
}