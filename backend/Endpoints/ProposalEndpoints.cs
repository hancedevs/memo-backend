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
            app.MapPost("/api/proposal", async ([FromBody] ProposalCreateDto dto, MemoDbContext context, IWebHostEnvironment env) =>
            {
               
                var proposal = new Proposal
                {
                    WeddingStoryId = dto.WeddingStoryId,
                    Story = dto.Story,
                    Date = dto.Date,

                    Location = dto.Location,

                };
                context.Proposals.Add(proposal);
                await context.SaveChangesAsync();
                var proposalMediaResponse = new ProposalResponseDto
                {
                    Id = proposal.Id,
                    WeddingStoryId = proposal.WeddingStoryId,
                    Story = proposal.Story,
                    Date = proposal.Date,
                    Location = proposal.Location
                };
                return Results.Ok(proposalMediaResponse);
            }).WithTags("Proposal").Produces<ProposalResponseDto>(StatusCodes.Status200OK).DisableAntiforgery();

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

            app.MapDelete("/api/proposal/delete/{proposalId}", async (Guid proposalId, MemoDbContext context, IWebHostEnvironment env) =>
            {
                var proposal = await context.Proposals.SingleOrDefaultAsync(x => x.Id == proposalId);
                var ProposalMedia = await context.ProposalMedias.Where(x => x.ProposalId == proposal.Id).ToListAsync();
                if (ProposalMedia != null)
                {
                    foreach (var media in ProposalMedia)
                    {
                        var uploadsDir = Path.Combine(env.ContentRootPath, "proposal");
                        if (!Directory.Exists(uploadsDir))
                        {
                            Directory.CreateDirectory(uploadsDir);
                        }
                        string filePath = Path.Combine(env.ContentRootPath, media.Url);
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
            app.MapPut("/api/proposal/update", async ([FromBody] ProposalUpdateDto dto, MemoDbContext context, IWebHostEnvironment env) =>
            {
                var Proposal = await context.Proposals.SingleOrDefaultAsync(h => h.Id == dto.Id);
                if (Proposal == null)
                    return Results.NotFound();
                Proposal.Story = dto.Story;
                Proposal.Location = dto.Location;
                Proposal.Date = dto.Date;
                context.Proposals.Update(Proposal);
                await context.SaveChangesAsync();
                
                return Results.Ok(Proposal);
            }).WithTags("Proposal").DisableAntiforgery().Produces<ProposalResponseDto>(StatusCodes.Status200OK);

            app.MapGet("/api/proposal/media/{proposalId}", async (Guid proposalId, MemoDbContext context) =>
            {
                var media = await context.ProposalMedias.Where(m => m.ProposalId == proposalId).ToListAsync();
                var mediaResponse = media.Select(m => new ProposalMediaResponseDto
                {
                    Id = m.Id,
                    ProposalId = m.ProposalId,
                    Url = m.Url,
                    Type = m.Type
                }).ToList();
                return media != null ? Results.Ok(mediaResponse) : Results.NotFound();
            }).WithTags("Proposal").Produces<ProposalMediaResponseDto>(StatusCodes.Status200OK);


            app.MapDelete("/api/proposal/delete-media/{proposalMeidaId}", async (Guid proposalMeidaId, MemoDbContext db, IWebHostEnvironment env) =>
            {
                try
                {
                    var media = await db.ProposalMedias.SingleOrDefaultAsync(x => x.Id == proposalMeidaId);
                    var storageRoot = Path.Combine(env.ContentRootPath, "storage");
                    string filePath = Path.Combine(storageRoot, media.Url);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        Console.WriteLine($"File {filePath} deleted successfully.");
                        db.ProposalMedias.Remove(media);
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        Console.WriteLine($"File {filePath} does not exist.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                

            }).WithTags("Proposal").WithDescription("This api is to remove image from How Met image list");
            app.MapPost("/api/proposal/fileUpload", async ([FromForm] ProposalMediaDto file, MemoDbContext db, IWebHostEnvironment env) =>
            {
                if (file.File.Length > 1 * 1024 * 1024) // 50MB limit
                    return Results.BadRequest("File too large.");
                try
                {
                    var proposal = await db.Proposals.SingleOrDefaultAsync(x => x.Id == file.ProposalId);
                    if (proposal == null)
                        return Results.NotFound("Proposal not found.");
                    var storageRoot = Path.Combine(env.ContentRootPath, "storage");
                    var proposalGallery = $"{proposal.WeddingStoryId}/proposal";
                    var uploadsDir = Path.Combine(storageRoot, proposalGallery);
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
                    var fileUrl = $"{proposalGallery}/{fileName}";
                    var media = new ProposalMedia
                    {
                        ProposalId = file.ProposalId,
                        Url = fileUrl,
                        Type = file.File.ContentType,

                    };
                    await db.ProposalMedias.AddAsync(media);
                    await db.SaveChangesAsync();
                    var proposalMedia = new ProposalMediaResponseDto
                    {
                        Id = media.Id,
                        ProposalId = file.ProposalId,
                        Url = fileUrl,
                        Type = file.File.ContentType
                    };
                    return Results.Ok(proposalMedia);
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
                return Results.BadRequest("File upload is failed");
               
            }).Produces<ProposalMediaResponseDto>(StatusCodes.Status200OK).WithTags("Proposal").WithDescription("Proposal").DisableAntiforgery();

        }


    }
}
