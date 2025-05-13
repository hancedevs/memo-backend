using backend.Dto;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Drawing;

namespace backend.Endpoints
{
    public static class HowWeMetEndpoints
    {
        public static void MapHowWeMetEndpoints(this WebApplication app)
        {
            app.MapPost("/api/howwemet", async ([FromForm] HowWeMetCreateDto dto, MemoDbContext context,IWebHostEnvironment env) =>
            {
                if (dto.Files.Any(x=>x.Length> 1 * 1024 * 1024)) // 50MB limit
                    return Results.BadRequest("File too large.");
                var howWeMet = new HowWeMet
                {
                    WeddingStoryId = dto.WeddingStoryId,
                    Story = dto.Story,
                    Date = dto.Date,
                    Location = dto.Location,

                };
                context.HowWeMetStories.Add(howWeMet);
                await context.SaveChangesAsync();
                var medias = new List<HowWeMetMedia>();
                var uploadsDir = Path.Combine(env.WebRootPath, "howwemet");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }
                foreach (var file in dto.Files)
                {
                    var fileName = $"howwemet-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    var fileUrl = $"/howwemet/{fileName}";
                    var media = new HowWeMetMedia
                    {
                        HowWeMetId =howWeMet.Id ,
                        Url = fileName,
                        Type = file.ContentType,
                    };
                    medias.Add(media);
                }
                await context.HowWeMetMedias.AddRangeAsync(medias);
                await context.SaveChangesAsync();
                var howwemetMediaResponse = new HowWeMetResponseDto
                {
                    Id = howWeMet.Id,
                    WeddingStoryId = howWeMet.WeddingStoryId,
                    Story = howWeMet.Story,
                    Date = howWeMet.Date,
                    Location = howWeMet.Location,
                    Media = medias.Select(media => new HowWeMetMediaResponseDto
                    {
                        Id = media.Id,
                        Url = media.Url,
                        Type = media.Type,

                    }).ToList()
                };
                return Results.Ok(howwemetMediaResponse);
            }).WithTags("HowWeMet").Produces<HowWeMetResponseDto>(StatusCodes.Status200OK).DisableAntiforgery();
            app.MapGet("/api/howwemet/{id}", async (Guid id, MemoDbContext context) =>
            {
                var howWeMet = await context.HowWeMetStories
                    .Include(h => h.Media)
                    .FirstOrDefaultAsync(h => h.Id == id);
                var howwemetResponse = new HowWeMetResponseDto
                {
                    Id = howWeMet.Id,
                    WeddingStoryId = howWeMet.WeddingStoryId,
                    Story = howWeMet.Story,
                    Date = howWeMet.Date,
                    Location = howWeMet.Location,
                    Media = howWeMet.Media.Select(media => new HowWeMetMediaResponseDto
                    {
                        Id = media.Id,
                        Url = media.Url,
                        Type = media.Type,
                    }).ToList()
                };
                
                return howwemetResponse != null ? Results.Ok(howwemetResponse) : Results.NotFound();
            }).WithTags("HowWeMet").Produces<HowWeMetResponseDto>(StatusCodes.Status200OK);
            app.MapDelete("/api/howwemet/delete/{howwemetId}", async(Guid howwemetId, MemoDbContext context,IWebHostEnvironment env) =>
            {
                var howwemet=await context.HowWeMetStories.SingleOrDefaultAsync(x=>x.Id== howwemetId);
                var howwemetMedia = await context.HowWeMetMedias.Where(x => x.HowWeMetId == howwemet.Id).ToListAsync();
                if (howwemetMedia != null)
                {
                    foreach (var media in howwemetMedia)
                    {
                        var uploadsDir = Path.Combine(env.WebRootPath, "howwemet");
                        if (!Directory.Exists(uploadsDir))
                        {
                            Directory.CreateDirectory(uploadsDir);
                        }
                        string filePath = Path.Combine(env.WebRootPath, media.Url);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                            Console.WriteLine($"File {filePath} deleted successfully.");
                        }
                        else
                        {
                            Console.WriteLine($"File {filePath} does not exist.");
                        }
                        context.HowWeMetMedias.Remove(media);
                    }
                }
                var result=context.HowWeMetStories.Remove(howwemet);
                await context.SaveChangesAsync();
                return Results.Ok(result);

            }).WithTags("HowWeMet");
            app.MapPut("/api/howwemet/update", async ([FromForm] HowWeMetUpdateDto dto, MemoDbContext context, IWebHostEnvironment env) =>
            {
                var howWeMet = await context.HowWeMetStories.Include(h => h.Media).FirstOrDefaultAsync(h => h.Id == dto.Id);
                if (howWeMet == null)
                    return Results.NotFound();
                howWeMet.Story = dto.Story;
                howWeMet.Location = dto.Location;
                howWeMet.Date = dto.Date;
                context.HowWeMetStories.Update(howWeMet);
                await context.SaveChangesAsync();
                var uploadsDir = Path.Combine(env.WebRootPath, "howwemet");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }
                var existingMedia = await context.HowWeMetMedias.Where(m => m.HowWeMetId == howWeMet.Id).ToListAsync();
                if (existingMedia.Any())
                {
                    foreach (var media in existingMedia)
                    {
                        
                        string filePath = Path.Combine(uploadsDir, media.Url);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                            Console.WriteLine($"File {filePath} deleted successfully.");
                        }
                        else
                        {
                            Console.WriteLine($"File {filePath} does not exist.");
                        }
                        context.HowWeMetMedias.Remove(media);
                    }
                }
                
                foreach (var file in dto.Files)
                {
                    var fileName = $"howwemet-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    var fileUrl = $"/howwemet/{fileName}";
                    var media = new HowWeMetMedia
                    {
                        HowWeMetId = howWeMet.Id,
                        Url = fileName,
                        Type = file.ContentType,
                    };
                    context.HowWeMetMedias.Add(media);
                }
                await context.SaveChangesAsync();
                return Results.Ok(howWeMet);
            }).WithTags("HowWeMet").Produces<HowWeMetResponseDto>(StatusCodes.Status200OK).DisableAntiforgery();
            app.MapGet("/api/howwemet/media/{howwemetId}", async (Guid howwemetId, MemoDbContext context) =>
            {
                var media = await context.HowWeMetMedias.Where(m => m.HowWeMetId == howwemetId).ToListAsync();
                return media != null ? Results.Ok(media) : Results.NotFound();
            }).WithTags("HowWeMet");
            

            app.MapDelete("/api/howwemet/delete-media/{howWemetMeidaId}", async (Guid howWemetMeidaId, MemoDbContext db,IWebHostEnvironment  env) =>
            {
                var media=await db.HowWeMetMedias.SingleOrDefaultAsync(x=>x.Id== howWemetMeidaId);
                var uploadsDir = Path.Combine(env.WebRootPath, "howwemet");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }
                string filePath = Path.Combine(env.WebRootPath, media.Url);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"File {filePath} deleted successfully.");
                }
                else
                {
                    Console.WriteLine($"File {filePath} does not exist.");
                }

            }).WithTags("HowWeMet").WithDescription("This api is to remove image from How Met image list");
        }

    }
}
