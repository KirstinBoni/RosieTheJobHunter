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

namespace RosieTheJobHunter.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            await GetJobs();
            HomeIndexModel model = new HomeIndexModel()
            {

            };
            return View();
        }

        public async Task GetJobs()
        {
            using (HttpClient client = new HttpClient())
            {
                StringBuilder uri =  new StringBuilder("https://jobs.github.com/positions.json?");
                var parameters = new Dictionary<string, string> { { "description", "software+engineering"  },
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
                
                
            }
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), _FileName);
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