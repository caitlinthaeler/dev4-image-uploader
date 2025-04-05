var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");


//initiates server endpoint to enable request processing pipeline
app.Run();
