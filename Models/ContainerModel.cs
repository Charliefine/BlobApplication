using Microsoft.AspNetCore.Mvc;

namespace BlobApplication.Models
{
    public class ContainerModel : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
