namespace backend.Dto
{
    public class ProposalCreateDto
    {
        
        public Guid WeddingStoryId { get; set; }
        public string Story { get; set; }
        public string Location { get; set; } // e.g., "Paris, France"
        public string Date { get; set; } // e.g., "2023-10-01"
    }
    public class ProposalResponseDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WeddingStoryId { get; set; }
        public string Story { get; set; }
        public string Location { get; set; } // e.g., "Paris, France"
        public string Date { get; set; } // e.g., "2023-10-01"
        public List<ProposalMediaResponseDto> Media { get; set; } = new List<ProposalMediaResponseDto>();
    } public class ProposalUpdateDto
    {
        public Guid Id { get; set; }
        public Guid WeddingStoryId { get; set; }
        public string Story { get; set; }
        public string Location { get; set; } // e.g., "Paris, France"
        public string Date { get; set; } // e.g., "2023-10-01"
    }
    public class ProposalMediaDto
    {
        public Guid ProposalId { get; set; }
        public IFormFile File { get; set; }
    }
    public class ProposalMediaResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProposalId { get; set; }
        public string Url { get; set; }
        public string Type { get; set; } // "image" or "video"
    }
}
