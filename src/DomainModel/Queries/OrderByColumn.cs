﻿namespace ArticleHarbor.DomainModel.Queries
{
    using System;

    public class OrderByColumn : IOrderByColumn
    {
        private readonly string name;
        private readonly OrderDirection direction;

        public OrderByColumn(string name, OrderDirection direction)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.name = name;
            this.direction = direction;
        }

        public string Name
        {
            get { return this.name; }
        }

        public OrderDirection OrderDirection
        {
            get { return this.direction; }
        }
    }
}