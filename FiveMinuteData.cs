using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibplctagWrapper;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;

namespace BlackHawk_Reporting
{
    class FiveMinuteData
    {

        SQLDataHandler dh;
        SqlDataReader dr;        
        private const int DataTimeout = 1000;
        PLCConnection con;
        DetailedReport detail;


        public void readData()
        {
            detail = new DetailedReport();
            con = new PLCConnection();

            setTimeRanges();

            try
            {
                // creates a tag to read B3:0, 1 item, from LGX ip address 192.168.0.100
                // The last entry in this new tag is the element count.  It is currently
                // set to 1
                //public Tag(string ipAddress, string path, CpuType cpuType, string name, int elementSize, int elementCount, int debugLevel = 0)
                //string name is the textual name of the tag in plc
                //elementSize is the size of the element in bytes
                //elementCount elements count: 1- single, n-array
                //public Tag(string ipAddress, string path, CpuType cpuType, string name, int elementSize, int elementCount, int debugLevel = 0)
                var tag5 = new Tag("10.14.6.100", "1, 0", CpuType.LGX, "REPORT_DINT[0]", 4, 6, 0);
                var tag6 = new Tag("10.14.6.100", "1, 0", CpuType.LGX, "REPORT_FLOAT[12]", 4, 3, 0);
                var tag12 = new Tag("10.14.6.100", "1, 0", CpuType.LGX, "REPORT_INT[8]", 2, 2, 0);

                using (var client = new Libplctag())
                {
                    // add the tag
                    client.AddTag(tag5);
                    client.AddTag(tag6);
                    client.AddTag(tag12);
                    //client.AddTag(tag2);
                    // check that the tag has been added, if it returns pending we have to retry
                    while (client.GetStatus(tag5) == Libplctag.PLCTAG_STATUS_PENDING)
                    {
                        Thread.Sleep(100);
                    }
                    // check that the tag has been added, if it returns pending we have to retry
                    while (client.GetStatus(tag6) == Libplctag.PLCTAG_STATUS_PENDING)
                    {
                        Thread.Sleep(100);
                    }
                    // check that the tag has been added, if it returns pending we have to retry
                    while (client.GetStatus(tag12) == Libplctag.PLCTAG_STATUS_PENDING)
                    {
                        Thread.Sleep(100);
                    }


                    // if the status is not ok, we have to handle the error
                    if (client.GetStatus(tag5) != Libplctag.PLCTAG_STATUS_OK)
                    {
                        con.setDownbit(-5);
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                        {
                            file.WriteLine("Error occured at: " + DateTime.Now + Convert.ToString((client.GetStatus(tag5))) + ("\n" + $"Five Minute Data tag 5 Read Error setting up tag internal state. Error{ client.DecodeError(client.GetStatus(tag5))}\n"));
                        }
                        return;
                    }
                    // if the status is not ok, we have to handle the error
                    if (client.GetStatus(tag6) != Libplctag.PLCTAG_STATUS_OK)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                        {
                            file.WriteLine("Error occured at: " + DateTime.Now + Convert.ToString((client.GetStatus(tag6))) + ("\n" + $"Five Minute Data tag 6 Read Error setting up tag internal state. Error{ client.DecodeError(client.GetStatus(tag6))}\n"));
                        }
                        return;
                    }
                    // if the status is not ok, we have to handle the error
                    if (client.GetStatus(tag12) != Libplctag.PLCTAG_STATUS_OK)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                        {
                            file.WriteLine("Error occured at: " + DateTime.Now + Convert.ToString((client.GetStatus(tag12))) + ("\n" + $"Five Minute Data tag 6 Read Error setting up tag internal state. Error{ client.DecodeError(client.GetStatus(tag12))}\n"));
                        }
                        return;
                    }


                    // Execute the read
                    var result = client.ReadTag(tag5, DataTimeout);
                    // Execute the read
                    var result1 = client.ReadTag(tag6, DataTimeout);
                    // Execute the read
                    var result2 = client.ReadTag(tag12, DataTimeout);

                    // Check the read operation result
                    if (result != Libplctag.PLCTAG_STATUS_OK)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                        {
                            file.WriteLine("Error occured at: " + DateTime.Now + Convert.ToString(($"Five Minute Data Read ERROR: Unable to read the data! Got error code {result}: {client.DecodeError(result)}\n")));
                        }
                        return;
                    }
                    if (result1 != Libplctag.PLCTAG_STATUS_OK)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                        {
                            file.WriteLine("Error occured at: " + DateTime.Now + Convert.ToString(($"Five Minute Data Read ERROR: Unable to read the data! Got error code {result1}: {client.DecodeError(result1)}\n")));
                        }
                        return;
                    }
                    if (result2 != Libplctag.PLCTAG_STATUS_OK)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                        {
                            file.WriteLine("Error occured at: " + DateTime.Now + Convert.ToString(($"Five Minute Data Read ERROR: Unable to read the data! Got error code {result2}: {client.DecodeError(result2)}\n")));
                        }
                        return;
                    }


                    // read the data from the array that was retrieved from the PLC
                    //double integer values
                    var plantfeedtotaltons = client.GetInt32Value(tag5, 0 * tag5.ElementSize);
                    var cleancoaltotaltons = client.GetInt32Value(tag5, 1 * tag5.ElementSize);
                    var stokercoaltotaltons = client.GetInt32Value(tag5, 2 * tag5.ElementSize);
                    var bypasstotaltons = client.GetInt32Value(tag5, 3 * tag5.ElementSize);
                    var refusetotaltons = client.GetInt32Value(tag5, 4 * tag5.ElementSize);
                    var scalpedtotaltons = client.GetInt32Value(tag5, 5 * tag5.ElementSize);

                    //float values
                    //var yield = client.GetFloat32Value(tag6, 0 * tag6.ElementSize);
                    //mag minutes float 12 needs added to the desktop reporting
                    //grand total number like the coal values
                    var magmins = client.GetFloat32Value(tag6, 0 * tag6.ElementSize);
                    var plantfeedrunmins = client.GetFloat32Value(tag6, 1 * tag6.ElementSize);
                    var downminstotal = client.GetFloat32Value(tag6, 2 * tag6.ElementSize);

                    //int 16 values
                    var plantfeedtph = client.GetInt16Value(tag12, 0 * tag12.ElementSize);
                    var cleancoaltph = client.GetInt16Value(tag12, 1 * tag12.ElementSize);

                    //code about this needs to be here and this data from the PLC needs used for updating DetailedReport table
                    //it is at the top for hard code testing
                    detail = new DetailedReport();
                    detail.createDetailedReport(plantfeedtotaltons, cleancoaltotaltons, plantfeedtph, cleancoaltph, downminstotal, plantfeedrunmins, magmins);

                    try
                    {
                        //Insert data from PLC into FiveMinuteData table every five minute read
                        dh.executeSql("Insert INTO FiveMinuteData(created_at, plantname, plantfeedtons, cleantons, stokertons, bypasstons, refusetons, scalpedtons) VALUES " +
                            "(GETDATE(), 'Kanawha Eagle', '" + plantfeedtotaltons + "', '" + cleancoaltotaltons + "', '" + stokercoaltotaltons + "', '" + bypasstotaltons + "', '" + 
                            refusetotaltons + "', '" + scalpedtotaltons + "')");
                    }catch(SqlException se)
                    {
                        //writing data to a text file to observe errors for troubleshooting
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                        {
                            file.WriteLine("Error occurred at Five Minute Data insert to database: " + DateTime.Now + " \r\n" + se + "\r\n");
                        }
                        return;
                    }
                    client.RemoveTag(tag5);
                    client.RemoveTag(tag6);
                }
            }
            catch(Exception ex)
            {
                //writing errors to text file for troubleshooting
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                {
                    file.WriteLine("Error occurred at Five Minute Data insert to database: " + DateTime.Now + " \r\n" + ex + "\r\n");
                }
                return;
            }
        }

        public void setTimeRanges()
        {
            dh = new SQLDataHandler();
            dr = dh.getReader("Select schrunmins, currshiftstarttime, newshiftstarttime, datetotakeeffect FROM ConfigurationSettings Where plantname = 'Kanawha Eagle';");
            dr.Read();
            int schrunmins = (int)dr["schrunmins"];
            String stime = dr["currshiftstarttime"].ToString();
            string newstime = dr["newshiftstarttime"].ToString();
            string datetochange = dr["datetotakeeffect"].ToString();
            string[] changedate = datetochange.Split(' ');
            string firstshiftstarttime = stime.Substring(1, 4) + " AM";
            string[] newshiftsttime = newstime.Split(' ');
            string nshiftstarttime = newshiftsttime[0] + " AM";
            string endingtime = "";
            


            //get todays date plus add the shift start time to the date and check if any rows exist
            string checktime = DateTime.Today.ToShortDateString() + " " + firstshiftstarttime;
            string updatedtime = DateTime.Today.ToShortDateString() + " " + nshiftstarttime;
            //todays date equals datetotakeeffect from configuration settings table we need to 
            //add the new time ranges to the detailed report and start end nums tables

            DateTime sqldatetochange = Convert.ToDateTime(datetochange);
            string newsqlchanged = Convert.ToString(sqldatetochange);
            string[] newdatechanged = newsqlchanged.Split(' ');
            
            if (DateTime.Today.ToShortDateString().Equals(newdatechanged[0]))
            {
                //update end time on last timerange in detailedreport and startendnums to new shiftstarttime
                dr = dh.getReader("SELECT TOP 1 endtime FROM DetailedReport Order By endtime DESC");
                dr.Read();

                //if the starting numbers were not null do calculations
                if (!DBNull.Value.Equals(dr["endtime"]))
                {
                    endingtime = dr["endtime"].ToString();
                    dh.executeSql("Update DetailedReport set endtime = '" + updatedtime + "', datetotakeeffect = NULL WHERE endtime = '" + endingtime + "'");
                }


                

                dr = dh.getReader("SELECT TOP 1 totime FROM StartEndNums Order By totime DESC");
                dr.Read();

                //if the starting numbers were not null do calculations
                if (!DBNull.Value.Equals(dr["totime"]))
                {
                    endingtime = dr["totime"].ToString();
                }


                dh.executeSql("Update StartEndNums set totime = '" + updatedtime + "' WHERE totime = '" + endingtime + "'");

                //take number of hours in day and divide by number of shifts from ConfigurationSettings table
                //this is how many hours we will add to find the end time
                string[] time = stime.Split(':');
                int stTime = Convert.ToInt32(time[0]);
                string[] newtime = newstime.Split(':');               
                int newstTime = Convert.ToInt32(newtime[0]);

                //converting shift start time string to datetime data type
                DateTime time1 = Convert.ToDateTime(nshiftstarttime);
                DateTime time2 = time1.AddHours(2);
                DateTime time3 = Convert.ToDateTime(nshiftstarttime);
                DateTime time4 = time1.AddHours(2);

                //get the shift start time
                dr = dh.getReader("SELECT currshiftstarttime from ConfigurationSettings WHERE plantname = 'Kanawha Eagle'");
                dr.Read();
                string shiftstart = (string)dr["currshiftstarttime"].ToString();


                //Get the time
                string now = DateTime.Now.ToShortTimeString();

                //Split time to use as comparison
                string[] timesplit = now.Split(' ');
                string timerangenums = timesplit[0];

                string[] hrmin = timerangenums.Split(':');
                int hr = Convert.ToInt32(hrmin[0]);
                int min = Convert.ToInt32(hrmin[1]);


                //loop through 12 times for each 2 hr time range for a 24 hour period
                //for the DetailedReport Table these will be updated as data is read from PLC
                for (int i = 0; i < 12; i++)
                {
                    string converttime = Convert.ToString(time1);
                    dh.executeSql("IF NOT EXISTS (Select starttime From DetailedReport WHERE starttime = '" + converttime + "')" +
                        " Insert INTO DetailedReport (starttime, endtime, schrunmins) VALUES ('" + time1 + "', '" + time2 + "', " + schrunmins + ")");
                    string converttime1 = Convert.ToString(time1);
                    checktime = DateTime.Today.ToShortDateString() + " " + converttime1;
                    time1 = time2;
                    time2 = time2.AddHours(2);
                }


                //loop through the time ranges to insert in the StartEndNums Table
                //these will be used for calculations for displaying detailed report
                //on the desktop app as well as the network application
                for (int i = 0; i < 12; i++)
                {
                    string converttime = Convert.ToString(time3);
                    dh.executeSql("IF NOT EXISTS (Select fromtime From StartEndNums WHERE fromtime = '" + converttime + "')" +
                        " Insert INTO StartEndNums (fromtime, totime) VALUES ('" + time3 + "', '" + time4 + "')");
                    string converttime1 = Convert.ToString(time3);
                    checktime = DateTime.Today.ToShortDateString() + " " + converttime1;
                    time3 = time4;
                    time4 = time4.AddHours(2);
                }
            }
            else
            {

                //if rows don't exist then insert the time ranges from todays shift start time until tomorrows last shift end time
                //this is done incase the program isn't running at the beginning of the shift.  We still need to add a start number as soon
                //as the program is back up and running as well as the time ranges used for the detailed report

                //converting shift start time string to datetime data type
                DateTime time1 = Convert.ToDateTime(firstshiftstarttime);
                DateTime time2 = time1.AddHours(2);
                DateTime time3 = Convert.ToDateTime(firstshiftstarttime);
                DateTime time4 = time1.AddHours(2);

                //get the shift start time
                dr = dh.getReader("SELECT currshiftstarttime from ConfigurationSettings WHERE plantname = 'Kanawha Eagle'");
                dr.Read();
                string shiftstart = (string)dr["currshiftstarttime"].ToString();


                //Get the time
                string now = DateTime.Now.ToShortTimeString();

                //Split time to use as comparison
                string[] timesplit = now.Split(' ');
                string timerangenums = timesplit[0];

                string[] hrmin = timerangenums.Split(':');
                int hr = Convert.ToInt32(hrmin[0]);
                int min = Convert.ToInt32(hrmin[1]);


                //loop through 12 times for each 2 hr time range for a 24 hour period
                //for the DetailedReport Table these will be updated as data is read from PLC
                for (int i = 0; i < 12; i++)
                {
                    string converttime = Convert.ToString(time1);
                    dh.executeSql("IF NOT EXISTS (Select starttime From DetailedReport WHERE starttime = '" + converttime + "')" +
                        " Insert INTO DetailedReport (starttime, endtime, schrunmins) VALUES ('" + time1 + "', '" + time2 + "', " + schrunmins + ")");
                    string converttime1 = Convert.ToString(time1);
                    checktime = DateTime.Today.ToShortDateString() + " " + converttime1;
                    time1 = time2;
                    time2 = time2.AddHours(2);
                }


                //loop through the time ranges to insert in the StartEndNums Table
                //these will be used for calculations for displaying detailed report
                //on the desktop app as well as the network application
                for (int i = 0; i < 12; i++)
                {
                    string converttime = Convert.ToString(time3);
                    dh.executeSql("IF NOT EXISTS (Select fromtime From StartEndNums WHERE fromtime = '" + converttime + "')" +
                        " Insert INTO StartEndNums (fromtime, totime) VALUES ('" + time3 + "', '" + time4 + "')");
                    string converttime1 = Convert.ToString(time3);
                    checktime = DateTime.Today.ToShortDateString() + " " + converttime1;
                    time3 = time4;
                    time4 = time4.AddHours(2);
                }
            }
        }

    }
}
