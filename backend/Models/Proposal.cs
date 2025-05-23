﻿using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Proposal
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WeddingStoryId { get; set; }
        public string Story { get; set; }
        public string Date { get; set; } // e.g., "2023-10-01"
        public string Location { get; set; } // e.g., "Paris, France"

        public virtual List<ProposalMedia> Media { get; set; } // One-to-many with ProposalMedia
        public virtual WeddingStory WeddingStory { get; set; } // Navigation property to WeddingStory

    }
}
