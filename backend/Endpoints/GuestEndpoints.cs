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
           
           
            app.MapPost("/api/guestmessages", async ([FromBody] GuestMessage message, MemoDbContext db) =>
            {
                db.GuestMessages.Add(message);
                await db.SaveChangesAsync();
                return Results.Ok(message);
            }).WithName("AddGuestMessage")
            .WithDescription("This is api to add blessings from guests").WithTags("Blessing");

            app.MapGet("/api/guestmessages/{id}", async (Guid id, MemoDbContext db) =>
            {
                var messages = await db.GuestMessages
                    .Where(m => m.WeddingId == id)
                    .ToListAsync();
                return Results.Ok(messages);
            }).WithName("GetGuestMessages")
             .WithDescription("This is api to get blessings from guests by wedding id").WithTags("Blessing");

            app.MapDelete("/api/guestmessages/{id}", async (Guid id, MemoDbContext db) =>
            {
                var message = await db.GuestMessages.FindAsync(id);
                if (message == null) return Results.NotFound();
                db.GuestMessages.Remove(message);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).WithName("DeleteGuestMessage").WithTags("Blessing").WithDescription("This is api to delete blessing by id");

           
        }

    }
}
