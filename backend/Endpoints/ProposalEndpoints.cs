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
            app.MapPost("/api/proposal", async ([FromForm] ProposalCreateDto dto, MemoDbContext context, IWebHostEnvironment env) =>
            {
                if (dto.Files.Any(x => x.Length > 1 * 1024 * 1024)) // 50MB limit
                    return Results.BadRequest("File too large.");
                var proposal = new Proposal
                {
                    WeddingStoryId = dto.WeddingStoryId,
                    Story = dto.Story,
                    Date = dto.Date,

                    Location = dto.Location,

                };
                context.Proposals.Add(proposal);
                await context.SaveChangesAsync();
                var medias = new List<ProposalMedia>();
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
                    var media = new ProposalMedia
                    {
                        ProposalId = proposal.Id,
                        Url = fileUrl,
                        Type = file.ContentType,
                    };
                    medias.Add(media);
                }
                await context.ProposalMedias.AddRangeAsync(medias);
                await context.SaveChangesAsync();
                var proposalMediaResponse = new ProposalResponseDto
                {
                    Id = proposal.Id,
                    WeddingStoryId = proposal.WeddingStoryId,
                    Story = proposal.Story,
                    Date = proposal.Date,
                    Location = proposal.Location,
                    Media = medias.Select(media=>new ProposalMediaResponseDto
                    {
                        Id = proposal.Id,
                        ProposalId = proposal.Id,
                        Url = media.Url,
                        Type = media.Type,
                    }).ToList()
                };
                return Results.Ok(proposalMediaResponse);
            }).WithTags("Proposal").Produces<ProposalResponseDto>(StatusCodes.Status200OK);

            app.MapGet("/api/proposal/{id}", async (Guid id, MemoDbContext context) =>
            {
                var proposal = await context.Proposals
                    .Include(h => h.Media)
                    .FirstOrDefaultAsync(h => h.Id == id);
                var proposalMediaResponse = new ProposalResponseDto
                {
                    Id = proposal.Id,
                    WeddingStoryId = proposal.WeddingStoryId,
                    Story = proposal.Story,
                    Date = proposal.Date,
                    Location = proposal.Location,
                    Media = proposal.Media.Select(media => new ProposalMediaResponseDto
                    {
                        Id = proposal.Id,
                        ProposalId = proposal.Id,
                        Url = media.Url,
                        Type = media.Type,
                    }).ToList()
                };
                return proposalMediaResponse != null ? Results.Ok(proposalMediaResponse) : Results.NotFound();
            }).WithTags("Proposal").Produces<ProposalResponseDto>(StatusCodes.Status200OK);

            app.MapDelete("/api/proposal/delete/{ProposalId}", async (Guid ProposalId, MemoDbContext context, IWebHostEnvironment env) =>
            {
                var proposal = await context.Proposals.SingleOrDefaultAsync(x => x.Id == ProposalId);
                var ProposalMedia = await context.ProposalMedias.Where(x => x.ProposalId == proposal.Id).ToListAsync();
                if (ProposalMedia != null)
                {
                    foreach (var media in ProposalMedia)
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
                        context.ProposalMedias.Remove(media);
                    }
                }
                var result = context.Proposals.Remove(proposal);
                await context.SaveChangesAsync();
                return Results.Ok(result);

            }).WithTags("Proposal");
            app.MapPut("/api/proposal/update", async ([FromBody] ProposalCreateDto dto, MemoDbContext context, IWebHostEnvironment env) =>
            {
                var Proposal = await context.Proposals.Include(h => h.Media).FirstOrDefaultAsync(h => h.Id == dto.Id);
                if (Proposal == null)
                    return Results.NotFound();
                Proposal.Story = dto.Story;
                Proposal.Location = dto.Location;
                Proposal.Date = dto.Date;
                context.Proposals.Update(Proposal);
                await context.SaveChangesAsync();
                var uploadsDir = Path.Combine(env.WebRootPath, "proposal");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }
                var existingMedia = await context.ProposalMedias.Where(m => m.ProposalId == Proposal.Id).ToListAsync();
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
                        context.ProposalMedias.Remove(media);
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
                    var media = new ProposalMedia
                    {
                        ProposalId = Proposal.Id,
                        Url = fileUrl,
                        Type = file.ContentType,
                    };
                    context.ProposalMedias.Add(media);
                }
                await context.SaveChangesAsync();
                return Results.Ok(Proposal);
            }).WithTags("Proposal").Produces<ProposalResponseDto>(StatusCodes.Status200OK);
            app.MapGet("/api/proposal/media/{proposalId}", async (Guid proposalId, MemoDbContext context) =>
            {
                var media = await context.ProposalMedias.Where(m => m.ProposalId == proposalId).ToListAsync();
                return media != null ? Results.Ok(media) : Results.NotFound();
            }).WithTags("Proposal");


            app.MapDelete("/api/proposal/delete-media/{ProposalMeidaId}", async (Guid ProposalMeidaId, MemoDbContext db, IWebHostEnvironment env) =>
            {
                var media = await db.ProposalMedias.SingleOrDefaultAsync(x => x.Id == ProposalMeidaId);
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
