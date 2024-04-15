using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogApplication.Models
{
    [Table("Posts")]
    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostID { get; set; }

        [ForeignKey("AppUser")]
        public int AuthorID { get; set; }
        public AppUser AppUser { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public bool PublishStatus { get; set; }

        public int CategoryID { get; set; }
        public Category Category { get; set; }
    }
}
