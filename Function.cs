using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;
using TraceAggregator.Extension;
using TraceAggregator.Services.Interfaces;

namespace TraceAggregator
{
    /// <summary>
    /// Trace aggregator
    /// .net core GCP function: https://codeblog.jonskeet.uk/2020/10/23/a-tour-of-the-net-functions-framework/
    /// </summary>
    [FunctionsStartup(typeof(Startup))]
    public class Function : IHttpFunction
    {
        private readonly ILogger _logger;
        private readonly IAggregatorService _aggregagorService;

        public Function(ILoggerFactory loggerFactory, ILogger<Function> logger, IAggregatorService aggregatorService)
        {
            _logger = logger;
            _aggregagorService = aggregatorService;
        }


        /// <summary>
        /// Logic for your function goes here.
        /// </summary>
        /// <param name="context">The HTTP context, containing the request and the response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleAsync(HttpContext context)
        {
            var query = context.Request.Query;
            var reduce = GetQueryStringParameterOrDefault<bool>(query, "reduce");           // Action to execute
            var filename = GetQueryStringParameterOrDefault<string>(query, "filename");     // Target tracks zip filename
            var factor = GetQueryStringParameterOrDefault<float?>(query, "factor");          // Reduction factor
            var keepBacklog = GetQueryStringParameterOrDefault<bool>(query, "keepBacklog");      // True if backlog should not be deleted once processed

            // --- Reduce existing tracks file
            if (reduce)
            {
                await _aggregagorService.ReduceCumulativeTracksZipFile(filename, factor);
            }
            // --- Aggregate backlog files into yearly tracks file
            else
            {
                await _aggregagorService.Run(factor, keepBacklog);
            }

            await context.Response.WriteAsync("[AggregatorService] Done !");
        }

        private T GetQueryStringParameterOrDefault<T>(IQueryCollection query, string paramName)
        {
            var paramValue = query[paramName].ToString();

            if (string.IsNullOrEmpty(paramValue))
            {
                return default(T);
            }
            else
            {
                return query[paramName].ToString().TryCast<T>();
            }
        }


       

        

    }
}
