using FitLab.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLab
{
    public static class SeedDemoData
    {
        /// <summary>
        /// If no user exists, creates one demo user with:
        /// - Intake basics (name/gender/dob/height, CompletedIntake=true)
        /// - 10 weeks of WeightHistory + BodyMeasurements
        /// - Last 10 days of WaterIntake + FoodIntake
        /// Then sets CurrentUserId in appstate.
        /// </summary>
        public static void SeedIfEmpty(LocalDatabaseService db)
        {
            var existing = db.LoadFirstUser();
            if (existing != null) return;

            var nowUtc = DateTime.UtcNow;
            var createdOn = nowUtc.AddDays(-70); // ~10 weeks ago

            var user = new User
            {
                CreatedOn = createdOn,
                Name = "Demo User",
                Gender = "Male",
                DateOfBirth = new DateTime(1995, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                HeightInches = 68, // 5'8"
                CompletedIntake = true,
                WorkoutPlan = new WorkoutPlan { PlanLength = 7, Days = new List<DailyWorkout>() }
            };

            // --- Weight: 10 weekly points (lbs), slight downward trend
            double startLbs = 200;
            var weekStart = StartOfWeek(createdOn);
            for (int i = 0; i < 10; i++)
            {
                var d = weekStart.AddDays(i * 7);
                var noise = Rand(-1.0, 1.0);
                double w = startLbs - i * Rand(0.6, 1.4) + noise;
                user.WeightHistory.Add(new WeightEntry
                {
                    Date = d,
                    WeightLbs = Math.Round(w, 1)
                });
            }

            // --- Body measurements (weekly, inches)
            for (int i = 0; i < 10; i++)
            {
                var d = weekStart.AddDays(i * 7);
                user.BodyMeasurements.Add(new WeeklyBodyMeasurement
                {
                    Date = d,
                    Chest = Round1(40 + i * Rand(-0.05, 0.05)),
                    Waist = Round1(36 + i * Rand(-0.15, -0.02)), // trending down
                    Hips = Round1(39 + i * Rand(-0.05, 0.05)),
                    Neck = Round1(14.5 + i * Rand(-0.03, 0.02)),
                    Shoulders = Round1(48 + i * Rand(-0.05, 0.05)),
                    UpperArm = Round1(14 + i * Rand(-0.04, 0.04)),
                    Forearm = Round1(11 + i * Rand(-0.03, 0.03)),
                    Wrist = Round1(6.8 + i * Rand(-0.02, 0.02)),
                    Thigh = Round1(22 + i * Rand(-0.06, 0.06)),
                    Calf = Round1(15 + i * Rand(-0.04, 0.04)),
                    Ankle = Round1(9 + i * Rand(-0.02, 0.02))
                });
            }

            // --- Water + Meals: last 10 days
            var mealNames = new[] { "Breakfast", "Lunch", "Dinner" };
            var dishes = new (string name, int min, int max)[]
            {
                ("Chicken Salad",    350, 550),
                ("Oatmeal + Fruit",  250, 400),
                ("Steak + Rice",     600, 900),
                ("Tuna Sandwich",    300, 500),
                ("Greek Yogurt",     100, 200),
                ("Pasta Marinara",   500, 800),
                ("Protein Shake",    180, 300)
            };

            for (int i = 9; i >= 0; i--)
            {
                var day = nowUtc.Date.AddDays(-i);

                // water 6–14 cups
                user.WaterIntake.Add(new DailyWaterIntake
                {
                    Date = day,
                    Cups = Randi(6, 14)
                });

                // 2–3 meals/day
                int mealCount = Randi(2, 3);
                var todaysMeals = new List<Meal>();
                for (int m = 0; m < mealCount; m++)
                {
                    var meal = new Meal { MealTime = mealNames[m] };
                    int dishCount = Randi(1, 3);
                    for (int k = 0; k < dishCount; k++)
                    {
                        var d = dishes[Randi(0, dishes.Length - 1)];
                        meal.Dishes.Add(new Dish
                        {
                            DishName = d.name,
                            PortionSize = "1 serving",
                            Ingredients = string.Empty,
                            Calories = Randi(d.min, d.max)
                        });
                    }
                    todaysMeals.Add(meal);
                }

                user.FoodIntake.Add(new DailyFoodIntake
                {
                    Date = day,
                    Meals = todaysMeals
                });
            }

            // Save + set current user
            db.SaveUser(user);
            db.SaveCurrentUserId(user.Id);
        }

        // --- helpers ---
        private static readonly Random _rng = new Random();

        private static double Rand(double min, double max) => _rng.NextDouble() * (max - min) + min;
        private static int Randi(int min, int max) => _rng.Next(min, max + 1);
        private static double Round1(double v) => Math.Round(v, 1);

        private static DateTime StartOfWeek(DateTime utc)
        {
            // Sunday start, keep UTC
            int diff = (int)utc.DayOfWeek; // Sun=0
            return utc.Date.AddDays(-diff);
        }
    }
}
