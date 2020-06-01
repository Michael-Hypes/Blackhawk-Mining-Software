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
    public partial class Manual_Data1 : Form
    {
        SQLDataHandler dh;
        SqlDataReader dr;
        String stime;
        string sqldescript = "";

        public Manual_Data1()
        {
            InitializeComponent();

            //using the sqlDataHandler class for sql connections and commands
            dh = new SQLDataHandler();

            //sql data reader to read the start time from the configuration settings
            dr = dh.getReader("SELECT currshiftstarttime From ConfigurationSettings Where plantname = 'Kanawha Eagle'");
            
            //read while there are rows
            while(dr.Read())
            {
                //store the datetime from sql
                stime = (String)dr["currshiftstarttime"].ToString();
            }

            //split the date time on the colon
            string[] starttimehr = stime.Split(':');

            //store the hour as an integer
            int starthr = Convert.ToInt32(starttimehr[0]);

            loadDDL(starthr);

            //display in message box to verify correct number
            //this code can be deleted once this part of the code is finished
            //MessageBox.Show(Convert.ToString(starthr));

            //This was Garretts code to load the drop down lists
            //Chris removed one line of code for the second drop down list
            //var item = DateTime.Today.AddHours(0); // 0:00:00
            //while (item <= DateTime.Today.AddHours(24)) // 24:00:00 //12:00 AM
            //{
            //    ddlTimeRange.Items.Add(item.TimeOfDay.ToString(@"hh\:mm"));
                
            //    item = item.AddMinutes(30);
            //}
        }

        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void FromTime_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSubmitMaintenance_Click(object sender, EventArgs e)
        {
            if(chkboxMaintenance.Checked)
            {
                //scheduled run minutes need set to zero for time range from and to that is entered
                //downtime minutes need set to 0 and availability set to zero
                string fromdate = dateFrom.Text + " " + TimeFrom.Text;
                string todate = dateTo.Text + " " + TimeTo.Text;
                
                dh.executeSql("Update DetailedReport set actrunhrs = 0.00, downmins = 0.00, schrunmins = 0, mandata = 'Scheduled Maintenance' WHERE starttime >= '" + fromdate + "' AND starttime < '" + todate + "'");
            }
            else
            {
                MessageBox.Show("Error, you must check the scheduled maintenance box\nto schedule maintenance!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            string description = tbDescription.Text;

            //Make sure user has selected a time range and has entered text into the textbox
            if (ddlTimeRange.SelectedIndex > -1 && description.Length > 0)
            {
                //Get the selected time range string
                string timerange = ddlTimeRange.SelectedItem.ToString();

                //Split the time range string into two seperate times
                string[] splttime = timerange.Split('-');
                string fromTime = splttime[0];
                string toTime = splttime[1];
                //MessageBox.Show(fromTime);
                //Format the date and time to match sql formatting
                string from = newdate + " " + fromTime;
                string to = newdate + " " + toTime;
                //string[] fromampm = fromTime.Split(' ');
                //string[] toampm = toTime.Split(' ');
                //string frmapm = fromampm[1];
                //string tapm = toampm[1];
                //MessageBox.Show(frmapm + " and " + tapm);
                
                manDataEntry(from, to, description);
                
            }
            else
            {
                MessageBox.Show("The time range and text box must be filled out!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                


                if(twohrmore < 12 && twohr.Equals("am"))
                {
                    timerange = strhr + ":00 AM - " + twohrmore + ":00 AM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohrmore = strhr + 2;
                }
                else if(twohrmore == 12 && strhr < 12 && sthr.Equals("am"))
                {
                    timerange = strhr + ":00 AM - " + twohrmore + ":00 PM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohr = "PM";
                    twohrmore = strhr + 2;
                }
                else if(strhr == 12 && sthr.Equals("am"))
                {
                    twohrmore = 2;
                    timerange = strhr + ":00 PM - " + twohrmore + ":00 PM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohr = "PM";
                    sthr = "PM";
                    twohrmore = strhr + 2;
                }
                else if(twohr.Equals("PM") && sthr.Equals("PM") && twohrmore < 12)
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
                else if(twohrmore > 12 && strhr == 12)
                {
                    sthr = "AM";
                    twohrmore = 2;
                    timerange = strhr + ":00 AM - " + twohrmore + ":00 AM";
                    ddlTimeRange.Items.Add(timerange);
                    strhr = twohrmore;
                    twohrmore = strhr + 2;
                }
                else if(sthr.Equals("AM") && strhr >= 2)
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
                else if(strhr < 12 && twohr.Equals("PM") && twohrmore < 13)
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


        public void manDataEntry(string from, string to, string description)
        {
 
            dr = dh.getReader("SELECT descript FROM ManualData WHERE descript IS NOT NULL AND fromtime = '" + from + "'");
            

            while (dr.Read())
            {
                sqldescript = (string)dr["descript"];
            }
            if (sqldescript.Length > 0)
            {
                sqldescript += ", " + description;
            
                try
                {
                    dh.executeSql("Update ManualData set descript = '" + sqldescript + "' WHERE fromtime = '" + from + "'");
                    dh.executeSql("Update DetailedReport set mandata = '" + sqldescript + "' WHERE starttime = '" + from + "'");
                }
                catch (SqlException se)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                    {
                        file.WriteLine("Error occurred updating database: " + DateTime.Now + " \r\n" + se + "\r\n");
                    }
                    MessageBox.Show("There has been an error updating the Downtime Event!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                MessageBox.Show("Manual Data Entry of time range:\nFROM: " + from + " TO: " + to + "\nDescription: " + description, "Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                try
                {
                    dh.executeSql("Insert INTO ManualData (fromtime, totime, descript) values ('" + from + "', '" + to + "', '" + description + "')");
                    dh.executeSql("Update DetailedReport set mandata = '" + description + "' WHERE starttime = '" + from + "'");
                }
                catch (SqlException ee)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                    {
                        file.WriteLine("Error occurred updating database: " + DateTime.Now + " \r\n" + ee + "\r\n");
                    }
                    MessageBox.Show("There has been an error updating the Downtime Event!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                MessageBox.Show("Manual Data Entry of time range:\nFROM: " + from + " TO: " + to + "\nDescription: " + description, "Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
