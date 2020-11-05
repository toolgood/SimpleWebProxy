using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Hosting;

namespace SimpleWebProxy.Codes
{
    public class DownloadHelper
    {
        private static Regex urlRegex = new Regex("#host(-.*?)#", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

        public static bool HasCookie(HttpContext context)
        {
            foreach (var item in context.Request.Cookies)
            {
                if (item.Key == "host")
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task Download(HttpContext context)
        {
            var scheme = context.Request.Scheme;
            var serverHost = context.Request.Host.Host;
            if (context.Request.Host.Port != 80) { serverHost += ":" + context.Request.Host.Port; }

            Dictionary<string, string> cookies = new Dictionary<string, string>();
            Dictionary<string, string> hostDict = new Dictionary<string, string>();
            string tarHost = null;
            foreach (var item in context.Request.Cookies)
            {
                if (item.Key == "host")
                {
                    tarHost = item.Value;
                }
                else if (item.Key.StartsWith("host-"))
                {
                    hostDict[item.Value] = item.Key.Substring(5);
                }
                else
                {
                    cookies[item.Key] = item.Value;
                }
            }
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (var item in context.Request.Headers) { headers[item.Key] = item.Value; }

            WebClientEx webClient = new WebClientEx();
            foreach (var item in context.Request.Headers)
            {
                if (item.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase)) continue;
                if (item.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase)) continue;
                if (item.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)) continue;

                else if (item.Key.Equals("Referer", StringComparison.OrdinalIgnoreCase))
                    webClient.Headers.Add("Referer", item.Value.ToString().Replace("//" + tarHost, "//" + serverHost));
                else
                    webClient.Headers.Add(item.Key, item.Value);
            }
            foreach (var item in cookies)
            {
                webClient.AddCookie(scheme + "://" + tarHost, item.Key, item.Value);
            }

            var path = context.Request.GetEncodedPathAndQuery();
            var url = scheme + "://" + tarHost + path;
            var m = urlRegex.Match(url);
            if (m.Success)
            {
                var name = m.Groups[0].Value;
                url = scheme + "://" + hostDict[name] + path.Replace(m.Value, "");
            }

            var datas = webClient.DownloadData(url);
            foreach (var item in webClient.ResponseHeaders.AllKeys)
            {
                context.Response.Headers[item] = webClient.ResponseHeaders[item].ToString();
            }
            context.Response.StatusCode = 200;
            if (webClient.ResponseHeaders["content-type"].Contains("text/html", StringComparison.OrdinalIgnoreCase))
            {
                var html = Encoding.UTF8.GetString(datas);
                html = HtmlHelper.ReplaceHtml(html, serverHost, tarHost, hostDict);

                await context.Response.WriteAsync(html);
                return;
            }
            if (webClient.ResponseHeaders["content-type"].Contains("text/css", StringComparison.OrdinalIgnoreCase))
            {
                var html = Encoding.UTF8.GetString(datas);
                html = HtmlHelper.ReplaceCss(html, serverHost, tarHost, hostDict);

                await context.Response.WriteAsync(html);
                return;
            }
            if (webClient.ResponseHeaders["content-type"].Contains("javascript", StringComparison.OrdinalIgnoreCase))
            {
                var html = Encoding.UTF8.GetString(datas);
                html = HtmlHelper.ReplaceScript(html, serverHost, tarHost, hostDict);

                await context.Response.WriteAsync(html);
                return;
            }
            await context.Response.Body.WriteAsync(datas, 0, datas.Length);
        }

        public static async Task Post(HttpContext context)
        {
            var scheme = context.Request.Scheme;
            var serverHost = context.Request.Host.Host;
            if (context.Request.Host.Port != 80) { serverHost += ":" + context.Request.Host.Port; }

            Dictionary<string, string> cookies = new Dictionary<string, string>();
            Dictionary<string, string> hostDict = new Dictionary<string, string>();
            string tarHost = null;
            foreach (var item in context.Request.Cookies)
            {
                if (item.Key == "host")
                {
                    tarHost = item.Value;
                }
                else if (item.Key.StartsWith("host-"))
                {
                    hostDict[item.Value] = item.Key.Substring(5);
                }
                else
                {
                    cookies[item.Key] = item.Value;
                }
            }
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (var item in context.Request.Headers) { headers[item.Key] = item.Value; }

            WebClientEx webClient = new WebClientEx();
            foreach (var item in context.Request.Headers)
            {
                if (item.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase)) continue;
                if (item.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase)) continue;
                if (item.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)) continue;

                else if (item.Key.Equals("Referer", StringComparison.OrdinalIgnoreCase))
                    webClient.Headers.Add("Referer", item.Value.ToString().Replace("//" + tarHost, "//" + serverHost));
                else
                    webClient.Headers.Add(item.Key, item.Value);
            }
            foreach (var item in cookies)
            {
                webClient.AddCookie(scheme + "://" + tarHost, item.Key, item.Value);
            }

            var path = context.Request.GetEncodedPathAndQuery();
            var url = scheme + "://" + tarHost + path;
            var m = urlRegex.Match(url);
            if (m.Success)
            {
                var name = m.Groups[0].Value;
                url = scheme + "://" + hostDict[name] + path.Replace(m.Value, "");
            }
            var body = await context.Request.BodyReader.ReadAsync();

            var datas = webClient.UploadData(url, body.Buffer.ToArray());
            foreach (var item in webClient.ResponseHeaders.AllKeys)
            {
                context.Response.Headers[item] = webClient.ResponseHeaders[item].ToString();
            }
            context.Response.StatusCode = 200;
            if (webClient.ResponseHeaders["content-type"].Contains("text/html", StringComparison.OrdinalIgnoreCase))
            {
                var html = Encoding.UTF8.GetString(datas);
                html = HtmlHelper.ReplaceHtml(html, serverHost, tarHost, hostDict);

                await context.Response.WriteAsync(html);
                return;
            }
            if (webClient.ResponseHeaders["content-type"].Contains("text/css", StringComparison.OrdinalIgnoreCase))
            {
                var html = Encoding.UTF8.GetString(datas);
                html = HtmlHelper.ReplaceCss(html, serverHost, tarHost, hostDict);

                await context.Response.WriteAsync(html);
                return;
            }
            if (webClient.ResponseHeaders["content-type"].Contains("javascript", StringComparison.OrdinalIgnoreCase))
            {
                var html = Encoding.UTF8.GetString(datas);
                html = HtmlHelper.ReplaceScript(html, serverHost, tarHost, hostDict);

                await context.Response.WriteAsync(html);
                return;
            }
            await context.Response.Body.WriteAsync(datas, 0, datas.Length);
        }

    }
}
