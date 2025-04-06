using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dev4_image_uploader.Models;

namespace dev4_image_uploader.Controllers
{
    [ApiController]
    [Route("/")]
    public class RoutesController : ControllerBase
    {
        // temp storage
        private Dictionary<string, ImageEntry> _imageEntries = new();

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            (bool success, string? validationMessage) = IsValidImage(file);

            if (!success)
            {
                return BadRequest(validationMessage);
            }
            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine("wwwroot/uploads", file.FileName); // flesh out in the future
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageEntry = new ImageEntry
            {
                FileName = fileName,
                FilePath = filePath,
                UploadDate = DateTime.UtcNow,
                FileSize = (int)file.Length
            };

            _imageEntries.Add(fileName, imageEntry);
            Console.WriteLine($"added image to dictionary. Number of entries: {_imageEntries.Count}");
            Console.WriteLine(_imageEntries.Values.ToList());
            Console.WriteLine("finished displaying dictionary");

            return Ok(new { FilePath = filePath,  message = "file uploaded successfully!"});
        }

        [HttpGet("recentUpload")]
        public async Task<IActionResult> getRecentImage()
        {
            return Ok("this is the recent upload route!");
        }
        

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> downloadImage(string fileName)
        {
            return Ok("this is the download route!");
        }

        [HttpGet("allUploads")]
        public async Task<IActionResult> getAllEntries()
        {
            Console.WriteLine("this is the allUploads route!");
            if (_imageEntries.Count == 0)
            {
                return NotFound("No images found.");
            }
            return Ok(_imageEntries.Values.ToList());
        }
        

        public (bool success, string? message) IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "No file uploaded.");
            }

            if (FilenameExists(Path.GetFileName(file.FileName)))
            {
                return (false, "File name already exists.");
            }

            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!validExtensions.Contains(fileExtension))
            {
                return (false, "Invalid file type.");
            }

            return (true, null);
        }

        public bool FilenameExists(string fileName)
        {
            // Check if the file name already exists in the storage
            return _imageEntries.ContainsKey(fileName);
        }
    }
}