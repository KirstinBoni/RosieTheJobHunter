using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RosieTheJobHunter.Model;
using RosieTheJobHunter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RosieTheJobHunter.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            string resume = ParsePDF();
            Dictionary<string, string> qualifications = new Dictionary<string, string>();
            qualifications.Add("Role", "");
            qualifications.Add("Location", "");
            qualifications.Add("Fulltime", "");
            string jobListings = await GetJobs(qualifications);
            //string documents = constructText(jobListings, resume);
            //string response = await extractKeywords(documents);
            List<double> matchCount = CompareAllEntries(resume, jobListings);
            HomeIndexModel model = new HomeIndexModel()
            {

            };
            return View();
        }

        public async Task<string> GetJobs(Dictionary<string, string> qualifications)
        {
            using (HttpClient client = new HttpClient())
            {
                StringBuilder uri =  new StringBuilder("https://jobs.github.com/positions.json?");
                var parameters = new Dictionary<string, string> { { "description", "software"  },
                                                                        {"location", "" },
                                                                        {"full_time", "true"  },
                                                                      };

                KeyValuePair<string, string> last = parameters.Last();
                foreach (KeyValuePair<string, string> param in parameters)
                {
                    uri.Append(param.Key + "=" + param.Value);
                    if (!param.Equals(last))
                    {
                        uri.Append("&");
                    }
                }

                HttpResponseMessage response = await client.GetAsync(uri.ToString());
                
                string responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(responseContent);
                return responseContent;
            }
        }

        public string ParsePDF()
        {
            /*
         ITextExtractionStrategy its = new iTextSharp.text.pdf.parser.LocationTextExtractionStrategy();
         using (PdfReader reader = new PdfReader("~/Content/MESResume.pdf")) 
         {
             StringBuilder text = new StringBuilder();

             for (int i = 1; i <= reader.NumberOfPages; i++)
             {
                 string thePage = PdfTextExtractor.GetTextFromPage(reader, i, its);
                 string[] theLines = thePage.Split('\n');
                 foreach (var theLine in theLines)
                 {
                     text.AppendLine(theLine);
                 }
             }*/
            return "hello";//text.ToString();
            //}
        }

        public string constructText(string jobApplications, string resume)
        {
            string newText = Regex.Replace(resume, @"\t|\n|\r", "");
            var doc= new List<Keyword>();
            doc.Add(
                        new Keyword { id = "1", text = newText }
                    );

            JArray jobListings = JArray.Parse(jobApplications);
            int counter = 2;
            foreach (var listing in jobListings)
            {
                var resumeSplit = resume.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string description = listing["description"].ToString();
                newText = Regex.Replace(description, @"\t|\n|\r", "");
                if (description.Equals(null))
                {
                    description = String.Empty;
                }
                doc.Add(
                          new Keyword { id = counter.ToString(), text = newText }
                        );

                counter++;
            }

            dynamic collectionWrapper = new
            {
                documents = doc
            };

            return JsonConvert.SerializeObject(collectionWrapper);
            
        }

        public async Task<Dictionary<String, List<String>>> extractKeywords(string request)
        {
            using(HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "b5a6c8f9ac314016af467cec31dd118c");
                var queryString = HttpUtility.ParseQueryString(string.Empty);
                var uri = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases?" + queryString;
                HttpResponseMessage response;
                byte[] byteData = Encoding.UTF8.GetBytes(request);

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PostAsync("https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases?", content);
                }
                return parseIt(await response.Content.ReadAsStringAsync());
            }
        }

        public Dictionary<String, List<String>> parseIt(string jsonString)
        {
            Dictionary<String, List<String>> mapper = new Dictionary<String, List<String>>();
            JObject obj = new JObject();

            obj = JObject.Parse(jsonString);

            JToken docMember = obj["documents"];

            JArray array = (JArray)docMember;

            for (int i = 0; i < array.Count; i++)
            {
                JArray subArray = (JArray)array[i]["keyPhrases"];
                String id = (String)array[i]["id"];
                List<String> keyWords = new List<String>();

                for (int j = 0; j < subArray.Count; j++)
                {
                    keyWords.Add(subArray[j].ToString());
                }
                mapper.Add(id, keyWords);
            }
            return mapper;
        }

        public List<double> CompareAllEntries(string resume, string jobApplications)
        {
            JArray jobListings = JArray.Parse(jobApplications);
            var resumeSplit = resume.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            List<double> matchCount = new List<double>();
            foreach (var listing in jobListings)
            {
                string description = listing["description"].ToString();
                double matches = (double)resumeSplit.Count(x => description.Contains(x));
                double matched = matches / (double)resumeSplit.Count();
                matchCount.Add(matched);
            }

            return matchCount;

        }


        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    string _FileName = System.IO.Path.GetFileName(file.FileName);
                    string _path = System.IO.Path.Combine(Server.MapPath("~/UploadedFiles"), _FileName);
                    file.SaveAs(_path);
                }
                ViewBag.Message = "File Uploaded Successfully!!";
                return View("~/Views/Home/Index.cshtml");
            }
            catch
            {
                ViewBag.Message = "File upload failed!!";
                return View("~/Views/Home/Index.cshtml");
            }
        }

        [HttpPost]
        public async Task<ActionResult> GetData(bool fulltime, string role, string location)
        {
            Dictionary<string, string> qualifications = new Dictionary<string, string>();
            qualifications.Add("Role", role);
            qualifications.Add("Location", location);
            qualifications.Add("Fulltime", fulltime.ToString());
            await GetJobs(qualifications);
            return View("~/Views/Home/Index.cshtml");
        }
    }
}