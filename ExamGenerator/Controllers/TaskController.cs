using ExamGenerator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                foreach(var tag in task.TAGS.ToList()){
                    tagsList += tag.Name + ", ";
                }
                if (tagsList != "")
                {
                    tagsList = tagsList.Remove(tagsList.Length - 2);
                }

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
            ViewBag.Tags = new MultiSelectList(genKolEnt.TAGS, "Id", "Name", null);
            return View();
        }

        [HttpPost]
        public ActionResult Create(TaskModel model, HttpPostedFileBase file)
        {
            try
            {
                ViewBag.Tags = new MultiSelectList(genKolEnt.TAGS, "Id", "Name", null);

                string taskContent = TakeLatexCode(file);

                if (ModelState.IsValid)
                {
                    if(genKolEnt.TASKS.Any(t => t.Name.Equals(model.Name))){
                        ViewBag.Message = "Zadanie o takiej nazwie już istnieje w bazie.";
                        return View(model);
                    }
                    
                    TASKS task = new TASKS()
                    {
                        Name = model.Name,
                        Content = taskContent // to trzeba zmienić
                    };

                    foreach (var idTag in model.TagIdList)
                    {
                        var t = genKolEnt.TAGS.Find(idTag);
                        task.TAGS.Add(t);
                    }

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

        private string TakeLatexCode(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                string fileName = Path.GetFileName(file.FileName);
                if (!Directory.Exists(Server.MapPath("~/Files")))
                {
                    Directory.CreateDirectory(Server.MapPath("~/Files"));
                }
                string path = Path.Combine(Server.MapPath("~/Files"), fileName);
                file.SaveAs(path);


                StreamReader reader = new StreamReader(path, Encoding.Default, true);
                String line = null;
                String ln = System.Environment.NewLine;
                StringBuilder stringBuilder = new StringBuilder();

                while ((line = reader.ReadLine()) != null)
                {
                    stringBuilder.Append(line);
                    stringBuilder.Append(ln);
                }

                String template = stringBuilder.ToString();
                reader.Close();

                return template;
            }
            else
                return null;
        }

        public ActionResult Edit(int id = 0)
        {
            var t = genKolEnt.TASKS.Find(id);

            List<int> listToWrite = new List<int>();

            foreach (var tag in t.TAGS.ToList())
            {
                var index = tag.Id;
                listToWrite.Add(index);
            }

            var task = new TaskModel
            {
                Name = t.Name,
                Id = t.Id,
                TagIdList = listToWrite
            };

            ViewBag.Tags = new MultiSelectList(genKolEnt.TAGS, "Id", "Name", null);
            ModelState.Clear();

            return View(task);
        }

        [HttpPost]
        public ActionResult Edit(int id, TaskModel model)
        {
            try
            {
                ViewBag.Tags = new MultiSelectList(genKolEnt.TAGS, "Id", "Name", null);

                if (ModelState.IsValid)
                {
                    if (genKolEnt.TASKS.Any(t => t.Name.Equals(model.Name) && t.Id != model.Id))
                    {
                        ViewBag.Message = "Zadanie o takiej nazwie już istnieje w bazie.";
                        return View(model);
                    }

                    var task = genKolEnt.TASKS.Find(model.Id);

                    task.Name = model.Name;

                    
                    foreach (var tag in task.TAGS.ToList())
                    {
                        task.TAGS.Remove(tag);
                    }

                    foreach (var idTag in model.TagIdList)
                    {
                        var t = genKolEnt.TAGS.Find(idTag);
                        task.TAGS.Add(t);
                    }

                    genKolEnt.SaveChanges();

                    return RedirectToAction("Index");
                }
                else
                    return View(model);
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message;
                return View(model);
            }
        }


        [HttpPost]
        public ActionResult Delete(int id = 0)
        {
            try
            {
                TASKS task = genKolEnt.TASKS.Find(id);
                foreach (var tag in task.TAGS.ToList())
                {
                    task.TAGS.Remove(tag);
                }

                genKolEnt.TASKS.Remove(task);
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
