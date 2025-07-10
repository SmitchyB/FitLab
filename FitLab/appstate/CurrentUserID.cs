using LiteDB;

namespace FitLab.AppState
{
    public class AppState
    {
        [BsonId]
        public int Id { get; set; } = 1;  // Always ID = 1, so you only have 1 record
        public Guid? CurrentUserId { get; set; }
    }
}
