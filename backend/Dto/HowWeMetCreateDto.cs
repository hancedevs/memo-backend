using backend.Models;

namespace backend.Dto
{
    public class HowWeMetCreateDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WeddingStoryId { get; set; }
        public string Story { get; set; }
        public string Location { get; set; } // e.g., "Paris, France"
        public string Date { get; set; } // e.g., "2023-10-01"
        public IFormFile[] Files { get; set; }

    }
    public class HowWeMetResponseDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WeddingStoryId { get; set; }
        public string Story { get; set; }
        public string Location { get; set; } // e.g., "Paris, France"
        public string Date { get; set; } // e.g., "2023-10-01"
        public List<HowWeMetMedia> Media { get; set; } = new List<HowWeMetMedia>();
    }
    public class HowWeMetMediaDto
    {
        public Guid Id { get; set; }
        public IFormFile File { get; set; }
    }
}
