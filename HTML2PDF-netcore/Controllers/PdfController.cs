﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                desc = descPath + (saveName == null ? DateTime.Now.ToString("yyyyMMdd-hhmmss-ffff") : saveName) + ".pdf";
            }
            else
            {
                descPath = "D:\\Pdf\\";
                if (!Directory.Exists(descPath)) Directory.CreateDirectory(descPath);
                desc = descPath + (saveName == null ? DateTime.Now.ToString("yyyyMMdd-hhmmss-ffff") : saveName) + ".pdf";
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
            // style
            htmlBuilder.Append("<style>");
            htmlBuilder.Append(".main {margin: 36px 0;}.main > h2{margin:0}body {background-color: #ffffff !important;}#smbad_head {width: 100%;margin-top: 80px;min-height: 402px;position: relative;}#smb_Img {top: 0;left: 0;width: 770px;min-height: 402px;border: #e0e0e0 1px solid;position: absolute;height: 100%;background-color: #ffffff;}#imgDetail {position: absolute;width: 400px;height: 400px;overflow: hidden;left: 50%;margin-left: -200px;top: 50%;margin-top: -200px}#imgDH {position: absolute;top: 0;left: 0;height: 100%;width: 40px;font-size: 0;}#imgDH>span {display: inline-block;vertical-align: middle;}#imgDH>.calibration {width: 5px;height: 100%;}#imgDH>.wrap {width: 34px;}#imgDH img {margin: 3px 0;cursor: pointer;border: #e0e0e0 1px solid}#smb_Intro {float: right;width: 415px;min-height: 402px;border: #e0e0e0 1px solid;background-color: #ffffff;}#smbproductName {margin: 30px 0 0 20px;font: 20px/30px Arial;color: #5c5c5c;font-weight: bold}#smbproductModel {margin: 0px 0 0 20px;font: 20px/30px Arial;color: #5c5c5c;font-weight: bold}#smbproductFeature {margin: 20px 20px 30px 20px}#smbproductFeature ul {list-style: none}#smbproductFeature ul li {padding-left: 12px;font: 12px/28px Arial;color: #5c5c5c;background: url(../image/point.gif) 0 12px no-repeat}#smbdetail_dh {text-align: center;border: #e0e0e0 1px solid;background-color: #f5f5f5;margin-top: 12px}#smbdetail_dh a {margin: 0 30px;font: 14px/58px  Arial;color: #8f8f8f;}#smbdetail_dh a.selected {color: #35363a;}#smbdetail_dh a:hover {color: #35363a;}#smbdetail_dh a#shopLink {color: #ff3d00;text-align: right;display: inline-block;width: 80px;background: url(../image/shopLink-bg.png) 0 15px no-repeat}#smbproductIntro {border: #e0e0e0 solid 1px;border-top: none !important;overflow: hidden;background-color: #ffffff;}#smbproductSpe {border: #e0e0e0 solid 1px;border-top: none !important;overflow: hidden;background-color: #ffffff;}#smbproductSpe h2 {text-align: center;width: 100%;height: 60px;padding-top: 10px;font: 18px/50px Arial;color: #35363a;font-weight: bold;}#smbproductSpe table {position: relative;width: 85%;left: 50%;margin-left: -42.5%;margin-bottom: 10px}#smbproductSpe>table tr td {padding: 20px;border-bottom: #e0e0e0 1px solid}#smbproductSpe>table tr td.first {border-top: #e0e0e0 1px solid;}#smbproductSpe table tr td.speName {font: 14px/22px Arial;color: #5c5c5c;font-weight: bold;border-right: #e0e0e0 1px solid;width: 20%}#smbproductSpe table tr td.speDetail {font: 12px/20px Arial;color: #5c5c5c;width: 80%;padding-left: 30px;}#smbproductSpe ul li {list-style: disc;margin: 3px 0;}.customSpe table {width: 960px !important;margin-left: -480px !important;border-color: #ffffff;border: 1px solid #999999 !important;border-left: none;margin-top: 70px;margin-bottom: 70px !important;}.customSpe table tr td {height: 40px;line-height: 2em;border: none;color: #4c4c4c;font-size: 12px;text-align: center;width: 16.66667%;border-left: 1px solid #999999;border-bottom: 1px solid #999999}.customSpe table tr td:first-child {border-left: none}.customSpe table tr:last-child td {border-bottom: none}.customSpe table tr td li {line-height: 2em;text-align: left;list-style-position: inside;margin: 0 10px;-moz-margin-start: 30px;-moz-margin-end: 20px;}.customSpe table tr td li:first-child {margin-top: 9px;}.customSpe table tr td li:last-child {margin-bottom: 9px;}.customSpe table tr:nth-child(1) td {font-weight: 800;font-size: 14px;}.customSpe table tr:not(:nth-child(1)) td:first-child {font-weight: 800;font-size: 14px;text-align: center;background-color: #f2f2f2;}.customSpe table tr:nth-child(1) td:not(:first-child) {background-color: #f2f2f2;text-align: center}#smbproductEx img {width:100%;}#smbproductEx table {margin:0 auto;border-color: #ffffff;border: 1px solid #999999 !important;border-left: none;}#smbproductEx table tr td {height: 40px;line-height: 2em;border: none;color: #4c4c4c;font-size: 12px;text-align: center;border-left: 1px solid #999999;border-bottom: 1px solid #999999}#smbproductEx table tr td p {text-align: left;padding-left: 15px}#smbproductEx table tr td:first-child {border-left: none}#smbproductEx table tr:last-child td {border-bottom: none}#smbproductEx table tr td li {line-height: 2em;text-align: left;margin: 0 20px;-moz-margin-start: 30px;-moz-margin-end: 20px;}#smbproductEx table tr td ul {padding-left:10px;}#smbproductEx table tr td li:first-child {margin-top: 9px;}#smbproductEx table tr td li:last-child {margin-bottom: 9px;}#smbproductEx table tr:nth-child(1) td {font-weight: 800;font-size: 14px;}#smbproductEx table tr:not(:nth-child(1)) td:first-child {font-weight: 800;font-size: 14px;text-align: center;background-color: #f2f2f2;}#smbproductEx table tr:nth-child(1) td:not(:first-child) {background-color: #f2f2f2;text-align: center}#smbproductEx {border: none;border-top: none !important;overflow: hidden;background-color: #ffffff;}#smbproductDownload {border: #e0e0e0 solid 1px;border-top: none !important;overflow: hidden;background-color: #ffffff;}#smbproductDownload table {position: relative;width: 85%;left: 50%;margin-left: -42%;margin-bottom: 10px;margin-top: 40px}/*#smbproductDownload table tr td{ vertical-align:top; border-right:#e0e0e0 1px solid; padding-left:20px; width:25%;}*/#smbproductDownload table tr td {vertical-align: top;border-right: #e0e0e0 1px solid;padding-left: 18px;width: 25%;}#smbproductDownload table tr td.last {border-right: none !important;}#smbproductDownload table tr td h2 {font: 18px/18px Arial;color: #35363a;font-weight: bold;margin-bottom: 20px;}#smbproductDownload ul li {margin: 10px 0;font: 14px/22px Arial;color: #5c5c5c;}#smbproductDownload ul li a {font: 14px/22px Arial;color: #5c5c5c;text-decoration: none}#smbproductDownload ul li a:hover {text-decoration: underline}#smbex {border-top: none;width: 100%;margin: 10px 0;}#smbexDetail {width: 49.8%;float: left;border: #e0e0e0 solid 1px;height: 320px;background-color: #ffffff;}#smbex #smbexDetail h2 {padding: 60px 0 20px 110px;font: 24px  Arial;color: #35363a;font-weight: bold}#smbexDetail ul {margin: 15px 0 0 110px}#smbexDetail ul li {margin: 7px 0;}#smbexDetail ul li a {font: 14px/22px Arial;color: #35363a;text-decoration: none}#smbexDetail ul li a:hover {text-decoration: underline}#smbexDetail .more {text-align: right;padding-right: 30px}#smbexDetail .more a {color: #35363a;text-decoration: underline}#relatedPro {float: left;border: #e0e0e0 solid 1px;border-left: none;background-color: #ffffff;height: 320px;width: 49.8%;}#relatedPro h2 {padding: 60px 0 20px 40px;font: 24px  Arial;color: #35363a;font-weight: bold}#relatedPro ul li {float: left;margin: 0 10px 0 40px;text-align: center;font: 12px/18px  Arial;color: #5c5c5c;font-weight: bold}#relatedPro ul li a {display: block;margin: 2px 0;color: #333333}#banner {height: 147px;width: 100%;margin-top: 60px;background: url(../image/bannerbg.jpg) 0 0 repeat-x;}#bannerTitle {float: left;font: 14px  Arial;color: #909090;font-weight: bold;width: 80px;height: 100px}#bannerTitle p {padding-top: 63px;}#bannerTitle img.interval {float: right;position: relative;bottom: 16px;}#bannerList {float: left;width: 1120px;margin: 0 auto;text-align: center}#bannerList a {margin: 0 15px;_margin: 0 15px;font: 14px  Arial;color: #909090;font-weight: bold;padding-bottom: 5px;position: relative;top: 63px}#bannerList a.selected {border-bottom: #5c5c5c solid 2px;font: 14px  Arial;color: #5c5c5c;font-weight: bold;}#bannerList a:hover {color: #5c5c5c;}#main {margin-top: 20px;min-height: 640px;_height: 640px;}#smbdetail h2.smbClass {margin: 15px 20px;font: 14px/22px Arial;font-weight: bold;color: #5c5c5c}#smbdetail ul li {position: relative;float: left;width: 298px;height: 400px;border-right: #e0e0e0 1px solid;border-bottom: #e0e0e0 1px solid;background-color: #ffffff}#smbdetail ul li.top {border-top: #e0e0e0 1px solid;}#smbdetail ul li.first {border-left: #e0e0e0 1px solid;}#smbdetail ul li .stateIco {display: block;position: absolute;right: 15px;top: 203px;margin: 0 !important;}#smbdetail ul li img {margin: 16px;}#smbdetail ul li hr {margin-left: 32px;width: 234px;border: #e0e0e0 1px solid;}#smbdetail ul li p {margin: 0 0 0 32px;font: 14px/22px Arial;}#smbdetail ul li p a {color: #5c5c5c}</style><style type=\"text/css\">#smbproductFeature li {font-size: 12px;line-height: 22px}#smbad_head {margin-top: 20px !important}</style><title></title><style>#smbproductIntro,#smbad_head div,#smbproductSpe {border: none}#smbproductSpe table {width: 100% !important;left: 50% !important;margin-left: -50% !important;margin-top: 10px !important;}ul {list-style: disc !important;}#smbproductFeature ul li {padding-left: 0;margin-left: 14px;background: none}.customSpe table tr td {}.customSpe table tr:nth-child(1) td {}.customSpe table tr:not(:nth-child(1)) td:first-child {}p,span,div,td {background: #ffffff;}html,body {background: #ffffff;}#smbproductIntro .detail-title {margin: 48px 0 12px 0 !important;}#smbproductIntro li,#smbproductIntro p {margin-left: 10px}#smbad_head li,#smbad_head span {line-height: 1.75em !important;}#smb_Intro p {font-weight: normal !important;line-height: 1.2em !important;color: #0071b4;}#smbproductSpe table td {background:none;}#smbproductSpe table tr:nth-of-type(2n) td {background: #dcebf4}#smbproductSpe table tr:nth-of-type(2n) {background: #dcebf4}#smbad_head {height: auto !important;overflow: auto !important;}.customSpe table tr td {border: none;}.customSpe table {border: none !important;border-collapse: collapse;}.customSpe table tr td {border: 1px solid #999999 !important;}.customSpe table tr td ul,#smb_Intro ul {padding-left: 25px;}#imgDetail {left: 0;margin-left: 0;}#smbproductFeature {margin-bottom: 20px;}.switch-class>p {margin-left: 0 !important}#smbproductIntro .detail-title:first-child {margin-top: 36px !important}#solution {}#solution > div:last-of-type {width:100%;overflow:hidden;} #solution > div:first-of-type {border-bottom:#0071b4 4px solid; font-size: 47px; color: #0071b4;width: 440px;line-height: 1.5em;padding: 16px 0; font-weight: bold} #solution > .solution-title {color: #0071b4; font-size: 36px; margin-top: 80px;} #solution > p {line-height:2em; font-size: 20px;font-weight: lighter;}#solution img {width:100%; margin-top:-100px;} #solution > p:nth-of-type(1), #solution > p:nth-of-type(2) {padding-top: 10px}#solution > .solution-title:last-of-type {margin: 70px 0 10px 0}#smbproductIntro,#smbproductEx {font: 14px/24px Arial;/*width:978px !important*/}#smbproductIntro .detail-title,#smbproductEx h3 {font: bold 18px/34px Arial;margin-top: 8px}.switch-class {/*color: #4c4c4c;*/}.switch-class>p:first-child {/*font-size: 16px;*/line-height: 26px;margin-bottom: 40px}.switch-class>span {font-size: 14px;line-height: 28px;}.switch-class>h3 {font-size: 15px;font-weight: 800;line-height: 24px;margin-top: 35px;margin-bottom: 20px;color:#236bad;}.switch-class>ul {list-style: disc;padding-left: 10px;}.switch-class>ul:last-child {margin-bottom: 30px;}");
            htmlBuilder.Append("#smbproductIntro {width:100%;}#smb_Intro {border: none;float: none;min-height: auto;font-size:14px;}.pdf-cover{width:100%;text-align:center;position:relative;margin-left:16px;}.pdf-cover .pdf-cover-img {margin-left:-16px;width:100%;position:absolute;top:-18px;left:0;z-index:2;}.pdf-cover .pdf-cover-pimg {width:400px;height:400px;position:absolute;top:660px;margin:0 auto}.pdf-cover .pdf-cover-pimg img {height:360px;}.pdf-sub-title {color:#0071b4;font-size:20px;margin:0;}");
            htmlBuilder.Append("#back-cover table {border-collapse: collapse;text-align: center;}#back-cover table td,#back-cover table th {border: 1px solid #000;height:10px;padding: 0 10px;}#back-cover table th {font-weight: 500;font-size: 12px;background: rgb(225,237,243)}#back-cover table a {text-decoration: none;color:rgb(90, 90, 92)}");
            htmlBuilder.Append("</style>");
            //// content start
            htmlBuilder.Append("<div class=\"main\">");
            // page1: cover
            htmlBuilder.Append("<div class=\"pdf-cover\"><img class=\"pdf-cover-img\" src=\"https://localhost:5001/src/images/coverBg.jpg\" data-bm=\"64\"><div class=\"pdf-cover-pimg\"><img src=\"https://www.tp-link.com.cn/content/images/products/400/1/555.jpg\" data-bm=\"65\"></div><div class=\"pdf-cover-pdesc\" style=\"position: absolute;top: 880px;z-index: 2\"><div style=\"font-size: 33px;text-align:left;\"><font data-bm=\"112\" style=\"vertical-align: inherit;\"><font data-bm=\"113\" style=\"vertical-align: inherit;\">TP-LINK三層ネットワーク交換機</font></font></div><div style=\"height: 36px;line-height: 36px;text-align:center;color: white;font-size: 22px;background-color: #236bad;border-radius:4px;padding:0 2px;\"><font data-bm=\"114\" style=\"vertical-align: inherit;\"><font data-bm=\"115\" style=\"vertical-align: inherit;\">千兆上联三層网站交換機TL-SL5452-コンボ</font></font></div></div></div>");
            // page2
            htmlBuilder.Append("<div style=\"page-break-after:always\"></div>");
            htmlBuilder.Append("<h2 name=\"ProductIntro\" class=\"pdf-sub-title\"><font data-bm=\"116\" style=\"vertical-align: inherit;\"><font data-bm=\"117\" style=\"vertical-align: inherit;\">製品の簡略化</font></font></h2><br><div id=\"smb_Intro\"><ul><li><span style=\"line-height:1.5;\"><font data-bm=\"118\" style=\"vertical-align: inherit;\"><font data-bm=\"119\" style=\"vertical-align: inherit;\">48個の10 / 100Base-T RJ45ポート</font></font></span></li><li><span style=\"line-height:1.5;\"><font data-bm=\"120\" style=\"vertical-align: inherit;\"><font data-bm=\"121\" style=\"vertical-align: inherit;\">4個の独立した千兆SFP端末</font></font></span></li><li><span style=\"line-height:1.5;\"><font data-bm=\"122\" style=\"vertical-align: inherit;\"><font data-bm=\"123\" style=\"vertical-align: inherit;\">2個の10/100 / 1000Base-T RJ45端子</font></font></span></li><li><span style=\"line-height:1.5;\"><font data-bm=\"124\" style=\"vertical-align: inherit;\"><font data-bm=\"125\" style=\"vertical-align: inherit;\">サポートRIP動的経路、静的経路、ARPプロキシ</font></font></span></li><li><span style=\"line-height:1.5;\"><font data-bm=\"126\" style=\"vertical-align: inherit;\"><font data-bm=\"127\" style=\"vertical-align: inherit;\">サポートDHCPサービス、DHCP中継、DHCP Snooping</font></font></span></li><li><span style=\"line-height:1.5;\"><font data-bm=\"128\" style=\"vertical-align: inherit;\"><font data-bm=\"129\" style=\"vertical-align: inherit;\">サポート四次元绑定、ARP / IP / DoS防御、802.1X認証</font></font></span></li><li><span style=\"line-height:1.5;\"><font data-bm=\"130\" style=\"vertical-align: inherit;\"><font data-bm=\"131\" style=\"vertical-align: inherit;\">対応VLAN、QoS、ACL、生成樹脂、播種、IPv6</font></font></span></li><li><span style=\"line-height:1.5;\"><font data-bm=\"132\" style=\"vertical-align: inherit;\"><font data-bm=\"133\" style=\"vertical-align: inherit;\">対応Web网、CLIコマンド行、SNMP</font></font></span></li></ul></div><div id=\"smbproductIntro\" style=\"padding: 0\"><div class=\"switch-class\"><p><font data-bm=\"134\" style=\"vertical-align: inherit;\"><font data-bm=\"135\" style=\"vertical-align: inherit;\">TP-LINKは、提案されている5つの系列千兆上三層ネットワーク交換機を新たに開発しました。充実した安全保護機構、優れたACL / QoS戦略、そして豊富なVLAN機能により、安全な管理が容易になり、中小企業、レストラン、そして企業のネットワークへの接続、アプリケーションの分野で幅広く利用されています。</font></font></p><h3><font data-bm=\"136\" style=\"vertical-align: inherit;\"><font data-bm=\"137\" style=\"vertical-align: inherit;\">百兆接、千兆上行</font></font></h3><ul><li><font data-bm=\"138\" style=\"vertical-align: inherit;\"><font data-bm=\"139\" style=\"vertical-align: inherit;\">全系列をサポートする「百兆以太网口+千兆网口/光口」組み合わせ、方便使用者灵活组网、各分野のニーズを満たす</font></font></li><li><font data-bm=\"140\" style=\"vertical-align: inherit;\"><font data-bm=\"141\" style=\"vertical-align: inherit;\">8/16/24/48のマルチスペックエンド製品が提供されていますが、すべてのエンドポートが回線速度変換機能を備えており、さまざまなユーザーのニーズに応えます。</font></font></li></ul><h3><font data-bm=\"142\" style=\"vertical-align: inherit;\"><font data-bm=\"143\" style=\"vertical-align: inherit;\">強力なサービス処理能力</font></font></h3><ul><li><font data-bm=\"144\" style=\"vertical-align: inherit;\"><font data-bm=\"145\" style=\"vertical-align: inherit;\">RIPの自動ルーティングプロトコルのサポート、動的テーブルの生成、更新、更新後のルーティングの問題。</font></font></li><li><font data-bm=\"146\" style=\"vertical-align: inherit;\"><font data-bm=\"147\" style=\"vertical-align: inherit;\">静的経路設定をサポートし、管理者は経路単位で経路を変更し、異なるネットワーク間の通信を実現する。</font></font></li><li><font data-bm=\"148\" style=\"vertical-align: inherit;\"><font data-bm=\"149\" style=\"vertical-align: inherit;\">DHCPサーバをサポートし、ネットワーク内のホストがIPアドレスを割り当てます。</font></font></li><li><font data-bm=\"150\" style=\"vertical-align: inherit;\"><font data-bm=\"151\" style=\"vertical-align: inherit;\">ＤＨＣＰをサポートし、異なるインターフェースまたはインターネット内の交換機でもＩＰアドレスを取得することができ、ＤＨＣＰサーバーの数を減らすことができる。</font></font></li><li><font data-bm=\"152\" style=\"vertical-align: inherit;\"><font data-bm=\"153\" style=\"vertical-align: inherit;\">代理ＡＲＰをサポートし、同一のネットワーク内の異なる物理的ネットワーク内のホストは正常に通信することができる。</font></font></li><li><font data-bm=\"154\" style=\"vertical-align: inherit;\"><font data-bm=\"155\" style=\"vertical-align: inherit;\">IEEE 802.1Q VLAN、MAC VLAN、プロトコルVLAN、プライベートVLANをサポートし、ユーザはさまざまな要件に応じてVLANを切り替えることができます。</font></font></li><li><font data-bm=\"156\" style=\"vertical-align: inherit;\"><font data-bm=\"157\" style=\"vertical-align: inherit;\">GVRPをサポートし、VLANの動的配信、登録、およびプロパティの伝達を実現し、手作業による設定量を減らし、設定の正確性を保証します。</font></font></li><li><font data-bm=\"158\" style=\"vertical-align: inherit;\"><font data-bm=\"159\" style=\"vertical-align: inherit;\">ＶＬＡＮ ＶＰＮ機能をサポートし、公衆網の入力端に加入者の私設網が外層ＶＬＡＮタグをカプセル化し、二層のＶＬＡＮタグが公衆網を通過できるようにする。</font></font></li><li><font data-bm=\"68\" style=\"vertical-align: inherit;\"><font data-bm=\"69\" style=\"vertical-align: inherit;\">ＱｏＳをサポートし、ポートベース、８０２.１ＰベースおよびＤＳＣＰベースの３つの優先順位モードおよびＥｑ、ＳＰ、ＷＲＲ、ＳＰ ＋ ＷＲＲの４つのキューリスト調整アルゴリズムをサポートする。</font></font></li><li><font data-bm=\"70\" style=\"vertical-align: inherit;\"><font data-bm=\"71\" style=\"vertical-align: inherit;\">ＡＣＬをサポートし、一致規則、処理操作、および時間制限を設定することによって、データパケットのフィルタリングを実現する。</font></font></li><li><font data-bm=\"72\" style=\"vertical-align: inherit;\"><font data-bm=\"73\" style=\"vertical-align: inherit;\">IGMP V1 / V2ブロードキャストプロトコルをサポートし、MLDスヌーピング、IGMPスヌーピングをサポートし、多端子高解像度の監視およびビデオ会議への参加要求を満たします。</font></font></li><li><font data-bm=\"74\" style=\"vertical-align: inherit;\"><font data-bm=\"75\" style=\"vertical-align: inherit;\">IPv6をサポートし、ネットワークがIPv4からIPv6へ移行するという要求を満たす。</font></font></li></ul><h3><font data-bm=\"76\" style=\"vertical-align: inherit;\"><font data-bm=\"77\" style=\"vertical-align: inherit;\">完全装備の安全な保護メカニズム</font></font></h3><ul><li><font data-bm=\"78\" style=\"vertical-align: inherit;\"><font data-bm=\"79\" style=\"vertical-align: inherit;\">IPアドレス、MACアドレス、VLAN、およびポートの4つの要素がサポートされています。</font></font></li><li><font data-bm=\"80\" style=\"vertical-align: inherit;\"><font data-bm=\"81\" style=\"vertical-align: inherit;\">ARP保護をサポートし、ローカルエリアネットワークでよく見られるWeb攻撃や中間者攻撃などのARP攻撃、ARP攻撃などを行います。</font></font></li><li><font data-bm=\"82\" style=\"vertical-align: inherit;\"><font data-bm=\"83\" style=\"vertical-align: inherit;\">IPアドレスの保護をサポートし、MACアドレス、IPアドレス、MAC / IPアドレスのいずれかを含むことを防ぎます。</font></font></li><li><font data-bm=\"84\" style=\"vertical-align: inherit;\"><font data-bm=\"85\" style=\"vertical-align: inherit;\">DoS防御をサポートし、防御ランドスキャン、SYNFIN、Xmascan、Ping Floodingなどを攻撃します。</font></font></li><li><font data-bm=\"86\" style=\"vertical-align: inherit;\"><font data-bm=\"87\" style=\"vertical-align: inherit;\">８０２．１Ｘ認証をサポートし、ローカルネットワークコンピュータに認証機能を提供し、認証結果に基づいて制御されたポートの認証状態を制御する。</font></font></li><li><font data-bm=\"88\" style=\"vertical-align: inherit;\"><font data-bm=\"89\" style=\"vertical-align: inherit;\">セキュリティで保護された、MACアドレス数を学習して最大数に達すると学習を停止し、MACアドレスの攻撃を防ぎ、ネットワークフローを制御します。</font></font></li><li><font data-bm=\"90\" style=\"vertical-align: inherit;\"><font data-bm=\"91\" style=\"vertical-align: inherit;\">DHCPスヌーピングをサポートし、DHCPサーバを設定し、DHCPサーバの適合性を保証します。</font></font></li></ul><h3><font data-bm=\"92\" style=\"vertical-align: inherit;\"><font data-bm=\"93\" style=\"vertical-align: inherit;\">さまざまな信頼性保護</font></font></h3><ul><li><font data-bm=\"94\" style=\"vertical-align: inherit;\"><font data-bm=\"95\" style=\"vertical-align: inherit;\">STP / RSTP / MSTP生成のための樹脂プロトコルをサポートし、二重回線を削除し、リンク構成要素を実現します。</font></font></li><li><font data-bm=\"96\" style=\"vertical-align: inherit;\"><font data-bm=\"97\" style=\"vertical-align: inherit;\">生成樹脂セキュリティ機能をサポートし、生&#8203;&#8203;成樹脂ネットワーク内の機器が様々な形式の悪意のある攻撃を受けないようにする。</font></font></li><li><font data-bm=\"98\" style=\"vertical-align: inherit;\"><font data-bm=\"99\" style=\"vertical-align: inherit;\">静的バランスおよび動的バランスをサポートし、リンク帯域幅を効果的に増加させ、負荷の均衡、リンク構成要素を実現し、リンクの信頼性を高める。</font></font></li></ul><h3><font data-bm=\"100\" style=\"vertical-align: inherit;\"><font data-bm=\"101\" style=\"vertical-align: inherit;\">リマツの運行維持</font></font></h3><ul><li><font data-bm=\"102\" style=\"vertical-align: inherit;\"><font data-bm=\"103\" style=\"vertical-align: inherit;\">Web Web、CLIコマンド実行（Console、Telnet）、SNMP（V1 / V2 / V3）などの多彩な管理と保護方式。</font></font></li><li><font data-bm=\"104\" style=\"vertical-align: inherit;\"><font data-bm=\"105\" style=\"vertical-align: inherit;\">HTTPS、SSL V3、TLSV1、SSHV1 / V2などの高密度方式をサポートし、より安全に管理できます。</font></font></li><li><font data-bm=\"106\" style=\"vertical-align: inherit;\"><font data-bm=\"107\" style=\"vertical-align: inherit;\">RMON、システム日、ポートフローをサポートし、ネットワークの最適化と改良を行います。</font></font></li><li><font data-bm=\"108\" style=\"vertical-align: inherit;\"><font data-bm=\"109\" style=\"vertical-align: inherit;\">ケーブルの検査、Pingの検査、Tracertの検査がサポートされています。</font></font></li><li><font data-bm=\"110\" style=\"vertical-align: inherit;\"><font data-bm=\"111\" style=\"vertical-align: inherit;\">ＬＬＤＰをサポートし、ネットワーク管理システムが問い合わせを行い、リンクの通信状況を判断する。</font></font></li><li><font data-bm=\"232\" style=\"vertical-align: inherit;\"><font data-bm=\"233\" style=\"vertical-align: inherit;\">サポートされているCPU監視、内部監視、Ping検査、Tracert検査、ケーブル検査。</font></font></li></ul></div></div>");
            // page3
            htmlBuilder.Append("<div style=\"page-break-after:always\"></div>");
            htmlBuilder.Append("<h2 name=\"ProductSpex\" class=\"pdf-sub-title\"><font data-bm=\"234\" style=\"vertical-align: inherit;\"><font data-bm=\"235\" style=\"vertical-align: inherit;\">製品の規格</font></font></h2><br><div id=\"smbproductSpe\"><div class=\"customSpe\"><table class=\"ke-zeroborder\" border=\"1\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td><font data-bm=\"236\" style=\"vertical-align: inherit;\"><font data-bm=\"237\" style=\"vertical-align: inherit;\">製品型番</font></font></td><td><font data-bm=\"238\" style=\"vertical-align: inherit;\"><font data-bm=\"239\" style=\"vertical-align: inherit;\">TL-SL5452-コンボ</font></font></td><td><font data-bm=\"240\" style=\"vertical-align: inherit;\"><font data-bm=\"241\" style=\"vertical-align: inherit;\">TL-SL5428-コンボ</font></font></td><td><font data-bm=\"242\" style=\"vertical-align: inherit;\"><font data-bm=\"243\" style=\"vertical-align: inherit;\">TL-SL5218-コンボ</font></font></td><td><font data-bm=\"244\" style=\"vertical-align: inherit;\"><font data-bm=\"245\" style=\"vertical-align: inherit;\">TL-SL5210</font></font></td></tr><tr><td><font data-bm=\"246\" style=\"vertical-align: inherit;\"><font data-bm=\"247\" style=\"vertical-align: inherit;\">百兆RJ45端口</font></font></td><td><font style=\"vertical-align: inherit;\"><font style=\"vertical-align: inherit;\">48</font></font></td><td><font style=\"vertical-align: inherit;\"><font style=\"vertical-align: inherit;\">24</font></font></td><td><font style=\"vertical-align: inherit;\"><font style=\"vertical-align: inherit;\">16</font></font></td><td><font style=\"vertical-align: inherit;\"><font style=\"vertical-align: inherit;\">8</font></font></td></tr><tr><td><font data-bm=\"248\" style=\"vertical-align: inherit;\"><font data-bm=\"249\" style=\"vertical-align: inherit;\">千兆RJ45端口</font></font></td><td><font data-bm=\"250\" style=\"vertical-align: inherit;\"><font data-bm=\"251\" style=\"vertical-align: inherit;\">2（复用）</font></font></td><td><font style=\"vertical-align: inherit;\"><font style=\"vertical-align: inherit;\">4</font></font></td><td><font style=\"vertical-align: inherit;\"><font style=\"vertical-align: inherit;\">2</font></font></td><td><font style=\"vertical-align: inherit;\"><font style=\"vertical-align: inherit;\">1</font></font></td></tr><tr><td><font data-bm=\"252\" style=\"vertical-align: inherit;\"><font data-bm=\"253\" style=\"vertical-align: inherit;\">千兆SFP端口</font></font></td><td><font style=\"vertical-align: inherit;\"><font style=\"vertical-align: inherit;\">4</font></font></td><td><font data-bm=\"254\" style=\"vertical-align: inherit;\"><font data-bm=\"255\" style=\"vertical-align: inherit;\">4（复用）</font></font></td><td><font data-bm=\"348\" style=\"vertical-align: inherit;\"><font data-bm=\"349\" style=\"vertical-align: inherit;\">2（复用）</font></font></td><td><font style=\"vertical-align: inherit;\"><font style=\"vertical-align: inherit;\">1</font></font></td></tr><tr><td><font data-bm=\"256\" style=\"vertical-align: inherit;\"><font data-bm=\"257\" style=\"vertical-align: inherit;\">RJ45コンソール端子</font></font></td><td colspan=\"4\"><font style=\"vertical-align: inherit;\"><font style=\"vertical-align: inherit;\">1</font></font></td></tr><tr><td><font data-bm=\"258\" style=\"vertical-align: inherit;\"><font data-bm=\"259\" style=\"vertical-align: inherit;\">MACアドレス容量</font></font></td><td><font data-bm=\"260\" style=\"vertical-align: inherit;\"><font data-bm=\"261\" style=\"vertical-align: inherit;\">16K</font></font></td><td><font style=\"vertical-align: inherit;\"><font style=\"vertical-align: inherit;\">8K</font></font></td><td><font style=\"vertical-align: inherit;\"><font style=\"vertical-align: inherit;\">8K</font></font></td><td><font style=\"vertical-align: inherit;\"><font style=\"vertical-align: inherit;\">8K</font></font></td></tr><tr><td><font data-bm=\"262\" style=\"vertical-align: inherit;\"><font data-bm=\"263\" style=\"vertical-align: inherit;\">経由</font></font></td><td colspan=\"4\"><ul><li><font data-bm=\"264\" style=\"vertical-align: inherit;\"><font data-bm=\"265\" style=\"vertical-align: inherit;\">RIP V1 / V2のサポート</font></font></li><li><font data-bm=\"266\" style=\"vertical-align: inherit;\"><font data-bm=\"267\" style=\"vertical-align: inherit;\">サポート静的経路</font></font></li><li><font data-bm=\"268\" style=\"vertical-align: inherit;\"><font data-bm=\"269\" style=\"vertical-align: inherit;\">サポートARPプロキシ</font></font></li></ul></td></tr><tr><td><font data-bm=\"270\" style=\"vertical-align: inherit;\"><font data-bm=\"271\" style=\"vertical-align: inherit;\">DHCP</font></font></td><td colspan=\"4\"><ul><li><font data-bm=\"272\" style=\"vertical-align: inherit;\"><font data-bm=\"273\" style=\"vertical-align: inherit;\">DHCPサポートサービスをサポート</font></font></li><li><font data-bm=\"274\" style=\"vertical-align: inherit;\"><font data-bm=\"275\" style=\"vertical-align: inherit;\">サポートDHCP</font></font></li><li><font data-bm=\"276\" style=\"vertical-align: inherit;\"><font data-bm=\"277\" style=\"vertical-align: inherit;\">DHCPスヌーピングのサポート</font></font></li><li><font data-bm=\"278\" style=\"vertical-align: inherit;\"><font data-bm=\"279\" style=\"vertical-align: inherit;\">対応オプション138、オプション82、オプション60</font></font></li></ul></td></tr><tr><td><font data-bm=\"280\" style=\"vertical-align: inherit;\"><font data-bm=\"281\" style=\"vertical-align: inherit;\">VLAN</font></font></td><td colspan=\"4\"><ul><li><font data-bm=\"282\" style=\"vertical-align: inherit;\"><font data-bm=\"283\" style=\"vertical-align: inherit;\">4KのVLANをサポート</font></font></li><li><font data-bm=\"284\" style=\"vertical-align: inherit;\"><font data-bm=\"285\" style=\"vertical-align: inherit;\">802.1Q VLAN、MAC VLAN、プロトコルVLAN、プライベートVLANをサポート</font></font></li><li><font data-bm=\"286\" style=\"vertical-align: inherit;\"><font data-bm=\"287\" style=\"vertical-align: inherit;\">サポートゲストVLAN、音声VLAN</font></font></li><li><font data-bm=\"288\" style=\"vertical-align: inherit;\"><font data-bm=\"289\" style=\"vertical-align: inherit;\">対応VLAN VPN（QinQ）</font></font></li><li><font data-bm=\"290\" style=\"vertical-align: inherit;\"><font data-bm=\"291\" style=\"vertical-align: inherit;\">GVRPプロトコルのサポート</font></font></li><li><font data-bm=\"292\" style=\"vertical-align: inherit;\"><font data-bm=\"293\" style=\"vertical-align: inherit;\">1：1とN：1のVLANマッピングをサポート</font></font></li></ul></td></tr><tr><td><font data-bm=\"294\" style=\"vertical-align: inherit;\"><font data-bm=\"295\" style=\"vertical-align: inherit;\">MACアドレス表</font></font></td><td colspan=\"4\"><ul><li><font data-bm=\"296\" style=\"vertical-align: inherit;\"><font data-bm=\"297\" style=\"vertical-align: inherit;\">IEEE 802.1d規格に準拠</font></font></li><li><font data-bm=\"298\" style=\"vertical-align: inherit;\"><font data-bm=\"299\" style=\"vertical-align: inherit;\">サポートMACアドレス自習学習と老化</font></font></li><li><font data-bm=\"300\" style=\"vertical-align: inherit;\"><font data-bm=\"301\" style=\"vertical-align: inherit;\">サポート静的、動的、フィルタリングアドレス表</font></font></li></ul></td></tr><tr><td><font data-bm=\"302\" style=\"vertical-align: inherit;\"><font data-bm=\"303\" style=\"vertical-align: inherit;\">安全特性</font></font></td><td colspan=\"4\"><ul><li><font data-bm=\"304\" style=\"vertical-align: inherit;\"><font data-bm=\"305\" style=\"vertical-align: inherit;\">ユーザー分類管理と口述保護に基づく</font></font></li><li><font data-bm=\"306\" style=\"vertical-align: inherit;\"><font data-bm=\"307\" style=\"vertical-align: inherit;\">サポートに基づく発行号、IPアドレス、MACアドレス制限</font></font></li><li><font data-bm=\"308\" style=\"vertical-align: inherit;\"><font data-bm=\"309\" style=\"vertical-align: inherit;\">HTTPS、SSL V3、TLS V1、SSH V1 / V2に対応</font></font></li><li><font data-bm=\"310\" style=\"vertical-align: inherit;\"><font data-bm=\"311\" style=\"vertical-align: inherit;\">IP-MAC-PORT-VLANのサポート</font></font></li><li><font data-bm=\"312\" style=\"vertical-align: inherit;\"><font data-bm=\"313\" style=\"vertical-align: inherit;\">サポートARP保護、IP源保護、DoS保護</font></font></li><li><font data-bm=\"314\" style=\"vertical-align: inherit;\"><font data-bm=\"315\" style=\"vertical-align: inherit;\">DHCPスヌーピング、DHCP攻撃防御のサポート</font></font></li><li><font data-bm=\"316\" style=\"vertical-align: inherit;\"><font data-bm=\"317\" style=\"vertical-align: inherit;\">802.1X認証、AAAのサポート</font></font></li><li><font data-bm=\"318\" style=\"vertical-align: inherit;\"><font data-bm=\"319\" style=\"vertical-align: inherit;\">支持端口安全、端口分離</font></font></li><li><font data-bm=\"320\" style=\"vertical-align: inherit;\"><font data-bm=\"321\" style=\"vertical-align: inherit;\">サポートCPU保護機能</font></font></li></ul></td></tr><tr><td><font data-bm=\"322\" style=\"vertical-align: inherit;\"><font data-bm=\"323\" style=\"vertical-align: inherit;\">アクセス制御（ACL）</font></font></td><td colspan=\"4\"><ul><li><font data-bm=\"324\" style=\"vertical-align: inherit;\"><font data-bm=\"325\" style=\"vertical-align: inherit;\">L2（Layer 2）〜L4（Layer 4）をサポート</font></font></li><li><font data-bm=\"326\" style=\"vertical-align: inherit;\"><font data-bm=\"327\" style=\"vertical-align: inherit;\">サポート・エンドミラー、ポート・オリエンテーション、流速制限、QoS</font></font></li></ul></td></tr><tr><td><font data-bm=\"328\" style=\"vertical-align: inherit;\"><font data-bm=\"329\" style=\"vertical-align: inherit;\">サービス品質（QoS）</font></font></td><td colspan=\"4\"><ul><li><font data-bm=\"330\" style=\"vertical-align: inherit;\"><font data-bm=\"331\" style=\"vertical-align: inherit;\">サポート8个端口队列</font></font></li><li><font data-bm=\"332\" style=\"vertical-align: inherit;\"><font data-bm=\"333\" style=\"vertical-align: inherit;\">サポートポートの優先順位、802.1Pの優先順位、DSCPの優先順位</font></font></li><li><font data-bm=\"334\" style=\"vertical-align: inherit;\"><font data-bm=\"335\" style=\"vertical-align: inherit;\">サポートSP、WRR、SP + WRR、Equ優先優先度調整法</font></font></li></ul></td></tr><tr><td><font data-bm=\"336\" style=\"vertical-align: inherit;\"><font data-bm=\"337\" style=\"vertical-align: inherit;\">生成樹脂</font></font></td><td colspan=\"4\"><ul><li><font data-bm=\"338\" style=\"vertical-align: inherit;\"><font data-bm=\"339\" style=\"vertical-align: inherit;\">STP（IEEE 802.1d）、RSTP（IEEE 802.1w）およびMSTP（IEEE 802.1s）プロトコルをサポート</font></font></li><li><font data-bm=\"340\" style=\"vertical-align: inherit;\"><font data-bm=\"341\" style=\"vertical-align: inherit;\">サポートリング保護、ルーブリッジ保護、TC保護、BPDU保護、BPDUフィルタリング</font></font></li></ul></td></tr><tr><td><font data-bm=\"342\" style=\"vertical-align: inherit;\"><font data-bm=\"343\" style=\"vertical-align: inherit;\">播種</font></font></td><td colspan=\"4\"><ul><li><font data-bm=\"344\" style=\"vertical-align: inherit;\"><font data-bm=\"345\" style=\"vertical-align: inherit;\">IGMP v1 / v2 / v3スヌーピングのサポート</font></font></li><li><font data-bm=\"346\" style=\"vertical-align: inherit;\"><font data-bm=\"347\" style=\"vertical-align: inherit;\">サポート快速开放机制</font></font></li><li><font data-bm=\"160\" style=\"vertical-align: inherit;\"><font data-bm=\"161\" style=\"vertical-align: inherit;\">サポート播種VLAN</font></font></li><li><font data-bm=\"162\" style=\"vertical-align: inherit;\"><font data-bm=\"163\" style=\"vertical-align: inherit;\">サポート播種フィルタリング、レポート統合、未知播種サポート</font></font></li></ul></td></tr><tr><td><font data-bm=\"164\" style=\"vertical-align: inherit;\"><font data-bm=\"165\" style=\"vertical-align: inherit;\">IPv6</font></font></td><td colspan=\"5\"><ul><li><font data-bm=\"166\" style=\"vertical-align: inherit;\"><font data-bm=\"167\" style=\"vertical-align: inherit;\">MLDスヌーピングのサポート</font></font></li><li><font data-bm=\"168\" style=\"vertical-align: inherit;\"><font data-bm=\"169\" style=\"vertical-align: inherit;\">IPv6 Ping、IPv6 Tracert、IPv6 Telnetをサポート</font></font></li><li><font data-bm=\"170\" style=\"vertical-align: inherit;\"><font data-bm=\"171\" style=\"vertical-align: inherit;\">IPv6 SNMP、IPv6 SSH、IPv6 SSLに対応</font></font></li></ul></td></tr><tr><td><font data-bm=\"172\" style=\"vertical-align: inherit;\"><font data-bm=\"173\" style=\"vertical-align: inherit;\">管理メンテナンス</font></font></td><td colspan=\"4\"><ul><li><font data-bm=\"174\" style=\"vertical-align: inherit;\"><font data-bm=\"175\" style=\"vertical-align: inherit;\">対応WEB ﾈﾈﾙ ﾈｯﾄ（HTTP、HTTPS、SSL V3、TLS V1）</font></font></li><li><font data-bm=\"176\" style=\"vertical-align: inherit;\"><font data-bm=\"177\" style=\"vertical-align: inherit;\">対応CLI（Telnet、SSH V1 / V2、本地串口）</font></font></li><li><font data-bm=\"178\" style=\"vertical-align: inherit;\"><font data-bm=\"179\" style=\"vertical-align: inherit;\">SNMP V1 / V2 / V3をサポート、パブリックMIBSおよびTP-LINK私有MIBに対応</font></font></li><li><font data-bm=\"180\" style=\"vertical-align: inherit;\"><font data-bm=\"181\" style=\"vertical-align: inherit;\">サポートLLDP、RMON</font></font></li><li><font data-bm=\"182\" style=\"vertical-align: inherit;\"><font data-bm=\"183\" style=\"vertical-align: inherit;\">サポートARP保護、IP源保護、DoS保護</font></font></li><li><font data-bm=\"184\" style=\"vertical-align: inherit;\"><font data-bm=\"185\" style=\"vertical-align: inherit;\">サポートCPU监制、内在监控</font></font></li><li><font data-bm=\"186\" style=\"vertical-align: inherit;\"><font data-bm=\"187\" style=\"vertical-align: inherit;\">支援系総合日志、分級警告</font></font></li><li><font data-bm=\"188\" style=\"vertical-align: inherit;\"><font data-bm=\"189\" style=\"vertical-align: inherit;\">サポートPing、Tracert検査、ケーブル検査</font></font></li></ul></td></tr><tr><td><font data-bm=\"190\" style=\"vertical-align: inherit;\"><font data-bm=\"191\" style=\"vertical-align: inherit;\">ケースサイズ（mm）</font></font></td><td><font data-bm=\"192\" style=\"vertical-align: inherit;\"><font data-bm=\"193\" style=\"vertical-align: inherit;\">440×220×44</font></font></td><td><font data-bm=\"194\" style=\"vertical-align: inherit;\"><font data-bm=\"195\" style=\"vertical-align: inherit;\">440×180×44</font></font></td><td><font data-bm=\"230\" style=\"vertical-align: inherit;\"><font data-bm=\"231\" style=\"vertical-align: inherit;\">440×180×44</font></font></td><td><font data-bm=\"196\" style=\"vertical-align: inherit;\"><font data-bm=\"197\" style=\"vertical-align: inherit;\">250×158×44</font></font></td></tr><tr><td><font data-bm=\"198\" style=\"vertical-align: inherit;\"><font data-bm=\"199\" style=\"vertical-align: inherit;\">入力電源</font></font></td><td colspan=\"4\"><font data-bm=\"200\" style=\"vertical-align: inherit;\"><font data-bm=\"201\" style=\"vertical-align: inherit;\">100〜240VAC、50 / 60Hz</font></font></td></tr><tr><td><font data-bm=\"202\" style=\"vertical-align: inherit;\"><font data-bm=\"203\" style=\"vertical-align: inherit;\">使用環境</font></font></td><td colspan=\"4\"><ul><li><font data-bm=\"204\" style=\"vertical-align: inherit;\"><font data-bm=\"205\" style=\"vertical-align: inherit;\">工作温度：0℃〜40℃</font></font></li><li><font data-bm=\"206\" style=\"vertical-align: inherit;\"><font data-bm=\"207\" style=\"vertical-align: inherit;\">保存温度：-40℃〜70℃</font></font></li><li><font data-bm=\"208\" style=\"vertical-align: inherit;\"><font data-bm=\"209\" style=\"vertical-align: inherit;\">工作湿度：10％〜90％RH不凝固</font></font></li><li><font data-bm=\"210\" style=\"vertical-align: inherit;\"><font data-bm=\"211\" style=\"vertical-align: inherit;\">保存湿度：5％〜90％RH不凝固</font></font></li></ul></td></tr></tbody></table></div></div>");
            // page4 back cover
            htmlBuilder.Append("<div style=\"page-break-after:always\"></div>");
            htmlBuilder.Append("<div style=\"height: 500px;background:rgb(255, 255, 255);position:absolute;z-index:0;top:-80px;left:-24px;\"></div><div style=\"height: 4000px;background:rgb(247, 250, 252);position:absolute;z-index:0;top:-24px;left:-24px;border-radius: 20px;\"></div><div style=\"height: 4000px;background:rgb(247, 250, 252);position:absolute;z-index:0;top:-24px;right:-24px;border-radius: 20px;\"></div><div style=\"height: 4000px;background:rgb(247, 250, 252);position:absolute;z-index:0;bottom:-24px;left:-24px;border-radius: 20px;\"></div><div style=\"height: 4000px;background:rgb(247, 250, 252);position:absolute;z-index:0;bottom:-24px;right:-24px;border-radius: 20px;\"></div><div style=\"width: 100%;height: 100%;margin: -15%;background: none;border-radius: 30px;\"><div id=\"back-cover\" style=\"position: absolute;left: 0;bottom: 0;background: none;\"><div style=\"background: none;height:36px;overflow:auto;\"><img src=\"https://localhost:5001/src/images/backCoverLogo.png\" style=\"width: 25%;float:left;\" data-bm=\"66\"><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"width: 71%;float:right;color:rgb(90, 90, 92)\"><tbody><tr style=\"height:11px;\"><th style=\"font-size:8px;height:11px;\"><font data-bm=\"212\" style=\"vertical-align: inherit;\"><font data-bm=\"213\" style=\"vertical-align: inherit;\">全製品の紹介はウェブサイトを検索する</font></font></th><th style=\"font-size:8px;\"><font data-bm=\"214\" style=\"vertical-align: inherit;\"><font data-bm=\"215\" style=\"vertical-align: inherit;\">技術サポートメール</font></font></th><th style=\"font-size:8px;\"><font data-bm=\"216\" style=\"vertical-align: inherit;\"><font data-bm=\"217\" style=\"vertical-align: inherit;\">オンライン購入</font></font></th><th style=\"font-size:8px;\"><font data-bm=\"218\" style=\"vertical-align: inherit;\"><font data-bm=\"219\" style=\"vertical-align: inherit;\">技術サポート</font></font></th></tr><tr><td style=\"font-size:9px;\"><a href=\"http://www.tp-link.com.cn\"><font data-bm=\"220\" style=\"vertical-align: inherit;\"><font data-bm=\"221\" style=\"vertical-align: inherit;\">www.tp-link.com.cn</font></font></a></td><td style=\"font-size:9px;\"><a href=\"mailto:smb@tp-link.com.cn\"><font data-bm=\"222\" style=\"vertical-align: inherit;\"><font data-bm=\"223\" style=\"vertical-align: inherit;\">smb@tp-link.com.cn</font></font></a></td><td style=\"font-size:9px;\"><a href=\"http://www.tp-linkshop.com.cn\"><font data-bm=\"224\" style=\"vertical-align: inherit;\"><font data-bm=\"225\" style=\"vertical-align: inherit;\">www.tp-linkshop.com.cn</font></font></a></td><td style=\"font-size:9px;\"><a><font data-bm=\"226\" style=\"vertical-align: inherit;\"><font data-bm=\"227\" style=\"vertical-align: inherit;\">400-8863-400</font></font></a></td></tr></tbody></table></div><div style=\"margin-top: 5px;font-size: 9px;line-height: 1.5em;color:rgb(109,111,113);background: none;\"><font data-bm=\"228\" style=\"vertical-align: inherit;\"><font data-bm=\"229\" style=\"vertical-align: inherit;\">TP-LINKのWebサイトには、各製品の高級ソフトウェアやドライバプログラムなどが用意されています。また、最新のソフトウェアやドライバをwww.tp-link.comで入手することもできます。TP-LINKは印刷物および図表の誤りについて全力を尽くしていますが、発生する可能性がある問題については、TP-LINKは一切責任を負いません。</font></font></div></div>");
            htmlBuilder.Append("</div>");
            //// content end

            return htmlBuilder.ToString();
        }
        #endregion
    }
}