using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChartLabFinCalculation.DAL;

namespace ChartLabFinCalculation.BL
{
   class UsersManager
    {
       static log4net.ILog log = log4net.LogManager.GetLogger(typeof(UsersManager));
        internal static void ManageFreeTrialUsers()
        {
            try
            {

               // UPDATE users SET isundertrial = 0  WHERE DATEDIFF(CURDATE(),DATE)>14 AND isundertrial = 1
                UsersDAO.updateTrialUsers();
            }
            catch (Exception ex)
            {
                log.Error("Error in weeklyADLine10Days calculations \n" + ex); ;
            }
        }
    }
}
