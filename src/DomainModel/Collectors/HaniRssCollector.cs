﻿namespace ArticleHarbor.DomainModel.Collectors
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Models;

    /// <summary>
    /// 한겨레 신문 뉴스 피드 수집기
    /// </summary>
    public class HaniRssCollector : IArticleCollector
    {
        private readonly string author;
        
        public HaniRssCollector(string author)
        {
            if (author == null)
                throw new ArgumentNullException("author");

            this.author = author;
            }

        public string Author
        {
            get { return this.author; }
        }

        public async Task<IEnumerable<Article>> CollectAsync()
        {
            using (var reader = new StreamReader(await GetStreamAsync(), Encoding.UTF8))
            {
                return from item in XDocument.Load(reader).Descendants("item")
                       select this.ConvertToArticle(item);
            }
        }

        private static async Task<Stream> GetStreamAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://www.hani.co.kr/");
                var response = await client.GetAsync("rss");
                return await response.Content.ReadAsStreamAsync();
            }
        }

        private Article ConvertToArticle(XElement item)
        {
            return new Article(
                id: -1,
                provider: "한겨레",
                guid: item.Element("guid").Value,
                subject: item.Element("title").Value,
                body: item.Element("description").Value,
                date: DateTime.Parse(item.Element("pubDate").Value, CultureInfo.CurrentCulture),
                url: item.Element("link").Value,
                userId: this.author);
        }
    }
}