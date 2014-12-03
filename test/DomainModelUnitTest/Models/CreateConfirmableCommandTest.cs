﻿namespace ArticleHarbor.DomainModel.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Jwc.Experiment.Xunit;
    using Xunit;

    public class CreateConfirmableCommandTest : IdiomaticTest<CreateConfirmableCommand>
    {
        [Test]
        public void SutIsModelCommand(CreateConfirmableCommand sut)
        {
            Assert.IsAssignableFrom<ModelCommand<object>>(sut);
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
                .WithAuto<CreateConfirmableCommand, Article>()
                .Create((roleName, sut, model) =>
                {
                    sut.Principal.Of(x => x.IsInRole(roleName));
                    var actual = sut.ExecuteAsync(model).Result;
                    Assert.Equal(sut, actual);
                });

            var keywordCases = TestCases.WithArgs(roleNames)
               .WithAuto<CreateConfirmableCommand, Keyword>()
               .Create((roleName, sut, model) =>
               {
                   sut.Principal.Of(x => x.IsInRole(roleName));
                   var actual = sut.ExecuteAsync(model).Result;
                   Assert.Equal(sut, actual);
               });

            var bookmarkCases = TestCases.WithArgs(roleNames)
                .WithAuto<CreateConfirmableCommand, Bookmark>()
                .Create((roleName, sut, model) =>
                {
                    sut.Principal.Of(x => x.IsInRole(roleName));
                    var actual = sut.ExecuteAsync(model).Result;
                    Assert.Equal(sut, actual);
                });

            return articleCases.Concat(keywordCases).Concat(bookmarkCases);
        }

        [Test]
        public IEnumerable<ITestCase> ExecuteAsyncWithInvalidRoleThrows()
        {
            yield return TestCase.WithAuto<CreateConfirmableCommand, Article>()
                .Create((sut, model) =>
                {
                    Assert.Throws<UnauthorizedException>(() => sut.ExecuteAsync(model).Result);
                });

            yield return TestCase.WithAuto<CreateConfirmableCommand, Keyword>()
                .Create((sut, model) =>
                {
                    Assert.Throws<UnauthorizedException>(() => sut.ExecuteAsync(model).Result);
                });

            yield return TestCase.WithAuto<CreateConfirmableCommand, Bookmark>()
                .Create((sut, model) =>
                {
                    Assert.Throws<UnauthorizedException>(() => sut.ExecuteAsync(model).Result);
                });
        }

        [Test]
        public void ExecuteAsyncUserAlwaysThrows(
            CreateConfirmableCommand sut,
            User user)
        {
            Assert.Throws<UnauthorizedException>(() => sut.ExecuteAsync(user).Result);
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