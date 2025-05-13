using backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Dto
{
    
        public class WeddingCreateDto
{
        public string BrideName { get; set; }
        public string GroomName { get; set; }
        public string BrideVows { get; set; }
        public string GroomVows { get; set; }
        public string ThankYouMessage { get; set; }
        public string? ThemePreference { get; set; }
        public string? TemplateChoice { get; set; }
        public string WeddingDate { get; set; } // e.g., "2023-10-01"
        public string WeddingLocation { get; set; } // e.g., "Paris, France"
        public string? CoverImage { get; set; } // e.g., "cover_image.jpg"
        public Guid PlannerId { get; set; } // Foreign key to Planner
        public bool IsPublic { get; set; } // Indicates if the wedding story is public or private
        //public IFormFile[] Gallery { get; set; }
        //public IFormFile CoverImage { get; set; }

    }
    public class WeddingResponseDto
    {
        public Guid Id { get; set; }
        public string BrideName { get; set; }
        public string GroomName { get; set; }
        public string BrideVows { get; set; }
        public string GroomVows { get; set; }
        public string ThankYouMessage { get; set; }
        public string? ThemePreference { get; set; }
        public string? TemplateChoice { get; set; }
        public string WeddingDate { get; set; } // e.g., "2023-10-01"
        public string WeddingLocation { get; set; } // e.g., "Paris, France"
        public string? CoverImage { get; set; } // e.g., "cover_image.jpg"
        public Guid PlannerId { get; set; } // Foreign key to Planner
        public bool IsPublic { get; set; } // Indicates if the wedding story is public or private
        public bool IsActive { get; set; } = true; // Indicates if the wedding story is active
        public bool IsDeleted { get; set; } = false;
        public List<MediaFileResponseDto> Gallery { get; set; } = new List<MediaFileResponseDto>();
        public WQRCodeResponse QrCode { get; set; } = new WQRCodeResponse();

        
        public List<GuestMessage> GuestMessages { get; set; } = new List<GuestMessage>();
        public ProposalResponseDto Proposal { get; set; } = new ProposalResponseDto(); // One-to-one with Proposal
        public HowWeMetResponseDto HowWeMet { get; set; } = new HowWeMetResponseDto(); // One-to-many with HowWeMet
        public PlannerResponseDto Planner { get; set; } = new PlannerResponseDto(); // Navigation property to Planner
        public List<OurJourney> OurJourneys { get; set; } = new List<OurJourney>();
    }
    public class WQRCodeResponse
    {
        public Guid Id { get; set; }
        public Guid WeddingId { get; set; }
        public string Url { get; set; }
        public string AssetUrl { get; set; }

        public int Scans { get; set; }
    }
    public class WeddingUpdateDto
    {
        public Guid Id { get; set; }
        public string BrideName { get; set; }
        public string GroomName { get; set; }
        public string BrideVows { get; set; }
        public string HowWeMet { get; set; }
        public string GroomVows { get; set; }
        public string Proposal { get; set; }
        public string ThankYouMessage { get; set; }
        public string? ThemePreference { get; set; }
        public string? TemplateChoice { get; set; }
        public string WeddingDate { get; set; } // e.g., "2023-10-01"
        public string WeddingLocation { get; set; } // e.g., "Paris, France"
        public string? CoverImage { get; set; } // e.g., "cover_image.jpg"
        public Guid PlannerId { get; set; } // Foreign key to Planner
        public bool IsPublic { get; set; } // Indicates if the wedding story is public or private
        public bool IsActive { get; set; } = true; // Indicates if the wedding story is active
        public bool IsDeleted { get; set; } = false;

    }

}