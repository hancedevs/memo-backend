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

            var weddings = await db.Weddings
                .Include(w => w.Gallery)
                .Include(w => w.QRCode)
                .ToListAsync();
            var stories = new List<WeddingResponseDto>();
            foreach (var story in weddings)
            {
                var coimage = story.Gallery.FirstOrDefault(x => x.IsCoverImage);
                var gallery = story.Gallery.Any() ? story.Gallery.Where(x => !x.IsCoverImage).Select(g => new MediaFileResponseDto
                {
                    Id = g.Id,
                    Url = g.Url,
                    Type = g.Type,
                    IsCoverImage = g.IsCoverImage
                }).ToList() : null;
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
                    Proposal = story.Proposal,
                    ThankYouMessage = story.ThankYouMessage,
                    Gallery = gallery,
                    QrCode = qrcode,
                    CoverImage = coimage != null ? new MediaFileResponseDto
                    {
                        Id = coimage.Id,
                        Url = coimage.Url,
                        Type = coimage.Type,
                        IsCoverImage = coimage.IsCoverImage
                    } : new MediaFileResponseDto(),
                };
                stories.Add(response);
            }

            return Results.Ok(stories);
        }).WithName("Wedding").WithTags("Wedding").WithDescription("This is to list weddings");

        app.MapPost("/api/weddings", async ([FromBody] WeddingCreateDto story, MemoDbContext db, HttpContext context, FileStorageService fileStorageService) =>
        {

            if (string.IsNullOrWhiteSpace(story.BrideName) || string.IsNullOrWhiteSpace(story.GroomName) || string.IsNullOrWhiteSpace(story.HowWeMet))
                return Results.BadRequest("Couple name and How met are required.");
            var weddingStory = new WeddingStory
            {
                BrideName = story.BrideName,
                ThankYouMessage = story.ThankYouMessage,
                Proposal = story.Proposal,
                GroomName = story.GroomName,
                GroomVows = story.GroomVows,
                BrideVows = story.BrideVows,

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

                .FirstOrDefaultAsync(w => w.Id == id);

            if (story == null)
            {
                return Results.NotFound("Wedding story not found.");
            }
            var coimage = story.Gallery.FirstOrDefault(x => x.IsCoverImage);
            var gallery = story.Gallery.Any() ? story.Gallery.Where(x => !x.IsCoverImage).Select(g => new MediaFileResponseDto
            {
                Id = g.Id,
                Url = g.Url,
                Type = g.Type,
                IsCoverImage = g.IsCoverImage
            }).ToList() : new List<MediaFileResponseDto>();
            var qrcode = story.QRCode != null ? new WQRCodeResponse
            {
                Id = story.QRCode.Id,
                Url = story.QRCode.Url,
                AssetUrl = story.QRCode.AssetUrl,
                Scans = story.QRCode.Scans
            } : new WQRCodeResponse();
            var response = new WeddingResponseDto
            {
                BrideName = story.BrideName,
                GroomName = story.GroomName,
                BrideVows = story.BrideVows,
                GroomVows = story.GroomVows,
                Proposal = story.Proposal,
                ThankYouMessage = story.ThankYouMessage,
                Gallery = gallery,
                QrCode = qrcode,
                CoverImage = coimage != null ? new MediaFileResponseDto
                {
                    Id = coimage.Id,
                    Url = coimage.Url,
                    Type = coimage.Type,
                    IsCoverImage = coimage.IsCoverImage
                } : new MediaFileResponseDto(),
            };
            return Results.Ok(response);
        }).WithName("GetWedding").WithTags("Wedding").WithDescription("This is api to get wedding by id");
        app.MapPut("/api/weddings/{id}", async (Guid id, [FromBody] WeddingStory updatedStory, MemoDbContext db, HttpContext context) =>
        {
            updatedStory.Id = id;
            var existingStory = await db.Weddings.FirstOrDefaultAsync(w => w.Id == id);
            if (existingStory == null) return Results.NotFound();

            existingStory.BrideName = updatedStory.BrideName;
            existingStory.GroomName = updatedStory.GroomName;
            existingStory.BrideVows = updatedStory.BrideVows;
            existingStory.GroomVows = updatedStory.GroomVows;
            existingStory.ThankYouMessage = updatedStory.ThankYouMessage;
            existingStory.Proposal = updatedStory.Proposal;

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
            db.Weddings.Remove(wedding);
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