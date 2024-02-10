using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using BlobApplication.Models;

namespace BlobApplication.Controllers
{
    public class ImageController : Controller
    {
        private readonly IBlobModel _blobModel;

        public ImageController(IBlobModel blobModel)
        {
            _blobModel = blobModel;
        }

        [HttpGet]
        public IActionResult ListImages()
        {
            return View(_blobModel.getImageUrls());
        }

        [HttpGet]
        public IActionResult ViewUploadImagePage()
        {
            return View("ListImages");
        }

        [HttpPost]
        public IActionResult UploadImage(IFormFile file)
        {
            // Upload image to blob
            string result = _blobModel.uploadImage(HttpContext, file);

            if (!String.IsNullOrEmpty(result))
            {
                ModelState.AddModelError("uploadFile", result);
            }

            // Reload page after uploading
            return RedirectToAction("ListImages");
        }

        [HttpPost]
        public IActionResult RemoveImage(string imageName)
        {
            // Remove image from blob
            string result = _blobModel.removeImage(imageName);

            if (!String.IsNullOrEmpty(result))
            {
                ModelState.AddModelError("removeFile", result);
            }

            // Load page after uploading
            return RedirectToAction("ListImages");
        }
    }
}
