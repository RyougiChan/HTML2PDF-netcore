using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HTML2PDF_netcore.Model;
using HTML2PDF_netcore.Plugins;
using iText.Html2pdf;
using iText.Html2pdf.Attach.Impl;
using iText.IO.Font;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Font;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Hosting;
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

        #region Constructor
        public PdfController(IConfiguration Configuration, IHostingEnvironment Host)
        {
            this.Configuration = Configuration;
            this.Host = Host;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// To create and save new PDF file via given HTML string.
        /// 通过给定的 HTML 字符串生成并保存 PDF 文件
        /// </summary>
        /// <param name="pdfHtmlString">The given HTML string</param>
        /// <param name="saveName">PDF saved name</param>
        /// <returns>return `SUCCESS` as success signal</returns>
        [Route("create")]
        public IActionResult Create(string pdfHtmlString, string saveName = null)
        {
            #region Parameters
            // Global Config
            string fontFamily = Configuration["PdfConfig:GlobalConfig:FontFamily"];
            // Path Config
            string descPath = Configuration["PdfConfig:PathConfig:DescPath"];
            string logPath = Configuration["PdfConfig:PathConfig:LogPath"];
            // MetaData Config
            string title = Configuration["PdfConfig:MetaData:Title"];
            string author = Configuration["PdfConfig:MetaData:Author"];
            string creator = Configuration["PdfConfig:MetaData:Creator"];
            string keywords = Configuration["PdfConfig:MetaData:Keywords"];
            string subject = Configuration["PdfConfig:MetaData:Subject"];
            // Header Config
            string headerText = Configuration["PdfConfig:Header:Text"];
            float headerFontSize = Convert.ToSingle(Configuration["PdfConfig:Header:FontSize"]);
            string headerFontColor = Configuration["PdfConfig:Header:FontColor"];
            string headerImageSource = Configuration["PdfConfig:Header:Image:Source"];
            float headerImageWidth = Convert.ToSingle(Configuration["PdfConfig:Header:Image:Width"]);
            float headerImagePositionX = Convert.ToSingle(Configuration["PdfConfig:Header:Image:Position:Left"]);
            float headerImagePositionY = Convert.ToSingle(Configuration["PdfConfig:Header:Image:Position:Top"]);
            // Footer Config
            string footerText = Configuration["PdfConfig:Footer:Text"];
            double footerFontSize = Convert.ToDouble(Configuration["PdfConfig:Footer:FontSize"]);
            string footerFontColor = Configuration["PdfConfig:Footer:FontColor"];
            string footerImageSource = Configuration["PdfConfig:Footer:Image:Source"];
            float footerImageWidth = Convert.ToSingle(Configuration["PdfConfig:Footer:Image:Width"]);
            float footerImagePositionX = Convert.ToSingle(Configuration["PdfConfig:Footer:Image:Position:Left"]);
            float footerImagePositionY = Convert.ToSingle(Configuration["PdfConfig:Footer:Image:Position:Top"]);
            #endregion

            #region Properties & Setting
            // Configure properties for converter | 配置转换器
            ConverterProperties properties = new ConverterProperties();
            // Resources path, store images/fonts for example | 资源路径, 存放如图片/字体等资源
            string resources = Host.WebRootPath + (osInfo.Platform == PlatformID.Unix ? "/src/font/" : "\\src\\font\\");
            // PDF saved path | 生成的PDF文件存放路径
            string desc = string.Empty;
            if (osInfo.Platform == PlatformID.Unix)
            {
                if (!Directory.Exists(descPath)) Directory.CreateDirectory(descPath);
                desc = descPath + (saveName ?? DateTime.Now.ToString("yyyyMMdd-hhmmss-ffff")) + ".pdf";
            }
            else
            {
                descPath = "D:\\Pdf\\";
                if (!Directory.Exists(descPath)) Directory.CreateDirectory(descPath);
                desc = descPath + (saveName ?? DateTime.Now.ToString("yyyyMMdd-hhmmss-ffff")) + ".pdf";
            }

            FontProvider fp = new FontProvider();
            // Add Standard fonts libs without chinese support | 添加标准字体库
            // fp.AddStandardPdfFonts();
            fp.AddDirectory(resources);
            properties.SetFontProvider(fp);
            // Set base uri to resource folder | 设置基础uri
            properties.SetBaseUri(resources);

            PdfWriter writer = new PdfWriter(desc);
            PdfDocument pdfDoc = new PdfDocument(writer);
            // Set PageSize | 设置页面大小
            pdfDoc.SetDefaultPageSize(PageSize.A4);
            // Set Encoding | 设置文本编码方式
            pdfDoc.GetCatalog().SetLang(new PdfString("UTF-8"));

            //Set the document to be tagged | 展示文档标签树
            pdfDoc.SetTagged();
            pdfDoc.GetCatalog().SetViewerPreferences(new PdfViewerPreferences().SetDisplayDocTitle(true));

            //https://developers.itextpdf.com/content/itext-7-examples/converting-html-pdf/pdfhtml-header-and-footer-example
            // create pdf font using custom resources | 自定义字体
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
            // Header handler | 页头处理
            PdfHeader headerHandler = new PdfHeader(header, sourcehansanscn);
            // Footer handler | 页脚处理
            PdfFooter footerHandler = new PdfFooter(footer, sourcehansanscn);

            // Assign event-handlers
            pdfDoc.AddEventHandler(PdfDocumentEvent.START_PAGE, headerHandler);
            pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, footerHandler);

            // Setup custom tagworker factory for better tagging of headers | 设置标签处理器
            DefaultTagWorkerFactory tagWorkerFactory = new DefaultTagWorkerFactory();
            properties.SetTagWorkerFactory(tagWorkerFactory);

            // Setup default outline(bookmark) handler | 设置默认大纲(书签)处理器
            // We used the createStandardHandler() method to create a standard handler. 
            // This means that pdfHTML will look for <h1>, <h2>, <h3>, <h4>, <h5>, and <h6>. 
            // The bookmarks will be created based on the hierarchy of those tags in the HTML file. 
            // To enable other tags, read the folllowing article for more details.
            // https://developers.itextpdf.com/content/itext-7-converting-html-pdf-pdfhtml/chapter-4-creating-reports-using-pdfhtml
            OutlineHandler outlineHandler = OutlineHandler.CreateStandardHandler();
            properties.SetOutlineHandler(outlineHandler);

            // Bookmark | 书签
            // https://developers.itextpdf.com/content/itext-7-examples/itext-7-bookmarks-tocs/toc-first-page
            // https://developers.itextpdf.com/content/best-itext-questions-stackoverview/actions-and-annotations/itext7-how-create-hierarchical-bookmarks
            // https://developers.itextpdf.com/content/itext-7-building-blocks/chapter-6-creating-actions-destinations-and-bookmarks
            // https://developers.itextpdf.com/examples/actions-and-annotations/clone-named-destinations
            // PdfOutline outline = null;
            // outline = CreateOutline(outline, pdfDoc, "bookmark", "bookmark");

            // Meta tags | 文档属性标签
            PdfDocumentInfo pdfMetaData = pdfDoc.GetDocumentInfo();
            pdfMetaData.SetTitle(title);
            pdfMetaData.SetAuthor(author);
            pdfMetaData.AddCreationDate();
            pdfMetaData.GetProducer();
            pdfMetaData.SetCreator(creator);
            pdfMetaData.SetKeywords(keywords);
            pdfMetaData.SetSubject(subject);
            #endregion

            // Start convertion | 开始转化
            //Document document =
            //    HtmlConverter.ConvertToDocument(pdfHtmlString, pdfDoc, properties);

            IList<IElement> elements = HtmlConverter.ConvertToElements(pdfHtmlString, properties);
            Document document = new Document(pdfDoc);
            CJKSplitCharacters splitCharacters = new CJKSplitCharacters();
            document.SetFontProvider(fp);
            document.SetSplitCharacters(splitCharacters);
            document.SetProperty(Property.SPLIT_CHARACTERS, splitCharacters);
            foreach (IElement e in elements)
            {
                try
                {
                    document.Add((AreaBreak)e);
                }
                catch
                {
                    document.Add((IBlockElement)e);
                }
            }

            // Close and release document | 关闭并释放文档资源
            document.Close();

            return Content("SUCCESS");
        }

        /// <summary>
        /// Test for `Create` method | Create 方法的测试
        /// </summary>
        /// <returns></returns>
        [Route("testcreate")]
        public IActionResult TestCreate()
        {
            string htmlString = this.GetTestHtmlString();
            this.Create(htmlString);
            return Content(htmlString);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Build a test HTML string
        /// </summary>
        /// <returns></returns>
        private string GetTestHtmlString()
        {
            StringBuilder htmlBuilder = new StringBuilder();
            //// content start
            htmlBuilder.Append("<div class=\"main\" style=\"margin: 30px 0;\">");
            htmlBuilder.Append("DHCPスヌーピングをサポートし、DHCPサーバを設定し、DHCPサーバの適合性を保証します。DoS防御をサポートし、防御ランドスキャン、SYNFIN、Xmascan、Ping Floodingなどを攻撃。");
            htmlBuilder.Append("</div>");
            //// content end

            return htmlBuilder.ToString();
        }
        #endregion
    }
}