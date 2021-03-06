using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OcrWaterMeter.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder..ConfigureWebHostDefaults(webBuilder =>
//{
//    webBuilder.UseStaticWebAssets();
//    webBuilder.UseStartup<Startup>();
//});
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
await builder.Build().RunAsync();
