﻿namespace DevSummit.Blog.Api.Domain.Entities;
public class User
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool Access { get; set; }
}
