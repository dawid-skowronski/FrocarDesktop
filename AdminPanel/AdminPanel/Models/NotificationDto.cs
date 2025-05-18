namespace AdminPanel.Models 
{ 
    public class NotificationDto 
    { 
        public int NotificationId { get; set; } 
        public int UserId { get; set; } 
        public UserDto User { get; set; } 
        public string Message { get; set; } 
        public string CreatedAt { get; set; } 
        public bool IsRead { get; set; } 
    } 
}