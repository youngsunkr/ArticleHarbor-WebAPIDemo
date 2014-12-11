﻿namespace ArticleHarbor.DomainModel.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class TransformableCommand<TReturn> : IModelCommand<TReturn>
    {
        private readonly IModelTransformer transformer;
        private readonly IModelCommand<TReturn> innerCommand;

        public TransformableCommand(
            IModelTransformer transformer, IModelCommand<TReturn> innerCommand)
        {
            if (transformer == null)
                throw new ArgumentNullException("transformer");

            if (innerCommand == null)
                throw new ArgumentNullException("innerCommand");

            this.transformer = transformer;
            this.innerCommand = innerCommand;
        }

        public IModelTransformer Transformer
        {
            get { return this.transformer; }
        }

        public IModelCommand<TReturn> InnerCommand
        {
            get { return this.innerCommand; }
        }

        public Task<IEnumerable<TReturn>> ExecuteAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TReturn>> ExecuteAsync(Article article)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TReturn>> ExecuteAsync(Keyword keyword)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TReturn>> ExecuteAsync(Bookmark bookmark)
        {
            throw new NotImplementedException();
        }
    }
}