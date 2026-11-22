using EasySolitaire.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Game>("#app");

await builder.Build().RunAsync();
