using backend.Dto;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Drawing;
using System.IO;

namespace backend.Endpoints
{
    public static class HowWeMetEndpoints
    {
        public static void MapHowWeMetEndpoints(this WebApplication app)
        {
            app.MapPost("/api/howwemet", async ([FromBody] HowWeMetCreateDto dto, MemoDbContext context,IWebHostEnvironment env) =>
            {
                
                var howWeMet = new HowWeMet
                {
                    WeddingStoryId = dto.WeddingStoryId,
                    Story = dto.Story,
                    Date = dto.Date,
                    Location = dto.Location,

                };
                context.HowWeMetStories.Add(howWeMet);
                await context.SaveChangesAsync();
                
                var howwemetMediaResponse = new HowWeMetResponseDto
                {
                    Id = howWeMet.Id,
                    WeddingStoryId = howWeMet.WeddingStoryId,
                    Story = howWeMet.Story,
                    Date = howWeMet.Date,
                    Location = howWeMet.Location
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
                        var uploadsDir = Path.Combine(env.ContentRootPath, "howwemet");
                        if (!Directory.Exists(uploadsDir))
                        {
                            Directory.CreateDirectory(uploadsDir);
                        }
                        string filePath = Path.Combine(env.ContentRootPath, media.Url);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                            context.HowWeMetMedias.Remove(media);
                            Console.WriteLine($"File {filePath} deleted successfully.");
                        }
                        else
                        {
                            Console.WriteLine($"File {filePath} does not exist.");
                        }
                        
                    }
                }
                var result=context.HowWeMetStories.Remove(howwemet);
                await context.SaveChangesAsync();
                return Results.Ok(result);

            }).WithTags("HowWeMet");
            app.MapPut("/api/howwemet/update", async ([FromBody] HowWeMetUpdateDto dto, MemoDbContext context, IWebHostEnvironment env) =>
            {
                var howWeMet = await context.HowWeMetStories.SingleOrDefaultAsync(h => h.Id == dto.Id);
                if (howWeMet == null)
                    return Results.NotFound();
                howWeMet.Story = dto.Story;
                howWeMet.Location = dto.Location;
                howWeMet.Date = dto.Date;
                context.HowWeMetStories.Update(howWeMet);
                await context.SaveChangesAsync();
                
                return Results.Ok(howWeMet);
            }).WithTags("HowWeMet").Produces<HowWeMetResponseDto>(StatusCodes.Status200OK).DisableAntiforgery();


           app.MapGet("/api/howwemet/media/{howwemetId}", async (Guid howwemetId, MemoDbContext context) =>
            {
                var media = await context.HowWeMetMedias.Where(m => m.HowWeMetId == howwemetId).ToListAsync();
                var mediaResponse = media.Select(m => new HowWeMetMediaResponseDto
                {
                    Id = m.Id,
                    HowWeMetId = m.HowWeMetId,
                    Url = m.Url,
                    Type = m.Type,
                }).ToList();

                return media != null ? Results.Ok(mediaResponse) : Results.NotFound();
            }).WithTags("HowWeMet").Produces<List<HowWeMetMediaResponseDto>>(StatusCodes.Status200OK);
            

            app.MapDelete("/api/howwemet/delete-media/{howWemetMeidaId}", async (Guid howWemetMeidaId, MemoDbContext db,IWebHostEnvironment  env) =>
            {
                var success = false;
                try
                {
                    var media = await db.HowWeMetMedias.SingleOrDefaultAsync(x => x.Id == howWemetMeidaId);
                    var storageRoot = Path.Combine(env.ContentRootPath, "storage");

                    var filePath = Path.Combine(storageRoot, media.Url);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        // If the file is deleted successfully, set success to true
                        db.HowWeMetMedias.Remove(media);
                        await db.SaveChangesAsync();
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
               

            }).WithTags("HowWeMet").WithDescription("This api is to remove image from How Met image list").Produces<bool>(StatusCodes.Status200OK);
            app.MapPost("/api/howwemet/fileUpload", async ([FromForm] HowWeMetMediaDto file, MemoDbContext db, IWebHostEnvironment env) =>
            {
                if (file.File.Length > 1 * 1024 * 1024) // 50MB limit
                    return Results.BadRequest("File too large.");
                try
                {
                    var howwemet = await db.HowWeMetStories.SingleOrDefaultAsync(x => x.Id == file.HowWeMetId);
                    if (howwemet == null)
                        return Results.NotFound("HowWeMet not found.");
                    var storageRoot = Path.Combine(env.ContentRootPath, "storage");
                    var howWeMetGallery = $"{howwemet.WeddingStoryId}/howwemet";
                    var uploadsDir = Path.Combine(storageRoot, howWeMetGallery);
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
                    var fileUrl = $"{howWeMetGallery}/{fileName}";
                    var media = new HowWeMetMedia
                    {
                        HowWeMetId = file.HowWeMetId,
                        Url = fileUrl,
                        Type = file.File.ContentType
                    };
                    await db.HowWeMetMedias.AddAsync(media);
                    await db.SaveChangesAsync();
                    var mediaResponse = new HowWeMetMediaResponseDto
                    {
                        Id = media.Id,
                        HowWeMetId = media.HowWeMetId,
                        Url = media.Url,
                        Type = media.Type,
                    };
                    return Results.Ok(mediaResponse);
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
                return Results.BadRequest("File upload failed.");

            }).Produces<HowWeMetMediaResponseDto>(StatusCodes.Status200OK).WithTags("HowWeMet").WithDescription("HowWeMet").DisableAntiforgery();

        }

    }
       
}
