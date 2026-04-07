using Microsoft.Data.SqlClient;

namespace AutoCarShowroom.Services
{
    public static class DatabaseIssueHelper
    {
        public static bool IsDatabaseConnectivityIssue(Exception exception)
        {
            for (var current = exception; current != null; current = current.InnerException)
            {
                if (current is SqlException sqlException)
                {
                    var message = sqlException.Message;
                    if (message.Contains("SSPI", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("network-related", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("server was not found", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("instance-specific error", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("cannot open database", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("login failed", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
