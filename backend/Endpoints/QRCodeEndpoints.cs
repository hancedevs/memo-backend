using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using SkiaSharp;

namespace backend.Endpoints
{
    public static class QRCodeEndpoints
    {
        public static void MapQRCodeEndpoints(this WebApplication app)
        {
           
            app.MapGet("/api/qrcodes/generate", async (Guid weddingId, MemoDbContext db, IWebHostEnvironment env) =>
            {
                // Check for existing QR code
                var existingQrCode = await db.QRCodes.FirstOrDefaultAsync(q => q.WeddingId == weddingId);
                var storageRoot = Path.Combine(env.ContentRootPath, "storage");
                var qrcodeFolderPath = $"{weddingId}/qrcodes";
                var mediaPath = Path.Combine(storageRoot, qrcodeFolderPath);

                if (existingQrCode != null)
                {
                    var qrcodePath = Path.Combine(storageRoot, existingQrCode.AssetUrl);

                    // If the QR code file exists, return its URL
                    if (File.Exists(qrcodePath))
                    {
                        return Results.Ok(new { url = existingQrCode.Url, assetUrl = existingQrCode.AssetUrl });
                    }

                    // If the file does not exist, delete the record from the database
                    db.QRCodes.Remove(existingQrCode);
                    await db.SaveChangesAsync();
                }

                // Get the domain from environment variables or configuration
                var domain = Environment.GetEnvironmentVariable("Frontend_Url") ?? "http://memo.plate.et";

                // Check if the wedding exists
                var story = await db.Weddings.FirstOrDefaultAsync(w => w.Id == weddingId);
                if (story == null) return Results.NotFound();

                // Generate the QR code URL
                var url = $"{domain}/story/{weddingId}";
                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCodeService(qrCodeData);

                // Generate QR code using SkiaSharp
                using var bitmap = qrCode.GetGraphic(20);

                // Ensure the media directory exists
                if (!Directory.Exists(mediaPath))
                {
                    Directory.CreateDirectory(mediaPath);
                }

                // Save the QR code as PNG
                var qrCodeFileName = $"qrcode-{weddingId}.png";
                var qrCodePath = Path.Combine(mediaPath, qrCodeFileName);
                try
                {
                    using (var stream = new FileStream(qrCodePath, FileMode.Create, FileAccess.Write))
                    {
                        bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error saving QR code: {ex.Message}");
                }

                // Save the QR code record in the database
                var qrScan = new WQRCode
                {
                    WeddingId = weddingId,
                    Url = url,
                    Scans = 0,
                    AssetUrl = Path.Combine(qrcodeFolderPath, qrCodeFileName) // Store relative path
                };
                await db.QRCodes.AddAsync(qrScan);
                await db.SaveChangesAsync();

                return Results.Ok(new { url = qrScan.Url, assetUrl = qrScan.AssetUrl });
            }).WithTags("Wedding");
            app.MapGet("/api/qrcodes/{id}", async (Guid id, MemoDbContext db) =>
            {
                var qrCode = await db.QRCodes.FirstOrDefaultAsync(q => q.Id == id);
                return qrCode != null ? Results.Ok(qrCode) : Results.NotFound();
            }).WithTags("Wedding");

        }
    }
}