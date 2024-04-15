using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogApplication.Models
{
    [Table("AppUsers")]
    public class AppUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        public string FullName { get; set; }

        public string Address { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Password { get; set; }

        public ICollection<Post> Posts { get; set; }
    }

    public class Login
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
