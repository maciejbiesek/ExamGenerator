using ExamGenerator.Models;
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
            ViewBag.Tags = new MultiSelectList(genKolEnt.TAGS, "Id", "Name", null);
            return View();
        }

        [HttpPost]
        public ActionResult Generate(ExaminationModel model)
        {
            try
            {
                ViewBag.Tags = new MultiSelectList(genKolEnt.TAGS, "Id", "Name", null);

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
                    
                    for (int i = numberOfQuestions; i > 0; i--)
                    {
                        //var taskList = genKolEnt.TASKS.Select
                        Random randomGenerator = new Random();
                        int index = randomGenerator.Next(0, taskList.Count);
                        questions = questions + taskList[index].Content + System.Environment.NewLine;
                        taskList.RemoveAt(index);
                    }

                    strOutput = strOutput.Replace("[ZADANIA]", questions);

                    //zapisuje na razie tylko jeden egzamin

                    EXAMS exam = new EXAMS();

                    exam.Name = model.Name;
                    exam.Content = strOutput;

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

        public ActionResult WriteToFile(int id)
        {
            var strOutput = genKolEnt.EXAMS.Find(id).Content;
            string fileName = "egzamin1.tex";
            string path = Path.Combine(Server.MapPath("~/Files"), fileName);

            // tworzenie pliku wynikowego .tex i zapisanie od niego stringa

            System.IO.File.WriteAllText(path, strOutput);
            
            /*StreamWriter writer = new StreamWriter(path);
            writer.WriteLine(strOutput);
            writer.Close();

            ConvertTexToPDF(path);
            */

            return base.File(path, "text/plain", fileName);

        }

        private void ConvertTexToPDF(string path)
        {
            //convert tex to pdf
            ProcessStartInfo procStartInfo = new ProcessStartInfo("pdflatex", path);
            procStartInfo.CreateNoWindow = true;
            procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process process = new Process();
            process.StartInfo = procStartInfo;
            process.Start();

            process.Kill();
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
