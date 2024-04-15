using BlogApplication.Models;
using System.Security.Principal;

namespace BlogApplication.Context
{
    public class DatabaseInitializer
    {
        public static void Initialize(DatabaseContext context)
        {
            if (context.AppUsers.Any())
            {
                return;
            }
            else
            {
                var appUsers = new AppUser[]
                {
                };
                context.AppUsers.AddRange(appUsers);
                context.SaveChanges();

                var categories = new Category[]
                {
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();

                var posts = new Post[]
                {
                };
                context.Post.AddRange(posts);
                context.SaveChanges();
            }
        }
    }
}
