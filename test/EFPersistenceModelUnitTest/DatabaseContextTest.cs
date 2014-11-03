﻿namespace EFPersistenceModelUnitTest
{
    using System.Collections.Generic;
    using System.Reflection;
    using DomainModel;
    using EFDataAccess;
    using EFPersistenceModel;
    using Moq;
    using Moq.Protected;
    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.Xunit;
    using Xunit;

    public class DatabaseContextTest : IdiomaticTest<DatabaseContext>
    {
        [Test]
        public void SutIsDatabaseContext(DatabaseContext sut)
        {
            Assert.IsAssignableFrom<IDatabaseContext>(sut);
        }

        [Test]
        public void ArticlesIsCorrect(DatabaseContext sut)
        {
            Assert.NotNull(sut.Context.Articles);
            
            var actual = sut.Articles;

            var repository = Assert.IsAssignableFrom<ArticleRepository>(sut.Articles);
            Assert.Same(sut.Context.Articles, repository.Articles);
        }

        [Test]
        public void ArticlesAlwaysReturnsSameInstance(DatabaseContext sut)
        {
            var actual = sut.Articles;
            Assert.Same(sut.Articles, actual);
        }

        [Test]
        public void DisposeCorrectlyDisposesEFContext(
            Mock<ArticleHarborContext> context,
            IFixture fixture)
        {
            fixture.Inject(context.Object);
            var sut = fixture.Create<DatabaseContext>();

            sut.Dispose();
            sut.Dispose();

            context.Protected().Verify("Dispose", Times.Once(), true);
        }

        [Test]
        public void DisposeSavesChanges(
            Mock<ArticleHarborContext> context,
            IFixture fixture)
        {
            fixture.Inject(context.Object);
            var sut = fixture.Create<DatabaseContext>();

            sut.Dispose();

            context.Verify(x => x.SaveChanges(), Times.Once());
        }

        protected override IEnumerable<MemberInfo> ExceptToVerifyInitialization()
        {
            yield return this.Properties.Select(x => x.Articles);
        }
    }
}