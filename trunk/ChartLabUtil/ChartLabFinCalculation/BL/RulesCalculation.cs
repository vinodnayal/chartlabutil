using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;

namespace ChartLabFinCalculation
{
    public class RulesCalculation
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(RulesCalculation));
        public static String SymbolRuleCsvFilePath;

        public static void CalculateSymbolRules()
        {


            try
            {
                //Calculating Rules
                
                List<SymbolRule> symbolRuleList = new List<SymbolRule>();
                log.Info("Process: Geting symbol List From DB");
                List<string> symbolList = SymbolHistoricalDAO.GetsymbolListFromDB();
                
                log.Info("Process: Geting BuySell and CTRating from DB");
                Dictionary<string, SymbolRatingAlert> ratingDict = RulesCalculationDAO.GetBuySellPlusCTRating();
                log.Info("Process: Geting LongShortAlerts from DB");
                Dictionary<string, SymbolRatingAlert> longRatingAlertDict = RulesCalculationDAO.GetLongShortAlerts(1);
                Dictionary<string, SymbolRatingAlert> shortRatingAlertDict = RulesCalculationDAO.GetLongShortAlerts(2);
                log.Info("Process: Geting LongShort Intermediate Rating Alert from DB");
                Dictionary<string, SymbolRatingAlert> longIntermediateRatingAlertDict = RulesCalculationDAO.GetIntermediateLongShortAlerts(1);
                Dictionary<string, SymbolRatingAlert> shortIntermediateRatingAlertDict = RulesCalculationDAO.GetIntermediateLongShortAlerts(2);
                log.Info("Process: calculating rules for symbol count "+symbolList.Count);
                foreach (string symbol in symbolList)
                {

                    SymbolRule symbolRule = CalculateRuleId(symbol, ratingDict, longRatingAlertDict, shortRatingAlertDict, longIntermediateRatingAlertDict, shortIntermediateRatingAlertDict);
                    symbolRule.symbol = symbol;

                    symbolRuleList.Add(symbolRule);
                }

                log.Info("Process: Inserting rules in DB " + symbolList.Count);
                CSVExporter.WriteToCSVSymbolRule(symbolRuleList, SymbolRuleCsvFilePath + "/SymbolRulesFile.csv");
                RulesCalculationDAO.InsertRules(SymbolRuleCsvFilePath);
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }

        }

        internal static SymbolRule CalculateRuleId(string symbol, Dictionary<string, SymbolRatingAlert> ratingDict, Dictionary<string, SymbolRatingAlert> longRatingAlertDict, Dictionary<string, SymbolRatingAlert> shortRatingAlertDict, Dictionary<string, SymbolRatingAlert> longIntermediateRatingAlertDict, Dictionary<string, SymbolRatingAlert> shortIntermediateRatingAlertDict)
        { 
            SymbolRule defaultrule = new SymbolRule();
            try
            {
               
                defaultrule.RuleId = "";

                log.Info("\n\nCalculating rule for symbol: " + symbol + "......\n");


                SymbolRule rule1 = CalculateRuleForOBOS(symbol, ratingDict);
                if (rule1.isTrue)
                {
                    return rule1;
                }
                SymbolRule rule2 = CalculateRuleForLongAlert(symbol, longRatingAlertDict);
                if (rule2.isTrue)
                {
                    return rule2;
                }
                SymbolRule rule3 = CalculateRuleForShortAlert(symbol, shortRatingAlertDict);
                if (rule3.isTrue)
                {
                    return rule3;
                }
                SymbolRule rule4 = CalculateRuleForIntermediateLongAlert(symbol, longIntermediateRatingAlertDict);
                if (rule4.isTrue)
                {
                    return rule4;
                }
                SymbolRule rule5 = CalculateRuleForIntermediateShortAlert(symbol, shortIntermediateRatingAlertDict);
                if (rule5.isTrue)
                {
                    return rule5;
                }
                SymbolRule rule6 = CalculateRuleForCurrentRatingAlert(symbol, ratingDict);
                if (rule6.isTrue)
                {
                    return rule6;
                }
                else
                {
                    return defaultrule;
                }
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
                return defaultrule;
            }

        }


        public static SymbolRule CalculateRuleForOBOS(string symbol, Dictionary<string, SymbolRatingAlert> ratingDict)
        {
            SymbolRule rule = new SymbolRule();
            try
            {
                
                rule.isTrue = false;
                if (ratingDict.ContainsKey(symbol))
                {

                    int ctrating = ratingDict[symbol].ctrating;
                    if (ctrating != (int)CTRatingEnum.Neutral)
                    {
                        rule.isTrue = true;
                        switch (ctrating)
                        {

                            case (int)CTRatingEnum.ExtremelyOverbought:
                                rule.RuleId = Constants.ExtremelyOverbought;
                                break;

                            case (int)CTRatingEnum.Overbought:
                                rule.RuleId = Constants.Overbought;
                                break;

                            case (int)CTRatingEnum.ApproachingOverbought:
                                rule.RuleId = Constants.ApproachingOverbought;
                                break;

                            case (int)CTRatingEnum.ExtremelyOversold:
                                rule.RuleId = Constants.ExtremelyOversold;
                                break;

                            case (int)CTRatingEnum.Oversold:
                                rule.RuleId = Constants.Oversold;
                                break;

                            case (int)CTRatingEnum.ApproachingOversold:
                                rule.RuleId = Constants.ApproachingOversold;
                                break;

                            default:
                                rule.RuleId = "";
                                rule.isTrue = false;
                                break;
                        }

                        if (rule.RuleId != "")
                        {
                            SymbolRule symbolrule = CalculateCTRatingHistoryChange(symbol, ctrating);
                            rule.ratingChangeDate = symbolrule.ratingChangeDate;
                            rule.changeDatePrice = symbolrule.changeDatePrice;
                        }
                    }
                }
                else
                {

                    log.Warn("Warn: Rule CalculateRule For OBOS, Key is not present of symbol " + symbol);

                }

            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }
            return rule;
         }

        private static SymbolRule CalculateCTRatingHistoryChange(string symbol, int ctrating)
        {
            SymbolRule rule = new SymbolRule();
            try
            {
                Dictionary<DateTime, SymbolRatingAlert> ctRatingHistDict = RulesCalculationDAO.GetCTRatingHistory(symbol);
                DateTime prevdate = new DateTime();
                DateTime curdate = new DateTime();
                double price = 0;
                int count = 0;
                foreach (KeyValuePair<DateTime, SymbolRatingAlert> pair in ctRatingHistDict)
                {
                    if (pair.Value.ctrating == ctrating)
                    {
                        prevdate = pair.Key;
                    }
                    else
                    {
                        if (count == 0)
                        {
                            curdate = pair.Key;
                            price = pair.Value.changeDatePrice;
                        }
                        else
                        {
                            curdate = prevdate;
                            price = ctRatingHistDict[curdate].changeDatePrice;
                        }
                        break;
                    }
                    count++;
                }

                rule.ratingChangeDate = curdate;
                rule.changeDatePrice = price;
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }

          return rule;
        }

        public static SymbolRule CalculateRuleForLongAlert(string symbol,Dictionary<string, SymbolRatingAlert> longRatingAlertDict)
        {
            SymbolRule rule = new SymbolRule();
            try
            {
                rule.isTrue = false;
                if (longRatingAlertDict.ContainsKey(symbol))
                {
                    int prevrating = longRatingAlertDict[symbol].prevrating;
                    int currating = longRatingAlertDict[symbol].currating;

                    rule.isTrue = true;
                    rule.prevrating = longRatingAlertDict[symbol].prevrating;
                    rule.currating = longRatingAlertDict[symbol].currating;
                    rule.ratingChangeDate = longRatingAlertDict[symbol].ratingChangeDate;
                    rule.changeDatePrice = longRatingAlertDict[symbol].changeDatePrice;

                    if ((prevrating == 3 & currating == 5) || (prevrating == 4 & currating == 5))
                    {
                        rule.RuleId = Constants.LongAlert1;
                    }
                    else if (prevrating == 3 & currating == 4)
                    {
                        rule.RuleId = Constants.LongAlert2;
                    }
                    else
                    {
                        rule.RuleId = "";
                        rule.isTrue = false;
                    }

                }
                else
                {
                    log.Warn("Warn: Calculate Rule For Long Alert, Key is not present of symbol " + symbol);
                }
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }

            return rule;
        }

        internal static SymbolRule CalculateRuleForShortAlert(string symbol, Dictionary<string, SymbolRatingAlert> shortRatingAlertDict)
        {
            SymbolRule rule = new SymbolRule();
            try
            {
                rule.isTrue = false;
                if (shortRatingAlertDict.ContainsKey(symbol))
                {
                    int prevrating = shortRatingAlertDict[symbol].prevrating;
                    int currating = shortRatingAlertDict[symbol].currating;

                    rule.isTrue = true;
                    rule.prevrating = shortRatingAlertDict[symbol].prevrating;
                    rule.currating = shortRatingAlertDict[symbol].currating;
                    rule.ratingChangeDate = shortRatingAlertDict[symbol].ratingChangeDate;
                    rule.changeDatePrice = shortRatingAlertDict[symbol].changeDatePrice;

                    if (prevrating == 4 & currating == 2)
                    {
                        rule.RuleId = Constants.ShortAlert1;
                    }
                    else if (prevrating == 3 & currating == 2)
                    {
                        rule.RuleId = Constants.ShortAlert2;
                    }
                    else if (prevrating == 3 & currating == 1)
                    {
                        rule.RuleId = Constants.ShortAlert3;
                    }
                    else if (prevrating == 2 & currating == 1)
                    {
                        rule.RuleId = Constants.ShortAlert4;
                    }
                    else
                    {
                        rule.RuleId = "";
                        rule.isTrue = false;
                    }
                }
                else
                {
                    log.Warn("Warn: Calculate Rule For short Alert, Key is not present of symbol " + symbol);
                }
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }

            return rule;
        }

        internal static SymbolRule CalculateRuleForIntermediateLongAlert(string symbol, Dictionary<string, SymbolRatingAlert> longIntermediateRatingAlertDict)
        {
            SymbolRule rule = new SymbolRule();
            try
            {
                rule.isTrue = false;
                if (longIntermediateRatingAlertDict.ContainsKey(symbol))
                {
                    int prevrating = longIntermediateRatingAlertDict[symbol].prevrating;
                    int currating = longIntermediateRatingAlertDict[symbol].currating;

                    rule.isTrue = true;
                    rule.prevrating = longIntermediateRatingAlertDict[symbol].prevrating;
                    rule.currating = longIntermediateRatingAlertDict[symbol].currating;
                    rule.ratingChangeDate = longIntermediateRatingAlertDict[symbol].ratingChangeDate;
                    rule.changeDatePrice = longIntermediateRatingAlertDict[symbol].changeDatePrice;

                    if (prevrating == 1 & currating == 3)
                    {
                        rule.RuleId = Constants.IntermediateLongAlert1;
                    }
                    else if (prevrating == 1 & currating == 2)
                    {
                        rule.RuleId = Constants.IntermediateLongAlert2;
                    }
                    else if (prevrating == 2 & currating == 3)
                    {
                        rule.RuleId = Constants.IntermediateLongAlert3;
                    }
                    else
                    {
                        rule.RuleId = "";
                        rule.isTrue = false;
                    }
                }
                else
                {
                    log.Warn("Warn: Calculate Rule For Intermediate Long Alert, Key is not present of symbol : " + symbol);
                }
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }

            return rule;
        }

        internal static SymbolRule CalculateRuleForIntermediateShortAlert(string symbol, Dictionary<string, SymbolRatingAlert> shortIntermediateRatingAlertDict)
        {
            SymbolRule rule = new SymbolRule();
            try
            {
                rule.isTrue = false;
                if (shortIntermediateRatingAlertDict.ContainsKey(symbol))
                {
                    int prevrating = shortIntermediateRatingAlertDict[symbol].prevrating;
                    int currating = shortIntermediateRatingAlertDict[symbol].currating;

                    rule.isTrue = true;
                    rule.prevrating = shortIntermediateRatingAlertDict[symbol].prevrating;
                    rule.currating = shortIntermediateRatingAlertDict[symbol].currating;
                    rule.ratingChangeDate = shortIntermediateRatingAlertDict[symbol].ratingChangeDate;
                    rule.changeDatePrice = shortIntermediateRatingAlertDict[symbol].changeDatePrice;

                    if (prevrating == 5 & currating == 4)
                    {
                        rule.RuleId = Constants.IntermediateShortAlert1;
                    }
                    else if (prevrating == 5 & currating == 3)
                    {
                        rule.RuleId = Constants.IntermediateShortAlert2;
                    }
                    else if (prevrating == 4 & currating == 3)
                    {
                        rule.RuleId = Constants.IntermediateShortAlert3;
                    }
                    else
                    {
                        rule.RuleId = "";
                        rule.isTrue = false;
                    }
                }
                else
                {
                    log.Warn("Warn: Calculate Rule For Intermediate Short Alert, Key is not present of symbol : " + symbol);
                }
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }

            return rule;
        }

        internal static SymbolRule CalculateRuleForCurrentRatingAlert(string symbol, Dictionary<string, SymbolRatingAlert> ratingDict)
        {
           SymbolRule rule = new SymbolRule();
           try
           {
               rule.isTrue = false;
               if (ratingDict.ContainsKey(symbol))
               {
                   int rating = ratingDict[symbol].currating;
                   rule.isTrue = true;
                   rule.prevrating = ratingDict[symbol].prevrating;
                   rule.currating = ratingDict[symbol].currating;
                   rule.ratingChangeDate = ratingDict[symbol].ratingChangeDate;
                   rule.changeDatePrice = ratingDict[symbol].changeDatePrice;


                   switch (rating)
                   {

                       case (int)RatingEnum.StrongBuy:
                           rule.RuleId = Constants.StrongBuy;
                           break;

                       case (int)RatingEnum.Buy:
                           rule.RuleId = Constants.Buy;
                           break;

                       case (int)RatingEnum.Neutral:
                           rule.RuleId = Constants.Neutral;
                           break;

                       case (int)RatingEnum.Sell:
                           rule.RuleId = Constants.Sell;
                           break;

                       case (int)RatingEnum.StrongSell:
                           rule.RuleId = Constants.StrongSell;
                           break;

                       default:
                           rule.RuleId = "";
                           rule.isTrue = false;
                           break;
                   }
               }
               else
               {
                   log.Warn("Warn: Calculate Rule For Current Rating Alert, Key is not present of symbol : " + symbol);
               }
           }
           catch (Exception ex)
           {

               log.Error("Error: " + ex);
           }
            return rule;
        }
    }
}
