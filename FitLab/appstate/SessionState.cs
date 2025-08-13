using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLab.AppState
{
    public static class SessionState
    {
        public static int CurrentWeek { get; set; }
        public static int CurrentWorkoutDay { get; set; }
        public static int CurrentAbsoluteDay { get; set; }

    }
}
