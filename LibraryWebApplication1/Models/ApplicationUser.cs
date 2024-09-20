namespace LibraryWebApplication1.Models
{
    public class ApplicationUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int IsLogged { get; set; }
    }
}
