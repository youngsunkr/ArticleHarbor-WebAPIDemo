﻿namespace ArticleHarbor.WebApiPresentationModel.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using DomainModel.Models;
    using DomainModel.Repositories;
    using DomainModel.Services;

    [Authorize]
    public class BookmarksController : ApiController
    {
        private readonly IBookmarkService bookmarkService;
        private readonly IRepositories repositories;
        private readonly IModelCommand<IModel> selectArticlesCommand;
        private readonly IModelCommand<IModel> insertCommand;
        private readonly IModelCommand<IModel> deleteCommand;

        public BookmarksController(
            IBookmarkService bookmarkService,
            IRepositories repositories,
            IModelCommand<IModel> selectArticlesCommand,
            IModelCommand<IModel> insertCommand,
            IModelCommand<IModel> deleteCommand)
        {
            if (bookmarkService == null)
                throw new ArgumentNullException("bookmarkService");

            if (repositories == null)
                throw new ArgumentNullException("repositories");

            if (selectArticlesCommand == null)
                throw new ArgumentNullException("selectArticlesCommand");

            if (insertCommand == null)
                throw new ArgumentNullException("insertCommand");

            if (deleteCommand == null)
                throw new ArgumentNullException("deleteCommand");

            this.bookmarkService = bookmarkService;
            this.repositories = repositories;
            this.selectArticlesCommand = selectArticlesCommand;
            this.insertCommand = insertCommand;
            this.deleteCommand = deleteCommand;
        }

        public IBookmarkService BookmarkService
        {
            get { return this.bookmarkService; }
        }

        public IRepositories Repositories
        {
            get { return this.repositories; }
        }

        public IModelCommand<IModel> SelectArticlesCommand
        {
            get { return this.selectArticlesCommand; }
        }

        public IModelCommand<IModel> InsertCommand
        {
            get { return this.insertCommand; }
        }

        public IModelCommand<IModel> DeleteCommand
        {
            get { return this.deleteCommand; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This is action method.")]
        public async Task<IEnumerable<Article>> GetAsync()
        {
            var userId = this.User.Identity.Name;
            var user = await this.repositories.Users.FindAsync(new Keys<string>(userId));

            return (await user.ExecuteAsync(this.selectArticlesCommand)).Cast<Article>();
        }

        public Task PostAsync(int id)
        {
            var actor = this.User.Identity.Name;
            var bookmark = new Bookmark(actor, id);
            return this.bookmarkService.AddAsync(bookmark);
        }

        public Task DeleteAsync(int id)
        {
            var actor = this.User.Identity.Name;
            var bookmark = new Bookmark(actor, id);
            return this.bookmarkService.RemoveAsync(bookmark);
        }
    }
}