﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExamGenerator.Models
{
    public class TaskModel
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }
        public List<int> TagIdList { get; set; }

    }
}