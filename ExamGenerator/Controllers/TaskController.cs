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
                taskRow.id = task.Id;
                taskRow.name = task.Name;
                taskRow.tags = tagsList;

                getModelTasks.Add(taskRow);
            }


            return View(getModelTasks);
        }

    }
}
