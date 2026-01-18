using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using static EpubCleaner.AnsiColorWriter;
using static EpubCleaner.Constants;

namespace EpubCleaner {
	public static class PdfPageTrimmer {
		/// <summary>
		/// Creates a new PDF file containing only the specified inclusive range of pages from the input PDF document.
		/// </summary>
		/// <remarks>If both start and end pages are null or the specified range includes the entire document, the
		/// method returns false and no output is created. The method does not overwrite the input file. The output file will
		/// not be created if the input path is invalid, the page range is invalid, or an error occurs during
		/// saving.</remarks>
		/// <param name="inputPdfPath">The full file path to the source PDF document. Must refer to an existing file with a '.pdf' extension.</param>
		/// <param name="outputPdfPath">The file path where the trimmed PDF will be saved. If null, the output will be saved in the same directory as the
		/// input file with '_trimmed' appended to the file name.</param>
		/// <param name="startPageInclusive">The first page in the range to keep, using 1-based indexing. If null, the range starts from the first page of the
		/// document. Must not be negative.</param>
		/// <param name="endPageInclusive">The last page in the range to keep, using 1-based indexing. If null, the range ends with the last page of the
		/// document. Must not be negative or greater than the totalPageCount number of pages.</param>
		/// <returns>true if the operation succeeds and the output PDF is created; otherwise, false.</returns>
		public static bool KeepPageRange(string inputPdfPath, string? outputPdfPath, int? startPageInclusive, int? endPageInclusive) {
			if (startPageInclusive == null && endPageInclusive == null) {
				Red(_neitherGivenMessage);
				return false;
			}

			if ((startPageInclusive is not null and < 0) || (endPageInclusive is not null and < 0)) {
				Red(_negativePageNumbersInvalid);
				return false;
			}

			FileInfo fileInfo = new(inputPdfPath);
			if (!fileInfo.Exists) {
				Red(_inputPdfPathFileNotFound);
				return false;
			} else {
				if (fileInfo.Extension == _pdf) {
					LightBlue(_foundValidDocument);
				} else {
					Red(_inputPdfPathInvalidExtension);
					return false;
				}
			}
			if (outputPdfPath == null) {
				Yellow(_outputPdfDocumentPathUnspecified);
			}

			Green(_outputPdfDocumentPathUnspecified);
			using PdfDocument input = PdfReader.Open(inputPdfPath, PdfDocumentOpenMode.Import);
			int totalPageCount = input.PageCount;

			if (startPageInclusive is null) {
				Yellow(_startPageNotSpecified);
				startPageInclusive = 1;
			}
			if (endPageInclusive is null) {
				endPageInclusive = totalPageCount;
				LightBlue($"{_endPageNotSpecified}{endPageInclusive}.");
				Yellow($"{_inclusiveRangeIs}{startPageInclusive}, {endPageInclusive}).");
			} else if (endPageInclusive > totalPageCount) {
				Red(_givenEndPageIsTooHigh);
				return false;
			}

			if (startPageInclusive is 1 && endPageInclusive == totalPageCount) {
				Red(_assumedPageRangeIsTheEntireDocument);
				return false;
			}

			if (endPageInclusive < startPageInclusive) {
				Red(_specifiedEndPageIsLessThanTheEndPage);
				return false;
			}

			using PdfDocument output = new();
			for (int i = startPageInclusive.Value; i <= endPageInclusive.Value; i++) {
				_ = output.AddPage(input.Pages[i - 1]);
			}

			if (outputPdfPath == null) {
				outputPdfPath = $"{fileInfo.FullName.TrimEnd(_pdf)}{_pdfTrimmed}";
				Yellow($"{_sinceTheOutputPathWasntSpecified}{outputPdfPath}");
			}

			try {
				output.Save(outputPdfPath!);
			} catch (Exception e) {
				Red($"{_error_saving}\"{e.Message}\"");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Removes the specified page ranges from a PDF file and saves the result to a new file.
		/// </summary>
		/// <remarks>Page numbers are 1-based. If a specified range partially or fully exceeds the document'startPage page
		/// count, it is clamped to valid pages. If all pages are removed, the output PDF will be empty.</remarks>
		/// <param name="inputPdfPath">The path to the input PDF file from which pages will be removed. Cannot be null or empty.</param>
		/// <param name="outputPdfPath">The path where the output PDF file will be saved. If a file already exists at this path, it will be overwritten.
		/// Cannot be null or empty.</param>
		/// <param name="removeRanges">An array of tuples specifying the inclusive page ranges to remove. Each tuple defines a start and end page number
		/// (1-based). Ranges outside the bounds of the document are automatically adjusted.</param>
		public static bool RemoveRanges(string inputPdfPath, string outputPdfPath, params (int? start, int? end)[] removeRanges) {
			using PdfDocument input = PdfReader.Open(inputPdfPath, PdfDocumentOpenMode.Import);
			int totalPageCount = input.PageCount;

			IEnumerable<(int? start, int? end)> invalidRanges =
				removeRanges.Where(range =>
					// Both are null, or
					((range.start is null) && (range.end is null)) ||
					// Non-null, however negative, or 
					((range.start is not null and < 0) && (range.end is not null and < 0)) ||
					// Start page is 0 or end page is greater than the total number of pages
					(range.start is 0) || ((range.end is not null) && (range.end > totalPageCount)));

			if (invalidRanges.Any()) {
				Red(_oneOrMoreInvalidRangesProvided);
				return false;
			}

			IEnumerable<(int? start, int? end)> uselessRanges =
				removeRanges.Where(range =>
					(range.start is not null and 1) && range.end != null && range.end == totalPageCount);

			if (uselessRanges.Any()) {
				Red(_tryingToRemoveAllPagesInvalid);
				return false;
			}

			FileInfo fileInfo = new(inputPdfPath);

			if (!fileInfo.Exists) {
				Red(_inputPdfPathFileNotFound);
				return false;
			} else {
				if (fileInfo.Extension == _pdf) {
					LightBlue(_foundValidDocument);
				} else {
					Red(_inputPdfPathInvalidExtension);
					return false;
				}
			}
			if (outputPdfPath == null) {
				Yellow(_outputPdfDocumentPathUnspecified);
			}

			Green(_loadingPdfDocumentMessage);

			bool[] remove = new bool[totalPageCount + 1]; // 1-based
			foreach ((int? start, int? end) range in removeRanges) {
				int startPage = Math.Max(1, range.start!.Value);
				int endPage = Math.Min(totalPageCount, range.end!.Value);
				for (int page = startPage; page <= endPage; page++) {
					remove[page] = true;
				}
			}

			using PdfDocument output = new();
			for (int p = 1; p <= totalPageCount; p++) {
				if (!remove[p])
					_ = output.AddPage(input.Pages[p - 1]);
			}

			try {
				output.Save(outputPdfPath!);
			} catch (Exception e) {
				Red($"{_error_saving}\"{e.Message}\"");
				return false;
			}
			return true;
		}
	}
}

