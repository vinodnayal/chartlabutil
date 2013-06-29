using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ChartLabFinCalculation
{
    public class Constants
    {
        public static string MyConString = ConfigurationManager.AppSettings["ConnectionString"];
        public static string MongoConString = ConfigurationManager.AppSettings["MongoServer"];
        public const int MAX_ID_DB = 10000;
        public const int Vol_ALERT_POSITIVE = 1;
        public const int Vol_ALERT_NEGATIVE = -1;
        public const int Vol_ALERT_NEUTRAL = 0;
        public const int HIST_DATA_LENGTH=4;
        public const int HIST_DATA_ONE_YEAR = 1;
        public const int HIST_DAYS_LENGTH_FOR_SA = 200;

        public static string SymbolHistoricalTble = "symbolshistorical";
        public static string SNPSymbolHistoricalTble = "snpsymbolshistorical";

        public static string W="Weekly";
        public static string M = "Monthly";
        public static string Q = "Quaterly";
        public static string Y = "Yearly";
        public static string P = "PreviousDay";
        public static string D_5 = "5days";
        public static string D_2 = "2days";
        public static string C = "Current";
        public static string M_1 = "oneMonth";
        public static string BATSUsername = "labchart";
        public static string BATSPassword = "testdev";
        public static int Strong_Symbol = 1;
        public static int Weak_Symbol = 0;
        public const int LONG_ALERT = 1;
        public const int SHORT_ALERT = 2;
        public const int SNP_ALERT = 3;
        public const double DEFAULT_VALUE = 0;
        public const string DEFAULT_DATE_FORMAT = "0001-01-01";
        public const string MIN_DATE_FORMAT = "0000-00-00";
        public const int DEFENSIVE_GROUP = 1;
        public const int AGGRESIVE_GROUP = 2;
        public const int START_DATE = 1;
        public const int START_MONTH = 1;
        public const string SnPSymbol = "AAPL";
        public const string GSPCSymbol = "^GSPC";
        public const string GSPCGoogleSymbol = ".INX";
        public const string ExtremelyOverbought = "OBOS1";
        public const string Overbought = "OBOS2";
        public const string ApproachingOverbought = "OBOS3";
        public const string ExtremelyOversold = "OBOS4";
        public const string Oversold = "OBOS5";
        public const string ApproachingOversold = "OBOS6";
        public const string LongAlert1 = "LA1";
        public const string LongAlert2 = "LA2";
        public const string ShortAlert1 = "SA1";
        public const string ShortAlert2 = "SA2";
        public const string ShortAlert3 = "SA3";
        public const string ShortAlert4 = "SA4";

        public const string IntermediateLongAlert1 = "ILA1";
        public const string IntermediateLongAlert2 = "ILA2";
        public const string IntermediateLongAlert3 = "ILA3";
        public const string IntermediateShortAlert1 = "ISA1";
        public const string IntermediateShortAlert2 = "ISA2";
        public const string IntermediateShortAlert3 = "ISA3";

        public const string StrongBuy = "CR5";
        public const string Buy = "CR4";
        public const string Neutral = "CR3";
        public const string Sell = "CR2";
        public const string StrongSell = "CR1";

        public const int RISK_FREE_RATE = 2;
        public const int EXPECTED_MARKET_RETURN = 6;
        public const int PortfolioAlertSubsId = 0;
        public const int WatchlistAlertsubsId = 3;
        public const int DivPortAlertSubsId = 2;

        public const string AdminEmail = "brett@thechartlab.com";
        public const string SmtpServer = "smtp.gmail.com";
        public const int SmtpPort = 587;
        public const string AdminMailPassword = "bartman2424";

        public const String HtmlStartString = "<html xmlns='http://www.w3.org/1999/xhtml' xml:lang='en' lang='en'> <body> "
        +"<div style='border: 2px #EEEEEE solid; width: 800px;'>"
          +"  <table style='width: 100%;background-color: #F5F5F5;'>"
            +"    <td><img height='45' src='http://www.chartlabpro.com/images/logoGray.png'/></td>"
              + "  <td><img src='http://www.chartlabpro.com/images/img111.png'/> <br><font color='Orange' size='3'>Alerts</font></td>"
              + "  <td><a style='float:right' href='http://www.chartlabpro.com/site/login'>Sign In</a></td>"
            +"</table>"
            +"<div style='padding: 20px'>"
               +"<div style='border: 4px #F5F5F5 solid; '>"
                  +"  <div style='background-color: #F5F5F5; height: 20px;'></div> "
                    +"<div  style='padding: 20px'>";



        public const String HtmlEndString = "</div></div><br> <table>"
                    +"<td><a href='http://www.chartlabpro.com/site/login' target='_blank'><img src='http://www.chartlabpro.com/images/goAccountBtn.png' height='31' ></a></td>"
                   +" <td><a href='http://www.facebook.com/pages/ChartLabPro/190086267736457' target='_blank'><img src='http://www.chartlabpro.com/images/img2.png' width='170' height='32' alt='facebook'></a></td>"
                 +"   <td><a href='https://twitter.com/ChartLabPro' target='_blank'><img src='http://www.chartlabpro.com/images/img3.png' width='86' height='32' alt='twitter'></a></td>"
                +"</table> </div></div></body></html>";
        
    }
}
