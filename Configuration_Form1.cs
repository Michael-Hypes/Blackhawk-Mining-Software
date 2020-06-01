using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlackHawk_Reporting
{
    public partial class Configuration_Form1 : Form
    {
        SQLDataHandler dh;

        public Configuration_Form1()
        {
            InitializeComponent();

            ShiftNumber.Items.Add(1);
            ShiftNumber.Items.Add(2);
            ShiftNumber.Items.Add(3);

            var item = DateTime.Today.AddHours(5); // 5:00 AM
            while (item <= DateTime.Today.AddHours(8)) // 9:00 AM
            {
                ShiftStart.Items.Add(item.TimeOfDay.ToString(@"hh\:mm") + "  AM");
                item = item.AddHours(1);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (ShiftStart.SelectedIndex > -1 && ShiftNumber.SelectedIndex > -1)
            {
                dh = new SQLDataHandler();
                string starttime = ShiftStart.Text;
                string[] start = starttime.Split(' ');
                string newtime = start[0] + ":00";
                string numshifts = ShiftNumber.Text;

                //split the date on the forward slash so that it can be formatted
                //to match ms sql date format
                string[] splitdate = EffectDate.Text.Split('/');

                //formatting the date to match the ms sql date format ex.  2019-11-28
                string newdate = splitdate[2] + "-" + splitdate[0] + "-" + splitdate[1];
                string converteddate = newdate + " " + starttime;
                string filepath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

                try
                {
                    dh.executeSql("Update ConfigurationSettings set newshiftstarttime = '" + starttime + "',  newnumshifts = '" + numshifts + "', datetotakeeffect = '" + converteddate + "' WHERE plantname = 'Kanawha Eagle'");
                }
                catch (SqlException se)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                    {
                        file.WriteLine("Error occurred updating configuration settings: " + DateTime.Now + " \r\n" + se + "\r\n");
                    }
                    MessageBox.Show("There has been an error updating the configuration settings!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                MessageBox.Show("Configuration Settings updated with Shift Start Time " + starttime + "\nNumber of Shifts: " + numshifts, "Successful", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("Both the shift start time and the number of shifts must be selected!", "Error", MessageBoxButtons.OK);
            }
        }
    }
}
