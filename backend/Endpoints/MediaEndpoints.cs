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
                var gallery = $"{file.WeddingId}/gallery";
                var storageRoot = Path.Combine(env.ContentRootPath, "storage");
                var uploadsDir = Path.Combine(storageRoot, gallery);

                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.File.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.File.CopyToAsync(stream);
                }
                var fileUrl = $"{gallery}/{fileName}";
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
            app.MapPut("/api/media/update/coverImage", async ([FromBody] CoverImageDto dto, MemoDbContext db, IWebHostEnvironment env) =>
            {
               

                var existingCoverImage = await db.Media.Where(m => m.WeddingId == dto.WeddingId && m.IsCoverImage).ToListAsync();
                var newCoverImage = await db.Media.FirstOrDefaultAsync(m => m.Id == dto.NewCoverImageId);
                if (existingCoverImage.Any())
                {
                    existingCoverImage.ForEach(x => x.IsCoverImage =false);

                    
                }

                newCoverImage.IsCoverImage = true;

                 db.Media.Update(newCoverImage);
                 db.Media.UpdateRange(existingCoverImage);
                await db.SaveChangesAsync();
                var response= new MediaFileResponseDto
                {
                    Id = newCoverImage.Id,
                    Url = newCoverImage.Url,
                    Type = newCoverImage.Type,
                    IsCoverImage = newCoverImage.IsCoverImage,
                    WeddingId = newCoverImage.WeddingId
                };

                return Results.Ok(response);
            }).WithTags("Wedding").Produces<MediaFileResponseDto>(StatusCodes.Status200OK).DisableAntiforgery();

            app.MapDelete("/api/wedding/delete-media/{mediaId}", async (Guid mediaId, MemoDbContext db, IWebHostEnvironment env) =>
            {
                var success = false;
                try
                {
                    var media = await db.Media.SingleOrDefaultAsync(x => x.Id == mediaId);

                    var storageRoot = Path.Combine(env.ContentRootPath, "storage");
                    
                       
                        string filePath = Path.Combine(storageRoot, media.Url);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        // If the file is deleted successfully, set success to true
                        db.Media.Remove(media);
                        var x = await db.SaveChangesAsync();
                        if (x!=0){
                            success = true;
                            Console.WriteLine($"File {filePath} not deleted successfully.");
                        }

                        success = true;
                            Console.WriteLine($"File {filePath} deleted successfully.");
                        }
                        else
                        {
                            Console.WriteLine($"File {filePath} does not exist.");
                        }
                       
                    
                    
                    return Results.Ok(success);
                }
                catch (Exception)
                {

                    throw;
                }


            }).WithTags("Wedding").WithDescription("This api is to remove image from How Met image list").Produces<bool>(StatusCodes.Status200OK);



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