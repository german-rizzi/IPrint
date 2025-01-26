using System.Diagnostics;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using Blazorise.Extensions;

namespace IPrint.Services
{
    public class PrinterService
    {
        public List<string> GetSystemPrinterNames()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetPrintersWindows();
            }
            else
            {
                return GetPrintersUnix();
            }
        }

        private List<string> GetPrintersWindows()
        {
            var printers = new List<string>();

            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                printers.Add(printer);
            }

            return printers;
        }

        private List<string> GetPrintersUnix()
        {
            var printers = new List<string>();

            try
            {
                var startInfo = new ProcessStartInfo("lpstat", "-p")
                {
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                var process = Process.Start(startInfo);
                var output = process.StandardOutput.ReadToEnd();
                printers = output.Split("\n", StringSplitOptions.RemoveEmptyEntries)
                                         .Select(line => line.Split(" ")[1])
                                         .Where(name => !string.IsNullOrWhiteSpace(name))
                                         .ToList();
                process.WaitForExit();

                if(printers.Count == 0){
                    printers.Add("Impresora Test");
                }
            }
            catch
            {
                throw;
            }

            return printers;
        }
    }
}
