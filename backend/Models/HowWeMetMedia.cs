using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class HowWeMetMedia
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid HowWeMetId { get; set; }
        public string Url { get; set; }
        public string Type { get; set; } // "image" or "video"
        public virtual HowWeMet HowWeMet { get; set; } // Navigation property to HowWeMet

    }
}
