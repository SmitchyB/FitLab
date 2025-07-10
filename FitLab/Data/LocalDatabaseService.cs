using LiteDB;
using System.Configuration;
using System.Diagnostics;
using FitLab.AppState;

namespace FitLab.Data
{
    public class LocalDatabaseService
    {
        private readonly string _dbPath = "FitLabData.db";

        public void SaveUser(User user)
        {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<User>("users");
            col.Upsert(user);
        }

        public User? LoadFirstUser()
        {
            using var db = new LiteDatabase(_dbPath);

            // Load the AppState from the *same db instance*
            var appstateCol = db.GetCollection<FitLab.AppState.AppState>("appstate");
            var state = appstateCol.FindById(1);
            var id = state?.CurrentUserId;

            Debug.WriteLine($"[FitLab] Attempting to load user with CurrentUserId: {id}");

            if (id.HasValue)
            {
                var col = db.GetCollection<User>("users");
                var user = col.FindById(id.Value);
                if (user != null)
                    Debug.WriteLine($"[FitLab] User with Id {id} found in database.");
                else
                    Debug.WriteLine($"[FitLab] User with Id {id} NOT found in database.");
                return user;
            }

            Debug.WriteLine("[FitLab] No CurrentUserId stored.");
            return null;
        }

        public void SaveCurrentUserId(Guid userId)
        {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<FitLab.AppState.AppState>("appstate");

            var state = col.FindById(1) ?? new FitLab.AppState.AppState();
            state.CurrentUserId = userId;
            col.Upsert(state);
        }

        public Guid? LoadCurrentUserId()
        {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<FitLab.AppState.AppState>("appstate");

            var state = col.FindById(1);
            return state?.CurrentUserId;
        }
    }
}
