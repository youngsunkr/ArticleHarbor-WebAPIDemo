﻿namespace WebApiPresentationModel
{
    using System;
    using System.Web.Http.ExceptionHandling;

    public class UnhandledExceptionLogger : ExceptionLogger
    {
        public override bool ShouldLog(ExceptionLoggerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (typeof(ArgumentException).IsInstanceOfType(context.Exception))
                return false;

            return base.ShouldLog(context);
        }
    }
}