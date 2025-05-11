using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Dto;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            app.MapPost("/api/auth/login", async (LoginDto model, AuthService userService, IJwtTokenService jwtTokenService) =>
            {
                var user = await userService.Authenticate(model.UserName, model.Password);
                if (user == null)
                    return Results.Unauthorized();

                var token = jwtTokenService.GenerateToken(user);
                return Results.Ok(new { Token = token });
            }).AddEndpointFilter(async (context, next) =>
            {
                var dto = context.GetArgument<LoginDto>(0);
                var errors = new List<string>();
                if (string.IsNullOrWhiteSpace(dto.UserName))
                    errors.Add("Username is required.");
                if (string.IsNullOrWhiteSpace(dto.Password))
                    errors.Add("Password is required.");
                if (dto.Password.Length < 8 || !System.Text.RegularExpressions.Regex.IsMatch(dto.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$"))
                    errors.Add("Password must be at least 8 characters and contain one uppercase letter, one lowercase letter, and one number.");
                return errors.Any() ? Results.BadRequest(new { Errors = errors }) : await next(context);
            }).WithTags("Auth");
            app.MapPost("/api/auth/register", async (RegisterDto model, AuthService userService) =>
            {
                var user = await userService.Register(model);
                if (user == null)
                    return Results.BadRequest("User already exists");
                return Results.Ok(user);
            }).AddEndpointFilter(async (context, next) =>
            {
                var dto = context.GetArgument<RegisterDto>(0);
                var errors = new List<string>();
                if (string.IsNullOrWhiteSpace(dto.Username))
                    errors.Add("Username is required.");
                if (string.IsNullOrWhiteSpace(dto.Password))
                    errors.Add("Password is required.");
                if (dto.Password.Length < 8 || !System.Text.RegularExpressions.Regex.IsMatch(dto.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$"))
                    errors.Add("Password must be at least 8 characters and contain one uppercase letter, one lowercase letter, and one number.");
                return errors.Any() ? Results.BadRequest(new { Errors = errors }) : await next(context);
            }).WithTags("Auth");
        }
    }
}