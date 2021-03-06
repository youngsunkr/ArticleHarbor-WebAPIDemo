﻿namespace ArticleHarbor.WebApiPresentationModel
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using DomainModel;
    using DomainModel.Models;
    using Jwc.Experiment.Xunit;
    using Ploeh.AutoFixture;
    using Xunit;

    public class PermissionAuthorizationAttributeTest : IdiomaticTest<PermissionAuthorizationAttribute>
    {
        [Test]
        public void SutIsAuthorizationAttribute(
            PermissionAuthorizationAttribute sut)
        {
            Assert.IsAssignableFrom<AuthorizationFilterAttribute>(sut);
        }

        [Test]
        public async Task OnAuthorizationAsyncWithNullPrincipalRepliesWithUnauthorized(
            PermissionAuthorizationAttribute sut,
            HttpActionContext context)
        {
            context.RequestContext.Principal = null;
            context.Response = null;

            await sut.OnAuthorizationAsync(context, CancellationToken.None);

            Assert.Equal(HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }

        [Test]
        public async Task OnAuthorizationAsyncWithUnauthenticatedRepliesWithUnauthorized(
            PermissionAuthorizationAttribute sut,
            HttpActionContext context)
        {
            context.RequestContext.Principal.Identity.Of(x => x.IsAuthenticated == false);
            await sut.OnAuthorizationAsync(context, CancellationToken.None);
            Assert.Equal(HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }

        [Test]
        public IEnumerable<ITestCase> OnAuthorizationAsyncRepliesWithUnauthorizedWhenUserDoesNotHaveCorrectPermissions()
        {
            var testData = new[]
            {
                new
                {
                    Permissions = Permissions.DeleteAny,
                    RoleName = "User"
                },
                new
                {
                    Permissions = Permissions.Create,
                    RoleName = "User"
                },
                new
                {
                    Permissions = Permissions.DeleteAny,
                    RoleName = "Author"
                },
            };
            return TestCases.WithArgs(testData).WithAuto<HttpActionContext, IFixture>().Create(
                (d, context, fixture) =>
                {
                    fixture.Inject(d.Permissions);
                    context.RequestContext.Principal.Of(x => x.IsInRole(d.RoleName));
                    context.Response = null;
                    context.RequestContext.Principal.Identity.Of(x => x.IsAuthenticated == true);
                    var sut = fixture.Create<PermissionAuthorizationAttribute>();

                    sut.OnAuthorizationAsync(context, CancellationToken.None).Wait();

                    Assert.Equal(HttpStatusCode.Unauthorized, context.Response.StatusCode);
                });
        }

        [Test]
        public IEnumerable<ITestCase> OnAuthorizationAsyncRepliesWithSuccessWhenUserHasCorrectPermissions()
        {
            var testData = new[]
            {
                new
                {
                    Permissions = Permissions.DeleteAny,
                    RoleName = "Administrator"
                },
                new
                {
                    Permissions = Permissions.None,
                    RoleName = "User"
                },
                new
                {
                    Permissions = Permissions.None,
                    RoleName = "Author"
                },
                new
                {
                    Permissions = Permissions.None,
                    RoleName = "Administrator"
                },
                new
                {
                    Permissions = Permissions.Delete | Permissions.Create,
                    RoleName = "Author"
                },
                new
                {
                    Permissions = Permissions.Delete | Permissions.Create,
                    RoleName = "Administrator"
                },
                new
                {
                    Permissions = Permissions.Authentication,
                    RoleName = "User"
                }
            };
            return TestCases.WithArgs(testData).WithAuto<HttpActionContext, IFixture>().Create(
                (d, context, fixture) =>
                {
                    fixture.Inject(d.Permissions);
                    context.RequestContext.Principal.Of(x => x.IsInRole(d.RoleName));
                    context.Response = null;
                    context.RequestContext.Principal.Identity.Of(x => x.IsAuthenticated == true);
                    var sut = fixture.Create<PermissionAuthorizationAttribute>();

                    sut.OnAuthorizationAsync(context, CancellationToken.None).Wait();

                    Assert.Null(context.Response);
                });
        }

        [Test]
        public void OnAuthorizationAsyncThrowsWhenUserRoleIsInvalid(
            PermissionAuthorizationAttribute sut,
            HttpActionContext context,
            string roleName)
        {
            context.RequestContext.Principal.Of(x => x.IsInRole(roleName));
            context.RequestContext.Principal.Identity.Of(x => x.IsAuthenticated == true);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => sut.OnAuthorizationAsync(context, CancellationToken.None).Wait());
        }
    }
}