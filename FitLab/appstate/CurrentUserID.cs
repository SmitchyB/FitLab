using LiteDB;

namespace FitLab.AppState
{
    /// <summary>
    /// This class represents the application state, specifically the current user ID.
    /// </summary>
    public class AppState
    {
        [BsonId] // This property is used as the primary key in the LiteDB database.
        public int Id { get; set; } = 1;  // Always ID = 1, so you only have 1 record
        public Guid? CurrentUserId { get; set; } // Nullable Guid to store the current user ID, if any.
    }
}
