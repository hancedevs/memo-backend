// Endpoints/WeddingEndpoints.cs
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using backend.Dto;
using backend;
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;
using Microsoft.OpenApi.Models;
using Org.BouncyCastle.Utilities.Collections;

public static class WeddingEndpoints
{
    public static void MapWeddingEndpoints(this WebApplication app)
    {
        app.MapGet("/api/weddings", async (MemoDbContext db, HttpContext context) =>
        {

            var weddings = await db.Weddings.Where(x=>x.IsActive==true&&x.IsDeleted==false)
                .ToListAsync();
            var stories = new List<WeddingResponseDto>();
            var planners = weddings.Any() ?
            await db.Planners.Where(x => weddings.Select(c => c.PlannerId).Contains(x.Id)).Select(y => new PlannerResponseDto
            {
                Id=y.Id,
                Name = y.Name,
                Email = y.Email,
                Phone = y.Phone,
                Logo= y.Logo,
            }).ToListAsync()
            
            :null;
            foreach (var story in weddings)
            {

                //var gallery = story.Gallery.Any() ? story.Gallery.Where(x => !x.IsCoverImage).Select(g => new MediaFileResponseDto
                //{
                //    Id = g.Id,
                //    Url = g.Url,
                //    Type = g.Type,
                //    IsCoverImage = g.IsCoverImage
                //}).ToList() : null;
                var qrcode = story.QRCode != null ? new WQRCodeResponse
                {
                    Id = story.QRCode.Id,
                    Url = story.QRCode.Url,
                    AssetUrl = story.QRCode.AssetUrl,
                    Scans = story.QRCode.Scans
                } : null;
                var response = new WeddingResponseDto
                {
                    Id = story.Id,
                    BrideName = story.BrideName,
                    GroomName = story.GroomName,
                    BrideVows = story.BrideVows,
                    GroomVows = story.GroomVows,
                    Proposal = story.Proposals!=null?new ProposalResponseDto
                    {
                        Id = story.Proposals.Id,
                        Story = story.Proposals.Story,
                        Date = story.Proposals.Date,
                        Location = story.Proposals.Location,
                    }: null,
                    ThankYouMessage = story.ThankYouMessage,
                    //Gallery = gallery,
                    QrCode = qrcode,
                    CoverImage = story.CoverImage,
                    HowWeMet = story.HowWeMetStories!=null?new HowWeMetResponseDto
                    {
                        Id = story.HowWeMetStories.Id,
                       Story = story.HowWeMetStories.Story,
                        Date = story.HowWeMetStories.Date,
                        Location = story.HowWeMetStories.Location,
                    }: null,
                    OurJourneys =story.OurJourneys,
                    ThemePreference = story.ThemePreference,
                    TemplateChoice = story.TemplateChoice,
                    WeddingDate = story.WeddingDate,
                    WeddingLocation = story.WeddingLocation,
                    PlannerId = story.PlannerId,
                    Planner = planners.SingleOrDefault(x=>x.Id==story.PlannerId),
                };
                stories.Add(response);
            }

            return Results.Ok(stories);
        }).WithName("Wedding").WithTags("Wedding").WithDescription("This is to list weddings").Produces<List<WeddingResponseDto>>(StatusCodes.Status200OK);

        app.MapPost("/api/weddings", async ([FromBody] WeddingCreateDto story, MemoDbContext db, HttpContext context, FileStorageService fileStorageService) =>
        {

            if (string.IsNullOrWhiteSpace(story.BrideName) || string.IsNullOrWhiteSpace(story.GroomName))
                return Results.BadRequest("Couple name and How met are required.");
            var weddingStory = new WeddingStory
            {
                BrideName = story.BrideName,
                GroomName = story.GroomName,
                BrideVows = story.BrideVows,
                GroomVows = story.GroomVows,
                ThankYouMessage = story.ThankYouMessage,
                CoverImage = story.CoverImage,
                ThemePreference = story.ThemePreference,
                TemplateChoice = story.TemplateChoice,
                WeddingDate = story.WeddingDate,
                WeddingLocation = story.WeddingLocation,
                PlannerId = story.PlannerId

            };
            await db.Weddings.AddAsync(weddingStory);
            await db.SaveChangesAsync();

            return Results.Ok(weddingStory);
        })
            .WithTags("Wedding")
        .WithDescription("This is api to add wedding").WithName("AddWedding");

        app.MapGet("/api/weddings/{id}", async (Guid id, MemoDbContext db) =>
        {
            var story = await db.Weddings
            .Include(w => w.QRCode)
            .Include(w => w.Gallery)
            .Include(w => w.Proposals)
            .Include(w => w.GuestMessages)
            .Include(w => w.HowWeMetStories)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (story == null)
            {
                return Results.NotFound("Wedding story not found.");
            }
            var planners =  await db.Planners.Where(x => x.Id == story.PlannerId).Select(y => new PlannerResponseDto
           {
               Id = y.Id,
               Name = y.Name,
               Email = y.Email,
               Phone = y.Phone,
               Logo = y.Logo,
           }).FirstOrDefaultAsync();
            var coimage = story.Gallery.FirstOrDefault(x => x.IsCoverImage);
            var gallery = story.Gallery.Any() ? story.Gallery.ToList().Select(g => new MediaFileResponseDto
            {
                Id = g.Id,
                Url = g.Url,
                Type = g.Type,
                IsCoverImage = g.IsCoverImage,
                WeddingId=g.WeddingId

            }).ToList() : new List<MediaFileResponseDto>();
            var qrcode = story.QRCode != null ? new WQRCodeResponse
            {
                Id = story.QRCode.Id,
                Url = story.QRCode.Url,
                AssetUrl = story.QRCode.AssetUrl,
                Scans = story.QRCode.Scans,
                WeddingId = story.QRCode.WeddingId,
            } : new WQRCodeResponse();
           
            var howmet = story.HowWeMetStories != null ? new HowWeMetResponseDto
            {
                Id = story.HowWeMetStories.Id,
                Story = story.HowWeMetStories.Story,
                Date = story.HowWeMetStories.Date,
                Location = story.HowWeMetStories.Location,
                WeddingStoryId = story.Id,
                Media = db.HowWeMetMedias.Where(c => c.HowWeMetId == story.HowWeMetStories.Id).Select(x => new HowWeMetMediaResponseDto
                {
                    Id = x.Id,
                    Url = x.Url,
                    Type = x.Type,
                    HowWeMetId = story.HowWeMetStories.Id
                }).ToList()
            } : null;

            var proposal = story.Proposals != null ? new ProposalResponseDto
            {
                Id = story.Proposals.Id,
                Story = story.Proposals.Story,
                Date = story.Proposals.Date,
                Location = story.Proposals.Location,
                WeddingStoryId = story.Id,
                Media = db.ProposalMedias.Where(c => c.ProposalId == story.Proposals.Id).Select(x => new ProposalMediaResponseDto
                {
                    Id = x.Id,
                    Url = x.Url,
                    Type = x.Type,
                    ProposalId = story.Proposals.Id
                }).ToList()
            } : null;
            //var guestMessage = db.GuestMessages.Where(c => c.WeddingId == story.Id).Select(x => new GuestMessageResponseDto
            //{
            //    Id = x.Id,
            //    Message = x.Message,
            //    SenderName = x.SenderName,
            //    RelationToCouple = x.RelationToCouple,
            //    WeddingId = story.Id
            //}).ToList();
            var response = new WeddingResponseDto
            {
                Id = story.Id,
                BrideName = story.BrideName,
                GroomName = story.GroomName,
                BrideVows = story.BrideVows,
                GroomVows = story.GroomVows,
                Proposal = proposal,
                ThankYouMessage = story.ThankYouMessage,
                Gallery = gallery,
                QrCode = qrcode,
                CoverImage = coimage !=null?coimage.Url:"",
                //GuestMessages = guestMessage,
                HowWeMet = howmet,
                OurJourneys = story.OurJourneys,
                ThemePreference = story.ThemePreference,
                TemplateChoice = story.TemplateChoice,
                WeddingDate = story.WeddingDate,
                WeddingLocation = story.WeddingLocation,
                PlannerId = story.PlannerId,
                Planner = planners,
            };
            return Results.Ok(response);
            
           

        }).WithName("GetWedding").WithTags("Wedding").WithDescription("This is api to get wedding by id");

        app.MapPut("/api/weddings/{id}", async (Guid id, [FromBody] WeddingUpdateDto updatedStory, MemoDbContext db, HttpContext context) =>
        {
            updatedStory.Id = id;
            var existingStory = await db.Weddings.FirstOrDefaultAsync(w => w.Id == id);
            if (existingStory == null) return Results.NotFound();

            existingStory.BrideName = updatedStory.BrideName;
            existingStory.GroomName = updatedStory.GroomName;
            existingStory.BrideVows = updatedStory.BrideVows;
            existingStory.GroomVows = updatedStory.GroomVows;
            existingStory.ThankYouMessage = updatedStory.ThankYouMessage;
            existingStory.WeddingDate = updatedStory.WeddingDate;
            existingStory.WeddingLocation = updatedStory.WeddingLocation;
            existingStory.CoverImage = updatedStory.CoverImage;
            existingStory.ThemePreference = updatedStory.ThemePreference;
            existingStory.TemplateChoice = updatedStory.TemplateChoice;
            existingStory.PlannerId = updatedStory.PlannerId;
            existingStory.IsPublic = updatedStory.IsPublic;
            existingStory.IsActive = updatedStory.IsActive;

            db.Weddings.Update(existingStory);
            await db.SaveChangesAsync();
            return Results.Ok(existingStory);
        }).WithName("updatewedding").WithDescription("This api is to update the wedding story").WithTags("Wedding");

        app.MapGet("/api/weddings/planner/{id}", async (Guid id, MemoDbContext db) =>
        {
            var weddings = await db.Weddings
                .Where(w => w.PlannerId == id)
                .ToListAsync();
            return Results.Ok(weddings);
        }).WithName("GetWeddingByPlannerId").WithTags("Wedding").WithDescription("This is api to get wedding by planner id");


        app.MapDelete("/api/weddings/{id}", async (Guid id, MemoDbContext db) =>
        {
            var wedding = await db.Weddings.FindAsync(id);
            if (wedding == null) return Results.NotFound();
            wedding.IsDeleted=true;
            db.Weddings.Update(wedding);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeleteWedding").WithTags("Wedding").WithDescription("This is api to delete wedding by id");

        app.MapGet("/api/weddings/qr/{id}", async (Guid id, MemoDbContext db) =>
        {
            var qrcode = await db.QRCodes.FirstOrDefaultAsync(q => q.WeddingId == id);
            if (qrcode == null) return Results.NotFound();
            return Results.Ok(qrcode);
        }).WithName("GetWeddingQRCode")
        .WithDescription("This is api to get wedding qr code").WithTags("QR Code");

        app.MapPost("/api/media/upload/coverImage", async ([FromForm] MediaFileDto file, MemoDbContext db, IWebHostEnvironment env) =>
        {
            if (file.File.Length > 1 * 1024 * 1024) // 50MB limit
                return Results.BadRequest("File too large.");

            if(file.WeddingId == Guid.Empty)
                return Results.BadRequest("WeddingId is required.");
            var existingCoverImage = await db.Weddings.FirstOrDefaultAsync(m => m.Id == file.WeddingId);
            if(existingCoverImage == null)
                return Results.NotFound("Wedding not found.");
            var uploadsDir = Path.Combine(env.WebRootPath, "media");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.File.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.File.CopyToAsync(stream);
            }
            var fileUrl = $"/media/{fileName}";
            existingCoverImage.CoverImage = fileUrl;    
            db.Weddings.Update(existingCoverImage);
            await db.SaveChangesAsync();
            return Results.Ok(new { Url = fileUrl });
        })
                 .WithTags("Wedding").DisableAntiforgery();

       
    }

    private static RouteHandlerBuilder WithResponseCache(this RouteHandlerBuilder builder, int duration)
    {
        return duration > 0
            ? builder.AddEndpointFilter(async (context, next) =>
            {
                context.HttpContext.Response.GetTypedHeaders().CacheControl = new()
                {
                    Public = true,
                    MaxAge = TimeSpan.FromSeconds(duration)
                };
                return await next(context);
            })
            : builder;
    }
}