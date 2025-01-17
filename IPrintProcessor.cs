using IPrint.Services;
using PdfiumViewer;
using System.Drawing;
using System.Drawing.Printing;
using Color = System.Drawing.Color;

namespace IPrint
{
	public class IPrintProcessor : IDisposable
	{
		private static readonly PrintSpoolerService spoolerService = new();
		private readonly int pollingInterval = 5000;
		private CancellationTokenSource _cts;

		public void StartProcessing()
		{
			_cts = new CancellationTokenSource();
			Task.Run(() => ProcessLoop(_cts.Token));
		}

		public void StopProcessing()
		{
			_cts.Cancel();
		}

		private async Task ProcessLoop(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				var entries = await spoolerService.GetPendingAsync();

				foreach (var entry in entries)
				{
					try
					{
						byte[] pdfBytes = await DownloadPdf(entry.Url);
						int width = CentimetersToPoints(10);
						int height = CentimetersToPoints(15);

						if (entry.IdEquipo == 1)
						{
							width = CentimetersToPoints(7);
							height = 0;
						}

						PrintPdf(pdfBytes, "", width, height, entry.Porait);

						// Si es exitoso, marcar como procesado
						await spoolerService.UpdateStatusAsync(1, entry.IdEmpresa);
					}
					catch (Exception)
					{
						// Si falla, incrementar intentos
						await spoolerService.UpdateStatusAsync(0, entry.IdEmpresa);
					}
				}

				// Esperar el intervalo antes de la siguiente consulta
				await Task.Delay(pollingInterval, token);
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

		static int CentimetersToPoints(int centimeter)
		{
			return (int)(centimeter * 300 / 2.54);
		}

		public void Dispose()
		{
			StopProcessing();
		}
	}
}
