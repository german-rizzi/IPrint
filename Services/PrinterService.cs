using System.Diagnostics;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

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

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "lpstat",
                    Arguments = "-p",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("printer"))
                {
                    var parts = line.Split(' ');
                    if (parts.Length > 1)
                    {
                        printers.Add(parts[1]);
                    }
                }
            }

            return printers;
        }
    }
}
