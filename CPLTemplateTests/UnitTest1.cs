using System;
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
			var translator = new BizSpeed.CPLTemplates.XmlToCPLParser();

            try
            {
				var cpl = translator.Translate(receiptText);
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
			var translator = new BizSpeed.CPLTemplates.XmlToCPLParser();

			try
			{
				var cpl = translator.Translate(receiptText);
				Assert.IsTrue(cpl.StartsWith("! U1 setvar \"device.languages\" \"line_print\""));
			}
			catch (Exception ex)
			{
				Assert.Fail($"Translation failed: {ex}");
			}
		}
		[TestMethod]
		public void Valid4InchReceiptWithGridHavingNoRows_WhenTranslated_ReturnsValidCPCL()
		{
			var receiptText = File.ReadAllText(Path.Combine(System.Environment.CurrentDirectory, "receipt_norows.xml"));
			var translator = new BizSpeed.CPLTemplates.XmlToCPLParser();

			try
			{
				var cpl = translator.Translate(receiptText);
				Assert.IsTrue(cpl.StartsWith("! U1 setvar \"device.languages\" \"line_print\""));
			}
			catch (Exception ex)
			{
				Assert.Fail($"Translation failed: {ex}");
			}
		}
		[TestMethod]
		public void DocumentWithSpecialCharacters_WhenTranslated_ReturnsValidCPCL()
        {
			var receiptText = File.ReadAllText(Path.Combine(System.Environment.CurrentDirectory, "SpecialCharacters.xml"));
			var translator = new BizSpeed.CPLTemplates.XmlToCPLParser();

			try
			{
				var cpl = translator.Translate(receiptText);
				Assert.IsTrue(cpl.StartsWith("! U1 setvar \"device.languages\" \"line_print\""));
			}
			catch (Exception ex)
			{
				Assert.Fail($"Translation failed: {ex}");
			}

		}
	}
}
