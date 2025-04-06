using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dev4_image_uploader.Models;
using dev4_image_uploader.Data;
using Microsoft.EntityFrameworkCore;

namespace dev4_image_uploader.Controllers
{
    [ApiController]
    [Route("/")]
    public class RoutesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string[] validExtensions = [ ".jpg", ".jpeg", ".png", ".gif" ];
        public RoutesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult DefaultRoute()
        {
            return Ok("Welcome to the Image Uploader API!");
        }

        [HttpPost("upload")] // CREATE
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            // Checks validity of user request
            (bool valid, string? validationMessage) = await IsValidImage(file);

            if (!valid)
            {
                return BadRequest(validationMessage);
            }

            // Attempts to save image data if filename is unique and image content is valid
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
                ContentType = file.ContentType,
                FileSize = (int)file.Length
            };

            // Save the image entry to the database
            _context.ImageEntries.Add(imageEntry);

            await _context.SaveChangesAsync();

            // Check if the imageEntry was saved successfully
            if (!await FilenameExists(fileName))
            {
                return StatusCode(500, "File upload failed.");
            }

            return Ok(new { FilePath = filePath,  message = "file uploaded successfully!"});
        }

        [HttpGet("recent")] // READ
        public async Task<IActionResult> GetRecentImage()
        {
            // returns the most recently uploaded image on the server
            if (_context.ImageEntries == null || _context.ImageEntries.Count() == 0)
            {
                return NotFound("No recently uploaded images.");
            }

            ImageEntry? mostRecentEntry = await _context.ImageEntries
            .Where(ef => ef.UploadDate == _context.ImageEntries.Max(e => e.UploadDate))
            .FirstOrDefaultAsync();

            if (mostRecentEntry == null) return StatusCode(500, "image data could not be fetched.");

            return Ok(mostRecentEntry);
        }

        [HttpGet("view/{id}")] // READ
        public async Task<IActionResult> DisplayImage(int id)
        {
            ImageEntry? imageEntry = await _context.ImageEntries.FindAsync(id);
            if (imageEntry == null || !System.IO.File.Exists(imageEntry.FilePath))
            {
                return NotFound("Image not found.");
            }
            var fileStream = new FileStream(imageEntry.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            return File(fileStream, imageEntry.ContentType ?? "application/octet-stream");

        }

        [HttpGet("describe/{id}")] // READ
        public async Task<IActionResult> DescribeImage(int id)
        {
            ImageEntry? imageEntry = await _context.ImageEntries.FindAsync(id);
            if (imageEntry == null)return NotFound("Image data not found.");            var fileStream = new FileStream(imageEntry.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            return Ok(imageEntry);

        }
        
        [HttpGet("download/{id}")] // READ
        public async Task<IActionResult> DownloadImage(int id)
        {
            ImageEntry? imageEntry = await _context.ImageEntries.FindAsync(id);
            if (imageEntry == null || !System.IO.File.Exists(imageEntry.FilePath))
            {
                return NotFound("Image not found.");
            }
            var fileStream = new FileStream(imageEntry.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            return File(fileStream, imageEntry.ContentType ?? "application/octet-stream", imageEntry.FileName);

        }

        [HttpGet("showAll")] // READ
        public async Task<IActionResult> GetAllEntries()
        {
            //Console.WriteLine("this is the allUploads route!");
            List<ImageEntry>? allEntries = await _context.ImageEntries.ToListAsync();
            if (allEntries == null || allEntries.Count == 0)
            {
                return NotFound("No images on the server yet.");
            }
            return Ok(allEntries);
        }

        // should require special privilages. for testing purposes mostly
        [HttpDelete("deleteAll")] // DELETE
        public async Task<IActionResult> DeleteAllEntries()
        {
            var allEntries = await _context.ImageEntries.ToListAsync();
            if (allEntries == null || allEntries.Count == 0)
            {
                return BadRequest("No images to delete.");
            }

            _context.ImageEntries.RemoveRange(allEntries);
            foreach (var entry in allEntries)
            {
                if (System.IO.File.Exists(entry.FilePath))
                {
                    System.IO.File.Delete(entry.FilePath);
                } 
                else
                {
                    Console.WriteLine($"File not found: {entry.FilePath}");
                }
            }
            await _context.SaveChangesAsync();

            if (await _context.ImageEntries.AnyAsync())
            {
                return StatusCode(500, "Failed to delete all images.");
            }

            return Ok("All images deleted successfully.");
        }

        public async Task<(bool valid, string? validationMessage)> IsValidImage(IFormFile file)
        {
            // can probably replace with exception handling

            if (file == null || file.Length == 0)
            {
                return (false, "No file uploaded.");
            }

            if (await FilenameExists(Path.GetFileName(file.FileName)))
            {
                return (false, "File name already exists.");
            }

            if (!validExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
            {
                return (false, "Invalid file type.");
            }

            return (true, null);
        }

        public async Task<bool> FilenameExists(string fileName)
        {
            // return true if filename exists in table
            var savedImageEntry = await _context.ImageEntries.FirstOrDefaultAsync(e => e.FileName == fileName);
            return savedImageEntry != null;
        }
    }
}