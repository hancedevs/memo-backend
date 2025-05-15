using backend.Dto;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints
{
    public static class GuestEndpoints
    {
        public static void MapGuestEndpoints(this WebApplication app)
        {
           
           
            app.MapPost("/api/guestmessage", async ([FromBody] GuestMessage message, MemoDbContext db) =>
            {
                db.GuestMessages.Add(message);
                await db.SaveChangesAsync();
                return Results.Ok(message);
            }).WithName("AddGuestMessage")
            .WithDescription("This is api to add blessings from guests").WithTags("Blessing");

            app.MapGet("/api/guestmessage/{id}", async (Guid id, MemoDbContext db) =>
            {
                var messages = await db.GuestMessages
                    .Where(m => m.WeddingId == id)
                    .ToListAsync();
                return Results.Ok(messages);
            }).WithName("GetGuestMessages")
             .WithDescription("This is api to get blessings from guests by wedding id").WithTags("Blessing");

            app.MapDelete("/api/guestmessage/{id}", async (Guid id, MemoDbContext db) =>
            {
                var message = await db.GuestMessages.FindAsync(id);
                if (message == null) return Results.NotFound();
                db.GuestMessages.Remove(message);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).WithName("DeleteGuestMessage").WithTags("Blessing").WithDescription("This is api to delete blessing by id");

            app.MapGet("/api/guestMessageByWeddingId/{weddingId}", async (Guid weddingId, MemoDbContext db) =>
            {
                var gm= await db.GuestMessages.Where(c=>c.WeddingId==weddingId).ToListAsync();

                var response = (from gue in gm select new GuestResponseDto { 
                Id = gue.Id,
                SenderName = gue.SenderName,
                Message = gue.Message,
                WeddingId = weddingId,
                }).ToList();

                return Results.Ok(response);
            }).WithTags("GuestMessage").Produces<List<GuestResponseDto>>(StatusCodes.Status200OK);
        }

    }
}
