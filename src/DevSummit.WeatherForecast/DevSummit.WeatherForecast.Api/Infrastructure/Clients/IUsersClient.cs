using DevSummit.WeatherForecast.Api.Domain.Entities;

namespace DevSummit.WeatherForecast.Api.Infrastructure.Clients;

public interface IUsersClient
{
    Task<Guid> GetUserIdByName(string? name);
    Task<User> GetUserById(Guid id);

}
