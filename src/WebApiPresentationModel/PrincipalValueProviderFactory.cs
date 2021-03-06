﻿namespace ArticleHarbor.WebApiPresentationModel
{
    using System;
    using System.Web.Http.Controllers;
    using System.Web.Http.ValueProviders;

    public class PrincipalValueProviderFactory : ValueProviderFactory, IUriValueProviderFactory
    {
        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            if (actionContext == null)
                throw new ArgumentNullException("actionContext");

            return new PrincipalValueProvider(actionContext.RequestContext.Principal);
        }
    }
}