using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class HowWeMetMedia
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid HowWeMetId { get; set; }
        public string MediaUrl { get; set; }
        public string MediaType { get; set; } // e.g., "image", "video"
        public string MediaTypeUrl { get; set; } // e.g., "image/png", "video/mp4"
        public string MediaName { get; set; } // e.g., "wedding_photo.png", "wedding_video.mp4"
        public string MediaDescription { get; set; } // e.g., "Bride and groom at the beach", "Proposal moment"
        public string MediaSize { get; set; } // e.g., "2MB", "500KB"
        public virtual HowWeMet HowWeMet { get; set; } // Navigation property to HowWeMet

    }
}
