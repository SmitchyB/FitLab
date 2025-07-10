namespace FitLab.Data
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public double HeightInches { get; set; }


        // Weight history
        public List<WeightEntry> WeightHistory { get; set; } = new();

        // True when intake is completed
        public bool CompletedIntake { get; set; }

        // Goals
        public List<Goal> Goals { get; set; } = new();

        // Daily water intake logs
        public List<DailyWaterIntake> WaterIntake { get; set; } = new();

        // Daily meals
        public List<DailyFoodIntake> FoodIntake { get; set; } = new();

        // Before pictures from intake
        public List<BeforePicture> BeforePictures { get; set; } = new();

        // Weekly progress photos
        public List<WeeklyProgress> WeeklyProgressPictures { get; set; } = new();
    }

    public class WeightEntry
    {
        public DateTime Date { get; set; }
        public double WeightLbs { get; set; }
        public double WeightKg { get; set; }
    }

    public class Goal
    {
        public string Description { get; set; } = string.Empty;
        public string Timeframe { get; set; } = string.Empty;
    }

    public class DailyWaterIntake
    {
        public DateTime Date { get; set; }
        public int Cups { get; set; }
    }

    public class Meal
    {
        public string MealTime { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PortionSize { get; set; } = string.Empty;
        public int Calories { get; set; }
    }

    public class DailyFoodIntake
    {
        public DateTime Date { get; set; }
        public List<Meal> Meals { get; set; } = new();
    }

    public class BeforePicture
    {
        public string FilePath { get; set; } = string.Empty;
        public DateTime DateTaken { get; set; }
        public string Type { get; set; } = string.Empty; // Frontal, Left, Right, Back
    }

    public class WeeklyProgress
    {
        public int WeekNumber { get; set; }
        public List<BeforePicture> Pictures { get; set; } = new();
    }
}
