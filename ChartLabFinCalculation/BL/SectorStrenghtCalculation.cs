using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChartLabFinCalculation.BL
{
    class SectorStrenghtCalculation
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SectorStrenghtCalculation));
        internal static int calculateSectStrengthAlertId(double ratingValue, double ctRatingValue, double ratingValueChangePct)
        {
            int alertId = 0;
            try
            {

                if (ratingValueChangePct > 0)
                    alertId = checkForPositiveSignal(ratingValue, ctRatingValue, ratingValueChangePct);
                else
                    alertId = checkForNegativeSignal(ratingValue, ctRatingValue, ratingValueChangePct);


            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }
            return alertId;
        }

        private static int checkForPositiveSignal(double ratingValue, double ctRatingValue, double ratingValueChangePct)
        {
            int alertId = 0;
            try
            {

                //Buy: If there is a 600% increase in rating that with a minimum rating of .11 rating
                if (ratingValue >= .11 && ratingValueChangePct >= 600)
                {
                    alertId = 1;

                }
                //Buy: If there is a 390% increase in rating with a minimum rating of .15 rating
                if (ratingValue >= .15 && ratingValueChangePct >= 390)
                {
                    alertId = 1;

                }
                //Buy: If there is a 140% increase in rating that with a minimum rating of .36 rating
                if (ratingValue >= .36 && ratingValueChangePct >= 140)
                {
                    alertId = 1;

                }
                //Buy: If there is a 45% increase in rating that with a minimum rating of .599 rating

                if (ratingValue >= .599 && ratingValueChangePct >= 45)
                {
                    alertId = 1;

                }
                //Buy: If there is a 30% increase the preceding rating starting with at least a .70 rating
                if (ratingValue >= .70 && ratingValueChangePct >= 30)
                {
                    alertId = 1;

                }

                //TODO
                //Buy: If there is a 600% increase in rating that with a minimum rating of .11 rating
                //if (ratingValue >= .11 && ratingValueChangePct >= 600)
                //{
                //    alertId = 1;

                //}
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }
            return alertId;
        }

        private static int checkForNegativeSignal(double ratingValue, double ctRatingValue, double ratingValueChangePct)
        {
            int alertId = 0;
            try
            {

                //Sell: if a rating above .92 drops by 13% or more the preceding day
                if (ratingValue <= .92 && ratingValueChangePct <= -13)
                {
                    alertId = 2;

                }
                //Sell: Any rating above .8599 that drops by 15.5% the preceding day
                if (ratingValue <= .8599 && ratingValueChangePct <= -15)
                {
                    alertId = 2;

                }
                //Sell: Any rating above .72 that drops by 19% the preceding day
                if (ratingValue <= .72 && ratingValueChangePct <= -19)
                {
                    alertId = 2;

                }
                //Sell: If a rating above .50 drops 27% the preceding day
                if (ratingValue <= .50 && ratingValueChangePct <= -27)
                {
                    alertId = 2;

                }
                //Sell: If a rating above .40 drops 55% the preceding day
                if (ratingValue <= .40 && ratingValueChangePct <= -55)
                {
                    alertId = 2;

                }
                // Any rating that is positive that drops below .00 by 85% sell
                if (ratingValue <= 0 && ratingValueChangePct <= -85)
                {
                    alertId = 2;

                }

                //Sell 50%: Approaching Overbought counter Trend -56. or over
                if (ctRatingValue < -69)
                {
                    alertId = 2;

                }
                if (ctRatingValue < -56)
                {
                    alertId = 2;

                }

                //TODO
                //Sell: If we fall below .00 for five consecutive declines

                //if (ratingValue >= .11 && ratingValueChangePct >= 600)
                //{
                //    alertId = 1;

                //}
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }
            return alertId;
        }
    }
}
