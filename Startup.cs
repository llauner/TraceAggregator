using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TraceAggregator.Services;
using TraceAggregator.Services.Interfaces;

namespace TraceAggregator
{
    public class Startup: FunctionsStartup
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddTransient<IStorageService, StorageService>();
            services.AddTransient<IAggregatorService, AggregatorService>();
            services.AddTransient<IZipStorageService, ZipStorageService>();

            services.AddLogging(configure => configure.AddConsole().AddDebug())
                .AddTransient<AggregatorService>();
        }
            
        
    }
}
