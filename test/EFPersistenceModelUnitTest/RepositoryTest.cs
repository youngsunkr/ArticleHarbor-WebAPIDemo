﻿namespace ArticleHarbor.EFPersistenceModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using DomainModel;
    using DomainModel.Models;
    using DomainModel.Queries;
    using DomainModel.Repositories;
    using EFDataAccess;
    using Jwc.Experiment.Xunit;
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

        protected override IEnumerable<MemberInfo> ExceptToVerifyGuardClause()
        {
            yield break;
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
                article = article.WithId(-1).WithUserId("user1");

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
                article = article.WithId(1).WithSubject(subject).WithUserId("user1");

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
                article = article.WithId(1).WithSubject(subject).WithUserId("user1");

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

        [Test]
        public IEnumerable<ITestCase> SelectAsyncWithPredicateReturnsCorrectResult()
        {
            yield return TestCase.WithAuto<TssArticleRepository>().Create(sut =>
            {
                var actual = sut.SelectAsync(Predicate.Equal("Id", 1)).Result;
                Assert.Equal(1, actual.Single().Id);
                Assert.Empty(sut.DbSet.Local);
            });
            yield return TestCase.WithAuto<TssArticleRepository>().Create(sut =>
            {
                var actual = sut.SelectAsync(Predicate.Equal("Guid", "1")).Result;
                Assert.Equal(1, actual.Single().Id);
                Assert.Empty(sut.DbSet.Local);
            });
            yield return TestCase.WithAuto<TssArticleRepository>().Create(sut =>
            {
                var actual = sut.SelectAsync(Predicate.Equal("body", "Body 1")).Result;
                Assert.Equal(1, actual.Single().Id);
                Assert.Empty(sut.DbSet.Local);
            });
            yield return TestCase.WithAuto<TssArticleRepository, IPredicate>().Create((sut, predicate) =>
            {
                predicate.Of(x => x.SqlText == "id <> @id");
                predicate.Of(x => x.Parameters == new IParameter[] { new Parameter("@id", -1) });

                var actual = sut.SelectAsync(predicate).Result;

                Assert.Equal(3, actual.Count());
                Assert.Empty(sut.DbSet.Local);
            });
            yield return TestCase.WithAuto<TssArticleRepository>().Create(sut =>
            {
                var actual = sut.SelectAsync(Predicate.None).Result;
                Assert.Equal(3, actual.Count());
            });
        }

        [Test]
        public IEnumerable<ITestCase> SelectAsyncWithSqlQueryReturnsCorrectResult()
        {
            yield return TestCase.WithAuto<TssArticleRepository>().Create(sut =>
            {
                var actual = sut.SelectAsync(
                    new SqlQuery(new NoTop(), new NoOrderByColumns(), new NoPredicate())).Result;

                Assert.Equal(3, actual.Count());
                Assert.Empty(sut.DbSet.Local);
            });
            yield return TestCase.WithAuto<TssArticleRepository>().Create(sut =>
            {
                var actual = sut.SelectAsync(
                    new SqlQuery(new Top(2), new NoOrderByColumns(), new NoPredicate())).Result;

                Assert.Equal(2, actual.Count());
                Assert.Empty(sut.DbSet.Local);
            });
            yield return TestCase.WithAuto<TssArticleRepository>().Create(sut =>
            {
                var actual = sut.SelectAsync(
                    new SqlQuery(new NoTop(), new NoOrderByColumns(), Predicate.Equal("Id", 1))).Result;
                Assert.Equal(1, actual.Single().Id);
            });
            yield return TestCase.WithAuto<TssArticleRepository>().Create(sut =>
            {
                var actual = sut.SelectAsync(
                    new SqlQuery(new Top(2), new NoOrderByColumns(), Predicate.Equal("UserId", "User2"))).Result;
                Assert.Equal(2, actual.Count());
            });
            yield return TestCase.WithAuto<TssArticleRepository>().Create(sut =>
            {
                var expected = new[] { "user2", "user2", "user1" };
                var actual = sut.SelectAsync(new SqlQuery(
                    Top.None,
                    new OrderByColumns(
                        new OrderByColumn("UserId", OrderDirection.Descending)),
                    Predicate.None)).Result;
                Assert.Equal(expected, actual.Select(x => x.UserId));
            });
            yield return TestCase.WithAuto<TssArticleRepository>().Create(sut =>
            {
                var expected = new[] { "user1", "user2", "user2" };
                var actual = sut.SelectAsync(new SqlQuery(
                    Top.None,
                    new OrderByColumns(
                        new OrderByColumn("UserId", OrderDirection.Ascending)),
                    Predicate.None)).Result;
                Assert.Equal(expected, actual.Select(x => x.UserId));
            });
            yield return TestCase.WithAuto<TssArticleRepository>().Create(sut =>
            {
                var actual = sut.SelectAsync(new SqlQuery(
                    Top.None,
                    new OrderByColumns(
                        new OrderByColumn("UserId", OrderDirection.Descending),
                        new OrderByColumn("Id", OrderDirection.Ascending)),
                    Predicate.None)).Result;
                Assert.Equal("2", actual.First().Guid);
            });
            yield return TestCase.WithAuto<TssArticleRepository>().Create(sut =>
            {
                var actual = sut.SelectAsync(new SqlQuery(
                    Top.None,
                    new OrderByColumns(
                        new OrderByColumn("UserId", OrderDirection.Ascending),
                        new OrderByColumn("Id", OrderDirection.Descending)),
                    Predicate.None)).Result;
                Assert.Equal("1", actual.First().Guid);
                Assert.Equal("3", actual.ElementAt(1).Guid);
            });
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
        
        [Test]
        public void InsertAsyncWithDuplicatedIdentityThrows(
            DbContextTransaction transaction,
            TssKeywordRepository sut)
        {
            try
            {
                var keyword = new Keyword(1, "WordA1");
                var e = Assert.Throws<AggregateException>(() => sut.InsertAsync(keyword).Result);
                Assert.IsType<DbUpdateException>(e.InnerException);
            }
            finally
            {
                transaction.Rollback();
                transaction.Dispose();
            }
        }

        [Test]
        public void DeleteAsyncCorrectlyDeletes(
            DbContextTransaction transaction,
            TssKeywordRepository sut)
        {
            try
            {
                var keys = new Keys<int, string>(1, "WordA1");

                sut.DeleteAsync(keys).Wait();
                sut.Context.SaveChanges();

                Assert.Equal(2, sut.DbSet.AsNoTracking().Count());
            }
            finally
            {
                transaction.Rollback();
                transaction.Dispose();
            }
        }

        [Test]
        public void DeleteAsyncThrowsWhenDeletingNonExistEntity(
            TssKeywordRepository sut, Keys<int, string> keys)
        {
            var e = Assert.Throws<AggregateException>(() => sut.DeleteAsync(keys).Wait());
            Assert.IsType<ArgumentException>(e.InnerException);
        }

        [Test]
        public IEnumerable<ITestCase> DeleteAsyncWithPredicateCorrectlyDeletes()
        {
            yield return TestCase.WithAuto<DbContextTransaction, TssKeywordRepository>().Create(
                (transaction, sut) =>
                {
                    try
                    {
                        sut.DeleteAsync(Predicate.Equal("ArticleId", 1)).Wait();
                        Assert.Equal(2, sut.DbSet.AsNoTracking().Count());
                        Assert.Empty(sut.DbSet.Local);
                    }
                    finally
                    {
                        transaction.Rollback();
                        transaction.Dispose();
                    }
                });
            yield return TestCase.WithAuto<DbContextTransaction, TssKeywordRepository>().Create(
                (transaction, sut) =>
                {
                    try
                    {
                        sut.DeleteAsync(Predicate.Equal("Word", "worda1")).Wait();
                        Assert.Equal(2, sut.DbSet.AsNoTracking().Count());
                        Assert.Empty(sut.DbSet.Local);
                    }
                    finally
                    {
                        transaction.Rollback();
                        transaction.Dispose();
                    }
                });
            yield return TestCase.WithAuto<DbContextTransaction, TssKeywordRepository>().Create(
                (transaction, sut) =>
                {
                    try
                    {
                        sut.DeleteAsync(Predicate.None).Wait();
                        Assert.Equal(0, sut.DbSet.AsNoTracking().Count());
                    }
                    finally
                    {
                        transaction.Rollback();
                        transaction.Dispose();
                    }
                });
        }
    }

    public class TssArticleRepository : Repository<Keys<int>, Article, EFDataAccess.Article>
    {
        private readonly ArticleHarborDbContext context;

        public TssArticleRepository(ArticleHarborDbContext context, DbSet<EFDataAccess.Article> dbSet)
            : base(context, dbSet)
        {
            this.context = context;
        }

        public override Task<Article> ConvertToModelAsync(EFDataAccess.Article persistence)
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

        public override Task<EFDataAccess.Article> ConvertToPersistenceAsync(Article model)
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

    public class TssKeywordRepository : Repository<Keys<int, string>, Keyword, EFDataAccess.Keyword>
    {
        public TssKeywordRepository(DbContext context, DbSet<EFDataAccess.Keyword> dbSet)
            : base(context, dbSet)
        {
        }

        public override Task<Keyword> ConvertToModelAsync(EFDataAccess.Keyword persistence)
        {
            return Task.FromResult(new Keyword(persistence.ArticleId, persistence.Word));
        }

        public override Task<EFDataAccess.Keyword> ConvertToPersistenceAsync(Keyword model)
        {
            return Task.FromResult(new EFDataAccess.Keyword
            {
                ArticleId = model.ArticleId,
                Word = model.Word
            });
        }
    }
}