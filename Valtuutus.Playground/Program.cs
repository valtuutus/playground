using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sysinfocus.AspNetCore.Components;
using Valtuutus.Core.Configuration;
using Valtuutus.Data.InMemory;
using Valtuutus.Playground;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSysinfocus(true);
builder.Services.AddValtuutusCore("entity user {}")
    .AddInMemory();

await builder.Build().RunAsync();