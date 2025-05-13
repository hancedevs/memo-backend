using backend.Models;

namespace backend.Dto
{
    public class HowWeMetCreateDto
    {
       
        public Guid WeddingStoryId { get; set; }
        public string Story { get; set; }
        public string Location { get; set; } // e.g., "Paris, France"
        public string Date { get; set; } // e.g., "2023-10-01"
        public IFormFileCollection Files { get; set; }

    } public class HowWeMetUpdateDto
    {
        public Guid? Id { get; set; }
        public Guid WeddingStoryId { get; set; }
        public string Story { get; set; }
        public string Location { get; set; } // e.g., "Paris, France"
        public string Date { get; set; } // e.g., "2023-10-01"
        public IFormFileCollection Files { get; set; }

    }
    public class HowWeMetResponseDto
    {
        public Guid Id { get; set; } 
        public Guid WeddingStoryId { get; set; }
        public string Story { get; set; }
        public string Location { get; set; } // e.g., "Paris, France"
        public string Date { get; set; } // e.g., "2023-10-01"
        public List<HowWeMetMediaResponseDto> Media { get; set; } = new List<HowWeMetMediaResponseDto>();
    }
    public class HowWeMetMediaDto
    {
        public Guid Id { get; set; }
        public IFormFileCollection File { get; set; }
    }  public class HowWeMetMediaResponseDto
    {
        public Guid Id { get; set; } 
        public Guid HowWeMetId { get; set; }
        public string Url { get; set; }
        public string Type { get; set; } // "image" or "video"
    }
}
