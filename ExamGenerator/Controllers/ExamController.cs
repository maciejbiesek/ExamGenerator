using ExamGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExamGenerator.Controllers
{
    public class ExamController : Controller
    {
        private generator_kolokwiowEntities genKolEnt = new generator_kolokwiowEntities();

        public ActionResult Index()
        {
            List<EXAMS> getExams = genKolEnt.EXAMS.Select(t => t).ToList();
            return View(getExams);
        }


    }
}
