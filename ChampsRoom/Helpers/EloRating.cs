using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChampsRoom.Helpers
{
    public class EloRating
    {
        public double Point1 { get; set; }
        public double Point2 { get; set; }

        public double FinalResult1 { get; set; }
        public double FinalResult2 { get; set; }

        public EloRating(double CurrentRating1, double CurrentRating2, double Score1, double Score2)
        {
            /*
            double CurrentR1 = 1500.0;
            double CurrentR2 = 1500.0;

            double Score1 = 20.0;
            double Score2 = 10;
            */

            double E = 0;

            if (Score1 != Score2)
            {
                if (Score1 > Score2)
                {
                    E = 120 - Math.Round(1 / (1 + Math.Pow(10, ((CurrentRating2 - CurrentRating1) / 400))) * 120);
                    FinalResult1 = CurrentRating1 + E;
                    FinalResult2 = CurrentRating2 - E;
                }
                else
                {
                    E = 120 - Math.Round(1 / (1 + Math.Pow(10, ((CurrentRating1 - CurrentRating2) / 400))) * 120);
                    FinalResult1 = CurrentRating1 - E;
                    FinalResult2 = CurrentRating2 + E;
                }
            }
            else
            {
                if (CurrentRating1 == CurrentRating2)
                {
                    FinalResult1 = CurrentRating1;
                    FinalResult2 = CurrentRating2;
                }
                else
                {
                    if (CurrentRating1 > CurrentRating2)
                    {
                        E = (120 - Math.Round(1 / (1 + Math.Pow(10, ((CurrentRating1 - CurrentRating2) / 400))) * 120)) - (120 - Math.Round(1 / (1 + Math.Pow(10, ((CurrentRating2 - CurrentRating1) / 400))) * 120));
                        FinalResult1 = CurrentRating1 - E;
                        FinalResult2 = CurrentRating2 + E;
                    }
                    else
                    {
                        E = (120 - Math.Round(1 / (1 + Math.Pow(10, ((CurrentRating2 - CurrentRating1) / 400))) * 120)) - (120 - Math.Round(1 / (1 + Math.Pow(10, ((CurrentRating1 - CurrentRating2) / 400))) * 120));
                        FinalResult1 = CurrentRating1 + E;
                        FinalResult2 = CurrentRating2 - E;
                    }
                }
            }

            Point1 = (((FinalResult1 - CurrentRating1) > 0) ? (FinalResult1 - CurrentRating1) : (FinalResult1 - CurrentRating1));
            Point2 = (((FinalResult2 - CurrentRating2) > 0) ? (FinalResult2 - CurrentRating2) : (FinalResult2 - CurrentRating2));
        }
    }


    public class EloRating2
    {
        /// 
        /// Updates the scores in the passed matchup. 
        /// 

        /// The Matchup to update
        /// Whether User 1 was the winner (false if User 2 is the winner)
        /// The desired Diff
        /// The desired KFactor

        public static void UpdateScores(Matchup matchup, bool user1WonMatch, int diff, int kFactor)
        {
            double est1 = 1 / Convert.ToDouble(1 + 10 ^ (matchup.User2Score - matchup.User1Score) / diff);
            double est2 = 1 / Convert.ToDouble(1 + 10 ^ (matchup.User1Score - matchup.User2Score) / diff);

            int sc1 = 0;
            int sc2 = 0;

            if (user1WonMatch)
                sc1 = 1;
            else
                sc2 = 1;

            matchup.User1Score = Convert.ToInt32(Math.Round(matchup.User1Score + kFactor * (sc1 - est1)));
            matchup.User2Score = Convert.ToInt32(Math.Round(matchup.User2Score + kFactor * (sc2 - est2)));
        }

        /// 
        /// Updates the scores in the match, using default Diff and KFactors (400, 100)
        /// 
        /// The Matchup to update
        /// Whether User 1 was the winner (false if User 2 is the winner)
        public static void UpdateScores(Matchup matchup, bool user1WonMatch)
        {
            UpdateScores(matchup, user1WonMatch, 400, 10);
        }

        public class Matchup
        {
            public int User1Score { get; set; }
            public int User2Score { get; set; }
        }

    }
}