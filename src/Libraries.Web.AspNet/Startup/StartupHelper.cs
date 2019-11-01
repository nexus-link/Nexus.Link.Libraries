#if !NETCOREAPP

using System;
using System.Configuration;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.Libraries.Web.AspNet.Startup
{
    public static class StartupHelper
    {
        public static void DeleteOldLogFilesFromAppData()
        {
            try
            {
                var maxAgeInDays = int.Parse(ConfigurationManager.AppSettings["FallbackLogger.Files.MaxAgeInDays"] ?? "60");
                var developmentMaxFileSizeInMiB = int.Parse(ConfigurationManager.AppSettings["FallbackLogger.Files.Development.MaxSizeInMebiBytes"] ?? "100");

                var appDataDir = new System.IO.DirectoryInfo($@"{AppDomain.CurrentDomain.BaseDirectory}\App_Data");
                if (appDataDir.Exists)
                {
                    foreach (var fileInfo in appDataDir.GetFiles())
                    {
                        if (fileInfo.Name.EndsWith("FulcrumTraceSource.trace.txt"))
                        {
                            if (fileInfo.LastWriteTime.AddDays(maxAgeInDays) < DateTime.Now
                                || FulcrumApplication.IsInDevelopment && fileInfo.Length > developmentMaxFileSizeInMiB * 1024 * 1024)
                            {
                                try
                                {
                                    fileInfo.Delete();
                                }
                                catch (Exception e)
                                {
                                    Log.LogWarning($"Could not delete old file {fileInfo.FullName}. LastWriteTime: {fileInfo.LastWriteTime}", e);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogWarning($"Error during startup deletion of old log files from App_Data", e);
            }
        }
    }
}

#endif