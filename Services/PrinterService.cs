using IPrint.Models;
using System.Management;

namespace IPrint.Services
{
    public class PrinterService
    {
        public SystemPrinter? Find(string name)
        {
            SystemPrinter? printer = null;
            // Consulta WMI para obtener las impresoras instaladas
            string query = "SELECT * FROM Win32_Printer";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject managementObject in searcher.Get())
                {
                    printer = new()
                    {
                        Id = managementObject["PortName"].ToString(),
                        Name = managementObject["Name"].ToString()
                    };

                    break;
                }
            }

            return printer;
        }
    }
}
