using RosieTheJobHunter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Newtonsoft.Json.Linq;

namespace RosieTheJobHunter.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            string resume = ParsePDF();
            string jobListings = await GetJobs();
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
            using (PdfReader reader = new PdfReader("C:/Users/t-kiboni/Downloads/KB_Resume.pdf")) 
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

        public decimal CompareAllEntries(string jobApplications, string resume)
        {
            JObject jobListings = JObject.Parse(jobApplications);
            foreach (var listing in jobListings)
            {
                var resumeSplit = resume.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var description = listing['description'];
                var res1 = (from string part in
                            select new
                            {
                                list = part,
                                count = part.Split(new char[] { ' ' }).Sum(p => s.Contains(p) ? 1 : 0)

                            }).OrderByDescending(p => p.count).First();

                Console.Write(res1.count);
            }

        }

        public decimal CompareEntries(JObject jobApplication, string resume)
        {

        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}