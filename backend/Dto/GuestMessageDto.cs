namespace backend.Dto
{
    public class GuestMessageDto
    {
       
        public Guid WeddingId { get; set; }
        public string Message { get; set; }
        public string SenderName { get; set; }
        public string RelationToCouple { get; set; }
    }
    public class GuestResponseDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WeddingId { get; set; }
        public string Message { get; set; }
        public string SenderName { get; set; }
        public string RelationToCouple { get; set; }
    }
}
