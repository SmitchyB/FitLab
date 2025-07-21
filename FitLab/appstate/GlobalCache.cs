using FitLab.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLab.appstate
{
    public static class GlobalCache
    {
        public static List<Exercise> AllExercises { get; set; } = new();

        public static List<string> MuscleGroups => AllExercises.Select(e => e.MuscleGroup).Distinct().ToList();
        public static List<string> EquipmentList => AllExercises.SelectMany(e => e.Equipment).Distinct().ToList();
        public static List<string> Difficulties => AllExercises.Select(e => e.Difficulty).Distinct().ToList();
        public static List<string> Types => AllExercises.SelectMany(e => e.Type).Distinct().ToList();

        public static void Reload()
        {
            AllExercises = LocalDatabaseService.LoadExercises();
        }
    }

}
