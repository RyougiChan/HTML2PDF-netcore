using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Colorspace;
using iText.Layout;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

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
            // Retrieve document | 获取 PDF 文档
            PdfDocumentEvent docEvent = (PdfDocumentEvent)evt;
            PdfDocument pdf = docEvent.GetDocument();
            PdfPage page = docEvent.GetPage();
            int pdfTotalPageNumber = pdf.GetNumberOfPages();
            int currentPageNumber = pdf.GetPageNumber(page);

            // Do not add footer on cover and back cover | 封面和封底不加
            //if (currentPageNumber == 1 || currentPageNumber == pdfTotalPageNumber)
            //{
            //    return;
            //}

            // Width of text in footer | 文字宽度
            float textWidth = pdfFont.GetWidth(footer["text"].ToString(), Convert.ToSingle(footer["fontSize"]));
            // Color of text | 文字颜色
            int[] rgb = footer["fontColor"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x.Trim())).ToArray();

            //// Doc parameters 文档参数
            Rectangle pageSize = page.GetPageSize();
            PdfCanvas pdfCanvas = new PdfCanvas(
                    page.GetLastContentStream(), page.GetResources(), pdf);
            Canvas canvas = new Canvas(pdfCanvas, pdf, pageSize);
            canvas.SetFontSize(Convert.ToSingle(footer["fontSize"]))
                .SetFont(pdfFont)
                .SetFontColor(new DeviceRgb(rgb[0], rgb[1], rgb[2]));

            // Footer text/page NO. and position | 文字/页码及位置
            string footerText = footer["text"].ToString();
            string footerPageNum = (currentPageNumber - 1 < 10 ? "0" : "") + (currentPageNumber - 1);
            float[] footerTextPos = new float[2];
            float[] footerPageNumPos = new float[2];
            float[] roundRectPos = new float[2];
            TextAlignment alignment = TextAlignment.CENTER;

            // Draw gradient background | 渐变背景
            PdfShading.Axial axial;

            if (currentPageNumber % 2 == 0)
            {
                // Align left in even number page, which page NO. is a odd | 偶数页左对齐(页码为奇数)
                footerTextPos[0] = Convert.ToSingle(footer["left"]);
                footerTextPos[1] = pageSize.GetBottom() + 19.7f;
                footerPageNumPos[0] = textWidth + Convert.ToSingle(footer["left"]) + 9.5f;
                footerPageNumPos[1] = pageSize.GetBottom() + 21.6f;
                roundRectPos[0] = textWidth + Convert.ToSingle(footer["left"]) + 4.0f;
                roundRectPos[1] = pageSize.GetBottom() + 22.0f;

                alignment = TextAlignment.LEFT;
                axial = new PdfShading.Axial(new PdfDeviceCs.Rgb(), 0, pageSize.GetHeight() - 30, DeviceRgb.WHITE.GetColorValue(), pageSize.GetWidth(), pageSize.GetHeight() - 30, new DeviceRgb(190, 230, 255).GetColorValue());
            }
            else
            {
                // Align right in odd number page, which page NO. is a even |奇数页右对齐(页码为偶数)
                footerTextPos[0] = pageSize.GetWidth() - Convert.ToSingle(footer["left"]) - 24.0f;
                footerTextPos[1] = pageSize.GetBottom() + 19.7f;
                footerPageNumPos[0] = pageSize.GetWidth() - Convert.ToSingle(footer["left"]) - 5.5f;
                footerPageNumPos[1] = pageSize.GetBottom() + 21.6f;
                roundRectPos[0] = pageSize.GetWidth() - Convert.ToSingle(footer["left"]) - 20.0f;
                roundRectPos[1] = pageSize.GetBottom() + 22.0f;

                alignment = TextAlignment.RIGHT;
                axial = new PdfShading.Axial(new PdfDeviceCs.Rgb(), 0, pageSize.GetHeight() - 30, new DeviceRgb(190, 230, 255).GetColorValue(), pageSize.GetWidth(), pageSize.GetHeight() - 30, DeviceRgb.WHITE
                    .GetColorValue());
            }

            // Draw gradient background and round corner rect | 渐变背景和圆角矩形
            PdfPattern.Shading pattern = new PdfPattern.Shading(axial);
            pdfCanvas.SetFillColorShading(pattern)
                .Rectangle(0, pageSize.GetBottom() + 14.0f, pageSize.GetWidth(), 24.0f)
                .Fill()
                .RoundRectangle(roundRectPos[0], roundRectPos[1], 20.0f, 9.0f, 2.0f)
                .SetFillColor(new DeviceRgb(18, 98, 170))
                .Fill();

            // Draw text
            canvas.ShowTextAligned(footerText, footerTextPos[0], footerTextPos[1], alignment);

            Canvas canvas2 = new Canvas(pdfCanvas, pdf, pageSize);
            canvas2.SetFontSize(7.2f);
            canvas2.SetFont(pdfFont);
            canvas2.SetFontColor(DeviceRgb.WHITE);
            // Draw page NO.
            canvas2.ShowTextAligned(footerPageNum, footerPageNumPos[0], footerPageNumPos[1], alignment);
        }
    }
}
