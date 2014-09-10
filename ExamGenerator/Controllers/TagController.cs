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

        public ActionResult Create()
        {
            ViewBag.Title = "Dodawanie nowego tagu";
            return View();
        }

        [HttpPost]
        public ActionResult Create(TAGS model)
        {
            if (ModelState.IsValid)
            {
                if (genKolEnt.TAGS.Any(u => u.Name == model.Name))
                {
                    ViewBag.Message = "Taki tag już istnieje w bazie.";
                    return View(model);
                }
                else
                {
                    var tag = new TAGS
                    {
                        Name = model.Name
                    };
                    genKolEnt.TAGS.Add(tag);
                    genKolEnt.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id = 0)
        {
            try
            {
                TAGS tag = genKolEnt.TAGS.Find(id);
                foreach (var t in tag.TASKS.ToList())
                {
                    tag.TASKS.Remove(t);
                }

                genKolEnt.TAGS.Remove(tag);
                genKolEnt.SaveChanges();

                return Content(Boolean.TrueString);
            }
            catch
            {
                return Content(Boolean.FalseString);
            }
        }

    }
}
