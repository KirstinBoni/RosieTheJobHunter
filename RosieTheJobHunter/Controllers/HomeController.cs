using RosieTheJobHunter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Newtonsoft.Json.Linq;
using RosieTheJobHunter.Model;
using Extensions;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace RosieTheJobHunter.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            string resume = ParsePDF();
            string jobListings = await GetJobs();
            //string documents = constructText(jobListings, resume);
            //string response = await extractKeywords(documents);
            List<double> matchCount = CompareAllEntries(resume, jobListings);
            HomeIndexModel model = new HomeIndexModel()
            {

            };
            return View();
        }

        public async Task<string> GetJobs()
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
                return responseContent;
            }
        }

        public string ParsePDF()
        {
       
            ITextExtractionStrategy its = new iTextSharp.text.pdf.parser.LocationTextExtractionStrategy();
            using (PdfReader reader = new PdfReader("C:/Users/t-master/Downloads/MESResume.pdf")) 
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
                }
                return text.ToString();
            }
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

        public async Task<string> extractKeywords(string request)
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
                return await response.Content.ReadAsStringAsync();
            }
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

        public void CompareEntries(JObject jobApplication, string resume)
        {

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
    }
}