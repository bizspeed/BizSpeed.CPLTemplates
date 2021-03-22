using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPLTemplateTests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
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
                Assert.IsTrue(cpl.StartsWith("! U1 setvar \"device.languages\" \"line_print\""));
            }
            catch (Exception ex)
            {
                Assert.Fail($"Translation failed: {ex}");
            }
        }

        [TestMethod]
		public void Valid4InchReceipt_WhenTranslated_ReturnsValidCPCL()
		{
			var receiptText = File.ReadAllText(Path.Combine(System.Environment.CurrentDirectory, "receipt4.xml"));
			TranslateDocument(receiptText);

		}
		[TestMethod]
		public void Valid4InchReceiptWithGridHavingNoRows_WhenTranslated_ReturnsValidCPCL()
		{
			var receiptText = File.ReadAllText(Path.Combine(System.Environment.CurrentDirectory, "receipt_norows.xml"));
			TranslateDocument(receiptText);

		}
		[TestMethod]
		public void DocumentWithSpecialCharacters_WhenTranslated_ReturnsValidCPCL()
        {
			var receiptText = File.ReadAllText(Path.Combine(System.Environment.CurrentDirectory, "SpecialCharacters.xml"));
			TranslateDocument(receiptText);

		}
		[TestMethod]
		public void DocumentWithVersion_WhenTranslated_ReturnsValidCPCL()
        {
			var receiptText = File.ReadAllText(Path.Combine(System.Environment.CurrentDirectory, "docversion.xml"));
			TranslateDocument(receiptText);
		}

		[TestMethod]
		public void DocumentWithAlignment_WhenTranslated_ReturnsValidCPCL()
		{
			var receiptText = File.ReadAllText(Path.Combine(System.Environment.CurrentDirectory, "textalignment.xml"));
			TranslateDocument(receiptText);
		}
	}
}
