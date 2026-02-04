using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProjectManager.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProjectManager API",
        Version = "v1"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:7192")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
