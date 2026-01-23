using Microsoft.Playwright;

namespace LLMRAGAssist {
	public static class HtmlToPdf {
		public static async Task RenderPdfAsync(string htmlFilePath, string pdfOutPath) {
			using IPlaywright playwright = await Playwright.CreateAsync();
			await using IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions {
				Headless = true
			});

			IPage page = await browser.NewPageAsync();
			_ = await page.GotoAsync(new System.Uri(htmlFilePath).AbsoluteUri);

			_ = await page.PdfAsync(new PagePdfOptions {
				Path = pdfOutPath,
				PrintBackground = true
			});
		}
	}
}
