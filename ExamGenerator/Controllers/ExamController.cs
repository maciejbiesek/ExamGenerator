using ExamGenerator.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            List<SelectListItem> ListaInt = new List<SelectListItem>();
            for (int i = 1; i <= 20; i++)
            {
                ListaInt.Add(new SelectListItem()
                {
                    Value = i.ToString(),
                    Text = i.ToString()
                });
            }

            ViewBag.ListaInt = ListaInt;
            ViewBag.Tags = new MultiSelectList(genKolEnt.TAGS, "Id", "Name");

            return View();
        }

        [HttpPost]
        public ActionResult Generate(ExaminationModel model)
        {
            try
            {
                List<SelectListItem> ListaInt = new List<SelectListItem>();
                for (int i = 1; i <= 20; i++)
                {
                    ListaInt.Add(new SelectListItem()
                    {
                        Value = i.ToString(),
                        Text = i.ToString()
                    });
                }

                ViewBag.ListaInt = ListaInt;
                ViewBag.Tags = new MultiSelectList(genKolEnt.TAGS, "Id", "Name");

                if (ModelState.IsValid)
                {
                    if (genKolEnt.TASKS.Any(t => t.Name.Equals(model.Name) && t.Id != model.Id))
                    {
                        ViewBag.Message = "Egzamin o takiej nazwie już istnieje w bazie.";
                        return View(model);
                    }
                    var taskList = genKolEnt.TASKS.Select(t => t).ToList();
                    String strOutput = ReadTemplate(); // final string with test
                    String questions = "";
                    List<String> qList = new List<string>();

                    /////////////////////////////////////////////

                    //liczba wszystkich zad w bazie danych
                    var tasksList = genKolEnt.TASKS.Select(t => t).ToList();
                    int numberOfTasks = tasksList.Count;
                    int numberOfQuestions = model.NumberOfQuestions; // zmienna                   
                    int liczbaGrup = model.NumberOfGroups;
                                        
                    
                    Wierzcholek[] tabX = new Wierzcholek[numberOfQuestions];
                    Wierzcholek[] tabY = new Wierzcholek[numberOfTasks];

                    var used_tasks = model.TagIdList;
                    for (int i = 0; i < numberOfQuestions; i++)
                    {
                        var TAGI = used_tasks[i].IdList;

                        //szukam zadania, które zawierają taki zestaw tagów
                        //var ZADANIA = genKolEnt.TASKS.AsEnumerable().Where(t => ScrambledEquals<int>(t.TAGS.Select(tag=>tag.Id).ToList(), TAGI)).ToList();

                        List<List<int>> whole_list = new List<List<int>>();

                        foreach (var t in TAGI)
                        {
                            var temp = genKolEnt.TAGS.Find(t).TASKS.Select(tas => tas.Id).ToList();
                            whole_list.Add(temp);
                        }
                        List<int> coll = whole_list[0]; //coll to są ZADANIA
                        whole_list.Remove(coll);

                        foreach (List<int> k in whole_list)
                        {
                            var temp = coll.Intersect(k);
                            coll = temp.ToList();
                        }

                        //liczba sasiadow wierzcholka
                        int z = coll.Count;
                        int[] tab = new int[z];

                        //pobieram id tych ZADAŃ
                        for (int j = 0; j < z; j++)
                            tab[j] = coll[j];

                        Wierzcholek wierzcholek = new Wierzcholek(true, i, tab);
                        tabX[i] = wierzcholek;
                    }
                                       

                    for (int i = 0; i < numberOfTasks; i++)  // dla każego zadania szukamy slotu, do którego pasuje
                    {
                        //var lista = genKolEnt.TASKS.Select(t => t.Id).ToList(); // robimy listę sąsiadów dla i-tego wierzchołka - zadania z bazy
                        ArrayList lista = new ArrayList();
                        for (int j = 0; j < numberOfQuestions; j++) // przeglądamy wszystkie wierzchołki - sloty
                        {
                            int l = tabX[j].getListaSasiadow().Length, k=0;
                            while (k < l) // jeżeli dany slot ma na liście sąsiadów i-ty wierzchołek - zadanie z bazy i, to wrzucamy slot na listę sąsiadów wierzchołka i
                            {
                                if (tabX[j].getListaSasiadow()[k] == i)
                                {
                                    lista.Add(j);
                                }
                                k++;
                            }
                            
                        }
                        
                        int[] tab = new int[lista.Count]; // musimy zmienić listę w tablicę, bo taki jest parametr w konstruktorze wierzchołka
                        lista.CopyTo(tab);
                        Wierzcholek wierzcholek = new Wierzcholek(false, i, tab, tasksList[i].Id);
                        tabY[i] = wierzcholek;
                    }

                    Graf graf = new Graf(tabX, tabY); // wow, mamy graf

                    List<Wierzcholek> listaDoWyrzucenia = new List<Wierzcholek>();
                    foreach (Wierzcholek v in tabY)
                    {
                        if (v.getListaSasiadow().Length == 0)
                            listaDoWyrzucenia.Add(v);
                    }
                    for (int j = 0; j < listaDoWyrzucenia.Count; j++)
                    {
                        graf = usunWierzcholek(graf, listaDoWyrzucenia[j]);
                        for (int k = j + 1; k < listaDoWyrzucenia.Count; k++)
                        {
                            Wierzcholek wierzcholek = new Wierzcholek(listaDoWyrzucenia[k].getDwupodzial(), listaDoWyrzucenia[k].getNumer() - 1, listaDoWyrzucenia[k].getListaSasiadow());
                            listaDoWyrzucenia[k] = wierzcholek;
                        }
                    }
                    tabX = graf.getListaWierzcholkowX();
                    tabY = graf.getListaWierzcholkowY();

                    // zrobimy sobie listę zbiorów
                    List<HashSet<Wierzcholek>> listaSkojarzen = new List<HashSet<Wierzcholek>>();
                    List<Wierzcholek[]> podzbiory = stworzPodzbiory(tabY, tabX.Length); // tworzymy wszystkie podzbiory k-elementowe zadań z bazy...
                    // ... jeżeli warunek Halla będzie dla nich spełniony, to znaczy, że istnieje skojarzenie ze slotami, czyli można utworzyć zestaw na kolokwium z tych zadań
                    foreach (Wierzcholek[] podzbior in podzbiory)
                    {
                        Wierzcholek[] doWyrzucenia = new Wierzcholek[tabY.Length - tabX.Length]; // jak bierzemy jakiś podzbiór zadań z bazy, to pozostałe zadania trzeba na chwilę wywalić, żeby sprawdzić, czy istnieje skojarzenie
                        int i = 0;
                        foreach (Wierzcholek v in tabY)
                        {
                            if (podzbior.Contains(v)) // jak zadanie jest w naszym podzbiorze, to nic nie robimy
                                continue;
                            else
                            {
                                doWyrzucenia[i] = v; // jak nie ma, to je na chwilę wywalamy z grafu
                                i++;
                            }
                        }

                        Graf nowyGraf = graf;
                        // właśnie tu wywalamy wszystkie zadania, których nie chcemy
                        for (int j = 0; j < doWyrzucenia.Length; j++)
                        {
                            nowyGraf = usunWierzcholek(nowyGraf, doWyrzucenia[j]);
                            for (int k = j + 1; k < doWyrzucenia.Length; k++)
                            {
                                Wierzcholek wierzcholek = new Wierzcholek(doWyrzucenia[k].getDwupodzial(), doWyrzucenia[k].getNumer() - 1, doWyrzucenia[k].getListaSasiadow());
                                doWyrzucenia[k] = wierzcholek;
                            }
                        }

                        if (sprawdzHalla(nowyGraf) == true) // dla otrzymanego grafu sprawdzamy, czy istnieje skojarzenie
                        {
                            HashSet<Wierzcholek> zestaw = new HashSet<Wierzcholek>(); // jeżeli tak, to robimy zestaw zadań z naszego podzbioru...
                            foreach (Wierzcholek v in podzbior)
                                zestaw.Add(v);
                            listaSkojarzen.Add(zestaw); // ... i wrzucamy go na listę
                        }
                    }



                    if (listaSkojarzen.Count() == 0) {// jeżeli lista jest pusta, to słabo, bo nie będzie żadnego zestawu...
                        ViewBag.Message = "Nie mozna utworzyc zestawu (nie ma odpowiedniego skojarzenia w grafie).";
                        return View(model);
                    }
                    else
                    {
                        //Console.WriteLine("Dostepne zestawy:"); // ... a jeżeli jest niepusta, to sobie wypiszemy, co nam wyszło
                        List<List<HashSet<Wierzcholek>>> listaWyjsc = new List<List<HashSet<Wierzcholek>>>(); // tutaj będziemy trzymać wszystkie możliwe n-elementowe zestawy kolokwiów, gdzie n jest podaną wcześniej liczbą grup
                        for (int i = 0; i < listaSkojarzen.Count; i++) // dla każdego skojarzenia będziemy dobierać skojarzenia z nim rozłączne...
                        {
                            List<HashSet<Wierzcholek>> listaRozlacznych = new List<HashSet<Wierzcholek>>(); // ... i trzymać je na tej liście
                            HashSet<Wierzcholek> zestaw = new HashSet<Wierzcholek>(listaSkojarzen.ElementAt(i)); // do tego skojarzenia będziemy dobierać pozostałe...
                            listaRozlacznych.Add(listaSkojarzen.ElementAt(i)); // ... więc będzie to oczywiście pierwszy element listy
                            foreach (HashSet<Wierzcholek> innyZestaw in listaSkojarzen) // teraz będziemy sprawdzać każde skojarzenie, czy jest rozłączne z danym
                            {
                                if (zestaw.Intersect(innyZestaw).Count() == 0) // to znaczy, że część wspólna skojarzeń danego i aktualnie rozpatywanego ma 0 elementów
                                {
                                    zestaw.UnionWith(innyZestaw); // łączymy nasze dwa zestawy, żeby zapewnić rozłączność w kolejnym przebiegu pętli
                                    listaRozlacznych.Add(innyZestaw); // wrzucamy rozpatrywany w tym przebiegu zestaw na listę
                                }
                            }
                            if (listaRozlacznych.Count >= liczbaGrup) // jeżeli na liście jest tyle zestawów, ile chcieliśmy (lub więcej)...
                                listaWyjsc.Add(listaRozlacznych); // to wrzucamy ją na listę "zestawów zestawów" :) żeby uniknąć problemów z nomenkulaturą, będziemy je nazywać wyjściami
                        }
                        Random rnd = new Random();
                        int r = rnd.Next(listaWyjsc.Count); // z listy wyjść losujemy sobie jedno
                        List<HashSet<Wierzcholek>> wyjscie = new List<HashSet<Wierzcholek>>(); // wrzucimy je tutaj, ale to za chwilę
                        if (listaWyjsc.ElementAt(r).Count != liczbaGrup) // jeżeli w wyjściu jest więcej zestawów, niż chcieliśmy, to losujemy z tego zestawu tyle grup, ile miało być
                        {
                            int s = rnd.Next(listaWyjsc.ElementAt(r).Count);
                            while (wyjscie.Count != liczbaGrup) // losujemy tak długo, aż będziemy mieli żądaną liczbę zestawów
                            {
                                if (wyjscie.Contains(listaWyjsc.ElementAt(r).ElementAt(s))) // oczywiście jeżeli wylosujemy kolejny raz to samo, to olewamy to i losujemy dalej...
                                {
                                    s = rnd.Next(listaWyjsc.ElementAt(r).Count);
                                    continue;
                                }
                                else wyjscie.Add(listaWyjsc.ElementAt(r).ElementAt(s)); // w przeciwnym wypadku dodajemy wylosowany zestaw do naszego wyjścia
                            }
                        }
                        else
                        {
                            foreach (HashSet<Wierzcholek> h in listaWyjsc.ElementAt(r)) // jeżeli wylosowany element listy miał tyle zestawów co trzeba, to po prostu wrzucamy je po kolei do naszego wyjścia
                            {
                                wyjscie.Add(h);
                            }
                        }
                        if (wyjscie.Count > 0) // jak da się ułożyć daną liczby zestawów
                        {
                            foreach (HashSet<Wierzcholek> h in wyjscie)
                            {   //lista egzaminow
                                foreach (Wierzcholek v in h)
                                {   //lista zadan
                                    var task = genKolEnt.TASKS.Find(v.getId());
                                    questions += task.Content + System.Environment.NewLine;
                                    //Console.Write(v.getId() + " ");                                    
                                }

                                qList.Add(questions);
                                questions = "";
                                //Console.WriteLine("");                                
                            }
                            /*
                            Console.WriteLine("Czy chcesz ulozyc zestawy na kolejny rok? (T/N)");
                            char odpowiedz = Char.Parse(Console.ReadLine());
                            if (odpowiedz == 116 || odpowiedz == 84)
                            {
                                Console.WriteLine("Ile zestawow ulozyc?");
                                int liczbaGrupDodatkowych = Int32.Parse(Console.ReadLine());
                                Console.WriteLine("Ile zadan moze sie powtarzac?");
                                int iloczyn = Int32.Parse(Console.ReadLine());
                                List<List<HashSet<Wierzcholek>>> listaWyjscDodatkowych = new List<List<HashSet<Wierzcholek>>>(); // na tej liście będziemy trzymać "zestawy zestawów", czyli wyjścia, na kolejny rok, coś może się w nich powtarzać (tylko w stosunku do roku bieżącego!)
                                for (int i = 0; i < listaSkojarzen.Count; i++) // robimy to samo, co wcześniej...
                                {
                                    List<HashSet<Wierzcholek>> listaRozlacznych = new List<HashSet<Wierzcholek>>();
                                    HashSet<Wierzcholek> zestaw = new HashSet<Wierzcholek>(listaSkojarzen.ElementAt(i));
                                    bool powtorkaZestaw = true; // ... aż dotąd; musimy sprawdzić, czy w zestawie, od którego zaczynamy, nie powtarza się zbyt dużo zadań...
                                    foreach (HashSet<Wierzcholek> h in wyjscie)
                                    {
                                        if (h.Intersect(zestaw).Count() > iloczyn)
                                        {
                                            powtorkaZestaw = false;
                                            break;
                                        }

                                    }
                                    if (powtorkaZestaw == false)
                                        break; // ... jeżeli tak jest, to przechodzimy do następnego przebiegu pętli - bierzemy inny zestaw; w przeciwnym przypadku jedziemy dalej
                                    // w kolejnych krokach robimy dokładnie to samo, co dla kolokwiów z bieżącego roku, dodatkowo uwzględniając, ile zadań może się powtarzać z tym wyjściem, które otrzymaliśmy wcześniej
                                    listaRozlacznych.Add(listaSkojarzen.ElementAt(i));
                                    foreach (HashSet<Wierzcholek> innyZestaw in listaSkojarzen)
                                    {
                                        if (zestaw.Intersect(innyZestaw).Count() == 0)
                                        {
                                            bool powtorka = true;
                                            foreach (HashSet<Wierzcholek> h in wyjscie)
                                            {
                                                if (h.Intersect(innyZestaw).Count() > iloczyn)
                                                {
                                                    powtorka = false;
                                                    break;
                                                }

                                            }
                                            if (powtorka == true)
                                            {
                                                zestaw.UnionWith(innyZestaw);
                                                listaRozlacznych.Add(innyZestaw);
                                            }
                                        }
                                    }
                                    if (listaRozlacznych.Count >= liczbaGrupDodatkowych)
                                        listaWyjscDodatkowych.Add(listaRozlacznych);
                                }
                                if (listaWyjscDodatkowych.Count > 0)
                                {
                                    r = rnd.Next(listaWyjscDodatkowych.Count);
                                    List<HashSet<Wierzcholek>> wyjscieDodatkowe = new List<HashSet<Wierzcholek>>();
                                    if (listaWyjscDodatkowych.ElementAt(r).Count != liczbaGrupDodatkowych)
                                    {
                                        int s = rnd.Next(listaWyjscDodatkowych.ElementAt(r).Count);
                                        while (wyjscieDodatkowe.Count != liczbaGrupDodatkowych)
                                        {
                                            if (wyjscieDodatkowe.Contains(listaWyjscDodatkowych.ElementAt(r).ElementAt(s)))
                                            {
                                                s = rnd.Next(listaWyjscDodatkowych.ElementAt(r).Count);
                                                continue;
                                            }
                                            else wyjscieDodatkowe.Add(listaWyjscDodatkowych.ElementAt(r).ElementAt(s));
                                        }
                                    }
                                    else
                                    {
                                        foreach (HashSet<Wierzcholek> h in listaWyjscDodatkowych.ElementAt(r))
                                        {
                                            wyjscieDodatkowe.Add(h);
                                        }
                                    }
                                    foreach (HashSet<Wierzcholek> h in wyjscieDodatkowe)
                                    {
                                        foreach (Wierzcholek v in h)
                                        {
                                            Console.Write(v.getId() + " ");
                                        }
                                        Console.WriteLine("");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Nie mozna ulozyc tylu zestawow");

                                }
                            } */
                        }
                        else
                        {
                            ViewBag.Message = "Nie można ułożyć tylu zestawów.";
                        }
                    }




                    ////////////////////////////////////////////////////////////////////////

                    /*for (int i = numberOfQuestions; i > 0; i--)
                    {
                        //var taskList = genKolEnt.TASKS.Select
                        Random randomGenerator = new Random();
                        int index = randomGenerator.Next(0, taskList.Count);
                        questions += taskList[index].Content + System.Environment.NewLine;
                        taskList.RemoveAt(index);
                    }*/

                    int version = 1;                    

                    foreach (var question in qList)
                    {
                        var EGZAMIN = strOutput;
                        EGZAMIN = EGZAMIN.Replace("[ZADANIA]", question);
                        EGZAMIN = EGZAMIN.Replace("[NAZWA]", model.Subject);
                        EGZAMIN = EGZAMIN.Replace("[GRUPA]", version.ToString());

                        EXAMS exam = new EXAMS();

                        exam.Name = model.Name;
                        exam.Content = EGZAMIN;
                        exam.Subject = model.Subject;
                        exam.Version = version;
                        exam.ExamDate = DateTime.Now;

                        //trzeba powiazac egzamin z tagami, które zawiera

                        genKolEnt.EXAMS.Add(exam);

                        version++;
                    }

                    ViewBag.Message = String.Format("Wygenerowano {0} egzamin(ów).", qList.Count);

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


        [HttpPost]
        public ActionResult Delete(int id = 0)
        {
            try
            {
                EXAMS exam = genKolEnt.EXAMS.Find(id);
                foreach (var task in exam.TAGS.ToList())
                {
                    exam.TAGS.Remove(task);
                }

                genKolEnt.EXAMS.Remove(exam);
                genKolEnt.SaveChanges();

                return Content(Boolean.TrueString);
            }
            catch
            {
                return Content(Boolean.FalseString);
            }
        }





        //sprawdzenie, czy dwa dane wierzchołki są połączone krawędzią; możemy zakładać, że należą one do różnych zbiorów dwupodziału
        public static bool jestSasiadem(Wierzcholek x, Wierzcholek y)
        {
            int numer = x.getNumer();
            foreach (int i in y.getListaSasiadow()) //przegląda listę sąsiadów wierzchołka y; jeżeli numer wierzchołka x jest na liście, zwraca true
            {
                if (i == numer)
                    return true;
                else continue;
            }
            return false;
        }

        //dla grafu G i wierzchołka v zwraca G-v; uwaga: jeżeli usuwamy inny wierzchołek niż ostatni w tablicy, numeracja wierzchołków "przesuwa się" w lewo
        public static Graf usunWierzcholek(Graf graf, Wierzcholek wierzcholek)
        {
            Wierzcholek[] tabX;
            Wierzcholek[] tabY;
            if (wierzcholek.getDwupodzial() == true) // blok operacji, który wywołujemy, jeżeli wierzchołek jest slotem
            {
                tabX = new Wierzcholek[graf.getListaWierzcholkowX().Length - 1]; // zmniejszamy tablicę o 1 element, bo wyrzucamy 1 wierzchołek
                tabY = new Wierzcholek[graf.getListaWierzcholkowY().Length];
                int numer = wierzcholek.getNumer();
                foreach (Wierzcholek i in graf.getListaWierzcholkowY()) // przeglądamy wszystkie wierzchołki - zadania z bazy
                {
                    if (jestSasiadem(wierzcholek, i)) // jeżeli wierzchołek jest sąsiadem danego, to musimy wyrzucić go z listy sąsiadów i zmienić numerację
                    {
                        int[] tab = new int[i.getListaSasiadow().Length - 1]; // tworzymy nową, krótszą listę sąsiadów
                        for (int j = 0; j < i.getListaSasiadow().Length - 1; j++)
                        {
                            if (i.getListaSasiadow()[j] < numer) // na początku kopiujemy numery ze starej listy
                                tab[j] = i.getListaSasiadow()[j];
                            else
                                tab[j] = i.getListaSasiadow()[j + 1] - 1; // następnie "przesuwamy" numery w lewo i zmniejszamy o 1 ze względu na założenie naszej funkcji o numeracji
                        }
                        Wierzcholek v = new Wierzcholek(false, i.getNumer(), tab, i.getId()); // tworzymy wierzchołek z nową listą sąsiadów
                        tabY[i.getNumer()] = v; // podmieniamy go w tablicy
                    }
                    else // tutaj lista sąsiadów pozostaje tej samej długości, ale trzeba obniżyć numery tych wierzchołków, które się "przesunęły"
                    {
                        int[] tab = new int[i.getListaSasiadow().Length];
                        for (int j = 0; j < i.getListaSasiadow().Length; j++)
                        {
                            if (i.getListaSasiadow()[j] < numer)
                                tab[j] = i.getListaSasiadow()[j];
                            else
                                tab[j] = i.getListaSasiadow()[j] - 1;
                        }
                        Wierzcholek v = new Wierzcholek(false, i.getNumer(), tab, i.getId());
                        tabY[i.getNumer()] = v;
                    }
                }
                for (int i = 0; i < tabX.Length; i++) // przeglądamy wszystkie wierzchołki - sloty
                {
                    if (graf.getListaWierzcholkowX()[i].getNumer() < numer)
                        tabX[i] = graf.getListaWierzcholkowX()[i]; // do momentu napotkania wyrzucanego wierzchołka kopiujemy numery
                    else // następnie "przesuwamy" pozostawione wierzchołki w lewo
                    {
                        Wierzcholek v = new Wierzcholek(true, graf.getListaWierzcholkowX()[i].getNumer(), graf.getListaWierzcholkowX()[i + 1].getListaSasiadow());
                        tabX[i] = v;
                    }
                }
            }
            else // jeżeli wierzchołek jest zadaniem z bazy, to postępujemy analogicznie, ze zmienionym nazewnictwem tablic itp
            {
                tabX = new Wierzcholek[graf.getListaWierzcholkowX().Length];
                tabY = new Wierzcholek[graf.getListaWierzcholkowY().Length - 1];
                int numer = wierzcholek.getNumer();
                foreach (Wierzcholek i in graf.getListaWierzcholkowX())
                {
                    if (jestSasiadem(wierzcholek, i))
                    {
                        int[] tab = new int[i.getListaSasiadow().Length - 1];
                        for (int j = 0; j < i.getListaSasiadow().Length - 1; j++)
                        {
                            if (i.getListaSasiadow()[j] < numer)
                                tab[j] = i.getListaSasiadow()[j];
                            else
                                tab[j] = i.getListaSasiadow()[j + 1] - 1;
                        }
                        Wierzcholek v = new Wierzcholek(true, i.getNumer(), tab, i.getId());
                        tabX[i.getNumer()] = v;
                    }
                    else
                    {
                        int[] tab = new int[i.getListaSasiadow().Length];
                        for (int j = 0; j < i.getListaSasiadow().Length; j++)
                        {
                            if (i.getListaSasiadow()[j] < numer)
                                tab[j] = i.getListaSasiadow()[j];
                            else
                                tab[j] = i.getListaSasiadow()[j] - 1;
                        }
                        Wierzcholek v = new Wierzcholek(true, i.getNumer(), tab, i.getId());
                        tabX[i.getNumer()] = v;
                    }
                }
                for (int i = 0; i < tabY.Length; i++)
                {
                    if (graf.getListaWierzcholkowY()[i].getNumer() < numer)
                        tabY[i] = graf.getListaWierzcholkowY()[i];
                    else
                    {
                        Wierzcholek v = new Wierzcholek(false, graf.getListaWierzcholkowY()[i].getNumer(), graf.getListaWierzcholkowY()[i + 1].getListaSasiadow(), graf.getListaWierzcholkowY()[i + 1].getId());
                        tabY[i] = v;
                    }
                }
            }
            Graf nowyGraf = new Graf(tabX, tabY);
            return nowyGraf;
        }

        // funkcja pomocnicza do testowania, wypisuje numery wierzchołków wraz z listami sąsiadów
        public static void wypiszGraf(Graf graf)
        {
            for (int i = 0; i < graf.getListaWierzcholkowX().Length; i++)
            {
                Console.Write("X" + graf.getListaWierzcholkowX()[i].getNumer() + ": ");
                for (int j = 0; j < graf.getListaWierzcholkowX()[i].getListaSasiadow().Length; j++)
                    Console.Write("Y" + graf.getListaWierzcholkowX()[i].getListaSasiadow()[j] + " ");
                Console.WriteLine("");
            }

            for (int i = 0; i < graf.getListaWierzcholkowY().Length; i++)
            {
                Console.Write("Y" + graf.getListaWierzcholkowY()[i].getNumer() + ": ");
                for (int j = 0; j < graf.getListaWierzcholkowY()[i].getListaSasiadow().Length; j++)
                    Console.Write("X" + graf.getListaWierzcholkowY()[i].getListaSasiadow()[j] + " ");
                Console.WriteLine("");
            }
        }

        // sprawdzenie, czy w danym grafie liczba wierzchołków - zadań z bazy połączonych z jakimkolwiek wierzchołkiem - slotem jest większa lub równa, niż liczba wierzchołków - slotów (potrzebne do sprawdzania warunku Halla)
        public static bool sprawdzSkojarzenie(Graf graf)
        {
            bool[] tablicaSasiadow = new bool[graf.getListaWierzcholkowY().Length]; // tworzymy tablicę logiczną, indeksujemy numerami zadań z bazy
            for (int i = 0; i < tablicaSasiadow.Length; i++)
                tablicaSasiadow[i] = false;
            for (int i = 0; i < graf.getListaWierzcholkowX().Length; i++) // przeglądamy wszystkie wierzchołki - sloty
            {
                for (int j = 0; j < graf.getListaWierzcholkowX()[i].getListaSasiadow().Length; j++)
                {
                    tablicaSasiadow[graf.getListaWierzcholkowX()[i].getListaSasiadow()[j]] = true; // jeżeli slot jest połączony z k-tym zadaniem z bazy, to w tablicy logicznej dajemy na k-tym miejscu "true"
                }
            }
            int licznik = 0;
            for (int i = 0; i < tablicaSasiadow.Length; i++) // zliczamy "true" w tablicy logicznej
            {
                if (tablicaSasiadow[i] == true)
                    licznik++;
            }
            if (licznik >= graf.getListaWierzcholkowX().Length) // liczba "true" w tablicy logicznej jest liczbą sąsiadów wierzchołków - slotów
                return true;
            else return false;
        }

        // generuje listę wszystkich podzbiorów dla danej tablicy obiektów; nie wiem jak działa, znalezione w internetach
        public static List<T[]> stworzPodzbiory<T>(T[] tab)
        {
            List<T[]> podzbiory = new List<T[]>();

            for (int i = 0; i < tab.Length; i++)
            {
                int licznik = podzbiory.Count;
                podzbiory.Add(new T[] { tab[i] });

                for (int j = 0; j < licznik; j++)
                {
                    T[] nowyPodzbior = new T[podzbiory[j].Length + 1];
                    podzbiory[j].CopyTo(nowyPodzbior, 0);
                    nowyPodzbior[nowyPodzbior.Length - 1] = tab[i];
                    podzbiory.Add(nowyPodzbior);
                }
            }

            return podzbiory;
        }

        // generuje listę wszystkich podzbiorów k-elementowych dla danej tablicy obiektów; nie wiem jak działa, modyfikacja znalezionego w internetach
        public static List<T[]> stworzPodzbiory<T>(T[] tab, int k)
        {
            List<T[]> podzbiory = new List<T[]>();

            for (int i = 0; i < tab.Length; i++)
            {
                int licznik = podzbiory.Count;
                podzbiory.Add(new T[] { tab[i] });

                for (int j = 0; j < licznik; j++)
                {
                    T[] nowyPodzbior = new T[podzbiory[j].Length + 1];
                    podzbiory[j].CopyTo(nowyPodzbior, 0);
                    nowyPodzbior[nowyPodzbior.Length - 1] = tab[i];
                    podzbiory.Add(nowyPodzbior);
                }
            }

            List<T[]> podzbioryK = new List<T[]>();
            foreach (T[] tablica in podzbiory)
            {
                if (tablica.Length == k)
                    podzbioryK.Add(tablica);
            }

            return podzbioryK;
        }

        // sprawdza, czy dla danego grafu zachodzi warunek Halla
        public static bool sprawdzHalla(Graf graf)
        {
            List<Wierzcholek[]> podzbiory = stworzPodzbiory(graf.getListaWierzcholkowX()); // generujemy wszystkie podzbiory ze zbioru slotów
            bool flaga = true; // jeżeli dla jakiegoś podzbioru lista jego sąsiadów będzie mniejsza, to zmienimy na false i koniec
            foreach (Wierzcholek[] podzbior in podzbiory) // bierzemy po kolei każdy podzbiór
            {
                Graf grafPomocniczny = graf;
                for (int i = 0; i < podzbior.Length; i++) // sprawdzać warunek będziemy przez dopełnienie...
                {
                    grafPomocniczny = usunWierzcholek(grafPomocniczny, podzbior[i]); // ... więc wywalamy z grafu wszystkie wierzchołki danego podzbioru...
                    for (int j = i + 1; j < podzbior.Length; j++) // ... i obniżamy numerację kolejnych, żeby wszystko się zgadzało
                    {
                        Wierzcholek wierzcholek = new Wierzcholek(podzbior[j].getDwupodzial(), podzbior[j].getNumer() - 1, podzbior[j].getListaSasiadow());
                        podzbior[j] = wierzcholek;
                    }
                }
                if (sprawdzSkojarzenie(grafPomocniczny) == false) // sprawdzamy, czy lista sąsiadów otrzymanego grafu jest mniejsza
                {
                    flaga = false; // jakiś podzbiór nie spełnia warunku o sąsiadach...
                    break; // ... więc skojarzenia nie będzie - koniec
                }

            }
            return flaga; // jeżeli nie zmieniliśmy flagi na false, to dla każdego podzbioru jest ok, czyli warunek Halla spełniony

        }
    }
}

public class Graf
{
    protected Wierzcholek[] listaWierzcholkowX; //zbior wierzcholkow podzbioru X (sloty), maja one wartosc logiczna true
    protected Wierzcholek[] listaWierzcholkowY; //zbior wierzcholkow podzbioru Y (baza), maja one wartosc logiczna false
    protected int liczbaWierzcholkow; //liczba wierzcholkow w grafie

    public void setListaWierzcholkowX(Wierzcholek[] listaWierzcholkowX)
    {
        this.listaWierzcholkowX = listaWierzcholkowX;
    }
    public void setListaWierzcholkowY(Wierzcholek[] listaWierzcholkowY)
    {
        this.listaWierzcholkowY = listaWierzcholkowY;
    }
    public Wierzcholek[] getListaWierzcholkowX()
    {
        return listaWierzcholkowX;
    }
    public Wierzcholek[] getListaWierzcholkowY()
    {
        return listaWierzcholkowY;
    }
    public int getLiczbaWierzcholkow()
    {
        return liczbaWierzcholkow;
    }
    public Graf(Wierzcholek[] listaWierzcholkowX, Wierzcholek[] listaWierzcholkowY)
    {
        setListaWierzcholkowX(listaWierzcholkowX);
        setListaWierzcholkowY(listaWierzcholkowY);
        liczbaWierzcholkow = listaWierzcholkowX.Length + listaWierzcholkowY.Length;
    }
}

public class Wierzcholek
{
    protected bool dwupodzial; //true dla X (sloty), false dla Y (baza)
    protected int numer; //numer wierzcholka w podzbiorze dwupodzialu
    protected int[] listaSasiadow; //numery przyleglych wierzcholkow z podzbioru o przeciwnej wartosci logicznej
    protected int id; // numer zadania - tylko jeśli wierzchołkiem jest zadanie z bazy

    public void setDwupodzial(bool dwupodzial)
    {
        this.dwupodzial = dwupodzial;
    }
    public void setNumer(int numer)
    {
        this.numer = numer;
    }
    public void setListaSasiadow(int[] listaSasiadow)
    {
        this.listaSasiadow = listaSasiadow;
    }
    public void setId(int id)
    {
        this.id = id;
    }
    public bool getDwupodzial()
    {
        return dwupodzial;
    }
    public int getNumer()
    {
        return numer;
    }
    public int[] getListaSasiadow()
    {
        return listaSasiadow;
    }
    public int getId()
    {
        return id;
    }
    public Wierzcholek(bool dwupodzial, int numer, int[] listaSasiadow)
    {
        setDwupodzial(dwupodzial);
        setNumer(numer);
        setListaSasiadow(listaSasiadow);
    }
    public Wierzcholek(bool dwupodzial, int numer, int[] listaSasiadow, int id)
    {
        setDwupodzial(dwupodzial);
        setNumer(numer);
        setListaSasiadow(listaSasiadow);
        setId(id);
    }
    
}





