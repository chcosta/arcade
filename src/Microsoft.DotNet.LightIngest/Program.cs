using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.DotNet.LightIngest
{ 
    public class Program
    {
        public static int Main(string[] args)
        {
            var exeDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            var exe = Path.Combine(exeDirectory, "lightingest.exe");
            var psi = new ProcessStartInfo(exe, string.Join(' ', args));
            var process = Process.Start(psi);
            process.WaitForExit();
            return process.ExitCode;
        }
    }
}
