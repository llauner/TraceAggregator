using TraceAggregator.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;

namespace TraceAggregator.Services
{
    public class ConfigurationService : IConfigurationService
    {
        // ########## GCP ##########
        public string GcpProjectId => GetSetting("GcpProjectId", "igcheatmap");

        public string TraceAggregatorBucketName => GetSetting("TraceAggregatorBucketName", "tracemap-trace-aggregator");





        #region Configuration Service
        private readonly IConfiguration _configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// GetSetting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private T GetSetting<T>(string key, T defaultValue = default(T)) where T : IConvertible
        {
            var val = _configuration?[key];
            if (string.IsNullOrEmpty(val))
                val = Environment.GetEnvironmentVariable(key);

            val = val ?? "";

            T result = defaultValue;
            if (!string.IsNullOrEmpty(val))
            {
                var typeDefault = default(T);
                if (typeof(T) == typeof(string))
                {
                    typeDefault = (T)(object)string.Empty;
                }
                result = (T)Convert.ChangeType(val, typeDefault.GetTypeCode());
            }
            return result;
        }
        #endregion

    }
}
