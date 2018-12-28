using iText.IO.Image;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;

namespace HTML2PDF_netcore.Model
{
    /// <summary>
    /// Header message of PDF file | PDF 页头处理器
    /// 
    /// [itext-7 examples](<seealso cref="https://developers.itextpdf.com/content/itext-7-examples/converting-html-pdf/pdfhtml-header-and-footer-example"/>)
    /// </summary>
    public class PdfHeader : IEventHandler
    {
        Dictionary<string, object> header;
        PdfFont pdfFont;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="header">
        /// Key-Value pair collection, Header body. 
        /// Keys includes: `text`, `fontSize`, `source`, `width`, `left` and `top`.
        /// The first two properties are for Header's Text.
        /// The following four properties are for Header's Image, LOGO as default.
        /// </param>
        /// <param name="pdfFont">Specific the font-family of text</param>
        public PdfHeader(Dictionary<string, object> header, PdfFont pdfFont)
        {
            this.header = header;
            this.pdfFont = pdfFont;
        }

        public void HandleEvent(Event evt)
        {
            // Retrieve document and
            PdfDocumentEvent docEvent = (PdfDocumentEvent)evt;
            PdfDocument pdf = docEvent.GetDocument();
            PdfPage page = docEvent.GetPage();
            Rectangle pageSize = page.GetPageSize();
            PdfCanvas pdfCanvas = new PdfCanvas(
                    page.GetLastContentStream(), page.GetResources(), pdf);
            Canvas canvas = new Canvas(pdfCanvas, pdf, pageSize);
            canvas.SetFontSize(Convert.ToSingle(header["fontSize"]));
            canvas.SetFont(pdfFont);
            // Write image
            string IMG = header["source"].ToString();
            ImageData img = ImageDataFactory.Create(IMG);
            iText.Layout.Element.Image imgModel = new iText.Layout.Element.Image(img);
            imgModel.SetWidth(Convert.ToSingle(header["width"]));
            imgModel.SetMarginLeft(Convert.ToSingle(header["left"]));
            imgModel.SetMarginTop(Convert.ToSingle(header["top"]));
            canvas.Add(imgModel);
            // Write text at position
            // TODO: change the text content for display
            string displayText = header["text"] + " - " + pdf.GetPageNumber(page);
            canvas.ShowTextAligned(displayText,
                        pageSize.GetWidth() - 36,
                        pageSize.GetTop() - 30, TextAlignment.RIGHT);
        }
    }
}
