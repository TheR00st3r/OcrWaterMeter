using OcrWaterMeter.Server.Database;

var dbFolder = Environment.GetEnvironmentVariable("DATADIR");
var liteDbName = Path.Combine(string.IsNullOrEmpty(dbFolder) ? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) : dbFolder, "WaterMeter.db");

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    WebRootPath = "wwwroot",
    Args = args,
});

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddLiteDb(liteDbName);
builder.Services.AddHealthChecks();

builder.WebHost.UseStaticWebAssets();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.MapHealthChecks("/healthz");
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // app.UseHsts();
}

// app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.MapGet("/version", () => Environment.GetEnvironmentVariable("BUILDNUMBER") ?? "unknown version");
app.MapGet("/build", () => Environment.GetEnvironmentVariable("BUILDID") ?? "unknown buildid");
app.MapGet("/commit", () => Environment.GetEnvironmentVariable("SOURCE_COMMIT") ?? "unknown version");

app.Run();
