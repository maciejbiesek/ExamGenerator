﻿using ExamGenerator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

        public ActionResult Generate()
        {
            ViewBag.Tags = new MultiSelectList(genKolEnt.TAGS, "Id", "Name");

            List<SelectListItem> listaInt = new List<SelectListItem>();
            for (int i = 1; i <= 20; i++)
            {
                listaInt.Add(new SelectListItem()
                {
                    Value = i.ToString(),
                    Text = i.ToString()
                });
            }

            ViewBag.ListaInt = listaInt;

            return View();
        }

        [HttpPost]
        public ActionResult Generate(ExaminationModel model)
        {
            try
            {
                ViewBag.Tags = new MultiSelectList(genKolEnt.TAGS, "Id", "Name");

                if (ModelState.IsValid)
                {
                    if (genKolEnt.TASKS.Any(t => t.Name.Equals(model.Name) && t.Id != model.Id))
                    {
                        ViewBag.Message = "Egzamin o takiej nazwie już istnieje w bazie.";
                        return View(model);
                    }

                    String strOutput = ReadTemplate(); // final string with test
                    String questions = "";
                    int numberOfQuestions = model.NumberOfQuestions; // zmienna
                                        
                    var taskList = genKolEnt.TASKS.Select(t => t).ToList();

                    var used_tasks = model.TagIdList;
                    
                    for (int i = numberOfQuestions; i > 0; i--)
                    {
                        //var taskList = genKolEnt.TASKS.Select
                        Random randomGenerator = new Random();
                        int index = randomGenerator.Next(0, taskList.Count);
                        questions = questions + taskList[index].Content + System.Environment.NewLine;
                        taskList.RemoveAt(index);
                    }

                    strOutput = strOutput.Replace("[ZADANIA]", questions);
                    strOutput = strOutput.Replace("[NAZWA]", model.Subject);
                    strOutput = strOutput.Replace("[GRUPA]", model.Version.ToString());

                    //zapisuje na razie tylko jeden egzamin

                    EXAMS exam = new EXAMS();

                    exam.Name = model.Name;
                    exam.Content = strOutput;
                    exam.Subject = model.Subject;

                    genKolEnt.EXAMS.Add(exam);
                    genKolEnt.SaveChanges();


                    //WriteToFile(strOutput);// make file -> kiedy kliknie sobie ściągnij
                    
                    // show massage
                    //Console.WriteLine("Wygenerowano pomyślnie.");
                    //Console.ReadLine();                    

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

        public ActionResult WriteToFileTex(int id)
        {
            var strOutput = genKolEnt.EXAMS.Find(id).Content;
            string fileName = "egzamin1.tex";
            string path = Path.Combine(Server.MapPath("~/Files"), fileName);
            StringBuilder temp = new StringBuilder();
            temp.Append(DateTime.Now.ToString()).Append("exam.tex"); // numer grupy/ wersji, chuj tam wie
            String fileNameOut = temp.ToString();

            // tworzenie pliku wynikowego .tex i zapisanie od niego stringa

            System.IO.File.WriteAllText(path, strOutput);
            
            /*StreamWriter writer = new StreamWriter(path);
            writer.WriteLine(strOutput);
            writer.Close();
            */

            return base.File(path, "text/plain", fileNameOut);

        }

        public ActionResult WriteToFilePdf(int id)
        {
            var strOutput = genKolEnt.EXAMS.Find(id).Content;
            string fileNameTex = "egzamin1.tex";
            string path = Path.Combine(Server.MapPath("~/Files"), fileNameTex);   

            // tworzenie pliku wynikowego .tex i zapisanie od niego stringa

            System.IO.File.WriteAllText(path, strOutput);

            ConvertTexToPDF(path);

            string fileNamePdf = "egzamin1.pdf";
            string pathPdf = Path.Combine(Server.MapPath("~/Files"), fileNamePdf);
            StringBuilder temp = new StringBuilder();
            temp.Append(DateTime.Now.ToString()).Append("exam.pdf"); // numer grupy/ wersji, chuj tam wie
            String fileNameOut = temp.ToString();

            return base.File(pathPdf, "application/pdf", fileNameOut);

        }

        private void ConvertTexToPDF(string path)
        {
            String dir = Server.MapPath("~/Files/");
            //convert tex to pdf
           
            ProcessStartInfo procStartInfo = new ProcessStartInfo("pdflatex", path);
            procStartInfo.CreateNoWindow = true;
            procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            procStartInfo.UseShellExecute = false;
            procStartInfo.WorkingDirectory = dir;

            Process process = new Process();
            process.StartInfo = procStartInfo;
            process.Start();
        }

        private string ReadTemplate()
        {
            string fileName = "template.tex";
            string path = Path.Combine(Server.MapPath("~/Files"), fileName);


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

    }
}
