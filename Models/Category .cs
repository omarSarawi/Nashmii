using System.ComponentModel.DataAnnotations.Schema;
using test2.Models;

namespace test2.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string  ? Title { get; set; }

        public string? Description { get; set; }

        
        public int? UserId { get; set; }

        public string? ImgUrl { get; set; }

        public User? User { get; set; } 

    }
}

