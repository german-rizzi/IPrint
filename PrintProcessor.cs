using Blazorise;
using IPrint.Entities;
using IPrint.Helpers;
using IPrint.Models;
using IPrint.Models.Encuentra;
using IPrint.Models.Encuentra.Responses;
using IPrint.Services;
using PdfiumViewer;
using System.Drawing;
using System.Drawing.Printing;
using Color = System.Drawing.Color;

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
						PrintPdf(pdfBytes, "", labelProfile.Configuration.Width, labelProfile.Configuration.Height, entry.Porait);

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
            PrintDocument printDoc = new PrintDocument();
            if (!string.IsNullOrEmpty(printerName))
            {
                printDoc.PrinterSettings.PrinterName = printerName;
            }

            int currentPage = 0, dpi = 300;
            using var pdfStream = new MemoryStream(pdfBytes);
            using var pdfDocument = PdfDocument.Load(pdfStream);

            var pixelsWidth = (int)(pageWidth * dpi / 2.54f);
            var pixelsHeight = (int)(pageHeight * dpi / 2.54f);

            printDoc.PrintPage += (sender, e) =>
            {
                e.PageSettings.Margins = new Margins(0, 0, 0, 0);

                // Renderiza la página actual a una imagen
                Bitmap bmp = RenderPdfPageToImage(pdfDocument, currentPage, pixelsWidth, pixelsHeight, dpi, rotate);

                // Dibuja la imagen en el área de impresión
                e.Graphics.DrawImage(bmp, 0, 0, pixelsWidth, pixelsHeight);

                currentPage++;

                // Verifica si hay más páginas
                e.HasMorePages = currentPage < pdfDocument.PageCount;
            };

            printDoc.Print();
        }

        static Bitmap RenderPdfPageToImage(PdfDocument pdfDocument, int pageIndex, int pixelsWidth, int pixelsHeight, int dpi, bool rotate)
        {
            // Crea un bitmap con las dimensiones correctas
            Bitmap bitmap = new Bitmap(pixelsWidth, pixelsHeight);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White); // Fondo blanco

                if (rotate)
                {
                    graphics.RotateTransform(90);
                }

                // Define un rectángulo para el área de renderizado
                var renderArea = new Rectangle(0, 0, pixelsWidth, pixelsHeight);

                // Renderizar la página dentro del área especificada
                pdfDocument.Render(pageIndex, graphics, dpi, dpi, renderArea, PdfRenderFlags.ForPrinting);
            }

            return bitmap;
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
