﻿namespace DomainModel
{
    using System;

    public class User
    {
        private readonly string id;
        private readonly string password;
        private readonly RoleTypes roles;

        public User(string id, string password, RoleTypes roles)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            if (password == null)
                throw new ArgumentNullException("password");

            this.id = id;
            this.password = password;
            this.roles = roles;
        }

        public string Id
        {
            get { return this.id; }
        }

        public string Password
        {
            get { return this.password; }
        }

        public RoleTypes Roles
        {
            get { return this.roles; }
        }
    }
}