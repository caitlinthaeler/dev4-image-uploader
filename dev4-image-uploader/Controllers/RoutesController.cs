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

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            (bool success, string? validationMessage) = await IsValidImage(file);

            if (!success)
            {
                return BadRequest(validationMessage);
            }

            // attempt to save image data if filename is unique and image content is valid
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
            //Console.WriteLine("this is the allUploads route!");
            var allEntries = await _context.ImageEntries.ToListAsync();
            if (allEntries == null || allEntries.Count == 0)
            {
                return NotFound("No images found.");
            }
            foreach (var entry in allEntries)
            {
                Console.WriteLine($"Id: {entry.Id}, FileName: {entry.FileName}, FilePath: {entry.FilePath}, UploadDate: {entry.UploadDate}, FileSize: {entry.FileSize}");
            }
            return Ok(allEntries);
        }
        

        public async Task<(bool success, string? message)> IsValidImage(IFormFile file)
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