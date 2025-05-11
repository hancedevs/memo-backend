using backend.Dto;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints
{
    public static class ProposalEndpoints
    {
        public static void MapProposalEndpoints(this WebApplication app)
        {
            app.MapPost("/api/proposal", async ([FromBody] HowWeMetCreateDto dto, MemoDbContext context, IWebHostEnvironment env) =>
            {
                if (dto.Files.Any(x => x.Length > 1 * 1024 * 1024)) // 50MB limit
                    return Results.BadRequest("File too large.");
                var howWeMet = new Proposal
                {
                    WeddingStoryId = dto.WeddingStoryId,
                    Story = dto.Story,
                    Date = dto.Date,

                    Location = dto.Location,

                };
                context.Proposals.Add(howWeMet);
                await context.SaveChangesAsync();
                var medias = new List<HowWeMetMedia>();
                var uploadsDir = Path.Combine(env.WebRootPath, "proposal");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }
                foreach (var file in dto.Files)
                {
                    var fileName = $"proposal-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    var fileUrl = $"/proposal/{fileName}";
                    var media = new HowWeMetMedia
                    {
                        HowWeMetId = howWeMet.Id,
                        Url = fileUrl,
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
                    Media = medias
                };
                return Results.Ok(howwemetMediaResponse);
            }).WithTags("Proposal").Produces<HowWeMetResponseDto>(StatusCodes.Status200OK);

            app.MapGet("/api/proposal/{id}", async (Guid id, MemoDbContext context) =>
            {
                var howWeMet = await context.Proposals
                    .Include(h => h.Media)
                    .FirstOrDefaultAsync(h => h.Id == id);
                return howWeMet != null ? Results.Ok(howWeMet) : Results.NotFound();
            }).WithTags("Proposal");

            app.MapDelete("/api/proposal/delete/{howwemetId}", async (Guid howwemetId, MemoDbContext context, IWebHostEnvironment env) =>
            {
                var proposal = await context.Proposals.SingleOrDefaultAsync(x => x.Id == howwemetId);
                var howwemetMedia = await context.HowWeMetMedias.Where(x => x.HowWeMetId == proposal.Id).ToListAsync();
                if (howwemetMedia != null)
                {
                    foreach (var media in howwemetMedia)
                    {
                        var uploadsDir = Path.Combine(env.WebRootPath, "proposal");
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
                var result = context.Proposals.Remove(proposal);
                await context.SaveChangesAsync();
                return Results.Ok(result);

            }).WithTags("Proposal");
            app.MapPut("/api/proposal/update", async ([FromBody] HowWeMetCreateDto dto, MemoDbContext context, IWebHostEnvironment env) =>
            {
                var howWeMet = await context.Proposals.Include(h => h.Media).FirstOrDefaultAsync(h => h.Id == dto.Id);
                if (howWeMet == null)
                    return Results.NotFound();
                howWeMet.Story = dto.Story;
                howWeMet.Location = dto.Location;
                howWeMet.Date = dto.Date;
                context.Proposals.Update(howWeMet);
                await context.SaveChangesAsync();
                var uploadsDir = Path.Combine(env.WebRootPath, "proposal");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }
                var existingMedia = await context.HowWeMetMedias.Where(m => m.HowWeMetId == howWeMet.Id).ToListAsync();
                if (existingMedia.Any())
                {
                    foreach (var media in existingMedia)
                    {

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

                foreach (var file in dto.Files)
                {
                    var fileName = $"proposal-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    var fileUrl = $"/proposal/{fileName}";
                    var media = new HowWeMetMedia
                    {
                        HowWeMetId = howWeMet.Id,
                        Url = fileUrl,
                        Type = file.ContentType,
                    };
                    context.HowWeMetMedias.Add(media);
                }
                await context.SaveChangesAsync();
                return Results.Ok(howWeMet);
            }).WithTags("Proposal").Produces<HowWeMetResponseDto>(StatusCodes.Status200OK);
            app.MapGet("/api/proposal/media/{howwemetId}", async (Guid howwemetId, MemoDbContext context) =>
            {
                var media = await context.HowWeMetMedias.Where(m => m.HowWeMetId == howwemetId).ToListAsync();
                return media != null ? Results.Ok(media) : Results.NotFound();
            }).WithTags("Proposal");


            app.MapDelete("/api/proposal/delete-media/{howWemetMeidaId}", async (Guid howWemetMeidaId, MemoDbContext db, IWebHostEnvironment env) =>
            {
                var media = await db.HowWeMetMedias.SingleOrDefaultAsync(x => x.Id == howWemetMeidaId);
                var uploadsDir = Path.Combine(env.WebRootPath, "proposal");
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

            }).WithTags("Proposal").WithDescription("This api is to remove image from How Met image list");
        }


    }
}
