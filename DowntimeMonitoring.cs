using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;


namespace BlackHawk_Reporting
{

    class DownTimeMonitoring
    {
        SqlConnection con = new SqlConnection("Data Source=DESKTOP-QDF9A1Q\\BHPP_REPORTING;Initial Catalog=BHCCReport;User id=sa;Password=BHReporting;");
        //SqlConnection con = new SqlConnection("Data Source=LAPTOP\\BHREPORTING;Initial Catalog=BHCCReport;User id=sa;Password=BHReporting;");
        //SqlConnection con = new SqlConnection("Data Source=DESKTOP-CL05KBC;Initial Catalog=BHCCReport;User id=sa;Password=BHReporting;");
        //SqlConnection con = new SqlConnection("Data Source = 192.168.1.23, 1433;Network Library=DBMSSOCN;Initial Catalog=KanawhaEagleDB;User id=sa;Password=BHReporting;");
        //SqlConnection con = new SqlConnection("Data Source=DESKTOP-VQBH5C1\\SQLEXPRESS;Initial Catalog=KanawhaEagleDB;User id=sa;Password=BHReporting;");

        //Example database connection through network
        //Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;
        //Initial Catalog = myDataBase; User ID = myUsername; Password=myPassword;

        //SqlCommand cmd;
        SQLDataHandler dh;
        SqlDataReader dr;
       // SqlDataAdapter adpt;
        //DataTable dt;
        
  

        //Creating an object of PLCConnection class to use its methods
        //PLCConnection downTimeBit;
        //This integer stores the bit that is retreived from the PLC in the PLCConnection class
        //Int16 downbit;

        //This boolean is used to monitor the change of the downbit.
        //If it doesn't change we don't need to insert or update a tuple in the sql database
        bool downtimeMonitor;
 

            public void downtime(int bitCheck, Boolean statBool, DataGridView dgv)
            {

                try
                {
                //Checking to see if the bit has changed from a 1 to a 0.  If the bit is 0 and the
                //boolean is true that means the bit has just changed.  If the boolean had been false
                //the bit had already been read as a 0

                if (bitCheck.Equals(0) && statBool.Equals(true))
                    {
                    
                        try
                        {
                            //setting the boolean value to false on first read of the bit being 0
                            setdowntimeMonitor(false);
                            dh = new SQLDataHandler();
                            //Inserting the downtime date and time into the database
                            dh.executeSql("Insert into Downtime(plantName, downtime, uptime) values ('Kanawha Eagle', GETDATE(), NULL)");
                        }catch(SqlException se)
                        {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                        {
                            file.WriteLine("Error occurred at Downtime Monitoring Downtime insert: " + DateTime.Now + " \r\n" + se + "\r\n");
                        }
                        return;
                        }
                    }

                    //Checking to see if the bit has changed back to a 1.  If the bit is 1 and the
                    //boolean value is false that means the bit has just changed.  If the boolean had been true
                    //the bit had already been read as a 1
                    if (bitCheck.Equals(1) && statBool.Equals(false))
                    {
                        //setting the boolean value back to true
                        setdowntimeMonitor(true);
                        //Instantiating a new data handler
                        dh = new SQLDataHandler();
                        try
                        {
                            //This finds the last auto incremented id in the downtime table so that we
                            //know what tuple to update with the uptime date and time
                            dr = dh.getReader("SELECT TOP 1 id FROM Downtime Order By id DESC");

                            //Reading the value of the id from the select statement above
                            dr.Read();
                            if (dr.HasRows)
                            {
                                Int32 idlocation = dr.GetInt32(0);

                                //Updating the correct tuple that corresponds to the downtime date and time
                                dh.executeSql("Update Downtime set uptime = GETDATE() WHERE id = '" + idlocation + "'");

                                //This select statement calculates the minutes that passed between the downtime date and time and the uptime date and time
                                dr = dh.getReader("Select DATEDIFF(second, downtime, uptime) From Downtime Where id = '" + idlocation + "'");
                                dr.Read();
                                Int32 downsec = dr.GetInt32(0);
                                double downmins = (Convert.ToDouble(downsec) / 60);


                                //Updating the tuple with the downtime minutes calculated above using the id that we found above
                                dh.executeSql("Update Downtime set downmins = '" + downmins + "' WHERE id = '" + idlocation + "'");
                            }
                        }catch(SqlException ex)
                        {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                        {
                            file.WriteLine("Error occurred at Downtime Monitoring Update: " + DateTime.Now + " \r\n" + ex + "\r\n");
                        }
                        return;
                        }

                    }
 
                }catch(Exception ee)
                {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                {
                    file.WriteLine("Error occurred at Downtime Monitoring overall try catch: " + DateTime.Now + " \r\n" + ee + "\r\n");
                }
                return;
                }
            }

 

        public Boolean getdowntimeMonitor()
        {
            return downtimeMonitor;
        }

        public void setdowntimeMonitor(Boolean value)
        {
            downtimeMonitor = value;
        }


    }
}
