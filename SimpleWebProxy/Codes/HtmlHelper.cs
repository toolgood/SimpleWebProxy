using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace SimpleWebProxy.Codes
{
    public class HtmlHelper
    {
        public static string ReplaceHtml(string html, string serverHost, string tarhost, Dictionary<string, string> hostDict)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            var linkNodes = document.QuerySelectorAll("link");
            foreach (var node in linkNodes) Replace(node, "href", serverHost, tarhost, hostDict);
            var scriptNodes = document.QuerySelectorAll("script");
            foreach (var node in scriptNodes) Replace(node, "src", serverHost, tarhost, hostDict);
            var imgNodes = document.QuerySelectorAll("img");
            foreach (var node in imgNodes) Replace(node, "src", serverHost, tarhost, hostDict);
            var aNodes = document.QuerySelectorAll("a");
            foreach (var node in aNodes) Replace(node, "href", serverHost, tarhost, hostDict);

            using (var ms = new MemoryStream())
            {
                document.Save(ms, Encoding.UTF8);
                var bytes = ms.ToArray();
                return Encoding.UTF8.GetString(bytes);
            }
        }

        public static string ReplaceCss(string css, string serverHost, string tarhost, Dictionary<string, string> hostDict)
        {
            StringBuilder stringBuilder = new StringBuilder(css);

            stringBuilder.Replace("//" + tarhost, "//" + serverHost);

            foreach (var host in hostDict)
            {
                css.Replace("//" + host.Key, "//" + serverHost + "/#host-" + host.Value + "#");
            }
            return stringBuilder.ToString();
        }

        public static string ReplaceScript(string script, string serverHost, string tarhost, Dictionary<string, string> hostDict)
        {
            StringBuilder stringBuilder = new StringBuilder(script);

            stringBuilder.Replace("//" + tarhost, "//" + serverHost);

            foreach (var host in hostDict)
            {
                script.Replace("//" + host.Key, "//" + serverHost + "/#host-" + host.Value + "#");
            }
            return stringBuilder.ToString();
        }

        private static void Replace(HtmlNode node, string attrName, string serverHost, string tarhost, Dictionary<string, string> hostDict)
        {
            var url = node.Attributes[attrName];
            if (url == null) { return; }
            if (url.Value.Contains("//" + tarhost))
            {
                url.Value = url.Value.Replace("//" + tarhost, "//" + serverHost);
                return;
            }
            foreach (var host in hostDict)
            {
                if (url.Value.Contains("//" + host.Key))
                {
                    url.Value = url.Value.Replace("//" + host.Key, "//" + serverHost + "/#host-" + host.Value + "#");
                    break;
                }
            }
        }

    }
}
