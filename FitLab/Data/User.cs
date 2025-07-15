namespace FitLab.Data
{
    // User data model
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Unique identifier for the user
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow; // Creation date in UTC
        public string Name { get; set; } = string.Empty; // User's name
        public string Gender { get; set; } = string.Empty; // User's Gender
        public DateTime DateOfBirth { get; set; } // User's date of birth
        public double HeightInches { get; set; } // User's height in inches
        public List<WeightEntry> WeightHistory { get; set; } = new(); // User's weight history
        public bool CompletedIntake { get; set; } // Whether the user has completed the intake process
        public List<Goal> Goals { get; set; } = new(); // User's fitness goals
        public List<DailyWaterIntake> WaterIntake { get; set; } = new(); // User's daily water intake records
        public List<DailyFoodIntake> FoodIntake { get; set; } = new(); // User's daily food intake records
        public List<WeeklyProgress> WeeklyProgressPictures { get; set; } = new(); // User's weekly progress pictures
    }
    // Weight Entry model for the Weight History
    public class WeightEntry
    {
        public DateTime Date { get; set; } // Date of the weight entry
        public double WeightLbs { get; set; } // Weight in pounds
    }
    // Goal model for the user's fitness goals
    public class Goal
    {
        public string Description { get; set; } = string.Empty; // Description of the goal
        public int TimeframeAmount { get; set; } // Amount of time to achieve the goal
        public string TimeframeUnit { get; set; } = string.Empty; // Timeframe unit for the goal (e.g., days, weeks, months)
        public DateTime Date { get; set; } // Date when the goal was set
        public bool IsCompleted { get; set; } // Whether the goal has been completed
        public DateTime? CompletedOn { get; set; } // Date when the goal was completed, if applicable
    }
    // Daily Water Intake model for tracking water consumption
    public class DailyWaterIntake
    {
        public DateTime Date { get; set; } // Date of the water intake record
        public int Cups { get; set; } // Number of cups of water consumed
    }
    // Meal model for tracking daily food intake
    public class Meal
    {
        public string MealTime { get; set; } = string.Empty; // Time of the meal (e.g., Breakfast, Lunch, Dinner)
        public string Description { get; set; } = string.Empty; // Description of the meal
        public string PortionSize { get; set; } = string.Empty; // Portion size of the meal
        public int Calories { get; set; } // Calories in the meal
    }
    // Daily Food Intake model for tracking meals consumed in a day
    public class DailyFoodIntake
    {
        public DateTime Date { get; set; } // Date of the food intake record
        public List<Meal> Meals { get; set; } = new(); // List of meals consumed on that day
    }
    // ProgressPicture model for tracking before pictures
    public class ProgressPicture
    {
        public string FilePath { get; set; } = string.Empty; // Path to the picture file
        public DateTime DateTaken { get; set; } // Date when the picture was taken
        public string Type { get; set; } = string.Empty; // Frontal, Left, Right, Back
    }
    // WeeklyProgress model for tracking weekly progress pictures
    public class WeeklyProgress
    {
        public List<ProgressPicture> Pictures { get; set; } = new(); // List of pictures taken for that week
    }
}
