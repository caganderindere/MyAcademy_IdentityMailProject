namespace IdentityMail.Web.Entities
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public ICollection<UserMessage> UserMessages { get; set; } = new List<UserMessage>();
    }
}
