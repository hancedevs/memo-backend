using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Dto;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints
{
    public static class MediaEndpoints
    {
        public static void MapMediaEndpoints(this WebApplication app)
        {
            app.MapPost("/api/media/upload", async ([FromForm] MediaFileDto file, MemoDbContext db, IWebHostEnvironment env) =>
            {
                if (file.File.Length > 1 * 1024 * 1024) // 50MB limit
                    return Results.BadRequest("File too large.");

                var uploadsDir = Path.Combine(env.WebRootPath, "media");
                Directory.CreateDirectory(uploadsDir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.File.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.File.CopyToAsync(stream);
                }
                var fileUrl = $"/media/{fileName}";
                var media = new Media
                {
                    WeddingId = file.WeddingId,
                    Url = fileUrl,
                    Type = file.File.ContentType,
                    IsCoverImage = false
                };
                await db.Media.AddAsync(media);
                await db.SaveChangesAsync();
                return Results.Ok(new { Url = fileUrl });
            }).WithTags("Wedding").WithDescription("Wedding").DisableAntiforgery();
            app.MapGet("/api/media/{weddingId}", async (Guid weddingId, MemoDbContext db) =>
            {
                var media = await db.Media.Where(m => m.WeddingId == weddingId).ToListAsync();
                return media != null ? Results.Ok(media) : Results.NotFound();
            }).WithTags("Wedding");
            app.MapPut("/api/media/update/coverImage", async ([FromForm] MediaFileDto file, MemoDbContext db, IWebHostEnvironment env) =>
            {
                if (file.File.Length > 1 * 1024 * 1024) // 50MB limit  
                    return Results.BadRequest("File too large.");

                var uploadsDir = Path.Combine(env.WebRootPath, "media");
                Directory.CreateDirectory(uploadsDir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.File.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.File.CopyToAsync(stream);
                }
                var fileUrl = $"/media/{fileName}";

                var existingCoverImage = await db.Media.FirstOrDefaultAsync(m => m.WeddingId == file.WeddingId && m.IsCoverImage);
                if (existingCoverImage != null)
                {
                    existingCoverImage.IsCoverImage = false;

                    // Remove the old image file from the server
                    var oldFilePath = Path.Combine(env.WebRootPath, existingCoverImage.Url.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                var newCoverImage = new Media
                {
                    WeddingId = file.WeddingId,
                    Url = fileUrl,
                    Type = file.File.ContentType,
                    IsCoverImage = true
                };

                await db.Media.AddAsync(newCoverImage);
                await db.SaveChangesAsync();

                return Results.Ok(new { Url = fileUrl });
            }).WithTags("Wedding").DisableAntiforgery();

            app.MapPost("/api/media/upload/coverImage", async ([FromForm] MediaFileDto file, MemoDbContext db, IWebHostEnvironment env) =>
        {
            if (file.File.Length > 1 * 1024 * 1024) // 50MB limit
                return Results.BadRequest("File too large.");

            var uploadsDir = Path.Combine(env.WebRootPath, "media");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.File.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.File.CopyToAsync(stream);
            }
            var fileUrl = $"/media/{fileName}";
            var media = new Media
            {
                WeddingId = file.WeddingId,
                Url = fileUrl,
                Type = file.File.ContentType,
                IsCoverImage = true
            };
            await db.Media.AddAsync(media);
            await db.SaveChangesAsync();
            return Results.Ok(new { Url = fileUrl });
        }).WithTags("Wedding").DisableAntiforgery();
            
        }
    }
    public class IgnoreAntiforgeryTokenFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            return await next(context);
        }
    }
}