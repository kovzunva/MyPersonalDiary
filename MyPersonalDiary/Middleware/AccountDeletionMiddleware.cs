using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MyPersonalDiary.Controllers;
using MyPersonalDiary.Data;
using MyPersonalDiary.Models;
using MyPersonalDiary.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace MyPersonalDiary.Middleware
{
    public class AccountDeletionMiddleware
    {
        private RequestDelegate _next; 

        public AccountDeletionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, AccountDeletionService accountDeletionService, UserManager<User> userManager)
        {
            var user = await userManager.GetUserAsync(context.User);

            if (user != null)
            {
                var currentUrl = context.Request.Path;
                if (currentUrl != "/Identity/Account/Logout")
                {
                    var deleteAt = user.DeleteAt;
                    if (deleteAt != null)
                    {
                        if (DateTimeOffset.Now >= deleteAt)
                        {
                            var isDeleted = await accountDeletionService.DeleteAccount(user);
                            if (isDeleted)
                            {
                                context.Response.Redirect("/");
                                return;
                            }
                            else
                            {
                                context.Response.Redirect("/Error");
                                return;
                            }
                        }
                        else
                        {

                            if (currentUrl != "/Account/CancelDelete")
                            {
                                context.Response.Redirect("/Account/CancelDelete");
                                return;
                            }
                        }

                    }
                }
            }

            await _next(context);
        }
    }
}
