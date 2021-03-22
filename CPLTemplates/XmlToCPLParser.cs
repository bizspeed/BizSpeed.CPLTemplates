using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using BizSpeed.CPLTemplates.Extensions;
using SkiaSharp;

namespace BizSpeed.CPLTemplates
{
    public class XmlToCPLParser : ICPLTranslator
    {
        private readonly string CR = "\r\n";
        private readonly int DEFAULT_PAGE_WIDTH = 4;
        private string DEFAULT_FONT = "! U1 SETLP 7 0 24";
        private string H1_FONT = "! U1 SETLP 4 0 47";
        private decimal DEFAULT_FONT_WIDTH_IN_DOTS = 12;
        private int PAGE_WIDTH;
        private int LEFT_MARGIN = 15;
        int MAX_CHARACTERS;

        public XmlToCPLParser()
        {

        }

        private string CleanXml(string xmlToClean)
        {
            return xmlToClean.Replace("\r", "").Replace("\n", "").Replace("\t", "");
        }

        public string Translate(string xmlToParse)
        {
            var cleanedXml = CleanXml(xmlToParse);

            XDocument xmlDoc = XDocument.Load(new StringReader(cleanedXml), LoadOptions.None);

            if (!ValidateXml(xmlDoc, out string validationMessage))
            {
                throw new FormatException(validationMessage);
            }

            Debug.WriteLine("Print XML: {0}", xmlDoc.ToString());
            var sb = new StringBuilder();

            // The first element should be the doc element
            XElement docElement = xmlDoc.Elements().FirstOrDefault(e => e.Name.ToString().ToLower() == "doc");

            if (docElement == null)
            {
                throw new InvalidDataException("The root element was not <doc>");
            }

            var pageWithAttribute = docElement.Attribute("pagewidth");
            if (!Int32.TryParse(pageWithAttribute?.Value ?? "4", out int pageWidth))
                pageWidth = DEFAULT_PAGE_WIDTH;

            var marginAttribute = docElement.Attribute("margin");
            if (!Int32.TryParse(marginAttribute?.Value ?? "15", out int margin))
                margin = 15;

            InitPageSettings(Math.Abs(pageWidth), Math.Abs(margin));

            sb.Append(WriteDoc());

            foreach (XElement element in docElement.Elements())
            {
                var elementName = element.Name.ToString().ToLower();

                if (elementName == "img")
                {
                    var sourceAttr = element.Attribute("source"); // a Base64 encoded string of the image

                    byte[] imageData;

                    if (sourceAttr == null)
                    {
                        continue;
                    }

                    imageData = Convert.FromBase64String(sourceAttr.Value);

                    if (imageData.Length == 0)
                        continue;

                    var align = EvaluateAlignment(element.Attribute("align"), "center");

                    var widthAttr = element.Attribute("width"); // expressed as a percentage of the total page width
                    var widthVal = widthAttr?.Value;
                    var width = Math.Round(Convert.ToInt32(widthVal ?? "100") / 100.0f, 2);

                    sb.Append(WriteImage(imageData, align, width));
                    continue;
                }

                if (elementName == "section")
                {
                    sb.Append(WriteFormattedTextSection(element));
                    continue;
                }

                if (elementName == "br")
                {
                    sb.Append(CR);
                    continue;
                }

                if (elementName == "text")
                {
                    int lineWidth = MAX_CHARACTERS;

                    var align = EvaluateAlignment(element.Attribute("align"), "left");

                    int indent = 0;

                    switch (align)
                    {
                        case "left":
                            indent = 0;
                            sb.Append(WebUtility.HtmlDecode(element.Value));
                            break;
                        case "right":
                            indent = lineWidth - element.Value.Length;
                            sb.Append(WebUtility.HtmlDecode(element.Value).PadLeft(lineWidth));
                            break;
                        case "center":
                            indent = Convert.ToInt32((lineWidth - element.Value.Length) / 2);
                            sb.Append(WebUtility.HtmlDecode(element.Value).PadLeft(indent + element.Value.Length));
                            break;
                        default:
                            sb.Append(WebUtility.HtmlDecode(element.Value));
                            break;
                    }

                    continue;
                }

                if (elementName == "b")
                {
                    sb.Append(WriteB(element.Value));
                    continue;
                }

                if (elementName == "h1")
                {
                    sb.Append(WriteH1(element.Value));
                    continue;
                }

                if (elementName == "line")
                {

                    var lineWidth = PAGE_WIDTH * 203;

                    if (element.Attributes().Any(a => a.Name == "width"))
                    {
                        XAttribute width = element.Attribute("width");
                        int.TryParse(width.Value, out lineWidth);
                    }

                    sb.Append(WriteLine(lineWidth));

                    //sb.Append(WriteTextLine());
                    continue;
                }

                if (elementName == "textline")
                {
                    int lineWidth = MAX_CHARACTERS;
                    string lineCharacter = "-";

                    if (element.Attributes().Any(a => a.Name == "width"))
                    {
                        XAttribute width = element.Attribute("width");
                        int.TryParse(width.Value, out lineWidth);
                    }

                    if (element.Attributes().Any(a => a.Name == "character"))
                    {
                        XAttribute character = element.Attribute("character");
                        lineCharacter = character.Value;
                    }

                    sb.Append(WriteTextLine(lineWidth, lineCharacter));
                    continue;
                }

                if (elementName == "grid")
                {
                    sb.Append(WriteGrid(element));
                }
            }

            sb.Append(CR);
            sb.Append(WriteDocEnd()).Append(CR);
            Debug.WriteLine(sb.ToString());
            return sb.ToString();
        }

        private string EvaluateAlignment(XAttribute alignAttr, string defaultAlignment = "left")
        {
            var alignVal = alignAttr?.Value;
            var align = (alignVal ?? defaultAlignment).ToLower();

            if (align != "left" && align != "center" && align != "right")
                align = defaultAlignment;

            return align;
        }

        private void InitPageSettings(int pageWidth, int margin)
        {
            PAGE_WIDTH = pageWidth;
            LEFT_MARGIN = margin;
            // TODO: Uncomment if "scaling" font to fit smaller page
            //if (pageWidth == 3)
            //{
            //    DEFAULT_FONT = "! U1 SETLP 0 0 9";
            //    H1_FONT = "! U1 SETLP 7 0 24";
            //    DEFAULT_FONT_WIDTH_IN_DOTS = 8;
            //}

            MAX_CHARACTERS = Convert.ToInt32(Math.Floor(((pageWidth * 203) - margin) / DEFAULT_FONT_WIDTH_IN_DOTS)) - 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageData"></param>
        /// <param name="align">'left' or 'center'</param>
        /// <param name="width">expressed as a percentage of the total usable page width</param>
        /// <returns></returns>
        private string WriteImage(byte[] imageData, string align, double width)
        {
            var imageWidthInPixels = (((PAGE_WIDTH * 203) - LEFT_MARGIN - 60) * width) * .95;

            return GenerateImageString(imageData, align, Convert.ToInt32(imageWidthInPixels));
        }

        private string GenerateImageString(byte[] imageBytes, string align = "left", int imageWidth = 320)
        {
            SKBitmap sigImg = null;

            var image = SKBitmap.Decode(imageBytes);
            var resizedImg = Resize(image, imageWidth);

            if (resizedImg != null)
                sigImg = RemoveTransparency(resizedImg);

            if (sigImg != null)
            {
                var bitmap = sigImg;
                var bitmapWidth = bitmap.Width.NextClosestDivisibleBy(8);

                var printCommand = new StringBuilder("! U1 END-PAGE").Append(CR);
                printCommand.Append($"! 0 200 200 {bitmap.Height} 1").Append(CR);

                var x = 35;

                if (align == "center")
                {
                    x = Convert.ToInt32(Math.Round(((PAGE_WIDTH * 203f) - LEFT_MARGIN - 60 - bitmap.Width) / 2, 0));
                }

                printCommand.Append($"EG {bitmapWidth / 8} {bitmap.Height} {x} 0 ");

                for (int row = 0; row < bitmap.Height; row++)
                {
                    var rasterLine = new StringBuilder();

                    for (int i = 0; i < bitmapWidth; i++)
                    {
                        if (i < bitmap.Width)
                        {
                            var pixel = bitmap.GetPixel(i, row);
                            if (pixel == SKColors.White)
                                rasterLine.Append("0");
                            else
                                rasterLine.Append("1");
                        }
                        else
                        {
                            rasterLine.Append("0");
                        }
                    }

                    var hexArray = rasterLine.ToString().ToHex();
                    printCommand.Append(String.Join("", hexArray.ToArray()));

                    //var charArray = hexArray.HexToChar().ToArray();
                    //var cgRow = String.Join("", charArray);
                    //printCommand.Append(cgRow);

                    //var byteArray = rasterLine.ToString().ToByte().ToArray();
                    //var charArray = Encoding.ASCII.GetString(byteArray);
                    //printCommand.Append(charArray);
                }

                printCommand.Append(CR);

                printCommand.Append("PRINT").Append(CR);

                printCommand.Append("! U1 BEGIN-PAGE").Append(CR);
                printCommand.Append(DEFAULT_FONT).Append(CR);

                var ret = printCommand.ToString();
                return ret;
            }

            return CR;
        }

        private SKBitmap Resize(SKBitmap image, int newWidth)
        {
            try
            {
                var original = image;

                int newHeight;

                if (original.Width > newWidth)
                {
                    var width = original.Width;
                    var height = original.Height;
                    var ratio = (float)newWidth / (float)width;
                    newHeight = (int)(ratio * height);
                }
                else
                {
                    newWidth = original.Width;
                    newHeight = original.Height;
                }

                var resized = original.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);

                return resized;
            }
            catch
            {
                return image;
            }
        }

        private SKBitmap RemoveTransparency(SKBitmap original)
        {
            // create a new bitmap with the same dimensions
            // also avoids the first copy if the color type is index8
            var copy = new SKBitmap(original.Width, original.Height);

            // we need a canvas to draw
            var canvas = new SKCanvas(copy);

            // clear the bitmap with the desired color for transparency
            canvas.Clear(SKColors.White);

            // draw the bitmap on top
            canvas.DrawBitmap(original, 0, 0);

            return copy;
        }

        private XmlSchemaSet GetSchemas()
        {
            var schemas = new XmlSchemaSet();
            var schemaString = LoadTemplateFromEmbeddedResource("BizSpeed.CPLTemplates.XmlToCPCL.xsd");
            schemas.Add("", XmlReader.Create(new StringReader(schemaString)));
            return schemas;
        }

        private bool ValidateXml(XDocument xml, out string message)
        {
            var schemas = GetSchemas();
            message = "";
            var errorMessage = "";
            var errors = false;

            xml.Validate(schemas, (o, e) =>
            {
                Debug.WriteLine($"Print XML is not valid: {e.Message}");
                errorMessage = e.Message;
                errors = true;
            });

            message = errorMessage;
            return !errors;
        }

        private string WriteDoc()
        {
            var sb = new StringBuilder("! U1 setvar \"device.languages\" \"line_print\"").Append(CR);
            sb.Append("! U").Append(CR);
            sb.Append("IN-DOTS").Append(CR);
            sb.Append("SETFF 0 2.5").Append(CR);
            sb.Append("PRINT").Append(CR);
            sb.Append("! 0 200 200 4 1").Append(CR);
            sb.Append("PRINT").Append(CR);
            sb.Append("! U1 BEGIN-PAGE").Append(CR);
            sb.Append("! U1 JOURNAL").Append(CR);
            sb.Append($"! U1 LMARGIN {LEFT_MARGIN}").Append(CR);
            sb.Append(DEFAULT_FONT).Append(CR);
            return sb.ToString();
        }

        private string WriteDocEnd()
        {
            var sb = new StringBuilder("! U1 END-PAGE").Append(CR);
            return sb.ToString();
        }

        private string WriteH1(string text)
        {
            var sb = new StringBuilder(H1_FONT).Append(CR);
            sb.Append(WebUtility.HtmlDecode(text)).Append(CR);
            sb.Append(DEFAULT_FONT).Append(CR);
            return sb.ToString();
        }

        private string WriteB(string text, int? width = null, string align = "left")
        {
            var cleanText = WebUtility.HtmlDecode(text);

            if (width != null)
            {
                if (cleanText.Length > width.Value)
                {
                    cleanText = cleanText.Substring(0, width.Value);
                }

                if (align == "left")
                {
                    cleanText = cleanText.PadRight(width.Value);
                }
                else
                {
                    cleanText = cleanText.PadLeft(width.Value);
                }
            }

            var sb = new StringBuilder("! U1 SETBOLD 2").Append(CR);
            sb.Append(cleanText);
            sb.Append(" ! U1 SETBOLD 0").Append(CR);
            return sb.ToString();
        }

        private string WriteBR()
        {
            return CR;
        }

        private string WriteLine(int? width = null)
        {
            var sb = new StringBuilder("! 0 200 200 4 1").Append(CR);
            sb.AppendFormat("LINE 30 0 {0} 0 4", width ?? (PAGE_WIDTH * 203)).Append(CR);
            sb.Append("PRINT").Append(CR);
            return sb.ToString();
        }

        private string WriteTextLine(int width = 65, string lineCharacter = "-")
        {
            var sb = new StringBuilder();
            for (var i = 1; i <= width; i++)
            {
                sb.Append(lineCharacter);
            }

            sb.Append(CR);
            return sb.ToString();
        }

        private string WriteFormattedTextSection(XElement element)
        {
            var sb = new StringBuilder();

            foreach (XElement item in element.Elements())
            {
                var elementName = item.Name.ToString().ToLower();

                if (elementName == "text")
                {
                    sb.Append(WritePaddedText(item.Value, MAX_CHARACTERS, EvaluateAlignment(item.Attribute("align"), "left")));
                    continue;
                }

                if (elementName == "b")
                {
                    sb.Append(WriteB(item.Value));
                    continue;
                }

                if (elementName == "br")
                {
                    sb.Append(CR);
                    continue;
                }

                if (elementName == "h1")
                {
                    sb.Append(WriteH1(item.Value));
                    continue;
                }
            }

            sb.Append(CR);
            return sb.ToString();
        }

        private string WriteGrid(XElement grid)
        {
            var sb = new StringBuilder();
            var columnWidths = new List<int>();
            var columnDefs = grid.Element("columns");

            if (columnDefs == null)
            {
                return "";
            }

            if (!columnDefs.Elements().Any(e => e.Name == "column"))
            {
                return "";
            }

            // Capture the column defs which, at the time, are simply a width value
            foreach (XElement column in columnDefs.Elements().Where(e => e.Name == "column"))
            {
                var width = 0;

                var widthAttribute = column.Attribute("width");
                if (widthAttribute != null)
                {
                    int.TryParse(widthAttribute.Value, out width);
                }

                columnWidths.Add(width);
            }

            // Grab the rows
            XElement rows = grid.Element("rows");

            if (rows == null)
            {
                return "";
            }

            var allRows = rows.Elements().Where(r => r.Name == "row");
            var index = 0;
            var maxIndex = allRows.Count() - 1;

            foreach (XElement row in rows.Elements().Where(e => e.Name == "row"))
            {
                if (index < maxIndex)
                {
                    sb.Append(WriteRow(row, columnWidths));
                }
                else
                {
                    sb.Append(WriteRow(row, columnWidths, false));
                }

                index++;
            }

            return sb.ToString();
        }

        private string WriteRow(XElement row, List<int> columnWidths, bool writeLineEnd = true)
        {

            var sb = new StringBuilder();
            var columnsExpected = columnWidths.Count;
            var index = 0;
            var elementIndex = 0;
            var cells = row.Elements().Where(e => e.Name == "cell");

            while (index < columnsExpected)
            {
                int colSpan = 1;

                try
                {
                    XElement cell = cells.ElementAt(elementIndex);

                    var colspanAttr = cell.Attributes().FirstOrDefault(a => a.Name == "colspan");

                    if (!int.TryParse(colspanAttr?.Value ?? "1", out colSpan))
                        colSpan = 1;

                    int width = 0;

                    for (int i = 0; i < colSpan; i++)
                    {
                        width += columnWidths.ElementAt(index + i);
                    }

                    var lineWidth = Convert.ToInt32(Math.Floor(DEFAULT_FONT_WIDTH_IN_DOTS * width));
                    var align = EvaluateAlignment(cell.Attribute("align"), "left");

                    if (cell.Elements().Count() == 0)
                    {
                        sb.Append(WritePaddedText(cell.Value, width, align));
                    }
                    else
                    {
                        var childElement = cell.Elements().First();

                        if (childElement.Name == "text")
                        {
                            sb.Append(WritePaddedText(childElement.Value, width, align));
                        }
                        else if (childElement.Name == "b")
                        {
                            sb.Append(WriteB(childElement.Value, width, align));
                        }
                        else if (childElement.Name == "line")
                        {
                            sb.Append(WriteLine(lineWidth));
                        }
                        else if (childElement.Name=="textline")
                        {
                            sb.Append(WriteTextLine(width, childElement.Attribute("character")?.Value ?? "-"));
                        }
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Debug.WriteLine($"Error parsing template: {ex}");
                    break;
                }

                index += colSpan;
                elementIndex++;
            }

            // In order to give more flexibility to layout, don't assume a line end must
            // be inserted at the end of each row.
            if (writeLineEnd)
            {
                sb.Append(CR);
            }
            return sb.ToString();
        }

        private string WritePaddedText(string text, int width, string align = "left")
        {
            var cleanText = WebUtility.HtmlDecode(text);

            if (cleanText.Length > width)
            {
                var substringLength = width - 1;
                if (substringLength < 0)
                {
                    substringLength = 0;
                }

                cleanText = cleanText.Substring(0, substringLength);
            }

            switch (align)
            {
                case "left":
                    return cleanText.PadRight(width);
                case "right":
                    return cleanText.PadLeft(width);
                case "center":
                    var indent = Convert.ToInt32((width - cleanText.Length) / 2);
                    var paddedText = cleanText.PadLeft(indent + cleanText.Length);
                    paddedText = paddedText.PadRight(width);
                    return paddedText;
                default:
                    return cleanText.PadRight(width);
            }
        }

        protected string LoadTemplateFromEmbeddedResource(string resourceName)
        {
            var text = string.Empty;

            var assembly = this.GetType().GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream(resourceName);

            using (var reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            return text;
        }
    }
}

