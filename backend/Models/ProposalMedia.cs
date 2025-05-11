using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class ProposalMedia
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProposalId { get; set; }
        public string Url { get; set; }
        public string Type { get; set; } // "image" or "video"
        public virtual Proposal Proposal { get; set; } // Navigation property to Proposal
    }
}
