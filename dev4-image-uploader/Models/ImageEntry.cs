using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dev4_image_uploader.Models;
public struct ImageEntry
{
    public string FileName { get; set; } // ex. my-image.jpg
    public string FilePath { get; set; } // ex. wwwroot/uploads/my-image.jpg
    public DateTime UploadDate { get; set; }
    public int FileSize { get; set; }
}