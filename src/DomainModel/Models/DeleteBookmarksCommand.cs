﻿namespace ArticleHarbor.DomainModel.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Repositories;

    public class DeleteBookmarksCommand : ModelCommand<IEnumerable<IModel>>
    {
        private readonly IRepositories repositories;

        public DeleteBookmarksCommand(IRepositories repositories)
        {
            if (repositories == null)
                throw new ArgumentNullException("repositories");

            this.repositories = repositories;
        }

        public override IEnumerable<IModel> Value
        {
            get { yield break; }
        }

        public IRepositories Repositories
        {
            get { return this.repositories; }
        }

        public override Task<IModelCommand<IEnumerable<IModel>>> ExecuteAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return this.ExecuteAsyncWith(user);
        }

        public override Task<IModelCommand<IEnumerable<IModel>>> ExecuteAsync(Article article)
        {
            if (article == null)
                throw new ArgumentNullException("article");

            return this.ExecuteAsyncWith(article);
        }

        private async Task<IModelCommand<IEnumerable<IModel>>> ExecuteAsyncWith(User user)
        {
            await this.repositories.Bookmarks.ExecuteDeleteCommandAsync(
                new EqualPredicate("UserId", user.Id));

            return this;
        }

        private async Task<IModelCommand<IEnumerable<IModel>>> ExecuteAsyncWith(Article article)
        {
            await this.repositories.Bookmarks.ExecuteDeleteCommandAsync(
                new EqualPredicate("ArticleId", article.Id));

            return this;
        }
    }
}