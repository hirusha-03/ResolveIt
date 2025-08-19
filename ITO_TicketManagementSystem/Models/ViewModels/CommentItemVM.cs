namespace ITO_TicketManagementSystem.Models.ViewModels
{
    public class CommentItemVM
    {
        
        public int TicketCommentId { get; set; }
        public int TicketId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedByUserId { get; set; } = string.Empty;
        public string CreatedByUserName { get; set; } = string.Empty;
    }
}

