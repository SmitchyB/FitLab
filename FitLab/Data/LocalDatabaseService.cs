using LiteDB;
using System.Configuration;
using System.Diagnostics;
using FitLab.AppState;

/// <summary>
/// This service handles local database operations using LiteDB.
/// </summary>
namespace FitLab.Data
{
    public class LocalDatabaseService
    {
        private readonly string _dbPath = "FitLabData.db"; // Default database path

        /// Constructor that can take a custom database path
        public void SaveUser(User user)
        {
            using var db = new LiteDatabase(_dbPath); // Open the database file
            var col = db.GetCollection<User>("users"); // Get or create the "users" collection
            col.Upsert(user); // Insert or update the user record
        }
        // This method saves a user to the database.
        public User? LoadFirstUser()
        {
            using var db = new LiteDatabase(_dbPath); // Open the database file

            // Load the AppState from the *same db instance*
            var appstateCol = db.GetCollection<FitLab.AppState.AppState>("appstate"); // Get or create the "appstate" collection
            var state = appstateCol.FindById(1); // Find the AppState record with ID 1
            var id = state?.CurrentUserId; // Get the CurrentUserId from the AppState

            if (id.HasValue) // Check if CurrentUserId is set
            {
                var col = db.GetCollection<User>("users"); // Get or create the "users" collection
                var user = col.FindById(id.Value); // Find the user by CurrentUserId
                return user; // Return the user if found
            }
            return null; // Return null if no user is found or CurrentUserId is not set
        }
        // This method saves the current user ID to the AppState in the database.
        public void SaveCurrentUserId(Guid userId)
        {
            using var db = new LiteDatabase(_dbPath); // Open the database file
            var col = db.GetCollection<FitLab.AppState.AppState>("appstate"); // Get or create the "appstate" collection
            var state = col.FindById(1) ?? new FitLab.AppState.AppState(); // Find the AppState record with ID 1, or create a new one if it doesn't exist
            state.CurrentUserId = userId; // Set the CurrentUserId to the provided userId
            col.Upsert(state); // Insert or update the AppState record
        }
        // This method retrieves the current user ID from the AppState in the database.
        public Guid? LoadCurrentUserId()
        {
            using var db = new LiteDatabase(_dbPath); // Open the database file
            var col = db.GetCollection<FitLab.AppState.AppState>("appstate"); // Get or create the "appstate" collection
            var state = col.FindById(1); // Find the AppState record with ID 1
            return state?.CurrentUserId; // Return the CurrentUserId if found, otherwise return null
        }
    }
}
