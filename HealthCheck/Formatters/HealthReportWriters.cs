using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck.Formatters
{
    internal static class HealthReportWriters
    {
        public static string ContentType() => Json.JSON_CONTENT_TYPE;
        public static string FormatResponse(HealthReport report)
        {
            return Json.Serialize(
                new
                {
                    Status = report.Status.ToString(),
                    HealthChecks = report.Entries.Select(e => new
                    {
                        e.Key,
                        Status = e.Value.Status.ToString(),
                        e.Value.Description,
                        e.Value.Data,
                        Exception = e.Value.Exception?.Message,
                    }),
                }
            );
        }
    }
}
