namespace DatingApp.API.Helpers
{
    public class UserParams : PaginationParams
    {
        public string CurrentUsername { get; set; }

        public string Gender { get; set; }

        public int MinAge { get; set; } = 18;

        public int MaxAge { get; set; } = 1500;

        public string OrderBy { get; set; }
    }
}