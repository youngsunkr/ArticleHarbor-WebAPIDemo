namespace ArticleHarbor.DomainModel
{
    using System;
    using System.Threading.Tasks;

    public interface IUserRepository
    {
        Task<User> SelectAsync(string id, string password);

        Task<User> SelectAsync(Guid apiKey);
    }
}