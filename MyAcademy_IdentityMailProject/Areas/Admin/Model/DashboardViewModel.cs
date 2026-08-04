namespace IdentityMail.Web.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int TotalMessages { get; set; }

        public int TodayMessages { get; set; }

        public int UnreadMessages { get; set; }

        public int TrashMessages { get; set; }
    }
}