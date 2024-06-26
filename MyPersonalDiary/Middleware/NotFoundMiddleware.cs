﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using MyPersonalDiary.Models;
using System;

namespace MyPersonalDiary.Middleware
{
    public class NotFoundMiddleware
    {
        private readonly RequestDelegate _next;

        public NotFoundMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, SignInManager<User> _signInManager)
        {
            if (context.Response.StatusCode == 404)
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    context.Response.Redirect("/Home/NotFoundGuestError");
                    return;
                }

                context.Response.Redirect("/Home/NotFoundError");
                return;
            }

            await _next(context);
        }
    }
}
