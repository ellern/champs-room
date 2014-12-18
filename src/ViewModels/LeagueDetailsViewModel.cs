using ChampsRoom.Models;
using System.Collections.Generic;

namespace ChampsRoom.ViewModels
{
    public class LeagueDetailsViewModel
    {
        public League League { get; set; }
        public ICollection<RatingsViewModel> Ratings { get; set; }
        public ICollection<Match> LatestMatches { get; set; }
    }
}