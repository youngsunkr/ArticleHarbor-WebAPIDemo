﻿namespace ArticleHarbor.EFPersistenceModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Linq;
    using System.Reflection;
    using DomainModel.Models;
    using DomainModel.Repositories;
    using EFDataAccess;
    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.Xunit;
    using Ploeh.SemanticComparison.Fluent;
    using Xunit;
    using Article = DomainModel.Models.Article;
    using Keyword = DomainModel.Models.Keyword;

    public abstract class RepositoryTest<TKeys, TModel, TPersistence>
        : IdiomaticTest<Repository<TKeys, TModel, TPersistence>>
        where TKeys : IKeys
        where TModel : IModel
        where TPersistence : class
    {
        [Test]
        public void SutIsRepository(Repository<TKeys, TModel, TPersistence> sut)
        {
            Assert.IsAssignableFrom<IRepository<TKeys, TModel>>(sut);
        }

        [Test]
        public void InitializeWithNullDatabaseThrows(IFixture fixture)
        {
            fixture.Inject<DbContext>(null);
            var e = Assert.Throws<TargetInvocationException>(
                () => fixture.Create<Repository<TKeys, TModel, TPersistence>>());
            Assert.IsType<ArgumentNullException>(e.InnerException);
        }

        [Test]
        public void InitializeWithNullDbSetThrows(IFixture fixture)
        {
            fixture.Inject<DbSet<TPersistence>>(null);
            var e = Assert.Throws<TargetInvocationException>(
                () => fixture.Create<Repository<TKeys, TModel, TPersistence>>());
            Assert.IsType<ArgumentNullException>(e.InnerException);
        }

        [Test]
        public void DatabaseIsCorrect(
            [Frozen] DbContext expected,
            Repository<TKeys, TModel, TPersistence> sut)
        {
            var actual = sut.Context;
            Assert.Equal(expected, actual);
        }

        [Test]
        public void DbSetIsCorrect(
            [Frozen] DbSet<TPersistence> expected,
            Repository<TKeys, TModel, TPersistence> sut)
        {
            var actual = sut.DbSet;
            Assert.Equal(expected, actual);
        }

        protected override IEnumerable<MemberInfo> ExceptToVerifyInitialization()
        {
            yield return this.Properties.Select(x => x.Context);
            yield return this.Properties.Select(x => x.DbSet);
        }
    }

    public class RepositoryOfArticleTest : RepositoryTest<Keys<int>, Article, EFDataAccess.Article>
    {
        [Test]
        public void FindAsyncReturnsCorrectResult(TssArticleRepository sut)
        {
            var keys = new Keys<int>(1);
            var article = sut.DbSet.Find(keys.ToArray());

            Article actual = sut.FindAsync(keys).Result;
            
            article.AsSource().OfLikeness<Article>()
                .Without(x => x.UserId)
                .ShouldEqual(actual);
        }

        [Test]
        public void SelectAsyncReturnsCorrectResult(TssArticleRepository sut)
        {
            var articles = sut.DbSet.AsNoTracking().ToArray();

            var actual = sut.SelectAsync().Result.ToArray();

            Assert.Equal(3, actual.Length);
            int index = 0;
            foreach (var article in articles)
                article.AsSource().OfLikeness<Article>()
                    .Without(x => x.UserId)
                    .ShouldEqual(actual[index++]);
            Assert.Equal(0, sut.DbSet.Local.Count);
        }

        [Test]
        public void InsertAsyncReturnsCorrectResult(
            DbContextTransaction transaction,
            TssArticleRepository sut,
            Article article)
        {
            try
            {
                var actual = sut.InsertAsync(article).Result;
                
                var count = sut.DbSet.AsNoTracking().Count();
                Assert.Equal(4, count);
                var expectedId = sut.DbSet.AsNoTracking().Select(x => x.Id).Max();
                Assert.Equal(expectedId, actual.Id);
                article.AsSource().OfLikeness<Article>()
                    .Without(x => x.Id)
                    .Without(x => x.UserId)
                    .ShouldEqual(actual);
            }
            finally
            {
                transaction.Rollback();
                transaction.Dispose();
            }
        }
        
        [Test]
        public void UpdateAsyncCorrectlyUpdatesWhenContextAlreadyHasCorrespondingEntity(
            DbContextTransaction transaction,
            TssArticleRepository sut,
            Article article,
            string subject)
        {
            try
            {
                var actual = sut.DbSet.Find(1);
                article = article.WithId(1).WithSubject(subject);

                sut.UpdateAsync(article).Wait();
                sut.Context.SaveChanges();

                actual = sut.DbSet.Find(1);
                Assert.Equal(subject, actual.Subject);
            }
            finally
            {
                transaction.Rollback();
                transaction.Dispose();
            }
        }

        [Test]
        public void UpdateAsyncCorrectlyUpdatesWhenContextDoesNotHaveCorrespondingEntity(
            DbContextTransaction transaction,
            TssArticleRepository sut,
            Article article,
            string subject)
        {
            try
            {
                article = article.WithId(1).WithSubject(subject);

                sut.UpdateAsync(article).Wait();
                sut.Context.SaveChanges();

                var actual = sut.DbSet.Find(1);
                Assert.Equal(subject, actual.Subject);
            }
            finally
            {
                transaction.Rollback();
                transaction.Dispose();
            }
        }

        [Test]
        public void UpdateAsyncWithIncorrectIdentityThrows(
            TssArticleRepository sut,
            Article article)
        {
            article = article.WithId(4);
            var e = Assert.Throws<AggregateException>(() => sut.UpdateAsync(article).Wait());
            Assert.IsType<ArgumentException>(e.InnerException);
        }
    }

    public class RepositoryOfKeywordTest : RepositoryTest<Keys<int, string>, Keyword, EFDataAccess.Keyword>
    {
        [Test]
        public void InsertAsyncReturnsCorrectResult(
            DbContextTransaction transaction,
            TssKeywordRepository sut,
            string word)
        {
            try
            {
                var keyword = new Keyword(1, word);

                var actual = sut.InsertAsync(keyword).Result;

                var count = sut.DbSet.AsNoTracking().Count();
                Assert.Equal(4, count);
                Assert.Equal(1, actual.ArticleId);
                keyword.AsSource().OfLikeness<Keyword>()
                    .ShouldEqual(actual);
            }
            finally
            {
                transaction.Rollback();
                transaction.Dispose();
            }
        }
    }

    public class TssArticleRepository : Repository<Keys<int>, Article, EFDataAccess.Article>
    {
        public TssArticleRepository(DbContext context, DbSet<EFDataAccess.Article> dbSet)
            : base(context, dbSet)
        {
        }

        public override Article ConvertToModel(EFDataAccess.Article persistence)
        {
            return new Article(
                persistence.Id,
                persistence.Provider,
                persistence.Guid,
                persistence.Subject,
                persistence.Body,
                persistence.Date,
                persistence.Url,
                "dummy userid");
        }

        public override EFDataAccess.Article ConvertToPersistence(Article model)
        {
            return new EFDataAccess.Article
            {
                Id = model.Id,
                Provider = model.Provider,
                Guid = model.Guid,
                Subject = model.Subject,
                Body = model.Body,
                Date = model.Date,
                Url = model.Url
            };
        }
    }

    public class TssKeywordRepository : Repository<Keys<int, string>, Keyword, EFDataAccess.Keyword>
    {
        public TssKeywordRepository(DbContext context, DbSet<EFDataAccess.Keyword> dbSet)
            : base(context, dbSet)
        {
        }

        public override Keyword ConvertToModel(EFDataAccess.Keyword persistence)
        {
            return new Keyword(persistence.ArticleId, persistence.Word);
        }

        public override EFDataAccess.Keyword ConvertToPersistence(Keyword model)
        {
            return new EFDataAccess.Keyword
            {
                ArticleId = model.ArticleId,
                Word = model.Word
            };
        }
    }
}