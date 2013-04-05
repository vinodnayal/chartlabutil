using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChartLabFinCalculation.BL
{
    class ValidationDataHelper
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(ValidationDataHelper));
        internal static void ValidateData()
        {
            try
            {
                //check daily data count in mysql
                //check daily data count in mongo
                //chck buy sell ratings
                //symbol analytics
                //email
                //
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }
        }
    }
}
