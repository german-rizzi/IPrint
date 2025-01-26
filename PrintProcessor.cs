using IPrint.Entities;
using IPrint.Helpers;
using IPrint.Models;
using IPrint.Services;
using PdfiumViewer;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using Color = System.Drawing.Color;

namespace IPrint
{
	public class PrintProcessor(PrintSpoolerService printSpoolerService, UserConfigService userConfigService, PrinterConfigService printerConfigService, LabelProfileService labelProfileService) : IDisposable
	{
		private readonly int PollingInterval = 5000;
		private CancellationTokenSource Cts;
		private bool running;

        public void StartProcessing()
		{
			if (running) StopProcessing();
			Cts = new CancellationTokenSource();
			Task.Run(() => ProcessLoop(Cts.Token));
			running = true;
		}

		public void StopProcessing()
		{
			Cts.Cancel();
			running = false;
		}

		private async Task ProcessLoop(CancellationToken token)
		{
			var userConfig = userConfigService.Get();
			var printersConfigs = printerConfigService.GetAll();
			var labelsProfiles = labelProfileService.GetAll();

            while (!token.IsCancellationRequested)
			{
				var entries = await printSpoolerService.GetByStatusAsync(0, 22);

				foreach (var entry in entries)
				{
					var printerConfig = printersConfigs.FirstOrDefault(e => e.Type.GetHashCode() == entry.PrinterId);
					var labelProfiles = labelsProfiles.Where(e => !printerConfig.LabelProfileIds.IsNullOrEmpty() && printerConfig.LabelProfileIds.Contains(e.Id)).ToList();
                    var labelProfile = MatchLabelProfile(entry, labelProfiles);

                    try
					{
						byte[] pdfBytes = await DownloadPdf(entry.Url);
						int width = CentimetersToPoints(labelProfile.Configuration.Width);
						int height = CentimetersToPoints(labelProfile.Configuration.Height);

						PrintPdf(pdfBytes, "", width, height, entry.Porait);

						// Si es exitoso, marcar como procesado
						await printSpoolerService.UpdateStatusAsync(entry.Id, 1, entry.CompanyId);
					}
					catch (Exception)
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

		static void PrintPdf(byte[] pdfBytes, string printerName, int pageWidth, int pageHeight, bool rotate)
		{
			PrintDocument printDoc = new PrintDocument();
			if (!string.IsNullOrEmpty(printerName))
			{
				printDoc.PrinterSettings.PrinterName = printerName;
			}

			int currentPage = 0;
			using var pdfStream = new MemoryStream(pdfBytes);
			using var pdfDocument = PdfDocument.Load(pdfStream);

			printDoc.PrintPage += (sender, e) =>
			{
				e.PageSettings.Margins = new Margins(0, 0, 0, 0);

				// Renderiza la página actual a una imagen
				Bitmap bmp = RenderPdfPageToImage(pdfDocument, currentPage, pageWidth, pageHeight, 300, rotate);

				// Dibuja la imagen en el área de impresión
				e.Graphics.DrawImage(bmp, 0, 0, pageWidth, pageHeight);

				currentPage++;

				// Verifica si hay más páginas
				e.HasMorePages = currentPage < pdfDocument.PageCount;
			};

			printDoc.Print();
		}

		LabelProfile? MatchLabelProfile(PrintSpooler printSpooler, List<LabelProfile> labelProfiles)
		{
			return null;
		}
		
		static Bitmap RenderPdfPageToImage(PdfDocument pdfDocument, int pageIndex, int width, int height, int dpi, bool rotate)
		{
			height = height > 0 ? height : (int)pdfDocument.PageSizes[pageIndex].Height;

			// Renderiza la página especificada
			Bitmap bitmap = new Bitmap(width, height);
			using (var graphics = Graphics.FromImage(bitmap))
			{
				graphics.Clear(Color.White); // Fondo blanco
				graphics.RotateTransform(rotate ? 90 : 0);

				// Define un rectángulo para el área de renderizado
				var renderArea = new Rectangle(0, 0, width, height);

				// Renderizar la página dentro del área especificada
				pdfDocument.Render(pageIndex, graphics, dpi, dpi, renderArea, PdfRenderFlags.ForPrinting);
			}

			return bitmap;
		}

		static int CentimetersToPoints(double centimeter)
		{
			return (int)(centimeter * 300 / 2.54);
		}

		public void Dispose()
		{
			StopProcessing();
		}
	}
}
