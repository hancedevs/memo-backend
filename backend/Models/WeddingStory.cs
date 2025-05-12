using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class WeddingStory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
   public string BrideName { get; set; }
   public string GroomName { get; set; }
        public string BrideVows { get; set; }
        public string? ThemePreference { get; set; }
        public string? TemplateChoice { get; set; }
        public string GroomVows { get; set; }
    public string ThankYouMessage { get; set; }
        public string WeddingDate { get; set; } // e.g., "2023-10-01"
        public string WeddingLocation { get; set; } // e.g., "Paris, France"
        public string? CoverImage { get; set; } // e.g., "cover_image.jpg"
        public Guid PlannerId { get; set; } // Foreign key to Planner
        public virtual List<Media> Gallery { get; set; }
        public virtual  List<GuestMessage> GuestMessages { get; set; }
        public WQRCode QRCode { get; set; } // One-to-one with QRCode
        public virtual Proposal Proposals { get; set; } // One-to-one with Proposal
        public virtual HowWeMet HowWeMetStories { get; set; } // One-to-many with HowWeMet
        public virtual Planner Planner { get; set; } // Navigation property to Planner
        public virtual List<OurJourney> OurJourneys { get; set; }

    }
}