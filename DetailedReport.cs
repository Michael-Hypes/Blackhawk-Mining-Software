using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace BlackHawk_Reporting
{
    class DetailedReport
    {

        SQLDataHandler dh;
        SqlDataReader dr;
        string shiftstart;
        int rawtonscalculated;
        int cleantonscalculated;
        int rawstartnum;
        int cleanstartnum;
        double downminstartnum;
        double downmincalculated;
  
        double runhrs;
        double convertrunhrsmins;
        double schrunhrs;
 
        double yield;
        double calcuatedrawtonsperhr;
        double calculatedcleantonsperhr;
        double calcmagmins;
        double magmins;


        public void createDetailedReport(int plantfeedrawtons, int cleancoaltons, int rawtonsperhr, int cleantonsperhr, double plcdownminutes, double plantrunmins, double plcmagmins)
        {

            // grab start numbers from StartEndNums table
            dh = new SQLDataHandler();

            

            //get the shift start time
            dr = dh.getReader("SELECT currshiftstarttime from ConfigurationSettings WHERE plantname = 'Kanawha Eagle'");
            dr.Read();
            shiftstart = (string)dr["currshiftstarttime"].ToString();

            //convert to date time so we can add 2hrs
            DateTime time1 = Convert.ToDateTime(shiftstart);
            DateTime time2 = time1.AddHours(2);

            //Get today's date
            string today = DateTime.Today.ToShortDateString();

            //Get previous day for use during midnight changes
            DateTime previous = DateTime.Today;
            DateTime previousday = previous.AddDays(-1);
            string prev = Convert.ToString(previousday);
            string[] prevday = prev.Split(' ');

            //Get the time
            string now = DateTime.Now.ToShortTimeString();

            //Split time to use as comparison
            string[] timesplit = now.Split(' ');
            string time = timesplit[0];
            
            //splitting hrs and minutes to use as comparisons
            string[] hrmin = time.Split(':');
            int hr = Convert.ToInt32(hrmin[0]);
            int min = Convert.ToInt32(hrmin[1]);

            //manually entered downtime descriptions
            string mandata = " ";

            //Get next day
            DateTime next = DateTime.Today;
            DateTime nextday = previous.AddDays(-1);
            string nxt = Convert.ToString(nextday);
            string[] nxtday = prev.Split(' ');


            //if shift start time is 7:00AM
            if (shiftstart.Equals("05:00:00") || shiftstart.Equals("07:00:00"))
            {
                //if the time range is between the starting and ending time range of 7AM to 9AM
                if (hr >= 7 && hr < 9 && timesplit[1].Equals("AM"))
                {
                     //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "7:00" && timesplit[1] == "AM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 05:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 05:00 AM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 07:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 07:00 AM'");
                        }
                        else
                        {
                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 07:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 07:00 AM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 07:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 07:00 AM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 07:00 AM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 07:00 AM'");
                        }
                    }
                }

                else if (hr >= 9 && hr < 11 && timesplit[1].Equals("AM"))
                {

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "9:00" && timesplit[1] == "AM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 07:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 07:00 AM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 09:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 09:00 AM'");
                        }
                        else
                        {
                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 09:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 09:00 AM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 09:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 09:00 AM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 09:00 AM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 09:00 AM'");
                        }
                    }
                }

                else if (hr >= 11 && hr < 12 && timesplit[1].Equals("AM"))
                {
 
                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "11:00" && timesplit[1] == "AM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 09:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 09:00 AM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 11:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 11:00 AM'");
                        }
                        else
                        {
                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 11:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 11:00 AM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 11:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 11:00 AM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 11:00 AM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 11:00 AM'");
                        }
                    }
                }

                else if (hr == 12 && timesplit[1].Equals("PM"))
                {
                    
                    //get the starting numbers from the StartEndNums table
                    dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 11:00 AM'");
                    dr.Read();

                    //if the starting numbers were not null do calculations
                    if (!DBNull.Value.Equals(dr["rawstart"]))
                    {
                        rawstartnum = Convert.ToInt32(dr["rawstart"]);
                        cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                        downminstartnum = (double)dr["downminutes"];
                        runhrs = (double)dr["runhrs"];
                        convertrunhrsmins = runhrs * 60;
                        magmins = (double)dr["magmins"];

                        //calculations for detailed report
                        rawtonscalculated = plantfeedrawtons - rawstartnum;
                        cleantonscalculated = cleancoaltons - cleanstartnum;
                        downmincalculated = plcdownminutes - downminstartnum;
                        schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                        calcmagmins = plcmagmins - magmins;


                        if (schrunhrs == 0)
                        {
                            calcuatedrawtonsperhr = 0.0;
                            calculatedcleantonsperhr = 0.0;
                        }
                        else
                        {
                            calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                            calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                        }



                        yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;





                        //insert data into detailed report table
                        dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                            ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                            " WHERE starttime = '" + today + " 11:00 AM'");
                    }

                    //if the starting values were null update the starting numbers, this could happen if the program wasn't
                    //running during the time ranges starting time
                    //no communication could also cause this
                    else
                    {
                        schrunhrs = plantrunmins / 60;
                        dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                            " WHERE fromtime = '" + today + " 11:00 AM'");
                        //calculations for detailed report
                        rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                        cleantonscalculated = cleancoaltons - cleancoaltons;
                        downmincalculated = plcdownminutes - plcdownminutes;
                        schrunhrs = plantrunmins / 60;
                        calcmagmins = plcmagmins - plcmagmins;


                        if (schrunhrs == 0)
                        {
                            calcuatedrawtonsperhr = 0.0;
                            calculatedcleantonsperhr = 0.0;
                        }
                        else
                        {
                            calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                            calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                        }

                        yield = 0;

                        schrunhrs = plantrunmins - plantrunmins;

                        //insert data into detailed report table
                        dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                            ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                            " WHERE starttime = '" + today + " 11:00 AM'");
                    }
                }

                else if (hr >= 1 && hr < 3 && timesplit[1].Equals("PM"))
                {
                    //get the downtime description to add to the detailed report
                    dr = dh.getReader("SELECT descript from ManualData Where fromtime = '" + today + " 01:00 PM'");
                    if (dr.HasRows)
                    {
                        dr.Read();
                        mandata = (string)dr["descript"];
                    }

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "1:00" && timesplit[1] == "PM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 11:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            // check for divide by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0.00;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }

                            

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 11:00 AM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 01:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            //check for dividing by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 01:00 PM'");
                        }
                        else
                        {
                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 01:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            //check for dividing by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 01:00 PM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 01:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            // check for divide by zero
                            if (rawtonscalculated == 0 || cleantonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }




                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 01:00 PM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 01:00 PM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins -plantrunmins;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }
                            //check for divide by zero
                            if (rawtonscalculated == 0 || cleantonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 01:00 PM'");
                        }
                    }
                }

                else if (hr >= 3 && hr < 5 && timesplit[1].Equals("PM"))
                {


                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "3:00" && timesplit[1] == "PM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 01:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                 
                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 01:00 PM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins / 60;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 03:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

             
                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 03:00 PM'");
                        }
                        else
                        {
                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins - plantrunmins;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 03:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 03:00 PM'");
                        }
                    }

                    //if the system time is between 3PM and 5PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 03:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
 

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 03:00 PM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 03:00 PM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            //this code needed everywhere
                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 03:00 PM'");
                        }
                    }
                }

                else if (hr >= 5 && hr < 7 && timesplit[1].Equals("PM"))
                {


                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "5:00" && timesplit[1] == "PM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 03:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            // check for divide by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0.00;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 03:00 PM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 05:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            //check for dividing by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 05:00 PM'");
                        }
                        else
                        {
                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 05:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            //check for dividing by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 05:00 PM'");
                        }
                    }

                    //if the system time is between 5PM and 7PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 05:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            // check for divide by zero
                            if (rawtonscalculated == 0 || cleantonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }




                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 05:00 PM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 05:00 PM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }
                            //check for divide by zero
                            if (rawtonscalculated == 0 || cleantonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 05:00 PM'");
                        }
                    }
                }


                //if the time range is between the starting and ending time range of 7PM to 9PM
                else if (hr >= 7 && hr < 9 && timesplit[1].Equals("PM"))
                {
    
                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "7:00" && timesplit[1] == "PM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 05:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            // check for divide by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0.00;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 05:00 PM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;
                            

                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins + " WHERE fromtime = '" + today + " 07:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            //check for dividing by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }

                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 07:00 PM'");
                        }
                        else
                        {
                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins + " WHERE fromtime = '" + today + " 07:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            //check for dividing by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }


                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 07:00 PM'");
                        }
                    }

                    //if the system time is between 5PM and 7PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 07:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            // check for divide by zero
                            if (rawtonscalculated == 0 || cleantonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }




                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 07:00 PM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 07:00 PM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }
                            //check for divide by zero
                            if (rawtonscalculated == 0 || cleantonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            }

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 07:00 PM'");
                        }
                    }
                }

                else if (hr >= 9 && hr < 11 && timesplit[1].Equals("PM"))
                {

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "9:00" && timesplit[1] == "PM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 07:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            
                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                            


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 07:00 PM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 09:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                        
                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 09:00 PM'");
                        }
                        else
                        {
                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 09:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 09:00 PM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 09:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                          
                         
                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;
                          




                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 09:00 PM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 09:00 PM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }
                         
                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 09:00 PM'");
                        }
                    }
                }

                else if (hr >= 11 && hr < 12 && timesplit[1].Equals("PM"))
                {

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "11:00" && timesplit[1] == "PM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 09:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 09:00 PM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 11:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 11:00 PM'");
                        }
                        else
                        {
                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 11:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 11:00 PM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 11:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 11:00 PM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 11:00 PM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 11:00 PM'");
                        }
                    }
                }

                else if (hr == 12 && timesplit[1].Equals("AM"))
                {

                    //get the starting numbers from the StartEndNums table
                    dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + prevday[0] + " 11:00 PM'");
                    dr.Read();

                    //if the starting numbers were not null do calculations
                    if (!DBNull.Value.Equals(dr["rawstart"]))
                    {
                        rawstartnum = Convert.ToInt32(dr["rawstart"]);
                        cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                        downminstartnum = (double)dr["downminutes"];
                        runhrs = (double)dr["runhrs"];
                        convertrunhrsmins = runhrs * 60;
                        magmins = (double)dr["magmins"];

                        //calculations for detailed report
                        rawtonscalculated = plantfeedrawtons - rawstartnum;
                        cleantonscalculated = cleancoaltons - cleanstartnum;
                        downmincalculated = plcdownminutes - downminstartnum;
                        schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                        calcmagmins = plcmagmins - magmins;


                        if (schrunhrs == 0)
                        {
                            calcuatedrawtonsperhr = 0.0;
                            calculatedcleantonsperhr = 0.0;
                        }
                        else
                        {
                            calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                            calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                        }



                        yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;





                        //insert data into detailed report table
                        dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                            " WHERE starttime = '" + prevday[0] + " 11:00 PM'");
                    }

                    //if the starting values were null update the starting numbers, this could happen if the program wasn't
                    //running during the time ranges starting time
                    //no communication could also cause this
                    else
                    {
                        schrunhrs = plantrunmins / 60;
                        dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                            " WHERE fromtime = '" + prevday[0] + " 11:00 PM'");
                        //calculations for detailed report
                        rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                        cleantonscalculated = cleancoaltons - cleancoaltons;
                        downmincalculated = plcdownminutes - plcdownminutes;
                        schrunhrs = plantrunmins / 60;
                        calcmagmins = plcmagmins - plcmagmins;


                        if (schrunhrs == 0)
                        {
                            calcuatedrawtonsperhr = 0.0;
                            calculatedcleantonsperhr = 0.0;
                        }
                        else
                        {
                            calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                            calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                        }

                        yield = 0;

                        schrunhrs = plantrunmins - plantrunmins;

                        //insert data into detailed report table
                        dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                            " WHERE starttime = '" + prevday[0] + " 11:00 PM'");
                    }
                }
            
            else if (hr >= 1 && hr < 3 && timesplit[1].Equals("AM"))
            {

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "1:00" && timesplit[1] == "AM")
                    {
                        dh.executeSql("DELETE FROM StartEndNums where fromtime < GETDATE() - 3");
                        dh.executeSql("DELETE FROM DetailedReport WHERE starttime < GETDATE() - 60");
                        dh.executeSql("DELETE FROM TrendingData WHERE created_at < GETDATE() - 60");
                        dh.executeSql("DELETE FROM FiveMinuteData WHERE created_at < GETDATE() - 60");
                        dh.executeSql("DELETE FROM ManualData WHERE fromtime < GETDATE() - 60");

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + prevday[0] + " 11:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + prevday[0] + " 11:00 PM'");
                        

                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 01:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 01:00 AM'");
                        
                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 01:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 01:00 AM'");
                        }
                        else
                        {
                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 01:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 01:00 AM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 01:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 01:00 AM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 01:00 AM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 01:00 AM'");
                        }
                    }
                }

                else if (hr >= 3 && hr < 5 && timesplit[1].Equals("AM"))
                {

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "3:00" && timesplit[1] == "AM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 01:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 01:00 AM'");
                       

                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 03:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 03:00 AM'");
                        }
                        //if previous two hour time range is empty still update this time range if time is exactly time range start time
                        else
                        {
                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 03:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 03:00 AM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 03:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 03:00 AM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 03:00 AM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 03:00 AM'");
                        }
                    }
                }

                else if (hr >= 5 && hr < 7 && timesplit[1].Equals("AM"))
                {

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "5:00" && timesplit[1] == "AM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 03:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 03:00 AM'");

                        

                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 05:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield +
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 05:00 AM'");
                        }
                        else
                        {
                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 05:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 05:00 AM'");
                        }
                    }

                    //if the system time is between 5AM to 7AM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 05:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = ((double)cleantonscalculated / (double)rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 05:00 AM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 05:00 AM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + 
                                ", rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 05:00 AM'");
                        }
                    }
                }
            }
            //if shift start time is 6:00AM or 8:00AM
            if (shiftstart.Equals("06:00:00") || shiftstart.Equals("08:00:00"))
            {
                //if the time range is between the starting and ending time range of 7AM to 9AM
                if (hr >= 6 && hr < 8 && timesplit[1].Equals("AM"))
                {
                    //get the downtime description to add to the detailed report
                    dr = dh.getReader("SELECT descript from ManualData Where fromtime = '" + today + " 06:00 AM'");
                    if (dr.HasRows)
                    {
                        dr.Read();
                        mandata = mandata + " " + (string)dr["descript"];
                    }

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "6:00" && timesplit[1] == "AM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 04:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = (cleantonscalculated / rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 04:00 AM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 06:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 06:00 AM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 06:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = (cleantonscalculated / rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 06:00 AM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 06:00 AM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 06:00 AM'");
                        }
                    }
                }

                else if (hr >= 8 && hr < 10 && timesplit[1].Equals("AM"))
                {
                    //get the downtime description to add to the detailed report
                    dr = dh.getReader("SELECT descript from ManualData Where fromtime = '" + today + " 08:00 AM'");
                    if (dr.HasRows)
                    {
                        dr.Read();
                        mandata = mandata + " " + (string)dr["descript"];
                    }

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "8:00" && timesplit[1] == "AM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 06:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = (cleantonscalculated / rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 06:00 AM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 08:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 08:00 AM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 08:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = (cleantonscalculated / rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 08:00 AM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 08:00 AM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 08:00 AM'");
                        }
                    }
                }

                else if (hr >= 10 && hr < 12 && timesplit[1].Equals("AM"))
                {
                    //get the downtime description to add to the detailed report
                    dr = dh.getReader("SELECT descript from ManualData Where fromtime = '" + today + " 10:00 AM'");
                    if (dr.HasRows)
                    {
                        dr.Read();
                        mandata = mandata + " " + (string)dr["descript"];
                    }

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "10:00" && timesplit[1] == "AM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 08:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = (cleantonscalculated / rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 08:00 AM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 10:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 10:00 AM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 10:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = (cleantonscalculated / rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 10:00 AM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 10:00 AM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 10:00 AM'");
                        }
                    }
                }

                

                else if (hr >= 12 && hr < 2 && timesplit[1].Equals("PM"))
                {
                    //get the downtime description to add to the detailed report
                    dr = dh.getReader("SELECT descript from ManualData Where fromtime = '" + today + " 12:00 PM'");
                    if (dr.HasRows)
                    {
                        dr.Read();
                        mandata = mandata + " " + (string)dr["descript"];
                    }

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "12:00" && timesplit[1] == "PM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 10:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            // check for divide by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0.00;
                            }
                            else
                            {
                                yield = (cleantonscalculated / rawtonscalculated) * 100;
                            }


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 10:00 AM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 12:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            //check for dividing by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = (cleantonscalculated / rawtonscalculated) * 100;
                            }



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 12:00 PM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 12:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            // check for divide by zero
                            if (rawtonscalculated == 0 || cleantonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = (cleantonscalculated / rawtonscalculated) * 100;
                            }




                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 12:00 PM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 12:00 PM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins - plantrunmins;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }
                            //check for divide by zero
                            if (rawtonscalculated == 0 || cleantonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = (cleantonscalculated / rawtonscalculated) * 100;
                            }


                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 12:00 PM'");
                        }
                    }
                }

                else if (hr >= 2 && hr < 4 && timesplit[1].Equals("PM"))
                {

                    //get the downtime description to add to the detailed report
                    dr = dh.getReader("SELECT descript from ManualData Where fromtime = '" + today + " 02:00 PM'");
                    if (dr.HasRows)
                    {
                        dr.Read();
                        mandata = mandata + " " + (string)dr["descript"];
                    }

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "2:00" && timesplit[1] == "PM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 12:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = (cleantonscalculated / rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 12:00 PM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins - plantrunmins;
                            calcmagmins = plcmagmins / 60;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 02:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 02:00 PM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 02:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = (cleantonscalculated / rawtonscalculated) * 100;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 02:00 PM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 02:00 PM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 02:00 PM'");
                        }
                    }
                }

                else if (hr >= 4 && hr < 6 && timesplit[1].Equals("PM"))
                {

                    //get the downtime description to add to the detailed report
                    dr = dh.getReader("SELECT descript from ManualData Where fromtime = '" + today + " 04:00 PM'");
                    if (dr.HasRows)
                    {
                        dr.Read();
                        mandata = mandata + " " + (string)dr["descript"];
                    }

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "4:00" && timesplit[1] == "PM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 02:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            // check for divide by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0.00;
                            }
                            else
                            {
                                yield = (cleantonscalculated / rawtonscalculated) * 100;
                            }


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 02:00 PM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 04:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            //check for dividing by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = (cleantonscalculated / rawtonscalculated) * 100;
                            }



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 04:00 PM'");
                        }
                    }

                    //if the system time is between 5PM and 7PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 04:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            // check for divide by zero
                            if (rawtonscalculated == 0 || cleantonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = (cleantonscalculated / rawtonscalculated) * 100;
                            }




                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 04:00 PM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 04:00 PM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }
                            //check for divide by zero
                            if (rawtonscalculated == 0 || cleantonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = (cleantonscalculated / rawtonscalculated) * 100;
                            }

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 04:00 PM'");
                        }
                    }
                }


                //if the time range is between the starting and ending time range of 7PM to 9PM
                else if (hr >= 6 && hr < 8 && timesplit[1].Equals("PM"))
                {
                    //get the downtime description to add to the detailed report
                    dr = dh.getReader("SELECT descript from ManualData Where fromtime = '" + today + " 06:00 PM'");
                    if (dr.HasRows)
                    {
                        dr.Read();
                        mandata = mandata + " " + (string)dr["descript"];
                    }

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "6:00" && timesplit[1] == "PM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 04:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            // check for divide by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0.00;
                            }
                            else
                            {
                                yield = (cleantonscalculated / rawtonscalculated) * 100;
                            }


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 04:00 PM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 06:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            //check for dividing by zero
                            if (rawtonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = (cleantonscalculated / rawtonscalculated) * 100;
                            }



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 06:00 PM'");
                        }
                    }

                    //if the system time is between 5PM and 7PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 06:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            // check for divide by zero
                            if (rawtonscalculated == 0 || cleantonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = (cleantonscalculated / rawtonscalculated) * 100;
                            }




                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 06:00 PM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 06:00 PM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }
                            //check for divide by zero
                            if (rawtonscalculated == 0 || cleantonscalculated == 0)
                            {
                                yield = 0;
                            }
                            else
                            {
                                yield = (cleantonscalculated / rawtonscalculated) * 100;
                            }

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 06:00 PM'");
                        }
                    }
                }

                else if (hr >= 8 && hr < 10 && timesplit[1].Equals("PM"))
                {
                    //get the downtime description to add to the detailed report
                    dr = dh.getReader("SELECT descript from ManualData Where fromtime = '" + today + " 08:00 PM'");
                    if (dr.HasRows)
                    {
                        dr.Read();
                        mandata = mandata + " " + (string)dr["descript"];
                    }

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "8:00" && timesplit[1] == "PM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 06:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = (cleantonscalculated / rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 06:00 PM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 08:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 08:00 PM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 08:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = (cleantonscalculated / rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 08:00 PM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 08:00 PM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 08:00 PM'");
                        }
                    }
                }

                else if (hr >= 10 && hr < 12 && timesplit[1].Equals("PM"))
                {
                    //get the downtime description to add to the detailed report
                    dr = dh.getReader("SELECT descript from ManualData Where fromtime = '" + today + " 10:00 PM'");
                    if (dr.HasRows)
                    {
                        dr.Read();
                        mandata = mandata + " " + (string)dr["descript"];
                    }

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "10:00" && timesplit[1] == "PM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 08:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = (cleantonscalculated / rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 08:00 PM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 10:00 PM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 10:00 PM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 10:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = (cleantonscalculated / rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 10:00 PM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 10:00 PM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 10:00 PM'");
                        }
                    }
                }

                

                else if (hr >= 12 && hr < 2 && timesplit[1].Equals("AM"))
                {
                    dh.executeSql("DELETE FROM StartEndNums where fromtime < GETDATE() - 3");
                    dh.executeSql("DELETE FROM DetailedReport WHERE starttime < GETDATE() - 60");
                    dh.executeSql("DELETE FROM TrendingData WHERE starttime < GETDATE() - 60");
                    dh.executeSql("DELETE FROM FiveMinuteData WHERE starttime < GETDATE() - 60");
                    dh.executeSql("DELETE FROM ManualData WHERE starttime < GETDATE() - 60");

                    //get the downtime description to add to the detailed report
                    dr = dh.getReader("SELECT descript from ManualData Where fromtime = '" + today + " 12:00 AM'");
                    if (dr.HasRows)
                    {
                        dr.Read();
                        mandata = mandata + " " + (string)dr["descript"];
                    }

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "12:00" && timesplit[1] == "AM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + prevday[0] + " 10:00 PM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = (cleantonscalculated / rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + prevday[0] + " 10:00 PM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 12:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 12:00 AM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 12:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = (cleantonscalculated / rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 12:00 AM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 12:00 AM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 12:00 AM'");
                        }
                    }
                }

                else if (hr >= 2 && hr < 4 && timesplit[1].Equals("AM"))
                {
                    //get the downtime description to add to the detailed report
                    dr = dh.getReader("SELECT descript from ManualData Where fromtime = '" + today + " 02:00 AM'");
                    if (dr.HasRows)
                    {
                        dr.Read();
                        mandata = mandata + " " + (string)dr["descript"];
                    }

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "2:00" && timesplit[1] == "AM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 12:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = (cleantonscalculated / rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 12:00 AM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 02:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 02:00 AM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 02:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = (cleantonscalculated / rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 02:00 AM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 02:00 AM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 02:00 AM'");
                        }
                    }
                }

                else if (hr >= 4 && hr < 6 && timesplit[1].Equals("AM"))
                {
                    //get the downtime description to add to the detailed report
                    dr = dh.getReader("SELECT descript from ManualData Where fromtime = '" + today + " 04:00 AM'");
                    if (dr.HasRows)
                    {
                        dr.Read();
                        mandata = mandata + " " + (string)dr["descript"];
                    }

                    //if the system time is the same as the time ranges starting time
                    if (timesplit[0] == "4:00" && timesplit[1] == "AM")
                    {

                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 02:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];


                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = (cleantonscalculated / rawtonscalculated) * 100;



                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 02:00 AM'");



                            //calculations for detailed report
                            //new time range numbers
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            //set the starting numbers from the StartEndNums table
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 04:00 AM'");




                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }


                            yield = 0;
                            schrunhrs = plantrunmins - plantrunmins;


                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 04:00 AM'");
                        }
                    }

                    //if the system time is between 9PM and 11PM
                    else
                    {
                        //get the starting numbers from the StartEndNums table
                        dr = dh.getReader("SELECT rawstart, cleanstart, downminutes, runhrs, magmins FROM StartEndNums WHERE fromtime = '" + today + " 04:00 AM'");
                        dr.Read();

                        //if the starting numbers were not null do calculations
                        if (!DBNull.Value.Equals(dr["rawstart"]))
                        {
                            rawstartnum = Convert.ToInt32(dr["rawstart"]);
                            cleanstartnum = Convert.ToInt32(dr["cleanstart"]);
                            downminstartnum = (double)dr["downminutes"];
                            runhrs = (double)dr["runhrs"];
                            convertrunhrsmins = runhrs * 60;
                            magmins = (double)dr["magmins"];

                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - rawstartnum;
                            cleantonscalculated = cleancoaltons - cleanstartnum;
                            downmincalculated = plcdownminutes - downminstartnum;
                            schrunhrs = (plantrunmins - convertrunhrsmins) / 60;
                            calcmagmins = plcmagmins - magmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }



                            yield = (cleantonscalculated / rawtonscalculated) * 100;





                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 04:00 AM'");
                        }

                        //if the starting values were null update the starting numbers, this could happen if the program wasn't
                        //running during the time ranges starting time
                        //no communication could also cause this
                        else
                        {
                            schrunhrs = plantrunmins / 60;
                            dh.executeSql("UPDATE StartEndNums set rawstart = " + plantfeedrawtons + ", cleanstart = " + cleancoaltons + ", downminutes = " + plcdownminutes + ", runhrs = " + schrunhrs + ", magmins = " + plcmagmins +
                                " WHERE fromtime = '" + today + " 04:00 AM'");
                            //calculations for detailed report
                            rawtonscalculated = plantfeedrawtons - plantfeedrawtons;
                            cleantonscalculated = cleancoaltons - cleancoaltons;
                            downmincalculated = plcdownminutes - plcdownminutes;
                            schrunhrs = plantrunmins / 60;
                            calcmagmins = plcmagmins - plcmagmins;


                            if (schrunhrs == 0)
                            {
                                calcuatedrawtonsperhr = 0.0;
                                calculatedcleantonsperhr = 0.0;
                            }
                            else
                            {
                                calcuatedrawtonsperhr = rawtonscalculated / schrunhrs;
                                calculatedcleantonsperhr = cleantonscalculated / schrunhrs;
                            }

                            yield = 0;

                            schrunhrs = plantrunmins - plantrunmins;

                            //insert data into detailed report table
                            dh.executeSql("UPDATE DetailedReport SET actrunhrs = " + schrunhrs + ", rawtons = " + rawtonscalculated + ", cleantons = " + cleantonscalculated + ", yield = " + yield + ", " +
                                "mandata = '" + mandata + "', rawtonsperhr = " + calcuatedrawtonsperhr + ", cleantonsperhr = " + calculatedcleantonsperhr + ", downmins = " + downmincalculated + ", magminutes = " + calcmagmins +
                                " WHERE starttime = '" + today + " 04:00 AM'");
                        }
                    }
                }
            }
        }
    }
}
