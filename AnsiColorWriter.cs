using static EpubCleaner.Constants;
namespace EpubCleaner {
	/// <summary>
	/// Provides static methods for writing text to the console in various ANSI colors using predefined color schemes.
	/// </summary>
	/// <remarks>The methods in this class output colored text using ANSI escape codes, which are supported by most
	/// modern terminals. If the console does not support ANSI escape codes, the output may not appear as intended. All
	/// methods are thread-safe and do not modify global console color state beyond the written text.</remarks>
	public static class AnsiColorWriter {
		public static void Green(string text, bool newLine = true) => WriteWithColor(text, 205, 239, 196, newLine);
		public static void Grey(string text, bool newLine = true) => WriteWithColor(text, 229, 229, 229, newLine);
		public static void LightBlue(string text, bool newLine = true) => WriteWithColor(text, 214, 230, 255, newLine);
		public static void LightIndigo(string text, bool newLine = true) => WriteWithColor(text, 224, 229, 248, newLine);
		public static void LightRed(string text, bool newLine = true) => WriteWithColor(text, 254, 184, 171, newLine);
		public static void Orange(string text, bool newLine = true) => WriteWithColor(text, 254, 223, 208, newLine);
		public static void Pink(string text, bool newLine = true) => WriteWithColor(text, 253, 221, 227, newLine);
		public static void Purple(string text, bool newLine = true) => WriteWithColor(text, 236, 225, 249, newLine);
		public static void Red(string text, bool newLine = true) => WriteWithColor(text, 234, 0, 30, newLine);
		public static void Teal(string text, bool newLine = true) => WriteWithColor(text, 172, 243, 228, newLine);
		public static void Yellow(string text, bool newLine = true) => WriteWithColor(text, 249, 227, 182, newLine);

		private static string RgbForeground(int r, int g, int b) =>
			$"\u001b[38;2;{r};{g};{b}m";

		private static string RgbBackground(int r, int g, int b) =>
			$"\u001b[48;2;{r};{g};{b}m";

		private static void WriteWithColor(string text, int r, int g, int b, bool newLine = true) {
			string coloredText = $"{RgbForeground(r, g, b)}{text}{_ansiReset}";
			if (newLine)
				Console.WriteLine(coloredText);
			else
				Console.Write(coloredText);
		}
	}
}
