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
using System.Linq;
using System.Threading.Tasks;

namespace HTML2PDF_netcore.Model
{
    /// <summary>
    /// Footer message of PDF file | PDF 页脚处理器
    /// 
    /// [itext-7 examples](<seealso cref="https://developers.itextpdf.com/content/itext-7-examples/converting-html-pdf/pdfhtml-header-and-footer-example"/>)
    /// </summary>
    public class PdfFooter : IEventHandler
    {
        Dictionary<string, object> footer;
        PdfFont pdfFont;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="footer">
        /// Key-Value pair collection, Footer body. 
        /// Keys includes: text, fontSize, source, width, left and top.
        /// The first two properties are for Footer's Text.
        /// The following four properties are for Footer's Image, LOGO as default.
        /// </param>
        /// <param name="pdfFont">Specific the font-family of text</param>
        public PdfFooter(Dictionary<string, object> footer, PdfFont pdfFont)
        {
            this.footer = footer;
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
            canvas.SetFontSize(Convert.ToSingle(footer["fontSize"]));
            canvas.SetFont(pdfFont);
            // Write image
            string IMG = footer["source"].ToString();
            ImageData img = ImageDataFactory.Create(IMG);
            iText.Layout.Element.Image imgModel = new iText.Layout.Element.Image(img);
            imgModel.SetWidth(Convert.ToSingle(footer["width"]));
            imgModel.SetMarginLeft(Convert.ToSingle(footer["left"]));
            imgModel.SetMarginTop(Convert.ToSingle(footer["top"]));
            canvas.Add(imgModel);
            // Write text at position
            // TODO: change the text content for display
            string displayText = footer["text"] + " - " + pdf.GetPageNumber(page);
            canvas.ShowTextAligned(displayText,
                    pageSize.GetWidth() / 2,
                    pageSize.GetBottom() + 30, TextAlignment.CENTER);
        }
    }
}
