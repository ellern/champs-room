using ChampsRoom.Models;
using System.Collections.Generic;

namespace ChampsRoom.ViewModels
{
    public class ResultCreateViewModel
    {
        public League League { get; set; }
        public Match Match { get; set; }
        public int UsersInLeague { get; set; }
        public ICollection<User> Users { get; set; }
    }
}