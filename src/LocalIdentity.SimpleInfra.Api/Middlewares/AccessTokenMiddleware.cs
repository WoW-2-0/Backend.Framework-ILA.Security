﻿using System.Security.Authentication;
using LocalIdentity.SimpleInfra.Application.Common.Identity.Services;
using LocalIdentity.SimpleInfra.Domain.Constants;

namespace LocalIdentity.SimpleInfra.Api.Middlewares;

public class AccessTokenMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var accessTokenService = context.RequestServices.GetRequiredService<IAccessTokenService>();

        var accessTokenIdValue = context.User.Claims.FirstOrDefault(claim => claim.Type == ClaimConstants.AccessTokenId)?.Value;
        if (accessTokenIdValue != null)
        {
            var accessTokenId = Guid.Parse(accessTokenIdValue);
            var foundAccessToken = await accessTokenService.GetByIdAsync(accessTokenId, true) ??
                                   throw new AuthenticationException("Access token not found");

            if (foundAccessToken.IsRevoked)
                throw new AuthenticationException("Access token revoked");
        }

        await next(context);
    }
}