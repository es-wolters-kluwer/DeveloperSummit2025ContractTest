using DevSummit.Blog.Api.Domain.Entities;

namespace DevSummit.Blog.Api.Infrastructure.Clients;

public interface IUsersClient
{
    Task<Guid> GetUserIdByName(string? name);
    Task<User> GetUserById(Guid id);

}
