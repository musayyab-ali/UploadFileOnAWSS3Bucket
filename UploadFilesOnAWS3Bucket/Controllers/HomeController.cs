using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using S3.Repository;
using System.Diagnostics;
using UploadFilesOnAWS3Bucket.Models;
using Amazon.Runtime;

namespace UploadFilesOnAWS3Bucket.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private S3DbContext _context { get; set; }
        public HomeController(ILogger<HomeController> logger, S3DbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var files = new List<S3FileModel>();
            files = _context.s3FileModels.ToList();
            return View(files);
        }

        [HttpPost]
        public async Task<IActionResult> uploadFileS3(IFormFile file)
        {
            var credentials = new BasicAWSCredentials("AKIA5Z4N3Y234A32CAHI", "l8r+teSnmAjiZfvf2cxEm4RsS5lVFkXIS3DNj4Oe");
            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USEast1 // Replace with your appropriate region
            };

            using (var amazonclient = new AmazonS3Client(credentials, config))
            {
                using (var memorystream = new MemoryStream())
                {
                    file.CopyTo(memorystream);
                    var request = new TransferUtilityUploadRequest
                    {
                        InputStream = memorystream,
                        Key = file.FileName,
                        BucketName = "hms-site",
                        ContentType = file.ContentType,
                    };
                    var transferUtility = new TransferUtility(amazonclient);
                    await transferUtility.UploadAsync(request);
                }
            }

            // Your existing code
            var model = new S3FileModel();
            model.FileName = file.FileName;
            model.CreatedDate = DateTime.Now;

            _context.s3FileModels.Add(model);
            _context.SaveChanges();

            ViewBag.Success = "File Upload Successfully on Aws s3 bucket";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteFile(int Id)
        {
            var model = new S3FileModel();
            model = _context.s3FileModels.FirstOrDefault(f => f.Id == Id);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFileToS3(string fileName)
        {
            var credentials = new BasicAWSCredentials("AKIA5Z4N3Y234A32CAHI", "l8r+teSnmAjiZfvf2cxEm4RsS5lVFkXIS3DNj4Oe");
            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USEast1 // Replace with your appropriate region
            };

            using (var amazonclient = new AmazonS3Client(credentials, config))
            {
                var transferUtility = new TransferUtility(amazonclient);
                await transferUtility.S3Client.DeleteObjectAsync(new DeleteObjectRequest()
                {
                    BucketName = "hms-site",
                    Key = fileName,
                });

                var model = new S3FileModel();
                model = _context.s3FileModels.FirstOrDefault(x => x.FileName.ToLower() == fileName.ToLower());
                _context.s3FileModels.Remove(model);
                _context.SaveChanges();

                ViewBag.Success = "File deleted Successfully on Aws s3 bucket";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}