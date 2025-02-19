# Use case Implementation

> “Add **read-only** and **write** roles in blog service.”

## Consumer-driven contract testing

### Test implementation
First of all we modify the contract and tests.

1. Edit **BlogData** and add the new users
```csharp
    private static User[] users = [
        new User
        {
            Id = Guid.Parse("62d7a17a-6273-4863-bc5f-2e096e81e749"),
            Name = "ReadOnlyUser",
            Email = "ReadOnlyUser@user.com",
            Role = UserRoles.ReadOnly
        },
        new User
        {

            Id = Guid.Parse("365d9ed3-eabf-465a-b48a-fdf32110501f"),
            Name = "FullAccessUser",
            Email = "FullAccessUser@user.com",
            Role = UserRoles.FullAccess
        },
        new User
        {
            Id = Guid.Parse("2a69e26a-d392-41b1-82e6-68b3e4a869fb"),
            Name = "NotPermitedUser",
            Email = "NotPermitedUser@user.com",
            Role = UserRoles.NotPermited              
        }
    ];
```
And adapt the returning object in the contract definition:
```csharp
.WithJsonBody(new { name = Match.Type(user.Name), email = Match.Regex(user.Email, mailRegexPattern), role = user.Role });
```

2. Add the tests for **FullAccess** and  **ReadOnly** user.
```csharp
public class BlogApiTests : IClassFixture<PactService>
{
    private readonly IPactBuilderV4 _pactBuilder;
    private readonly CustomWebApplicationFactory<Program> _factory;

    private const string readOnlyUserName = "ReadOnlyUser";
    private const string fullAccessUserName = "FullAccessUser";
    private const string notPermitedUserName = "NotPermitedUser";
    private const string notExistingUserName = "NotExistingUser";

    private const string getId = "2707f508-ffcf-43c4-994f-66099700df4e";
    private const string titleById = "Lorem ipsum";

    public BlogApiTests(PactService pactService, ITestOutputHelper output)
    {
        _factory = new CustomWebApplicationFactory<Program>();
        _pactBuilder = pactService.CreatePactBuilder("Blog", "UsersPermissions", output);
    }

    #region GetBlogPosts
    [Fact]
    public async Task GivenUserWithFullAccess_WhenGetBlogPosts_ThenReturnBlogPosts()
    {
        _pactBuilder.ConfigureRequestPactBuilder(fullAccessUserName, "User with full access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", fullAccessUserName);

            // Act
            var response = await client.GetAsync("api/articles");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var blogPosts = JsonSerializer.Deserialize<IEnumerable<Article>>(responseString);
            Assert.True(blogPosts?.Count() >= 7);
        });
    }

    [Fact]
    public async Task GivenUserWithReadOnlyAccess_WhenGetBlogPosts_ThenReturnBlogPosts()
    {
        _pactBuilder.ConfigureRequestPactBuilder(readOnlyUserName, "User with read only access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", readOnlyUserName);

            // Act
            var response = await client.GetAsync("api/articles");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var blogPosts = JsonSerializer.Deserialize<IEnumerable<Article>>(responseString);
            Assert.True(blogPosts?.Count() >= 7);
        });
    }

    [Fact]
    public async Task GivenNotExistingUser_WhenGetBlogPosts_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notExistingUserName, "Not existing User");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notExistingUserName);

            // Act
            var response = await client.GetAsync("api/articles");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }

    [Fact]
    public async Task GivenUserWithoutAccess_WhenGetBlogPosts_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notPermitedUserName, "User without access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notPermitedUserName);

            // Act
            var response = await client.GetAsync("api/articles");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }
    #endregion

    #region GetBlogPost
    [Fact]
    public async Task GivenUserWithFullAccess_GetGetBlogPost_ThenReturnBlogPost()
    {
        _pactBuilder.ConfigureRequestPactBuilder(fullAccessUserName, "User with full access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", fullAccessUserName);

            // Act
            var response = await client.GetAsync($"api/articles/{getId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var blogPost = JsonSerializer.Deserialize<ArticleDto>(responseString,
                new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            Assert.Equal(titleById, blogPost?.Title);
        });
    }

    [Fact]
    public async Task GivenUserWithReadOnlyAccess_GetGetBlogPost_ThenReturnBlogPost()
    {
        _pactBuilder.ConfigureRequestPactBuilder(readOnlyUserName, "User with read only access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", readOnlyUserName);

            // Act
            var response = await client.GetAsync($"api/articles/{getId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var blogPost = JsonSerializer.Deserialize<ArticleDto>(responseString,
                new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            Assert.Equal(titleById, blogPost?.Title);
        });
    }


    [Fact]
    public async Task GivenNotExistingUser_GetGetBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notExistingUserName, "Not existing User");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notExistingUserName);

            // Act
            var response = await client.GetAsync($"api/articles/{getId}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }
    [Fact]
    public async Task GivenUserWithoutAccess_GetGetBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notPermitedUserName, "User without access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notPermitedUserName);

            // Act
            var response = await client.GetAsync($"api/articles/{getId}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }
    #endregion

    #region PostBlogPost
    [Fact]
    public async Task GivenUserWithFullAcces_PostBlogPost_ThenBlogPostIsAdded()
    {
        _pactBuilder.ConfigureRequestPactBuilder(fullAccessUserName, "User with full access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", fullAccessUserName);
            var articleDto = BlogData.GetDefaultArticleDto();

            // Act
            var response = await client.PostAsync("api/articles", new StringContent(JsonSerializer.Serialize(articleDto), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            var articleId = await response.Content.ReadAsStringAsync();
            var article = _factory.GetArticleById(Guid.Parse(articleId));

            Assert.NotNull(article);
            Assert.Equal(articleDto.Title, article?.Title);
            Assert.Equal(articleDto.Content, article?.Content);

            //CleanUp
            _factory.DeleteArticle(article.Id);
        });
    }

    [Fact]
    public async Task GivenUserWithReadOnlyAccess_PostBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(readOnlyUserName, "User with read only access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", readOnlyUserName);
            var articleDto = BlogData.GetDefaultArticleDto();

            // Act
            var response = await client.PostAsync("api/articles", new StringContent(JsonSerializer.Serialize(articleDto), Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }


    [Fact]
    public async Task GivenUserWithoutAccess_PostBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notPermitedUserName, "User without access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notPermitedUserName);
            var articleDto = BlogData.GetDefaultArticleDto();

            // Act
            var response = await client.PostAsync("api/articles", new StringContent(JsonSerializer.Serialize(articleDto), Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }

    [Fact]
    public async Task GivenNotExistingUser_PostBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notExistingUserName, "Not existing User");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notExistingUserName);
            var articleDto = BlogData.GetDefaultArticleDto();

            // Act
            var response = await client.PostAsync("api/articles", new StringContent(JsonSerializer.Serialize(articleDto), Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }

    #endregion

    #region PutBlogPost
    [Fact]
    public async Task GivenUserWithFullAccess_PutBlogPost_ThenBlogPostIsUpdated()
    {
        _pactBuilder.ConfigureRequestPactBuilder(fullAccessUserName, "User with full access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", fullAccessUserName);
            var article = BlogData.GetDefaultArticle();
            _factory.SaveArticle(article);

            dynamic articleDto = new { Title = "Updated Article", Content = "Updated Article Content" };
            // Act
            var response = await client.PutAsync($"api/articles/{article.Id}", new StringContent(JsonSerializer.Serialize(articleDto), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedArticle = _factory.GetArticleById(article.Id);

            Assert.NotNull(updatedArticle);
            Assert.Equal(articleDto.Title, updatedArticle?.Title);
            Assert.Equal(articleDto.Content, updatedArticle?.Content);

            //CleanUp
            _factory.DeleteArticle(updatedArticle.Id);
        });
    }
    [Fact]
    public async Task GivenUserWithReadOnlyAccess_PutBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(readOnlyUserName, "User with read only access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", readOnlyUserName);
            var article = BlogData.GetDefaultArticle();
            _factory.SaveArticle(article);

            dynamic articleDto = new { Title = "Updated Article", Content = "Updated Article Content" };
            // Act
            var response = await client.PutAsync($"api/articles/{article.Id}", new StringContent(JsonSerializer.Serialize(articleDto), Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            //CleanUp
            _factory.DeleteArticle(article.Id);
        });
    }

    [Fact]
    public async Task GivenUserWithoutAccess_PutBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notPermitedUserName, "User without access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notPermitedUserName);
            var article = BlogData.GetDefaultArticle();
            _factory.SaveArticle(article);

            dynamic articleDto = new { Title = "Updated Article", Content = "Updated Article Content" };
            // Act
            var response = await client.PutAsync($"api/articles/{article.Id}", new StringContent(JsonSerializer.Serialize(articleDto), Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            //CleanUp
            _factory.DeleteArticle(article.Id);
        });
    }


    [Fact]
    public async Task GivenNotExistingUser_PutBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notExistingUserName, "Not existing User");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notExistingUserName);
            var article = BlogData.GetDefaultArticle();
            _factory.SaveArticle(article);

            dynamic articleDto = new { Title = "Updated Article", Content = "Updated Article Content" };
            // Act
            var response = await client.PutAsync($"api/articles/{article.Id}", new StringContent(JsonSerializer.Serialize(articleDto), Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            //CleanUp
            _factory.DeleteArticle(article.Id);
        });
    }

    #endregion

    #region DeleteBlogPost
    [Fact]
    public async Task GivenUserWithFullAccess_DeleteBlogPost_ThenBlogPostIsDeleted()
    {
        _pactBuilder.ConfigureRequestPactBuilder(fullAccessUserName, "User with full access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", fullAccessUserName);
            var article = BlogData.GetDefaultArticle();
            _factory.SaveArticle(article);

            // Act
            var response = await client.DeleteAsync($"api/articles/{article.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var deletedArticle = _factory.GetArticleById(article.Id);

            Assert.Null(deletedArticle);
        });
    }

    [Fact]
    public async Task GivenUserWithReadOnlyAccess_DeleteBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(readOnlyUserName, "User with read only access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", readOnlyUserName);
            var article = BlogData.GetDefaultArticle();
            _factory.SaveArticle(article);

            // Act
            var response = await client.DeleteAsync($"api/articles/{article.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            //CleanUp
            _factory.DeleteArticle(article.Id);
        });
    }

    [Fact]
    public async Task GivenUserWithoutAccess_DeleteBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notPermitedUserName, "User without access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notPermitedUserName);
            var article = BlogData.GetDefaultArticle();
            _factory.SaveArticle(article);

            // Act
            var response = await client.DeleteAsync($"api/articles/{article.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            //CleanUp
            _factory.DeleteArticle(article.Id);
        });
    }

    [Fact]
    public async Task GivenNotExistingUser_DeleteBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notExistingUserName, "Not existing User");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notExistingUserName);
            var article = BlogData.GetDefaultArticle();
            _factory.SaveArticle(article);

            // Act
            var response = await client.DeleteAsync($"api/articles/{article.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            //CleanUp
            _factory.DeleteArticle(article.Id);
        });
    }
    #endregion
}
```
3. Implement minimun to compile the solution

So change the **User** entity in blog API.
```csharp
public enum UserRoles
{
    NotPermited = 0,
    ReadOnly = 1,
    FullAccess = 2    
}

public class User
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public UserRoles Role { get; set; }
}
```
**HasUser** method in **UserService** class
```csharp
    public async Task<bool> HasAccess(string userName)
    {
        try
        {
            var userId = await usersClient.GetUserIdByName(userName);
            if (userId == Guid.Empty)
            {
                return false;
            }

            var user = await usersClient.GetUserById(userId);
            return user.Access;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while checking access for user {UserName}", userName);
            return false;
        }
    }
```
and mappings in **UserClient Class
```csharp

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

    internal record UserViewDto(Guid Id, string Name, string Email);
    internal record UserDto(string Name, string Email, UserRoles Role);
```
### Make the test pass in Blog Service
1. Modify **HasAccess** method to make the decission based on the operation and the user in **UserService**. 
```csharp
public async Task<bool> HasAccess(string userName, string operation)
    {
        try
        {
            var userId = await usersClient.GetUserIdByName(userName);
            if (userId == Guid.Empty)
            {
                return false;
            }

            var user = await usersClient.GetUserById(userId);

            if (user.Role == UserRoles.FullAccess)
            {
                // Full access has access to all operations
                return true;
            }

            if (user.Role == UserRoles.ReadOnly && operation == "Read")
            {
                // ReadOnly has access to read operation
                return true;
            }
                                         
            // Other kind of combination operation and role has no access
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while checking access for user {UserName}", userName);
            return false;
        }
    }
```
**Note** change the interface too.

2. Change the **Middleware** to pass the operation parameter.
```csharp
 public async Task InvokeAsync(HttpContext context)
 {
     if (!context.Request.Headers.ContainsKey("Username"))
     {
         context.Response.StatusCode = StatusCodes.Status400BadRequest;
         await context.Response.WriteAsync("Username header is missing.");
         return;
     }

     var username = context.Request.Headers["Username"].ToString();

     string operation = GetOperationFromMethod(context.Request.Method);
     if (!await _service.HasAccess(username, operation))
     {
         context.Response.StatusCode = StatusCodes.Status401Unauthorized;
         await context.Response.WriteAsync("Access denied.");
         return;
     }

     await _next(context);
 }

 private static string GetOperationFromMethod(string method) => method switch
 {
     "POST" => "Create",
     "PUT" => "Update",
     "DELETE" => "Delete",
     "GET" => "Read",
     _ => "Unknown"
 };
 ```

 ## Implement new contract in Provider service

1. Modify the **User** entity to add *Role* property.
```csharp
public enum UserRoles
{
    NoAccess = 0,
    ReadOnly = 1,
    FullAccess = 2
}
public class User
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public UserRoles Role { get; set; }
}
```

2. Modify **UsersRepository** to save the new property. Really we modify the constructor to add new user with no access and adapt the existent users. And the **update* method.

* Constructor:
```csharp
public UsersRepository()
{
    users = new List<User>
    {
        new User
        {
            Id = Guid.Parse("cba1b9da-7664-4022-9267-1de95f456865"),
            Name = "John Doe",
            Email = "John.Doe@email.com",
            Role = UserRoles.FullAccess
        },
        new User
        {
            Id = Guid.Parse("ebf28d90-3c1e-4e0a-89a3-32e8f84dc703"),
            Name = "Jane Doe",
            Email = "Jane.Doe@email.com",
            Role = UserRoles.ReadOnly,
        },
        new User
        {
            Id = Guid.Parse("f1b1b9da-7664-4022-9267-1de95f456865"),
            Name = "John Smith",
            Email = "Johm Smith@email.com",
            Role = UserRoles.NoAccess,
        }
    };
}   
```

* **Update** method
```csharp
    public void Update(User user)
    {
        var userToUpdate = users.Find(u => u.Id == user.Id);
        if (userToUpdate != null)
        {            
            userToUpdate.Email = user.Email;
            userToUpdate.Role = user.Role;         
        }        
    }
```

3. Adapt the Controller to send the **Role** when necessary

* Modify the *DTO*.

```csharp
public record UserDto(string Name, string Email, int Role);
```

* And the Mappings.
```csharp
    // GET api/<UsersController>/5
    [HttpGet("{id}")]
    public ActionResult<UserDto> GetById(string id)
    {
        logger.LogInformation("Getting user by id");
        var user = repository.GetById(Guid.Parse(id));
        if (user == null)
        {
            logger.LogError("User not found");
            return NotFound();
        }
        return new UserDto(user.Name, user.Email, (int)user.Role);
    }

    // POST api/<UsersController>
    [HttpPost]
    public ActionResult<string> Post([FromBody] UserDto user)
    {
        logger.LogInformation("Adding user");   
        var result = service.AddUser(new User { Name = user.Name, Email = user.Email, Role = (UserRoles)user.Role });
        if (!result.IsValid)
        {
            logger.LogError(result.Message);
            return BadRequest(result.Message);
        }
        return Ok(result.Message);
    }

    // PUT api/<UsersController>/5
    [HttpPut("{id}")]
    public ActionResult<string> Put(string id, [FromBody] UserDto user)
    {
        logger.LogInformation("Updating user");
        var result = service.UpdateUser(new User { Id = Guid.Parse(id), Name = user.Name, Email = user.Email, Role = (UserRoles)user.Role });
        if (!result.IsValid)
        {
            logger.LogError(result.Message);    
            if (result.Message == "Usuario no encontrado")
            {
                return NotFound(result.Message);
            }
            return BadRequest(result.Message);
        }
        return NoContent();
    }
```

4. Finally we add the new users for contract testing in the **ProviderStateMidleware** and set *PermitedUser* in *WeatherForecast* services with ReadoOnly Role
```csharp
 public ProviderStateMiddleware(RequestDelegate next, IUsersRepository usersRepository)
 {
     _next = next;
     _usersRepository = usersRepository;
     _providerStates = new Dictionary<string, Func<IDictionary<string, object>, Task>>
     {
         ["User with full access"] = InsertUserWithFullAccess,
         ["User with read only access"] = InsertUserWithReadOnlyAccess,
         ["User with access"] = InsertUserWithAccess,
         ["User without access"] = InsertUserWithoutAccess,
         ["Not existing User"] = InsertNoUser
     };
 }

 private async Task InsertUserWithFullAccess(IDictionary<string, object> parameters)
 {
     var user = new User
     {
         Id = Guid.Parse("365d9ed3-eabf-465a-b48a-fdf32110501f"),
         Name = "FullAccessUser",
         Email = "FullAccessUser@user.com",
         Role = UserRoles.FullAccess
     };
     InsertUserIfNotExists(user);
     await Task.CompletedTask;
 }

 private async Task InsertUserWithReadOnlyAccess(IDictionary<string, object> parameters)
 {
     var user = new User
     {
         Id = Guid.Parse("62d7a17a-6273-4863-bc5f-2e096e81e749"),
         Name = "ReadOnlyUser",
         Email = "ReadOnlyAccessUser@user.com",
         Role = UserRoles.ReadOnly
     };
     InsertUserIfNotExists(user);
     await Task.CompletedTask;
 }

 private async Task InsertUserWithAccess(IDictionary<string, object> parameters)
 {
     var user = new User
     {
         Id = Guid.Parse("757d4594-79b2-480c-8fc4-ddd7061c18cb"),
         Name = "PermitedUser",
         Email = "PermitedUser@user.com",
         Role = UserRoles.ReadOnly
     };
     InsertUserIfNotExists(user);
     await Task.CompletedTask;
 }
     
 private async Task InsertUserWithoutAccess(IDictionary<string, object> parameters)
 {
     var user = new User
     {
         Id = Guid.Parse("2a69e26a-d392-41b1-82e6-68b3e4a869fb"),
         Name = "NotPermitedUser",
         Email = "NotPermitedUser@user.com",
         Role = UserRoles.NoAccess
     };
     InsertUserIfNotExists(user);
     await Task.CompletedTask;
}

private void InsertUserIfNotExists(User user)
{
    var userExists = _usersRepository.GetById(user.Id);
    if (userExists == null)
    {
        _usersRepository.Add(user);
    }
}
```

## Backward compatibility in UsersPermissions Controller

1. Add **HasAccess** field to *UsersDto*
```csharp
public record UserDto(string Name, string Email, bool HasAccess, int Role);
```

2. Update **Get/{id}** operation to fullfill the DTO 
```csharp
 // GET api/<UsersController>/5
 [HttpGet("{id}")]
 public ActionResult<UserCardDto> GetById(string id)
 {
     logger.LogInformation("Getting user by id");
     var user = repository.GetById(Guid.Parse(id));
     if (user == null)
     {
         logger.LogError("User not found");
         return NotFound();
     }
     return new UserCardDto(user.Name, user.Email, HasAccess(user.Role), (int)user.Role);
 }

private static bool HasAccess(UserRoles role) => role != UserRoles.NoAccess;
 ```

