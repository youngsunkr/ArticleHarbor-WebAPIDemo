﻿namespace ArticleHarbor.WebApiPresentationModel.Models
{
    using System.Collections.Generic;
    using Jwc.Experiment.Xunit;
    using Ploeh.Albedo;

    public class PostArticleViewModelTest
    {
        [Test]
        public IEnumerable<ITestCase> PropertiesAreReadWritable()
        {
            var properties = new Properties<PostArticleViewModel>();
            var testData = new[]
            {
                properties.Select(x => x.Provider),
                properties.Select(x => x.Guid),
                properties.Select(x => x.Subject),
                properties.Select(x => x.Body),
                properties.Select(x => x.Date),
                properties.Select(x => x.Url)
            };

            return TestCases.WithArgs(testData).WithAuto<ReadWritablePropertyAssertion>()
                .Create((p, a) => a.Verify(p));
        }
    }
}