﻿namespace ArticleHarbor.ArticleCollectorDaemon
{
    using System;
    using System.Data.Entity;
    using ArticleHarbor.DomainModel;
    using ArticleHarbor.DomainModel.Collectors;
    using ArticleHarbor.DomainModel.Services;
    using ArticleHarbor.EFDataAccess;
    using ArticleHarbor.EFPersistenceModel;

    internal class Programss
    {
        private static void Main()
        {
            using (var context = new ArticleHarborDbContext(
                new ArticleHarborDbContextTestInitializer()))
            {
                var executor = CreateExecutor(context);
                executor.ExecuteAsync().Wait();
                context.SaveChanges();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)", Justification = "It is appropriate to represent as literal because of simplity.")]
        private static ArticleCollectingExecutor CreateExecutor(ArticleHarborDbContext context)
        {
            return new ArticleCollectingExecutor(
                collector: new CompositeArticleCollector(
                    new IArticleCollector[]
                    {
                        new HaniRssCollector("user1"),
                        new ArticleCollectorTransformation(
                            new CompositeArticleCollector(
                                new IArticleCollector[]
                                {
                                    new FacebookRssCollector("user2", "177323639028540"), // ASP.NET Korea group
                                    new FacebookRssCollector("user2", "200708093411111") // C# study group
                                }),
                            new SubjectFromBodyTransformation(50))
                    }),
                service: new ArticleService(
                    new ArticleRepository(context),
                    new ArticleWordService(
                        new ArticleWordRepository(context),
                        new ArticleRepository(context),
                        KoreanNounExtractor.Execute)),
                delay: 10,
                callback: a => Console.WriteLine("Added: " + a.Subject));
        }
    }
}