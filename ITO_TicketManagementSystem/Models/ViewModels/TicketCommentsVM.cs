namespace ITO_TicketManagementSystem.Models.ViewModels
{
    public class TicketCommentsVM
    {
        public int TicketId { get; set; }
        public string TicketTitle { get; set; } = string.Empty;
        public List<CommentItemVM> Comments { get; set; } = new();
        public string NewComment { get; set; } = string.Empty;
        public bool CanAddComment { get; set; }
        public string? CreatedByUserId { get; internal set; }
        public string CreatedByUserName { get; internal set; }
    }

}
