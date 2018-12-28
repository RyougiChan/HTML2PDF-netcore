using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

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
            // Retrieve document | 获取 PDF 文档
            PdfDocumentEvent docEvent = (PdfDocumentEvent)evt;
            PdfDocument pdf = docEvent.GetDocument();
            PdfPage page = docEvent.GetPage();
            // Header is generated at the very start of a page generation | 页头生成的时候位于页面生成最先
            // So `pdfTotalPageNumber == currentPageNumber` will be `true` forever | 故 `pdfTotalPageNumber == currentPageNumber` 恒成立
            int pdfTotalPageNumber = pdf.GetNumberOfPages();
            int currentPageNumber = pdf.GetPageNumber(page);

            // do not add header on cover | 封面不加头部
            if (currentPageNumber == 1)
            {
                return;
            }

            // Width of text in header | 文字宽度
            float textWidth = pdfFont.GetWidth(header["text"].ToString(), Convert.ToSingle(header["fontSize"]));
            int[] rgb = header["fontColor"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x.Trim())).ToArray();

            //// Doc parameters | 文档参数
            Rectangle pageSize = page.GetPageSize();
            PdfCanvas pdfCanvas = new PdfCanvas(
                    page.GetLastContentStream(), page.GetResources(), pdf);
            Canvas canvas = new Canvas(pdfCanvas, pdf, pageSize);
            canvas.SetFontSize(Convert.ToSingle(header["fontSize"]))
                .SetFont(pdfFont)
                .SetFontColor(new DeviceRgb(rgb[0], rgb[1], rgb[2]));

            // LOGO
            string IMG = header["source"].ToString();
            ImageData img = ImageDataFactory.Create(IMG);
            Image imgModel = new Image(img);
            imgModel.SetWidth(Convert.ToSingle(header["width"]));

            // Setup text/LOGO and position 文字/LOGO及位置
            string headerText = header["text"].ToString();
            float[] headerTextPos = new float[2];
            float[] headerImgPos = new float[2];
            TextAlignment alignment = TextAlignment.CENTER;

            if (currentPageNumber % 2 == 0)
            {
                // Align left | 左对齐
                headerTextPos[0] = Convert.ToSingle(header["left"]) + Convert.ToSingle(header["width"]) + 4.5f;
                headerImgPos[0] = Convert.ToSingle(header["left"]);
                alignment = TextAlignment.LEFT;
            }
            else
            {
                // Align right | 右对齐
                headerTextPos[0] = pageSize.GetWidth() - 36;
                headerImgPos[0] = pageSize.GetWidth() - Convert.ToSingle(header["left"]) - textWidth - Convert.ToSingle(header["width"]);
                alignment = TextAlignment.RIGHT;
            }
            headerTextPos[1] = pageSize.GetTop() - 37;
            headerImgPos[1] = Convert.ToSingle(header["top"]);

            imgModel.SetMarginLeft(headerImgPos[0]);
            imgModel.SetMarginTop(headerImgPos[1]);
            canvas.Add(imgModel);

            canvas.ShowTextAligned(headerText, headerTextPos[0], headerTextPos[1], alignment);
        }
    }
}
