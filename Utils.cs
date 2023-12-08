using System.Diagnostics;

namespace BidaCanNoManagement
{
    public static class Utils
    {
        public static string GetCurrentDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory ?? Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
        }
    }
}
