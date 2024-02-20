using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPersonalDiary.Models
{
    public class Post
    {
        public int Id { get; set; }

        [DataType(DataType.MultilineText)]
        public string Content { get; set; }
        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        [NotMapped]
        public bool CanEditAndDelete { get; set; }
    }
}