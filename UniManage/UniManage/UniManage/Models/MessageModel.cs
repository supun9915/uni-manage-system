using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class MessageModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public int ReceiverId { get; set; }

        /// <summary>Links replies into a thread. Null = root message.</summary>
        public int? ParentMessageId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public bool IsReadByReceiver { get; set; } = false;

        public bool IsDeletedBySender { get; set; } = false;

        public bool IsDeletedByReceiver { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(SenderId))]
        public UserModel? Sender { get; set; }

        [ForeignKey(nameof(ReceiverId))]
        public UserModel? Receiver { get; set; }

        [ForeignKey(nameof(ParentMessageId))]
        public MessageModel? ParentMessage { get; set; }

        public ICollection<MessageModel> Replies { get; set; } = new List<MessageModel>();
    }
}
