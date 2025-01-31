using IPrint.Entities;
using IPrint.Helpers;
using IPrint.Models;
using IPrint.Models.Encuentra;
using IPrint.Models.Encuentra.Responses;
using IPrint.Services;
using PdfiumViewer;
using System.Drawing.Printing;

namespace IPrint
{
    public class PrintProcessor(PrintSpoolerService printSpoolerService, UserConfigService userConfigService, PrinterConfigService printerConfigService, LabelProfileService labelProfileService, EncuentraService encuentraService, AlertService alertService) : IDisposable
	{
		private readonly int PollingInterval = 5000;
		private CancellationTokenSource Cts;
		private bool Running;
		private UserConfig UserConfig;
		private List<PrinterConfig> PrintersConfigs;
		private List<LabelProfile> LabelsProfiles;
		private EncuentraUser EncuentraUser;
		private EncuentraSucursal EncuentraSucursal;

        public void StartProcessing()
		{
			if (Running) StopProcessing();
			Task.Run(() => InitAsync());
			Running = true;
		}

		public void StopProcessing()
		{
			if (Cts is not null)
			{
                Cts.Cancel();
            }

			Running = false;
		}

		private async Task InitAsync()
		{
			try
			{
				UserConfig = userConfigService.Get() ?? throw new ArgumentNullException("User config not found");
                
				var encuentraTokenValidationResponse = await ValidateEncuentraToken(UserConfig.Token) ?? throw new ArgumentNullException("Encuentra token validation error");
				EncuentraUser = encuentraTokenValidationResponse.User ?? throw new ArgumentNullException("Encuentra user not fount");
                EncuentraSucursal = encuentraTokenValidationResponse.Sucursal ?? throw new ArgumentNullException("Encuentra sucursal not fount");
                
				PrintersConfigs = printerConfigService.GetAll();

                if (PrintersConfigs.IsNullOrEmpty())
                {
                    throw new ArgumentNullException("Printers configs not found");
                }

                LabelsProfiles = labelProfileService.GetAll();

                if (LabelsProfiles.IsNullOrEmpty())
                {
                    throw new ArgumentNullException("Labels profiles not found");
                }

                Cts = new CancellationTokenSource();
                await ProcessLoopAsync(Cts.Token);
            }
			catch (Exception ex)
			{
                Dispose();
				alertService.ShowAlert("Error", "Ah ocurrido un error inesperado al iniciar el sistema de impresión");
            }
        }

        private async Task ProcessLoopAsync(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				//var entries = await printSpoolerService.GetByStatusAsync(0, UserConfig.ComputerId, EncuentraUser.CompanyId);
				var entries = new List<PrintSpooler>() { 
					new()
					{
						Id = "1",
						Url = "https://encuentra.200.com.uy/files/pdf/22_0_935700.pdf",
						Porait = false,
						Printed = 0,
						PrinterId = 2,
						EquipId = UserConfig.ComputerId,
						CompanyId = EncuentraUser.CompanyId
                    } 
				};

				foreach (var entry in entries)
				{
                    try
					{
                        var printerConfig = PrintersConfigs.FirstOrDefault(e => e.Type.GetHashCode() == entry.PrinterId || entry.PrinterId > 2 && e.Type == LabelTypes.Etiqueta);

                        if (printerConfig is null)
                        {
                            throw new ArgumentNullException("Printer config not found for the current print");
                        }

                        var labelProfiles = LabelsProfiles.Where(e => !printerConfig.LabelProfileIds.IsNullOrEmpty() && printerConfig.LabelProfileIds.Contains(e.Id)).ToList();
                        var labelProfile = MatchLabelProfile(entry, labelProfiles);

                        if (labelProfile is null)
                        {
                            throw new ArgumentNullException("Label profile not found for the current print");
                        }

                        byte[] pdfBytes = await DownloadPdf(entry.Url);
						PrintPdf(pdfBytes, printerConfig.Name, labelProfile.Configuration.Width, labelProfile.Configuration.Height, entry.Porait);

						// Si es exitoso, marcar como procesado
						await printSpoolerService.UpdateStatusAsync(entry.Id, 1, entry.CompanyId);
					}
					catch (Exception ex)
					{
						// Si falla, incrementar intentos
						await printSpoolerService.UpdateStatusAsync(entry.Id, 0, entry.CompanyId);
					}
				}

				// Esperar el intervalo antes de la siguiente consulta
				await Task.Delay(PollingInterval, token);
			}
		}

		static async Task<byte[]> DownloadPdf(string url)
		{
			using var client = new HttpClient();
			return await client.GetByteArrayAsync(url);
		}

        static void PrintPdf(byte[] pdfBytes, string printerName, double pageWidth, double pageHeight, bool rotate)
        {
            PrintDocument printDocument = new PrintDocument();

            if (!string.IsNullOrEmpty(printerName))
            {
                printDocument.PrinterSettings.PrinterName = printerName;
            }

            int currentPage = 0;
            using var pdfStream = new MemoryStream(pdfBytes);
            using var pdfDocument = PdfiumViewer.PdfDocument.Load(pdfStream);

            printDocument.PrintPage += (sender, e) =>
            {
                if (currentPage >= pdfDocument.PageCount)
                {
                    e.HasMorePages = false;
                    return;
                }

                // Obtener el tamaño de la hoja de la impresora
                var printArea = e.PageBounds;
                var pdfRotation = rotate ? PdfRotation.Rotate90 : PdfRotation.Rotate0;

                int dpi = 300; // Puedes probar con 600 para aún mejor calidad
                int pixelsWidth = (int)(printArea.Width * dpi / 100);
                int pixelsHeight = (int)(printArea.Height * dpi / 100);

                // Renderizar la página PDF
                using (var image = pdfDocument.Render(currentPage, pixelsWidth, pixelsHeight, dpi, dpi, pdfRotation, PdfRenderFlags.ForPrinting))
                {
                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                    // Dibujar la imagen en la hoja ajustando el tamaño
                    e.Graphics.DrawImage(image, printArea);
                }

                currentPage++;
                e.HasMorePages = (currentPage < pdfDocument.PageCount);
            };

            printDocument.Print();
        }

        LabelProfile? MatchLabelProfile(PrintSpooler printSpooler, List<LabelProfile> labelProfiles)
		{
			if (labelProfiles.All(e => e.Conditionals.IsNullOrEmpty()))
			{
				return labelProfiles.FirstOrDefault();
			}
			else
			{
				return null;
			}
		}

		public void Dispose()
		{
			StopProcessing();
		}

		private async Task<EncuentraTokenValidationResponse?> ValidateEncuentraToken(string token)
		{
            EncuentraTokenValidationResponse? result = null;
            var response = await encuentraService.ValidateTokenAsync(token);

			if(response?.Status?.Equals("200") ?? false)
			{
				result = response.Result;
			}

			return result;
		}
    }
}
