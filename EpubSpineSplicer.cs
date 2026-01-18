using EpubCleaner;

namespace LLMRAGAssist {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Xml.Linq;
	using static AnsiColorWriter;
	using static Constants;

	public static class EpubSpineSlicer {
		/// <summary>
		/// Modifies the spine of an EPUB file to retain only the content documents within the specified inclusive range, and
		/// saves the result to a new EPUB file.
		/// </summary>
		/// <remarks>The method validates the input EPUB file and ensures the specified spine range is within bounds.
		/// If the output path is not provided, the method generates a default output file name. Existing files at the output
		/// path will be overwritten. The method does not modify the original input file.</remarks>
		/// <param name="epubInPath">The path to the input EPUB file to be processed. Must refer to an existing file with a valid EPUB extension.</param>
		/// <param name="epubOutPath">The path where the output EPUB file will be saved. If null, a default path is generated based on the input file
		/// name.</param>
		/// <param name="keepStartInclusive">The zero-based index of the first spine item to keep, inclusive. If null, defaults to 1. Must not be negative.</param>
		/// <param name="keepEndInclusive">The zero-based index of the last spine item to keep, inclusive. If null, all items from the start index to the end
		/// of the spine are kept. Must not be negative.</param>
		/// <returns>true if the spine was successfully modified and the output EPUB was saved; otherwise, false.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the EPUB container file is missing required metadata, such as the container.xml file.</exception>
		public static bool KeepSpineRange(string epubInPath, string epubOutPath, int? keepStartInclusive, int? keepEndInclusive) {
			if (keepStartInclusive == null && keepEndInclusive == null) {
				Red(_neitherProvidedEpub);
				return false;
			}

			if (keepStartInclusive is null) {
				Yellow(_startPageNotSpecified);
				keepStartInclusive = 1;
			}

			if ((keepStartInclusive is not null and < 0) || (keepEndInclusive is not null and < 0)) {
				Red(_negativePageNumbersInvalid);
				return false;
			}

			FileInfo fileInfo = new(epubInPath);
			if (!fileInfo.Exists) {
				Red(_inputEpubPathInvalid);
				return false;
			} else {
				if (fileInfo.Extension == _epub) {
					LightBlue(_foundValidDocument);
				} else {
					Red(_expectedEpubExtension);
					return false;
				}
			}

			if (epubOutPath == null) {
				Yellow(_unspecifiedOutputEpubPath);
			}

			Green(_openingEpub);
			using ZipArchive inZip = ZipFile.OpenRead(epubInPath);

			LightBlue(_extractingContainer);
			ZipArchiveEntry containerEntry = inZip.GetEntry(_containerXmlPath) ?? throw new InvalidOperationException(_error_missingContainerXmlPath);
			XDocument containerXml;
			using (Stream s = containerEntry.Open()) {
				containerXml = XDocument.Load(s);
			}

			XNamespace cns = _cns;
			Green(_searchingForOpf);
			string? opfPath = containerXml.Descendants(cns + _rootfile)
							.Attributes(_fullpath)
							.Select(a => a.Value)
							.FirstOrDefault();


			if (opfPath == null) {
				Red(_error_couldntLocateOpf);
				return false;
			}

			LightBlue(_opfPathFound);
			Green(_attemptingToRetrieveOPF);
			ZipArchiveEntry? opfEntry = inZip.GetEntry(opfPath!);

			if (opfEntry == null) {
				Red($"{_error_opfMissingPrefix}{opfPath}");
				return false;
			}
			LightBlue(_opfElementFound);

			XDocument opf;
			using (Stream s = opfEntry.Open()) {
				opf = XDocument.Load(s);
			}
			Purple(_opfLoaded);
			Yellow(_ifOpfNamespaceAvailable);
			XNamespace opfNs = opf.Root?.Name.Namespace ?? XNamespace.None;

			if (opfNs == XNamespace.None) {
				Orange(_opfNamespaceNotFound);
			}

			Teal(_epubSpineExplanation);
			Green(_searchingDescendents);
			XElement? spine = opf.Descendants($"{opfNs}{_spine}").FirstOrDefault();

			if (spine == null) {
				Red(_error_opfMissingSpine);
				return false;
			}
			LightBlue(_foundSpine);
			Green(_collectingReferencesWithinSpine);
			List<XElement> itemrefs = spine.Elements($"{opfNs}{_itemRef}").ToList();
			int spineCount = itemrefs.Count;

			if (itemrefs.Count == 0) {
				Red(_opfSpineHasNoItemRefEntries);
				return false;
			}

			LightBlue($"{_foundReferencesInManifest}{itemrefs.Count}");

			keepStartInclusive = Math.Max(0, keepStartInclusive!.Value);
			keepEndInclusive = Math.Min(itemrefs.Count - 1, keepEndInclusive!.Value);
			if (keepStartInclusive > keepEndInclusive) {
				Red(_greaterThanEndIssue);
				return false;
			}

			Green(_replacingSpineWithRange);
			Pink(_epubExplanationPreface);
			Teal(_epubExplanation);
			List<XElement> kept = itemrefs.Skip((int)keepStartInclusive).Take((int)(keepEndInclusive - keepStartInclusive + 1)).ToList();
			spine.Elements(opfNs + _itemRef).Remove();
			foreach (XElement? ir in kept) {
				spine.Add(ir);
			}

			if (epubOutPath == null) {
				epubOutPath = $"{fileInfo.FullName.TrimEnd(_epub)}{_epubTrimmed}";
				Yellow($"{_sinceTheOutputPathWasntSpecified}{epubOutPath}");
			}

			if (File.Exists(epubOutPath)) {
				Orange(_epubGettingOverwritten);
				File.Delete(epubOutPath);
			}
			try {
				using ZipArchive outZip = ZipFile.Open(epubOutPath, ZipArchiveMode.Create);

				foreach (ZipArchiveEntry entry in inZip.Entries) {
					ZipArchiveEntry outEntry = outZip.CreateEntry(entry.FullName, CompressionLevel.Optimal);
					using Stream outStream = outEntry.Open();

					if (string.Equals(entry.FullName, opfPath, StringComparison.OrdinalIgnoreCase)) {
						using StreamWriter writer = new(outStream);
						opf.Save(writer);
					} else {
						using Stream inStream = entry.Open();
						inStream.CopyTo(outStream);
					}
				}
			} catch (Exception e) {
				Red($"{_error_saving}\"{e.Message}\"");
				return false;
			}
			return true;

		}
	}

}
