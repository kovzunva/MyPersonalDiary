using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MyPersonalDiary.Controllers;
using MyPersonalDiary.Data;
using MyPersonalDiary.Interfaces;
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

        public async Task Invoke(HttpContext context, IAccountService accountService)
        {
            var user = await accountService.GetCurrentUserAsync();

            if (user != null)
            {
                var currentUrl = context.Request.Path;
                var deleteAt = user.DeleteAt;
                if (deleteAt != null && currentUrl != "/Identity/Account/Logout" && 
                    currentUrl != "/Account/CancelDelete")
                {
                    context.Response.Redirect("/Account/CancelDelete");
                    return;

                }
            }

            await _next(context);
        }
    }
}
