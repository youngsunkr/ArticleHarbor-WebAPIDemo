﻿namespace ArticleHarbor.DomainModel.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Jwc.Experiment.Xunit;
    using Xunit;

    public class InsertConfirmableCommandTest : IdiomaticTest<InsertConfirmableCommand>
    {
        [Test]
        public void SutIsModelCommand(InsertConfirmableCommand sut)
        {
            Assert.IsAssignableFrom<ModelCommand<IEnumerable<IModel>>>(sut);
        }

        [Test]
        public void ExecuteAsyncUserThrows(
            InsertConfirmableCommand sut,
            User user)
        {
            Assert.Throws<NotSupportedException>(() => sut.ExecuteAsync(user).Result);
        }

        [Test]
        public IEnumerable<ITestCase> ExecuteAsyncWithValidRoleDoesNotThrow()
        {
            var roleNames = new[]
            {
                "Author",
                "Administrator"
            };

            var articleCases = TestCases.WithArgs(roleNames)
                .WithAuto<InsertConfirmableCommand, Article>()
                .Create((roleName, sut, model) =>
                {
                    sut.Principal.Of(x => x.IsInRole(roleName) == true);
                    var actual = sut.ExecuteAsync(model).Result;
                    ////Assert.Equal(sut, actual);
                });

            var keywordCases = TestCases.WithArgs(roleNames)
               .WithAuto<InsertConfirmableCommand, Keyword>()
               .Create((roleName, sut, model) =>
               {
                   sut.Principal.Of(x => x.IsInRole(roleName) == true);
                   var actual = sut.ExecuteAsync(model).Result;
                   ////Assert.Equal(sut, actual);
               });

            var bookmarkCases = TestCases.WithArgs(roleNames)
                .WithAuto<InsertConfirmableCommand, Bookmark>()
                .Create((roleName, sut, model) =>
                {
                    sut.Principal.Of(x => x.IsInRole(roleName) == true);
                    var actual = sut.ExecuteAsync(model).Result;
                    ////Assert.Equal(sut, actual);
                });

            return articleCases.Concat(keywordCases).Concat(bookmarkCases);
        }

        [Test]
        public IEnumerable<ITestCase> ExecuteAsyncWithInvalidRoleThrows()
        {
            yield return TestCase.WithAuto<InsertConfirmableCommand, Article>()
                .Create((sut, model) =>
                {
                    Assert.Throws<UnauthorizedException>(() => sut.ExecuteAsync(model).Result);
                });

            yield return TestCase.WithAuto<InsertConfirmableCommand, Keyword>()
                .Create((sut, model) =>
                {
                    Assert.Throws<UnauthorizedException>(() => sut.ExecuteAsync(model).Result);
                });

            yield return TestCase.WithAuto<InsertConfirmableCommand, Bookmark>()
                .Create((sut, model) =>
                {
                    Assert.Throws<UnauthorizedException>(() => sut.ExecuteAsync(model).Result);
                });
        }

        [Test]
        public void ExecuteAsyncBookmarkWithUserRoleDoesNotThrow(
            InsertConfirmableCommand sut,
            Bookmark bookmark)
        {
            sut.Principal.Of(x => x.IsInRole("User") == true);
            var actual = sut.ExecuteAsync(bookmark).Result;
            ////Assert.Equal(sut, actual);
        }

        protected override IEnumerable<MemberInfo> ExceptToVerifyGuardClause()
        {
            yield return this.Methods.Select(x => x.ExecuteAsync(default(Article)));
            yield return this.Methods.Select(x => x.ExecuteAsync(default(Keyword)));
            yield return this.Methods.Select(x => x.ExecuteAsync(default(Bookmark)));
            yield return this.Methods.Select(x => x.ExecuteAsync(default(User)));
        }
    }
}