﻿namespace ArticleHarbor.EFPersistenceModel
{
    using System;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using DomainModel.Models;
    using EFDataAccess;
    using Article = DomainModel.Models.Article;

    public class ArticleRepository2 : Repository<Keys<int>, Article, EFDataAccess.Article>
    {
        private readonly ArticleHarborDbContext context;

        public ArticleRepository2(
            ArticleHarborDbContext context, DbSet<EFDataAccess.Article> dbSet)
            : base(context, dbSet)
        {
            this.context = context;
        }

        public override Task<Article> ConvertToModelAsync(EFDataAccess.Article persistence)
        {
            if (persistence == null)
                throw new ArgumentNullException("persistence");

            return this.ConvertToModelAsyncWith(persistence);
        }

        public override Task<EFDataAccess.Article> ConvertToPersistenceAsync(Article model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            return this.ConvertToPersistenceAsyncWith(model);
        }

        private Task<Article> ConvertToModelAsyncWith(EFDataAccess.Article persistence)
        {
            var article = new Article(
                persistence.Id,
                persistence.Provider,
                persistence.Guid,
                persistence.Subject,
                persistence.Body,
                persistence.Date,
                persistence.Url,
                persistence.UserId);

            return Task.FromResult(article);
        }

        private Task<EFDataAccess.Article> ConvertToPersistenceAsyncWith(Article model)
        {
            var article = new EFDataAccess.Article
            {
                Id = model.Id,
                Provider = model.Provider,
                Guid = model.Guid,
                Subject = model.Subject,
                Body = model.Body,
                Date = model.Date,
                Url = model.Url,
                UserId = model.UserId
            };

            return Task.FromResult(article);
        }
    }
}