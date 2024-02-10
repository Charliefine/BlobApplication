using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;

namespace BlobApplication.Models
{
    public class BlobModel : IBlobModel
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly CosmosClient _cosmosClient;

        public BlobModel(string connectionString, string containerName, CosmosClient cosmosClient)
        {
            _connectionString = connectionString;
            _containerName = containerName;
            _cosmosClient = cosmosClient;
        }

        // Get list of image urls
        public IEnumerable<string> getImageUrls()
        {
            BlobContainerClient containerClient = getBlobContainerClient();
            List<string> toReturn = new List<string>();

            // List all blobs in the container
            foreach (var blobItem in containerClient.GetBlobs())
            {
                // Add URL with CDN
                var cdnUrl = $"https://cdnexam-endpoint.azureedge.net/{_containerName}/{blobItem.Name}";
                toReturn.Add(cdnUrl);
            }

            // Log operation in CosmosDB
            // Use this for debug purpose 
            //logOperation("Debug", "FETCH", "Fetch images");

            return toReturn;
        }

        public string removeImage(string imageName)
        {
            BlobClient blobClient = new BlobClient(_connectionString, _containerName, imageName);
            try
            {
                blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                string errorMsg = "Something went wrong during deleting file [" + imageName + "] from Azure Storage. Error message: " + ex.Message;
                logOperation("Error", "DELETE", errorMsg);
                return errorMsg;
            }

            logOperation("Information", "DELETE", "Delete image: " + imageName);
            return "";
        }

        public string uploadImage(HttpContext httpContext, IFormFile file)
        {
            BlobContainerClient containerClient = getBlobContainerClient();
            if (httpContext.Request.Method == "POST")
            {
                if (file != null && file.Length > 0)
                {
                    try
                    {
                        // Upload file to Azure Storage
                        using (var stream = file.OpenReadStream())
                        {
                            stream.Position = 0;
                            containerClient.UploadBlobAsync(file.FileName, stream);
                            logOperation("Information", "PUT", "Uploaded file [" + file.FileName + "].");
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = "Something went wrong during uploading file to Azure Storage. Error message: " + ex.Message;
                        logOperation("Error", "PUT", errorMsg);
                        return errorMsg;
                    }
                }
                string noFileMsg = "No file found. Please select file to upload.";
                logOperation("Information", "PUT", noFileMsg);
                return noFileMsg;
            }
            return "";
        }

        // Helper methods
        private BlobContainerClient getBlobContainerClient()
        {
            // Initialize BlobServiceClient
            var blobServiceClient = new BlobServiceClient(_connectionString);

            // Get a reference to the container
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            return containerClient;
        }

        private void logOperation(string severity, string operation, string description)
        {
            var container = _cosmosClient.GetContainer("blobapplication-db", "logs");
            var item = new { id = Guid.NewGuid().ToString(), severity = severity, operation = operation, timestamp = DateTime.UtcNow, description = description };
            container.CreateItemAsync(item, new PartitionKey(item.id));
        }
    }
}