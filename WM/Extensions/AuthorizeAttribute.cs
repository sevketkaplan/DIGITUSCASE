using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using DataAccessLayer.WMDbContext;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        string token = context.HttpContext.Request.Query["token"];
        var handler = new JwtSecurityTokenHandler();
        var jsontoken =(SecurityToken) handler.ReadToken(token);
       
        var user = (Admin)context.HttpContext.Items["Account"];
        if (user == null)
        {
            // not logged in
            context.Result = new JsonResult(new {isSuccess=false, message = "Giriş yapılmadı" }) { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}