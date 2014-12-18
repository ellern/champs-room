using ChampsRoom.Models;
using System.Collections.Generic;

namespace ChampsRoom.ViewModels
{
    public class TeamsIndexViewModel
    {
        public League League { get; set; }
        public ICollection<Team> Teams { get; set; }
        public ICollection<User> Users { get; set; }
    }
}