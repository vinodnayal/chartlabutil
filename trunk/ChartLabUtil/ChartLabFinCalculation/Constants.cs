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
        public const int MAX_ID_DB = 100000;
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
        


    public const String HtmlStartStringWithCss = @"
<html xmlns='http://www.w3.org/1999/xhtml' xml:lang='en' lang='en'>
<style>
.alertTable{
border:1px solid #f8f9fb;
border-spacing:0px
}

.alertTable td{
border-bottom:1px solid #f8f9fb;
}

.alertTable th{
	text-transform:uppercase;
	font-size:12px;
	padding:14px 11px;
	font-weight:bold;
	
	background: url(data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiA/Pgo8c3ZnIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDEgMSIgcHJlc2VydmVBc3BlY3RSYXRpbz0ibm9uZSI+CiAgPGxpbmVhckdyYWRpZW50IGlkPSJncmFkLXVjZ2ctZ2VuZXJhdGVkIiBncmFkaWVudFVuaXRzPSJ1c2VyU3BhY2VPblVzZSIgeDE9IjAlIiB5MT0iMCUiIHgyPSIwJSIgeTI9IjEwMCUiPgogICAgPHN0b3Agb2Zmc2V0PSIwJSIgc3RvcC1jb2xvcj0iI2Y4ZjlmYiIgc3RvcC1vcGFjaXR5PSIxIi8+CiAgICA8c3RvcCBvZmZzZXQ9IjEwMCUiIHN0b3AtY29sb3I9IiNlNWU4ZjAiIHN0b3Atb3BhY2l0eT0iMSIvPgogIDwvbGluZWFyR3JhZGllbnQ+CiAgPHJlY3QgeD0iMCIgeT0iMCIgd2lkdGg9IjEiIGhlaWdodD0iMSIgZmlsbD0idXJsKCNncmFkLXVjZ2ctZ2VuZXJhdGVkKSIgLz4KPC9zdmc+);
	background: -moz-linear-gradient(top,  #f8f9fb 0%, #e5e8f0 100%);
	background: -webkit-gradient(linear, left top, left bottom, color-stop(0%,#f8f9fb), color-stop(100%,#e5e8f0));
	background: -webkit-linear-gradient(top,  #f8f9fb 0%,#e5e8f0 100%);
	background: -o-linear-gradient(top,  #f8f9fb 0%,#e5e8f0 100%);
	background: -ms-linear-gradient(top,  #f8f9fb 0%,#e5e8f0 100%);
	background: linear-gradient(to bottom,  #f8f9fb 0%,#e5e8f0 100%);
	filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#f8f9fb', endColorstr='#e5e8f0',GradientType=0 );
background: #f8f9fb;
}
.redInfo{
	display:block;
	border:1px solid #94443e;
	width:93px;
	padding:3px 5px;
	color:#fff;
	overflow:hidden;

	background:#d26c69 url(data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiA/Pgo8c3ZnIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDEgMSIgcHJlc2VydmVBc3BlY3RSYXRpbz0ibm9uZSI+CiAgPGxpbmVhckdyYWRpZW50IGlkPSJncmFkLXVjZ2ctZ2VuZXJhdGVkIiBncmFkaWVudFVuaXRzPSJ1c2VyU3BhY2VPblVzZSIgeDE9IjAlIiB5MT0iMCUiIHgyPSIwJSIgeTI9IjEwMCUiPgogICAgPHN0b3Agb2Zmc2V0PSIwJSIgc3RvcC1jb2xvcj0iI2QyNmM2OSIgc3RvcC1vcGFjaXR5PSIxIi8+CiAgICA8c3RvcCBvZmZzZXQ9IjUlIiBzdG9wLWNvbG9yPSIjY2E1MjUwIiBzdG9wLW9wYWNpdHk9IjEiLz4KICAgIDxzdG9wIG9mZnNldD0iMTYlIiBzdG9wLWNvbG9yPSIjYzk1MjRmIiBzdG9wLW9wYWNpdHk9IjEiLz4KICAgIDxzdG9wIG9mZnNldD0iMTAwJSIgc3RvcC1jb2xvcj0iI2FlNDAzYyIgc3RvcC1vcGFjaXR5PSIxIi8+CiAgPC9saW5lYXJHcmFkaWVudD4KICA8cmVjdCB4PSIwIiB5PSIwIiB3aWR0aD0iMSIgaGVpZ2h0PSIxIiBmaWxsPSJ1cmwoI2dyYWQtdWNnZy1nZW5lcmF0ZWQpIiAvPgo8L3N2Zz4=);
	background: -moz-linear-gradient(top,  #d26c69 0%, #ca5250 5%, #c9524f 16%, #ae403c 100%);
	background: -webkit-gradient(linear, left top, left bottom, color-stop(0%,#d26c69), color-stop(5%,#ca5250), color-stop(16%,#c9524f), color-stop(100%,#ae403c));
	background: -webkit-linear-gradient(top,  #d26c69 0%,#ca5250 5%,#c9524f 16%,#ae403c 100%);
	background: -o-linear-gradient(top,  #d26c69 0%,#ca5250 5%,#c9524f 16%,#ae403c 100%);
	background: -ms-linear-gradient(top,  #d26c69 0%,#ca5250 5%,#c9524f 16%,#ae403c 100%);
	background: linear-gradient(to bottom,  #d26c69 0%,#ca5250 5%,#c9524f 16%,#ae403c 100%);
	filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#d26c69', endColorstr='#ae403c',GradientType=0 );
background:#d26c69;
}

.greenInfo{
        display:block;
	width:93px;
	padding:3px 5px;
	color:#fff;
	overflow:hidden;
	border:1px solid #709961;

      background:#94cf80;
}

.support
{
  vertical-align: middle ;  background-color:#EDF4EB;  padding:4px; color: green;   font-weight: bold;
}

.resistance
{
  vertical-align: middle ;background-color:#F1DFDF;  padding:4px; color: maroon; font-weight: bold;
}
.stock
{
width:200px;
}

.lastprice
{
width:90px;
}
.change
{
width:100px;
}
.supportTd
{
width:80px;
}
.resistanceTd
{
width:80px;
}
.alert
{
width:320px;
}
.lngterm
{
width:160px;
text-align:center;
}

.wlHeader{
font-weight:bold;
color:white;
background:#ff7f27;
}
.portHeader{
font-weight:bold;
color:white;
background:#112E55;
}
.commonWlHeader{
font-weight:bold;
color:white;
background:#008040;
}
</style>
<body> <div style='border: 2px #EEEEEE solid; width: 1000px;'> 
<table style='width: 100%;background-color: #F5F5F5;'>    <td><img height='45' src='http://www.chartlabpro.com/images/logoGray.png'/>
</td>  <td><img src='http://www.chartlabpro.com/images/img111.png'/> <br><font color='Orange' size='3'>Alerts</font></td>  <td>
<a style='float:right' href='http://www.chartlabpro.com/site/login'>Sign In</a></td></table><div style='padding: 20px'><div style='border: 4px #F5F5F5 solid; '>  
<div style='background-color: #F5F5F5; height: 20px;'></div> <div  style='padding: 20px'>
";

    }
}
