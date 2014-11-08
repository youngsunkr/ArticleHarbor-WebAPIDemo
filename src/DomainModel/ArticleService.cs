﻿namespace DomainModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository articles;
        private readonly IArticleWordRepository articleWords;
        private readonly Func<string, IEnumerable<string>> nounExtractor;

        public ArticleService(
            IArticleRepository articles,
            IArticleWordRepository articleWords,
            Func<string, IEnumerable<string>> nounExtractor)
        {
            if (articles == null)
                throw new ArgumentNullException("articles");

            if (articleWords == null)
                throw new ArgumentNullException("articleWords");

            if (nounExtractor == null)
                throw new ArgumentNullException("nounExtractor");

            this.articles = articles;
            this.articleWords = articleWords;
            this.nounExtractor = nounExtractor;
        }

        public IArticleRepository Articles
        {
            get { return this.articles; }
        }

        public IArticleWordRepository ArticleWords
        {
            get { return this.articleWords; }
        }

        public Func<string, IEnumerable<string>> NounExtractor
        {
            get { return this.nounExtractor; }
        }

        public Task<IEnumerable<Article>> GetAsync()
        {
            return this.articles.SelectAsync();
        }

        public Task<Article> SaveAsync(Article article)
        {
            if (article == null)
                throw new ArgumentNullException("article");

            return this.SaveAsyncImpl(article);
        }

        public Task RemoveAsync(int id)
        {
            return this.articles.DeleteAsync(id);
        }

        private async Task InsertArticleWordsAsync(Article article)
        {
            var words = await Task.Run(() => this.nounExtractor(article.Subject).ToArray())
                .ConfigureAwait(false);

            await Task.WhenAll(
                words.Select(x => this.ArticleWords.InsertAsync(new ArticleWord(article.Id, x))));
        }

        private async Task<Article> SaveAsyncImpl(Article article)
        {
            var oldArticle = await this.articles.SelectAsync(article.Id);
            if (oldArticle == null)
            {
                var newArticle = await this.articles.InsertAsync(article);
                await this.InsertArticleWordsAsync(newArticle);
                return newArticle;
            }

            await this.articles.UpdateAsync(article);
            if (article.Subject != oldArticle.Subject)
            {
                await this.ArticleWords.DeleteAsync(article.Id);
                await this.InsertArticleWordsAsync(article);
            }

            return article;
        }
    }
}