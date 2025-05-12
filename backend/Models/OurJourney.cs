using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class OurJourney
    {
        [Key]
        public Guid Id { get; set; }= Guid.NewGuid();
        [Required]
        public string Name { get; set; }
        [Required]
        [MaxLength(100)]
        public string Description { get; set; }
        [Required]
        public string Date { get; set; }
        public Guid WeddingId { get; set; }
        public virtual WeddingStory WeddingStory { get; set; }
    }
}
