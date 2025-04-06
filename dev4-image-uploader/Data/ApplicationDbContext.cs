using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using dev4_image_uploader.Models;

namespace dev4_image_uploader.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }

        public DbSet<ImageEntry> ImageEntries { get; set; } = null!;
    }
}