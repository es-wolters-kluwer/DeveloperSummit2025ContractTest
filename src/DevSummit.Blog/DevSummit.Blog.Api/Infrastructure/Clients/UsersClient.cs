﻿using DevSummit.Blog.Api.Domain.Entities;
using System.Data;

namespace DevSummit.Blog.Api.Infrastructure.Clients;

public class UsersClient : IUsersClient
{
    private readonly HttpClient httpClient;

    public UsersClient(HttpClient httpClient)
    {
       this.httpClient = httpClient;
    }

    public async Task<Guid> GetUserIdByName(string name)
    {
        var response = await httpClient.GetAsync($"api/users?name={name}");
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserViewDto>>();
        return users?.FirstOrDefault()?.Id ?? Guid.Empty;
    }

    public async Task<User> GetUserById(Guid id)
    {
        var response = await httpClient.GetAsync($"api/users/{id}");
        response.EnsureSuccessStatusCode();
        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        
        return new User
        {
            Id = id,
            Name = userDto.Name,
            Email = userDto.Email,
            Role = userDto.Role
        };
    }
}

internal record UserViewDto(Guid Id, string Name, string Email);
internal record UserDto(string Name, string Email, UserRoles Role);
