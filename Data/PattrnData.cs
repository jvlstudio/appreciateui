using System;
using System.Web;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace MobilePatterns.Data
{
    public class PattrnData : PatternSource
    {
        public static Uri BaseUrl = new Uri("http://pttrns.com");

        public List<WebMenu> GetMenus()
        {
            var w = new HttpWebRequest(BaseUrl);
            var response = w.GetResponse();
            var models = new List<WebMenu>();

            using (var r = response.GetResponseStream())
            {
                var html = new HtmlAgilityPack.HtmlDocument();
                html.Load(r);

                var nodes = html.DocumentNode.SelectNodes("//div[@id='container']/nav/ul[2]/li/a");
                foreach (var n in nodes)
                {
                    try
                    {
                        var uri = new Uri(BaseUrl, n.Attributes["href"].Value);
                        models.Add(new WebMenu() { Name = n.InnerText, Uri = uri, Source = this });
                    }
                    catch 
                    {
                    }
                }
            }

            return models;
        }

        public List<PatternImages> GetPatterns(Uri uri)
        {
            var w = new HttpWebRequest(uri);
            var response = w.GetResponse();
            var models = new List<PatternImages>();

            using (var r = response.GetResponseStream())
            {
                var html = new HtmlAgilityPack.HtmlDocument();
                html.Load(r);

                var nodes = html.DocumentNode.SelectNodes("//div[@id='main']/section/a/img");
                foreach (var n in nodes)
                {
                    try
                    {
                        Uri url = null;
                        if (Utils.Util.IsRetina && n.Attributes.Contains("data-pttrns-retina"))
                        {
                            url = new Uri(BaseUrl, n.Attributes["data-pttrns-retina"].Value);
                        }

                        //Fallback
                        if (url == null)
                            url = new Uri(BaseUrl, n.Attributes["src"].Value);

                        models.Add(new PatternImages() { Image = url.ToString() });
                    }
                    catch 
                    {
                    }
                }
            }

            return models;
        }
    }
}
