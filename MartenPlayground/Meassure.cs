using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Serilog;

namespace MartenPlayground
{
    class Meassure
    {
        public static void Run(Action action, [CallerFilePath] string callerFilePath = "")
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            var operation = Path.GetFileNameWithoutExtension(callerFilePath);
            var message = $"{operation} took: {stopwatch.Elapsed.TotalMilliseconds} ms.";
            if (stopwatch.Elapsed < TimeSpan.FromSeconds(1))
            {
                Log.Information(message);
            }
            else
            {
                Log.Warning(message);
            }
        }
    }
}