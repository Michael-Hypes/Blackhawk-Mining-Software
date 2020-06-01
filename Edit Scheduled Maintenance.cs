using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace BlackHawk_Reporting
{
    public partial class Edit_Scheduled_Maintenance : Form
    {
        SQLDataHandler dh;
        SqlDataReader dr;
        String stime;
        string[] ampm;


        public Edit_Scheduled_Maintenance()
        {
            InitializeComponent();
            //using the sqlDataHandler class for sql connections and commands
            dh = new SQLDataHandler();

            //sql data reader to read the start time from the configuration settings
            dr = dh.getReader("SELECT currshiftstarttime From ConfigurationSettings Where plantname = 'Kanawha Eagle'");

            //read while there are rows
            while (dr.Read())
            {
                //store the datetime from sql
                stime = (String)dr["currshiftstarttime"].ToString();
            }

            //split the date time on the colon
            string[] starttimehr = stime.Split(':');

            //store the hour as an integer
            int starthr = Convert.ToInt32(starttimehr[0]);

            loadDDL(starthr);
        }

        private void tbMins_TextChanged(object sender, EventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(tbMins.Text, "[^0-9]"))
            {
                MessageBox.Show("Please enter only numbers.");
                tbMins.Text = tbMins.Text.Remove(tbMins.Text.Length - 1);
            }
        }

        public void loadDDL(int strhr)
        {
            int twohrmore;

            string timerange = "";
            string twohr = "am";
            string sthr = "am";
            twohrmore = strhr + 2;

            for (int i = 0; i < 12; i++)
            {



                if (twohrmore < 12 && twohr.Equals("am"))
                {
                    timerange = strhr + ":00 AM - " + twohrmore + ":00 AM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohrmore = strhr + 2;
                }
                else if (twohrmore == 12 && strhr < 12 && sthr.Equals("am"))
                {
                    timerange = strhr + ":00 AM - " + twohrmore + ":00 PM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohr = "PM";
                    twohrmore = strhr + 2;
                }
                else if (strhr == 12 && sthr.Equals("am"))
                {
                    twohrmore = 2;
                    timerange = strhr + ":00 PM - " + twohrmore + ":00 PM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohr = "PM";
                    sthr = "PM";
                    twohrmore = strhr + 2;
                }
                else if (twohr.Equals("PM") && sthr.Equals("PM") && twohrmore < 12)
                {
                    timerange = strhr + ":00 PM - " + twohrmore + ":00 PM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohrmore = strhr + 2;
                }
                else if (twohrmore == 12 && strhr < 12 && sthr.Equals("PM"))
                {
                    twohr = "AM";
                    timerange = strhr + ":00 PM - " + twohrmore + ":00 AM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohrmore = strhr + 2;
                }
                else if (twohrmore > 12 && strhr == 12)
                {
                    sthr = "AM";
                    twohrmore = 2;
                    timerange = strhr + ":00 AM - " + twohrmore + ":00 AM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohrmore = strhr + 2;
                }
                else if (sthr.Equals("AM") && strhr >= 2)
                {
                    timerange = strhr + ":00 AM - " + twohrmore + ":00 AM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohrmore = strhr + 2;
                }
                else if (twohrmore == 13 && sthr.Equals("am"))
                {
                    twohrmore = 1;
                    timerange = strhr + ":00 AM - " + twohrmore + ":00 PM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohr = "PM";
                    twohrmore = strhr + 2;
                }
                else if (strhr < 12 && twohr.Equals("PM") && twohrmore < 13)
                {
                    timerange = strhr + ":00 PM - " + twohrmore + ":00 PM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohr = "PM";
                    sthr = "PM";
                    twohrmore = strhr + 2;
                }
                else if (twohrmore == 13 && strhr < 12 && sthr.Equals("PM"))
                {
                    twohrmore = 1;
                    timerange = strhr + ":00 PM - " + twohrmore + ":00 AM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohr = "AM";
                    twohrmore = strhr + 2;
                }
                else if (strhr < 12 && twohr.Equals("AM"))
                {
                    timerange = strhr + ":00 AM - " + twohrmore + ":00 AM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohr = "AM";
                    twohrmore = strhr + 2;
                }
            }
        }

        private void DataSubmitbtn_Click(object sender, EventArgs e)
        {
            //getting todays date for use in sql query
            DateTime dtoday = DateTime.Today;
            string today;

            //convert datetime to string
            today = Convert.ToString(dtoday);

            //Split datetime at space
            string[] splitdatetime = today.Split(' ');
            //Take only the date as the time range is chosen by the user
            string ndate = splitdatetime[0];

            //split the date on the forward slash so that it can be formatted
            //to match ms sql date format
            string[] splitdate = ndate.Split('/');

            //formatting the date to match the ms sql date format ex.  2019-11-28
            string newdate = splitdate[2] + "-" + splitdate[0] + "-" + splitdate[1];
            
            string mins = tbMins.Text;

            //Make sure user has selected a time range and has entered text into the textbox
            if (ddlTimeRange.SelectedIndex > -1 && mins.Length > 0)
            {
                //Get the selected time range string
                string timerange = ddlTimeRange.SelectedItem.ToString();
                //Get the minutes from the textbox
                int schMinutes = Convert.ToInt32(mins);

                //Split the time range string into two seperate times
                string[] splttime = timerange.Split('-');
                string fromTime = splttime[0];
                string toTime = splttime[1];
                ampm = fromTime.Split(' ');

                //Format the date and time to match sql formatting
                string from = newdate + " " + fromTime;
                string to = newdate + " " + toTime;
                
                editSchMins(fromTime, from, schMinutes);

            }
            else
            {
                MessageBox.Show("The time range and text box must be filled out!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public void editSchMins(string fromTime, string from, int schMinutes)
        {
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

            string[] hrmin = time.Split(':');
            int hr = Convert.ToInt32(hrmin[0]);
            int min = Convert.ToInt32(hrmin[1]);

            //get the shift start time
            dh = new SQLDataHandler();
            dr = dh.getReader("SELECT currshiftstarttime, currnumshifts from ConfigurationSettings WHERE plantname = 'Kanawha Eagle'");
            dr.Read();

            //where shift nums equals 2
            string stime = (string)dr["currshiftstarttime"].ToString();
            int numshifts = (int)dr["currnumshifts"];
            string[] endtime = stime.Split(':');
            int edtm = Convert.ToInt32(endtime[0]);


            //if it is after midnight then the pm time ranges need updated for the previous day
            //which is what will show on the dashboard until first shift starts for the current day
            if (hr == 12 && timesplit[1].Equals("AM") || hr < edtm && timesplit[1].Equals("AM") && ampm[1].Equals("PM"))
            {               
                try
                {
                    dh.executeSql("Update DetailedReport set schrunmins = " + schMinutes + " WHERE starttime = '" + prevday[0] + " " + fromTime + "'");
                }
                catch (SqlException se)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                    {
                        file.WriteLine("Error occurred updating database: " + DateTime.Now + " \r\n" + se + "\r\n");
                    }
                    MessageBox.Show("There has been an error updating the scheduled run minutes!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                MessageBox.Show("Scheduled Minutes Changed To: " + schMinutes, " Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                try
                {
                    dh.executeSql("Update DetailedReport set schrunmins = " + schMinutes + " WHERE starttime = '" + from + "'");
                }
                catch (SqlException se)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                    {
                        file.WriteLine("Error occurred updating database: " + DateTime.Now + " \r\n" + se + "\r\n");
                    }
                    MessageBox.Show("There has been an error updating the scheduled run minutes!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                MessageBox.Show("Scheduled Minutes Changed To: " + schMinutes, " Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }          
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
