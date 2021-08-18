using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureDevOpsWorkItemVisualizer.BlazorWebAssembly
{
   public class Program
   {
      public static async Task Main(string[] args)
      {
         var builder = WebAssemblyHostBuilder.CreateDefault(args);
         builder.RootComponents.Add<App>("#app");

         ConfigureServices(builder.Services, builder);

         await builder.Build().RunAsync();
      }

      private static void ConfigureServices(IServiceCollection services, WebAssemblyHostBuilder builder)
      {
         services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
         services.AddBlazoredLocalStorage();
      }
   }
}
