// Endpoints/PlannerEndpoints.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Dto;
using backend;
using backend.Models;
using Org.BouncyCastle.Asn1.Ocsp;

public static class PlannerEndpoints
{
    public static void MapPlannerEndpoints(this WebApplication app)
    {
        app.MapPost("/api/planner", async ([FromBody] PlannerCreateDto dto, MemoDbContext db,IWebHostEnvironment env) =>
        {
            try
            {
                var planner = new Planner
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Phone = dto.Phone
                };
                if (dto.Logo.Length > 0)
                {
                    var logodirc = Path.Combine(env.WebRootPath, "logo");
                    if (Directory.Exists(logodirc))
                    {
                        Directory.CreateDirectory(logodirc);
                    }
                    var fileName = $"logo-{Guid.NewGuid()}{Path.GetExtension(dto.Logo.FileName)}";
                    var fileUrl = $"logo/{fileName}";
                    var filePath = Path.Combine(logodirc, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.Logo.CopyToAsync(stream);
                    }
                    planner.Logo = fileUrl;
                }
                var plannerId = await db.Planners.AddAsync(planner);
                await db.SaveChangesAsync();
                return Results.Ok(planner);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }).WithTags("Planner").Produces<Planner>(StatusCodes.Status200OK).AddEndpointFilter(async (context, next) =>
        {
            var dto = context.GetArgument<PlannerCreateDto>(0);
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Name))
                errors.Add("Name is required.");
            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(dto.Email))
                errors.Add("Invalid email format.");
            
            return errors.Any() ? Results.BadRequest(new { Errors = errors }) : await next(context);
        });
        app.MapPut("/api/planner/update", async (Guid plannerId, [FromBody] PlannerCreateDto dto, MemoDbContext db, IWebHostEnvironment env) =>
        {
            var planner = await db.Planners.SingleOrDefaultAsync(x => x.Id == plannerId);
            if(planner == null)
            {
                return Results.BadRequest("There is no planner with that Id");
            }
            planner.Phone = dto.Phone;
            planner.Name = dto.Name;
            planner.Email = dto.Email;
            if (dto.Logo.Length > 0)
            {
                var logodirc = Path.Combine(env.WebRootPath, "logo");
                if (Directory.Exists(logodirc))
                {
                    Directory.CreateDirectory(logodirc);
                }
                string filePath = Path.Combine(env.WebRootPath, planner.Logo);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"File {filePath} deleted successfully.");
                }
                else
                {
                    Console.WriteLine($"File {filePath} does not exist.");
                }
                var fileName = $"logo-{Guid.NewGuid()}{Path.GetExtension(dto.Logo.FileName)}";
                var fileUrl = $"logo/{fileName}";
                 filePath = Path.Combine(logodirc, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Logo.CopyToAsync(stream);
                }
                planner.Logo = fileUrl;
            }
            db.Planners.Update(planner);
            await db.SaveChangesAsync();
            return Results.Ok(planner);
        }).WithTags("Planner").Produces<Planner>(StatusCodes.Status200OK).AddEndpointFilter(async (context, next) =>
        {
            var dto = context.GetArgument<PlannerCreateDto>(0);
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Name))
                errors.Add("Name is required.");
            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(dto.Email))
                errors.Add("Invalid email format.");

            return errors.Any() ? Results.BadRequest(new { Errors = errors }) : await next(context);
        });

        app.MapGet("/api/planner", async (MemoDbContext context) =>
        {
            var planners = await context.Planners.Select(p => new { p.Id, p.Name, p.Email }).ToListAsync();
            return Results.Ok(planners);
        }).WithTags("Planner").Produces<List<Planner>>(StatusCodes.Status200OK);
        app.MapGet("/api/planner/{plannerId}",async (Guid plannerId, MemoDbContext context) =>
        {
            var planner = await context.Planners.SingleOrDefaultAsync(x => x.Id == plannerId);
            if (planner == null)
            {
                return Results.NotFound("Planner not found.");
            }
            return Results.Ok(planner);
        }).WithTags("Planner").Produces<Planner>(StatusCodes.Status200OK);
    }
}