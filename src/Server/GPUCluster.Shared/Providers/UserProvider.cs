using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public interface IUserProvider
{
    string GetUserId();
}

public class UserIdProvider : IUserProvider
{
    private IHttpContextAccessor _accessor;
    public UserIdProvider(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string GetUserId()
    {
        var identity = _accessor.HttpContext.User.Identity;
        if (identity.IsAuthenticated)
        {
            var result = _accessor.HttpContext.User
                .FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
            return result;
        }
        else
        {
            return Guid.Empty.ToString();
        }
    }
}