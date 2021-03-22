using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace CPLTemplates.Test
{
	public class UnitTest1
	{
		[Fact]
		public void Valid3InchReceipt_WhenTranslated_ReturnsValidCPCL()
		{
			var receiptText = File.ReadAllText(Path.Combine(System.Environment.CurrentDirectory, "receipt.xml"));
			TranslateDocument(receiptText);
		}

		private static void TranslateDocument(string receiptText)
		{
			var translator = new BizSpeed.CPLTemplates.XmlToCPLParser();

			try
			{
				var cpl = translator.Translate(receiptText);
				Debug.WriteLine(cpl);
				Assert.StartsWith("! U1 setvar \"device.languages\" \"line_print\"", cpl);
			}
			catch (Exception ex)
			{
				Assert.True(false, $"Translation failed: {ex}");
			}
		}

		[Fact]
		public void Valid4InchReceipt_WhenTranslated_ReturnsValidCPCL()
		{
			var receiptText = File.ReadAllText(Path.Combine(System.Environment.CurrentDirectory, "receipt4.xml"));
			TranslateDocument(receiptText);

		}
		[Fact]
		public void Valid4InchReceiptWithGridHavingNoRows_WhenTranslated_ReturnsValidCPCL()
		{
			var receiptText = File.ReadAllText(Path.Combine(System.Environment.CurrentDirectory, "receipt_norows.xml"));
			TranslateDocument(receiptText);

		}
		[Fact]
		public void DocumentWithSpecialCharacters_WhenTranslated_ReturnsValidCPCL()
		{
			var receiptText = File.ReadAllText(Path.Combine(System.Environment.CurrentDirectory, "SpecialCharacters.xml"));
			TranslateDocument(receiptText);

		}
		[Fact]
		public void DocumentWithVersion_WhenTranslated_ReturnsValidCPCL()
		{
			var receiptText = File.ReadAllText(Path.Combine(System.Environment.CurrentDirectory, "docversion.xml"));
			TranslateDocument(receiptText);
		}

		[Fact]
		public void DocumentWithAlignment_WhenTranslated_ReturnsValidCPCL()
		{
			var receiptText = File.ReadAllText(Path.Combine(System.Environment.CurrentDirectory, "textalignment.xml"));
			TranslateDocument(receiptText);
		}

	}
}
