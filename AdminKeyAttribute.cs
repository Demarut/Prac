using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AdminKeyAttribute : Attribute, IAuthorizationFilter {
    public void OnAuthorization(AuthorizationFilterContext context) {
        if (!context.HttpContext.Request.Headers.TryGetValue("Admin-Key", out var key) || key != "MySecretKey123") {
            context.Result = new ContentResult { StatusCode = 401, Content = "Unauthorized" };
        }
    }
}