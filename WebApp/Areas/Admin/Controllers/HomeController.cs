using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebApp.Areas.Admin.Controllers
{

    [Area("admin")]
    [Route("admin/homeadmin")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class HomeController : Controller
    {
        [Route("")]
        [Route("index")]
        [AuthorizeToken]
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "phanquyen");
            }
            else
            {
                return View();
            }
        }

        //[HttpPost]
        //[Route("countCompany")]
        //public IActionResult countCompany()
        //{

        //    try
        //    {
        //        if (DateTime.TryParse(tungay, out DateTime tungayDate) && DateTime.TryParse(denngay, out DateTime denngayDate))
        //        {
        //            DateOnly tungayDateOnly = new DateOnly(tungayDate.Year, tungayDate.Month, tungayDate.Day);
        //            DateOnly denngayDateOnly = new DateOnly(denngayDate.Year, denngayDate.Month, denngayDate.Day);
        //            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + $"/ThongKe/Get_PhieuMuon_API/{tungay}/{denngay}").Result;

        //            if (response.IsSuccessStatusCode)
        //            {
        //                string data = response.Content.ReadAsStringAsync().Result;

        //                // Log the response data to understand its structure
        //                Console.WriteLine("Response Data: " + data);

        //                try
        //                {
        //                    // Deserialize the response directly to List<ThongKeSach>
        //                    phieu = JsonConvert.DeserializeObject<ThongKePhieu>(data);

        //                    return Ok(new { success = true, phieuList = phieu });
        //                }
        //                catch (JsonException ex)
        //                {
        //                    // Log the deserialization error for debugging
        //                    Console.WriteLine("Deserialization error: " + ex.Message);
        //                    return StatusCode(500, new { success = false, message = "Error processing API response." });
        //                }
        //            }
        //            else
        //            {
        //                return BadRequest(new { success = false, message = "Failed to retrieve data from API." });
        //            }
        //        }
        //        else
        //        {
        //            return BadRequest("Invalid date format.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception details
        //        Console.WriteLine("Exception: " + ex.Message);
        //        Console.WriteLine("Stack Trace: " + ex.StackTrace);
        //        return StatusCode(500, new { success = false, message = ex.Message });
        //    }


        
    }
}
