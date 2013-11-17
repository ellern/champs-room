using ChampsRoom.Models;
using System.Collections.Generic;

namespace ChampsRoom.ViewModels
{
    public class ResultDetailsViewModel
    {
        public League League { get; set; }
        public User User { get; set; }
        public ICollection<Match> Matches { get; set; }
    }
}