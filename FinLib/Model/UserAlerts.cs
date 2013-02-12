using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib.Model
{
   public class UserAlerts
    {
        public int userId { get; set; }

        public string portfolioAlerts { get; set; }

        public string watchlistAlerts { get; set; }

        public int watchlistId { get; set; }
    }
}
