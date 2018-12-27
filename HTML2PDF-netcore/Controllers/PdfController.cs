using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HTML2PDF_netcore.Model;
using iText.Html2pdf;
using iText.Html2pdf.Attach.Impl;
using iText.IO.Font;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Font;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HTML2PDF_netcore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        #region Fields
        public IConfiguration Configuration;
        public IHostingEnvironment Host;
        OperatingSystem osInfo = Environment.OSVersion;
        #endregion

        public PdfController(IConfiguration Configuration, IHostingEnvironment Host)
        {
            this.Configuration = Configuration;
            this.Host = Host;
        }

        [Route("create")]
        public IActionResult Create(string pdfHtmlString, string saveName)
        {
            #region Parameters
            // Global
            string fontFamily = Configuration["PdfConfig:GlobalConfig:FontFamily"];
            // PathConfig
            string descPath = Configuration["PdfConfig:PathConfig:DescPath"];
            string logPath = Configuration["PdfConfig:PathConfig:LogPath"];
            // MetaData
            string title = Configuration["PdfConfig:MetaData:Title"];
            string author = Configuration["PdfConfig:MetaData:Author"];
            string creator = Configuration["PdfConfig:MetaData:Creator"];
            string keywords = Configuration["PdfConfig:MetaData:Keywords"];
            string subject = Configuration["PdfConfig:MetaData:Subject"];
            // Header
            string headerText = Configuration["PdfConfig:Header:Text"];
            float headerFontSize = Convert.ToSingle(Configuration["PdfConfig:Header:FontSize"]);
            string headerFontColor = Configuration["PdfConfig:Header:FontColor"];
            string headerImageSource = Configuration["PdfConfig:Header:Image:Source"];
            float headerImageWidth = Convert.ToSingle(Configuration["PdfConfig:Header:Image:Width"]);
            float headerImagePositionX = Convert.ToSingle(Configuration["PdfConfig:Header:Image:Position:Left"]);
            float headerImagePositionY = Convert.ToSingle(Configuration["PdfConfig:Header:Image:Position:Top"]);
            // Footer
            string footerText = Configuration["PdfConfig:Footer:Text"];
            double footerFontSize = Convert.ToDouble(Configuration["PdfConfig:Footer:FontSize"]);
            string footerFontColor = Configuration["PdfConfig:Footer:FontColor"];
            string footerImageSource = Configuration["PdfConfig:Footer:Image:Source"];
            float footerImageWidth = Convert.ToSingle(Configuration["PdfConfig:Footer:Image:Width"]);
            float footerImagePositionX = Convert.ToSingle(Configuration["PdfConfig:Footer:Image:Position:Left"]);
            float footerImagePositionY = Convert.ToSingle(Configuration["PdfConfig:Footer:Image:Position:Top"]);
            #endregion

            #region Properties & Setting
            ConverterProperties properties = new ConverterProperties();
            string resources = Host.WebRootPath + (osInfo.Platform == PlatformID.Unix ? "/src/font/" : "\\src\\font\\");
            string desc = osInfo.Platform == PlatformID.Unix ?
                descPath + saveName :
                "D:\\Pdf\\" + "TEST_" + DateTime.Now.ToString("yyyyMMdd-hhmmss-ffff") + ".pdf";

            FontProvider fp = new FontProvider();
            // Add Standard fonts libs without chinese support
            fp.AddStandardPdfFonts();
            fp.AddDirectory(resources);
            properties.SetFontProvider(fp);
            // Set base uri to resource folder
            properties.SetBaseUri(resources);

            PdfWriter writer = new PdfWriter(desc);
            PdfDocument pdfDoc = new PdfDocument(writer);
            // Set PageSize
            pdfDoc.SetDefaultPageSize(PageSize.A4);
            pdfDoc.GetCatalog().SetLang(new PdfString("UTF-8"));

            //Set the document to be tagged
            pdfDoc.SetTagged();
            pdfDoc.GetCatalog().SetViewerPreferences(new PdfViewerPreferences().SetDisplayDocTitle(true));

            //https://developers.itextpdf.com/content/itext-7-examples/converting-html-pdf/pdfhtml-header-and-footer-example
            //Create event-handlers
            PdfFont sourcehansanscn = PdfFontFactory.CreateFont(resources + fontFamily, PdfEncodings.IDENTITY_H);
            Dictionary<string, object> header = new Dictionary<string, object>()
            {
                { "text", headerText },
                { "fontSize", headerFontSize },
                { "fontColor", headerFontColor },
                { "source", Host.WebRootPath + headerImageSource },
                { "width", headerImageWidth },
                { "left", headerImagePositionX },
                { "top", headerImagePositionY }
            };
            Dictionary<string, object> footer = new Dictionary<string, object>()
            {
                { "text", footerText },
                { "fontSize", footerFontSize },
                { "fontColor", footerFontColor },
                { "source", Host.WebRootPath + footerImageSource },
                { "width", footerImageWidth },
                { "left", footerImagePositionX },
                { "top", footerImagePositionY }
            };
            PdfHeader headerHandler = new PdfHeader(header, sourcehansanscn);
            PdfFooter footerHandler = new PdfFooter(footer, sourcehansanscn);

            //Assign event-handlers
            pdfDoc.AddEventHandler(PdfDocumentEvent.START_PAGE, headerHandler);
            pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, footerHandler);

            //Setup custom tagworker factory for better tagging of headers
            DefaultTagWorkerFactory tagWorkerFactory = new DefaultTagWorkerFactory();
            properties.SetTagWorkerFactory(tagWorkerFactory);

            // https://developers.itextpdf.com/content/itext-7-converting-html-pdf-pdfhtml/chapter-4-creating-reports-using-pdfhtml
            // we used the createStandardHandler() method to create a standard handler. This means that pdfHTML will look for <h1>, <h2>, <h3>, <h4>, <h5>, and <h6>. The bookmarks will be created based on the hierarchy of those tags in the HTML file. 
            OutlineHandler outlineHandler = OutlineHandler.CreateStandardHandler();
            properties.SetOutlineHandler(outlineHandler);

            //书签 bookmark
            //https://developers.itextpdf.com/content/itext-7-examples/itext-7-bookmarks-tocs/toc-first-page
            //https://developers.itextpdf.com/content/best-itext-questions-stackoverview/actions-and-annotations/itext7-how-create-hierarchical-bookmarks
            //https://developers.itextpdf.com/content/itext-7-building-blocks/chapter-6-creating-actions-destinations-and-bookmarks
            //https://developers.itextpdf.com/examples/actions-and-annotations/clone-named-destinations
            //PdfOutline outline = null;
            //outline = CreateOutline(outline, pdfDoc, "bookmark", "bookmark");

            //Set meta tags
            PdfDocumentInfo pdfMetaData = pdfDoc.GetDocumentInfo();
            pdfMetaData.SetTitle(title);
            pdfMetaData.SetAuthor(author);
            pdfMetaData.AddCreationDate();
            pdfMetaData.GetProducer();
            pdfMetaData.SetCreator(creator);
            pdfMetaData.SetKeywords(keywords);
            pdfMetaData.SetSubject(subject);
            #endregion

            Document document =
                HtmlConverter.ConvertToDocument(pdfHtmlString, pdfDoc, properties);

            document.Close();

            return Content("SUCCESS");
        }
    }
}