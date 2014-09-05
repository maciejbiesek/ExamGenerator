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
            List<TaskModel> getModelTasks = new List<TaskModel>();

            foreach(var task in getTasks){
                string tagsList = "";
                foreach(var tag in task.TAGS){
                    tagsList += tag.Name + ", ";
                }
                tagsList = tagsList.Remove(tagsList.Length - 2);

                TaskModel taskRow = new TaskModel();
                taskRow.Id = task.Id;
                taskRow.Name = task.Name;
                taskRow.Tags = tagsList;

                getModelTasks.Add(taskRow);
            }
            
            return View(getModelTasks);
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
                if (ModelState.IsValid)
                {
                    if(genKolEnt.TASKS.Any(t => t.Name.Equals(model.Name))){
                        ViewBag.Message = "Zadanie o takiej nazwie już istnieje w bazie.";
                        return View(model);
                    }
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
                    else
                    return View(model);
            }
            catch(Exception e)
            {
                ViewBag.Message = e.Message;
                return View(model);
            }
        }

    }
}
