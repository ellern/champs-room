using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChampsRoom.Helpers
{
    public static class EloRating
    {
        public static int CalculateChange(double ratingA, double ratingB, double scoreA, double scoreB)
        {
            var r1 = scoreA > scoreB ? ratingA : ratingB;
            var r2 = scoreA < scoreB ? ratingA : ratingB;

            var calc = 120 * (1 / (1 + Math.Pow(10, (r1-r2) / 400)));

            return Convert.ToInt32(Math.Round(calc, MidpointRounding.AwayFromZero));
        }
    }
}