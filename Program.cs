using System.Text.Json;
using EwrsDocAnalyses.Misc;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
  {
      options.AddPolicy(name: "AllowAll",
          builder =>
          {
              builder.WithOrigins("http://localhost:26136","https://localhost:26136","https://localhost:44476", "https://localhost:7142", "https://localhost:7231")
              .AllowAnyMethod()
              .AllowCredentials()
              .AllowAnyHeader();
          });
  });

builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton((provider) =>
{
    var hubContext = provider.GetRequiredService<IHubContext<SignalRHub>>();
    return new FolderWatcherService(hubContext);
});
 builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
            });

var app = builder.Build();

app.UseDefaultFiles();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapHub<SignalRHub>("/Signalr");
app.MapFallbackToFile("index.html");
app.Run();