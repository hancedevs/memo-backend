using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class ProposalMedia
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProposalId { get; set; }
        public string MediaUrl { get; set; }
        public string MediaType { get; set; } // e.g., "image", "video"
        public string MediaTypeUrl { get; set; } // e.g., "image/png", "video/mp4"
        public string MediaName { get; set; } // e.g., "wedding_photo.png", "wedding_video.mp4"
        public string MediaDescription { get; set; } // e.g., "Bride and groom at the beach", "Proposal moment"
    }
}
