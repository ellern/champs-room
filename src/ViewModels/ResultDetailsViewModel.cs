using ChampsRoom.Models;
using System.Collections.Generic;

namespace ChampsRoom.ViewModels
{
    public class ResultDetailsViewModel
    {
        public League League { get; set; }
        public User User { get; set; }
        public ResultStatisticsViewModel Statistics { get; set; }
        public ICollection<Match> Matches { get; set; }
    }

    public class ResultStatisticsViewModel
    {
        public int Played { get; set; }
        public int Won { get; set; }
        public int Lost { get; set; }
        public int Draw { get; set; }
        public double WinRatio { get; set; }

        public int WinningStreak { get; set; }
        public int BestWinningStreak { get; set; }
        public int LoosingStreak { get; set; }
        public int WorstLoosingStreak { get; set; }
        public int GoalsConceded { get; set; }
        public double GoalsConcededPerMatch { get; set; }
        public int GoalsScored { get; set; }
        public double GoalsScoredPerMatch { get; set; }
        public int EggsConceded { get; set; }
        public int EggsGiven { get; set; }
        public Match BestWin { get; set; }
        public Match WorstLost { get; set; }
    }
}