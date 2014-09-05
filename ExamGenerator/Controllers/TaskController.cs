using ExamGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExamGenerator.Controllers
{
    public class TaskController : Controller
    {
        private generator_kolokwiowEntities genKolEnt = new generator_kolokwiowEntities();


        public ActionResult Index()
        {
            List<TASKS> getTasks = genKolEnt.TASKS.Select(t => t).ToList();
            return View(getTasks);
        }

        public ActionResult Create()
        {
            TaskModel model = new TaskModel();

            ViewBag.Tags = new MultiSelectList(genKolEnt.TAGS, "Id", "Name");

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(TaskModel model)
        {
            try
            {
                TASKS task = new TASKS()
                {
                    Id = model.Id,
                    Name = model.Name,
                    Content = model.Content // to trzeba zmienić

                };
                genKolEnt.TASKS.Add(task);
                genKolEnt.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View(model);
            }
        }

    }
}
