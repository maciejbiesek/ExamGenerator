using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExamGenerator.Models;

namespace ExamGenerator.Controllers
{
    public class TagController : Controller
    {
        private generator_kolokwiowEntities genKolEnt = new generator_kolokwiowEntities();

        public ActionResult Index()
        {
            List<TAGS> getTags = genKolEnt.TAGS.Select(t => t).ToList();
            return View(getTags);
        }

    }
}
