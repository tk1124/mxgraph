using com.mxgraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Web;

namespace ConverterSample
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            LoadStencils();

            ConvertToPng(args[0]);

            if (args.Length < 2)
            {
                return;

            }

            ConvertEncodedToPng(args[1]);
        }

        static void ConvertToPng(string path)
        {
            var doc = mxUtils.ParseXml(mxUtils.ReadFile(path));
            var imgPath = Path.ChangeExtension(path, ".png");

            GraphXmlToPng(doc, imgPath);
        }

        static void ConvertEncodedToPng(string path)
        {
            var doc = mxUtils.ParseXml(mxUtils.ReadFile(path));
            var encoded = doc.SelectSingleNode("/mxfile/diagram").InnerText;

            var deflated = Convert.FromBase64String(encoded);

            string urlencoded = null;
            using (var input = new MemoryStream(deflated))
            using (var output = new MemoryStream())
            using (var deflater = new DeflateStream(input, CompressionMode.Decompress))
            {
                deflater.CopyTo(output);
                urlencoded = Encoding.UTF8.GetString(output.ToArray());
            }

            var decoded = HttpUtility.UrlDecode(urlencoded);
            var decodedDoc = mxUtils.ParseXml(decoded);

            var imgPath = Path.ChangeExtension(path, ".png");
            GraphXmlToPng(decodedDoc, imgPath);
        }

        static void GraphXmlToPng(XmlDocument doc, string path)
        {
            var codec = new mxCodec(doc);

            var graph = new mxGraph();
            codec.Decode(doc.DocumentElement, graph.Model);

            Image img = mxCellRenderer.CreateImage(graph, null, 1, Color.White, true, new mxRectangle(0, 0, 1100, 850));

            img.Save(path, ImageFormat.Png);
        }

        static void LoadStencils()
        {
            const string StencilAws3Path = @"D:\Users\tetsu\Documents\20171007_drawio-sample\drawio\war\stencils\aws3.xml";

            var doc = mxUtils.ParseXml(mxUtils.ReadFile(StencilAws3Path));

            var rootName = doc.DocumentElement.GetAttribute("name");
            foreach (XmlNode node in doc.FirstChild.ChildNodes)
            {
                var element = (XmlElement)node;
                var stencil = new mxStencil(element);

                var name = element.GetAttribute("name")
                    .Replace(' ', '_')
                    .ToLower();
                if (!string.IsNullOrWhiteSpace(rootName))
                {
                    name = rootName + "." + name;
                }
                mxStencilRegistry.AddStencil(name, stencil);
            }
        }
    }
}
