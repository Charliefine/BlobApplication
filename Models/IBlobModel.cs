using Azure.Storage.Blobs;

namespace BlobApplication.Models
{
    public interface IBlobModel
    {
        IEnumerable<string> getImageUrls();
        string uploadImage(HttpContext httpContext, IFormFile file);
        string removeImage(string imageName);
    }
}