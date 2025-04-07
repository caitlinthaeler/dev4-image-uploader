using Microsoft.EntityFrameworkCore;
using dev4_image_uploader.Data;

var builder = WebApplication.CreateBuilder(args);

// store in an environment variable
var connection = "Server=localhost,1433;Database=ImageUploaderDb;User Id=sa;Password=caitlinAdmin123;TrustServerCertificate=True;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connection));

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.UseStaticFiles();
app.UseCors("AllowAllOrigins");
app.UseRouting();
app.MapControllers();


//initiates server endpoint to enable request processing pipeline
app.Run();
