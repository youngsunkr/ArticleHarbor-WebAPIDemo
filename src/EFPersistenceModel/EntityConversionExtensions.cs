namespace ArticleHarbor.EFPersistenceModel
{
    using System;
    using DomainModel.Models;

    internal static class EntityConversionExtensions
    {
        public static Article ToDomain(this EFDataAccess.Article article)
        {
            return new Article(
                article.Id,
                article.Provider,
                article.Guid,
                article.Subject,
                article.Body,
                article.Date,
                article.Url,
                article.UserId);
        }

        public static EFDataAccess.Article ToPersistence(this Article article)
        {
            return new EFDataAccess.Article
            {
                Id = article.Id,
                Provider = article.Provider,
                Guid = article.Guid,
                Subject = article.Subject,
                Body = article.Body,
                Date = article.Date,
                Url = article.Url,
                UserId = article.UserId
            };
        }

        public static Keyword ToDomain(this EFDataAccess.Keyword keyword)
        {
            return new Keyword(keyword.ArticleId, keyword.Word);
        }

        public static EFDataAccess.Keyword ToPersistence(
            this Keyword keyword)
        {
            return new EFDataAccess.Keyword
            {
                ArticleId = keyword.ArticleId,
                Word = keyword.Word
            };
        }

        public static User ToDomain(this EFDataAccess.User user, string roleName)
        {
            return new User(user.Id, (Role)Enum.Parse(typeof(Role), roleName), user.ApiKey);
        }

        public static Bookmark ToDomain(this EFDataAccess.Bookmark bookmark)
        {
            return new Bookmark(bookmark.UserId, bookmark.ArticleId);
        }

        public static EFDataAccess.Bookmark ToPersistence(this Bookmark bookmark)
        {
            return new EFDataAccess.Bookmark
            {
                ArticleId = bookmark.ArticleId,
                UserId = bookmark.UserId
            };
        }
    }
}