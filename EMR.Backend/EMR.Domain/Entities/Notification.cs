using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMR.Domain.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        // Specific user ID if the notification is for a single user (e.g., Doctor).
        // Null if targeting a whole role/group.
        public int? UserId { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        // Target role if broadcasting to a group (e.g., "Receptionist", "Admin").
        public string? RoleTarget { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
