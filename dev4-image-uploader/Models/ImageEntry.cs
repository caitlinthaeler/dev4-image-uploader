using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dev4_image_uploader.Models;
public class ImageEntry
{
    public int Id { get; set; } // ex. 1234567890
    public string FileName { get; set; } = null!; // ex. my-image.jpg
    public string FilePath { get; set; } = null!; // ex. wwwroot/uploads/my-image.jpg
    public DateTime UploadDate { get; set; }
    public int FileSize { get; set; }
}