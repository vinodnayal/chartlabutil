using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;

namespace ChartLabFinCalculation
{
    class LongShortAlertDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(LongShortAlertDAO));

        public static void UpdateLongShortAlerts()
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deletelongshortSymbolsComm = new OdbcCommand("delete FROM lngshrtsymbols", con);
            OdbcCommand deleteintermediateAlertSymbolsComm = new OdbcCommand("delete FROM intermediatelngshrtsymbols", con);
            OdbcCommand deletelongshortAlertsComm = new OdbcCommand("delete FROM longshortAlerts", con);

            OdbcCommand insertLongSymbolsComm = new OdbcCommand("INSERT INTO lngshrtsymbols (symbol,prevrating,currating,ratingdiff,changedate,lngshrtid) " +
                                                    "SELECT T3.symbol,T3.prevrtng,T3.currtng,T3.diff,T3.chngedate,1 AS lngId FROM " +
                                                    "(SELECT T1.symbol,((T2.ratingValue-T1.ratingValue)*100/t1.ratingValue) AS diff,T1.rating AS prevrtng,T2.rating AS currtng,T1.preSBRatingDate AS chngedate FROM " +
                                                    "(SELECT h.symbol,h.rating,h.ratingdate,h.ratingValue,s.preSBRatingDate FROM historybuysellrating AS h JOIN " +
                                                    "symbolanalytics AS s " +
                                                    "ON h.symbol=s.symbol " +
                                                    "WHERE s.preSBRatingDate>=(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.D_5 + "') AND h.ratingdate=s.preSBRatingDate)T1 " +
                                                    "JOIN " +
                                                    "(SELECT * FROM  historybuysellrating WHERE ratingDate =(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.C + "'))T2 " +
                                                    "ON T1.symbol=T2.symbol " +
                                                    "WHERE T1.rating<T2.rating AND T2.rating>=4) T3", con);

            OdbcCommand insertLongAlertsComm = new OdbcCommand("INSERT INTO longshortalerts (symbol,prevrating,currentrating,lngshrtid,preratingdate) " +
                                                    "(SELECT symbol,prevrating,currating,lngshrtid,changedate FROM lngshrtsymbols WHERE lngshrtid=1 " +
                                                    "ORDER BY changedate DESC, ratingdiff DESC LIMIT 3)", con);


            OdbcCommand insertIntermediateLongAlertsComm = new OdbcCommand("INSERT INTO intermediatelngshrtsymbols (symbol,prevrating,currating,ratingdiff,changedate,intermediatelngshrtid) " +
                                                    "SELECT T3.symbol,T3.prevrtng,T3.currtng,T3.diff,T3.chngedate,1 AS lngId FROM " +
                                                    "(SELECT T1.symbol,((T2.ratingValue-T1.ratingValue)*100/t1.ratingValue) AS diff,T1.rating AS prevrtng,T2.rating AS currtng,T1.preSBRatingDate AS chngedate FROM " +
                                                    "(SELECT h.symbol,h.rating,h.ratingdate,h.ratingValue,s.preSBRatingDate FROM historybuysellrating AS h JOIN " +
                                                    "symbolanalytics AS s " +
                                                    "ON h.symbol=s.symbol " +
                                                    "WHERE s.preSBRatingDate>=(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.D_5 + "') AND h.ratingdate=s.preSBRatingDate)T1 " +
                                                    "JOIN " +
                                                    "(SELECT * FROM  historybuysellrating WHERE ratingDate =(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.C + "'))T2 " +
                                                    "ON T1.symbol=T2.symbol " +
                                                    "WHERE T1.rating<T2.rating AND T2.rating<4) T3", con);

            OdbcCommand insertShortSymbolsComm = new OdbcCommand("INSERT INTO lngshrtsymbols (symbol,prevrating,currating,ratingdiff,changedate,lngshrtid) " +
                                                    "SELECT T3.symbol,T3.prevrtng,T3.currtng,T3.diff,T3.chngedate,2 AS shrtId FROM " +
                                                    "(SELECT T1.symbol,((T2.ratingValue-T1.ratingValue)*100/t1.ratingValue) AS diff,T1.rating AS prevrtng,T2.rating AS currtng,T1.preSBRatingDate AS chngedate FROM " +
                                                    "(SELECT h.symbol,h.rating,h.ratingdate,h.ratingValue,s.preSBRatingDate FROM historybuysellrating AS h JOIN " +
                                                    "symbolanalytics AS s  " +
                                                    "ON h.symbol=s.symbol " +
                                                    "WHERE s.preSBRatingDate>=(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.D_5 + "') AND h.ratingdate=s.preSBRatingDate)T1 " +
                                                    "JOIN " +
                                                    "(SELECT * FROM  historybuysellrating WHERE ratingDate =(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.C + "'))T2 " +
                                                    "ON T1.symbol=T2.symbol " +
                                                    "WHERE  T1.rating>T2.rating and T2.rating<=2) T3", con);

            OdbcCommand insertShortAlertsComm = new OdbcCommand("INSERT INTO longshortalerts (symbol,prevrating,currentrating,lngshrtid,preratingdate) " +
                                              "(SELECT symbol,prevrating,currating,lngshrtid,changedate FROM lngshrtsymbols WHERE lngshrtid=2 " +
                                              "ORDER BY changedate DESC, ratingdiff DESC LIMIT 3)", con);

            OdbcCommand insertIntermediateShortAlertsComm = new OdbcCommand("INSERT INTO intermediatelngshrtsymbols (symbol,prevrating,currating,ratingdiff,changedate,intermediatelngshrtid) " +
                                                    "SELECT T3.symbol,T3.prevrtng,T3.currtng,T3.diff,T3.chngedate,2 AS shrtId FROM " +
                                                    "(SELECT T1.symbol,((T2.ratingValue-T1.ratingValue)*100/t1.ratingValue) AS diff,T1.rating AS prevrtng,T2.rating AS currtng,T1.preSBRatingDate AS chngedate FROM " +
                                                    "(SELECT h.symbol,h.rating,h.ratingdate,h.ratingValue,s.preSBRatingDate FROM historybuysellrating AS h JOIN " +
                                                    "symbolanalytics AS s  " +
                                                    "ON h.symbol=s.symbol " +
                                                    "WHERE s.preSBRatingDate>=(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.D_5 + "') AND h.ratingdate=s.preSBRatingDate)T1 " +
                                                    "JOIN " +
                                                    "(SELECT * FROM  historybuysellrating WHERE ratingDate =(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.C + "'))T2 " +
                                                    "ON T1.symbol=T2.symbol " +
                                                    "WHERE  T1.rating>T2.rating and T2.rating>2) T3", con);


            OdbcCommand updatePriceinLongShortAlert = new OdbcCommand("UPDATE longshortAlerts l, " +
                                                    "(SELECT DISTINCT t1.symbol,t2.close FROM longshortAlerts AS t1 JOIN symbolshistorical AS t2 " +
                                                    "ON t1.symbol=t2.symbol " +
                                                    "WHERE t2.date=(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.D_2 + "')) Temp1, " +
                                                    "(SELECT DISTINCT t1.symbol,t2.close FROM longshortAlerts AS t1 JOIN symbolshistorical AS t2 " +
                                                    "ON t1.symbol=t2.symbol " +
                                                    "WHERE t2.date=(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.D_5 + "'))  Temp2 " +
                                                    "SET l.twodayprice=Temp1.close,l.fivedayprice=Temp2.close " +
                                                    "WHERE l.symbol=Temp1.symbol AND l.symbol=Temp2.symbol", con);

            OdbcCommand insertSnPsymbol = new OdbcCommand("INSERT INTO longshortAlerts (symbol,lngshrtid,fivedayprice,twodayprice) " +
                                                            "(SELECT 'SPY',3,(SELECT `close` FROM symbolshistorical WHERE `date`=(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.D_5 + "') AND symbol='SPY')," +
                                                            "(SELECT `close` FROM symbolshistorical WHERE `date`=(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.D_2 + "') AND symbol='SPY'))", con);

            try
            {

                con.Open();

                deletelongshortSymbolsComm.ExecuteNonQuery();
                log.Info("\n Data deleted from table longshortSymbols\n");

                deletelongshortAlertsComm.ExecuteNonQuery();
                log.Info("\n Data deleted from table longshortAlerts\n");

                insertLongSymbolsComm.ExecuteNonQuery();
                insertShortSymbolsComm.ExecuteNonQuery();

                insertLongAlertsComm.ExecuteNonQuery();
                insertShortAlertsComm.ExecuteNonQuery();


                deleteintermediateAlertSymbolsComm.ExecuteNonQuery();
                insertIntermediateLongAlertsComm.ExecuteNonQuery();
                insertIntermediateShortAlertsComm.ExecuteNonQuery();

                updatePriceinLongShortAlert.ExecuteNonQuery();
                insertSnPsymbol.ExecuteNonQuery();
                log.Info("\n Data Updated in table longshortAlerts\n");

                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

        }
    }
}
