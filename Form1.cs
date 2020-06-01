using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibplctagWrapper;
using System.Threading;
using System.Data.SqlClient;
using System.Globalization;

namespace BlackHawk_Reporting
{
    public partial class Form1 : Form
    {
        
        //Creating an instance of the datahandler for sql connection
        SQLDataHandler dh;
        DataTable dt;
        SqlDataReader dr;
        BindingSource bs;
        DataTable dt1;
        DataTable dt2;
        //BindingSource bs1;
        //SQLDataHandler dh1;
        OneMinuteData trend;
        FiveMinuteData plcread;
        FiveMinuteData settime;
        SqlConnection cn;
        SqlConnection cn1;
        SqlConnection cn2;
        SqlDataAdapter adpt;
        SqlDataAdapter adpt1;
        SqlDataAdapter adpt2;
        string firstshiftstarttime;
        string firstshiftendtime;
        string secondshiftstarttime;
        string secondshiftendtime;
        string thirdshiftstarttime;
        string thirdshiftendtime;
        int shifthr1;
        int shifthr2;
        int shifthr3;
        
        string firstshifttotalendtimerange;
        string secondshifttotalendtimerange;
        string thirdshifttotalendtimerange = "";
        int lblrawtons = 0;
        int lblcleantons = 0;
        double lbldownmins = 0;
        double lblactrunhrs = 0.00;
        int schrunmins1 = 0;
        double yield1 = 0.00;
        double avail = 0.00;

        int lblrawtons2 = 0;
        int lblcleantons2 = 0;
        double lbldownmins2 = 0;
        double lblactrunhrs2 = 0.00;
        int schrunmins2 = 0;
        double yield2 = 0.00;
        double avail2 = 0.00;

        int lblrawtons3 = 0;
        int lblcleantons3 = 0;
        double lbldownmins3 = 0;
        double lblactrunhrs3 = 0.00;
        int schrunmins3 = 0;
        double yield3 = 0.00;
        double avail3 = 0.00;


        readonly string cstring = "Server=DESKTOP-0GD0RIC;Database=BHCCReport;User Id=sa;Password=Fred17dbase;";
        //readonly string cstring = "Data Source=DESKTOP-QDF9A1Q\\BHPP_REPORTING;Initial Catalog=BHCCReport;User id=sa;Password=BHReporting;";
        //readonly string cstring = "Data Source=LAPTOP\\BHREPORTING;Initial Catalog=BHCCReport;User id=sa;Password=BHReporting;";      
        //readonly string cstring = "Data Source=DESKTOP-CL05KBC;Initial Catalog=BHCCReport;User id=sa;Password=BHReporting;";
        //readonly string cstring = "Data Source=192.168.1.23, 1433;Network Library=DBMSSOCN;Initial Catalog=KanawhaEagleDB;User id=sa;Password=BHReporting;";
        //readonly string cstring = "Data Source=DESKTOP-VQBH5C1\\SQLEXPRESS;Initial Catalog=KanawhaEagleDB;User id=sa;Password=BHReporting;";



        //Create object of Downtime Monitoring class
        DownTimeMonitoring dm;

        //Creating an object of PLCConnection class to use its methods
        PLCConnection downTimeBit;
        //This integer stores the bit that is retreived from the PLC in the PLCConnection class
        Int16 downbit;

        //This boolean is used to monitor the change of the downbit.
        //If it doesn't change we don't need to insert or update a tuple in the sql database
        //bool downtimeMonitor;

        //This boolean will check for errors to stop the polling
        //The user will have to try a manual connection
        //bool errorCheck = true;

        int timetilstart = 0;
        int totalmins = 0;
        int totalsecs = 0;
        int shiftnums;

        public Form1()
        {
            InitializeComponent();
            this.Text = "Shift Report";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            settime = new FiveMinuteData();
            desktopConfig();

            settime.setTimeRanges();
            //Allow cancellation of the background workers
            bgWorker5MinData.WorkerSupportsCancellation = true;
            bgw1Minute.WorkerSupportsCancellation = true;
            bgwRetrieveData.WorkerSupportsCancellation = true;
            bgwTrending.WorkerSupportsCancellation = true;
            //Check the current time and create a timer if necessary to start 
            //reading data at the 5 minute interval
            checkTime();
            plcread = new FiveMinuteData();

            //Set the start time and the scheduled run minutes for the 
            //two hour time interval report
            //plcread.insertInitialValues(7, 120);

            //When the form loads fill the table
            fillTable();
            this.Refresh();
            //Check the plant status to see if the plant is running or down
            checkStatus();
            tmrTrending.Enabled = true;
            trend = new OneMinuteData();


            if (bgwTrending.IsBusy != true)
            {
                bgwTrending.RunWorkerAsync();
            }

        }

        private void bgw1Minute_DoWork(object sender, DoWorkEventArgs e)
        {

        }


        private void BgwRetrieveData_DoWork(object sender, DoWorkEventArgs e)
        {

            //Using this to wait 2 seconds before connecting to PLC again for monitoring downtime bit
            Thread.Sleep(2000);

            //Creating a connection to the PLC in the PLCConnection classto check plant status
            downTimeBit.createConnection("PLANT_STATUS", 2, 1);

            //Storing the integer bit that is retreived from the PLC
            downbit = downTimeBit.getDownbit();
            Boolean stat = dm.getdowntimeMonitor();
            //MessageBox.Show("Here is the downbit" + downbit);
            //Calling the downtime method passing the status bit as downbit, and the status boolean as stat
            dm.downtime(downbit, stat, dataGridView1);

        }

        private void BgwRetrieveData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (downbit.Equals(1))
            {
                btnRunIndicator.Text = "Running";
                btnRunIndicator.BackColor = Color.Chartreuse;
            }
            else if (downbit.Equals(-5))
            {
                btnRunIndicator.Text = "No Comm";
                btnRunIndicator.BackColor = Color.Gray;
            }
            else
            {
                btnRunIndicator.Text = "Plant Down";
                btnRunIndicator.BackColor = Color.Red;
            }
            //Starting the background worker thread again for continuous polling of the PLC
            bgwRetrieveData.RunWorkerAsync();
        }




        private void connectToPLCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dt = dh.getTable("Select * from Downtime");
            bs = new BindingSource();
            bs.DataSource = dt;
            dh.showData(dataGridView1, "Select * from DetailedReport");
            dm = new DownTimeMonitoring();
            //Instantiate the PLCConnection object
            downTimeBit = new PLCConnection();
            downTimeBit.createConnection("PLANT_STATUS", 2, 1);
            int setbool = downTimeBit.getDownbit();
            if (setbool.Equals(1))
            {
                dm.setdowntimeMonitor(true);
                btnRunIndicator.Text = "Running";
                btnRunIndicator.ForeColor = Color.Chartreuse;

                //Start the background worker thread
                bgwRetrieveData.RunWorkerAsync();
            }
            else if (setbool.Equals(-5))
            {
                btnRunIndicator.Text = "Communication Error";
                btnRunIndicator.BackColor = Color.Gray;
            }
            else
            {
                dm.setdowntimeMonitor(false);
                //Inserting the downtime date and time into the database
                dh.executeSql("Insert into Downtime(downtime, uptime) values (GETDATE(), NULL)");

                btnRunIndicator.Text = "Plant Down";
                //Start the background worker thread
                bgwRetrieveData.RunWorkerAsync();
            }
        }

        private void toolsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void disconnectFromPLCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bgwRetrieveData.CancelAsync();
            bgWorker5MinData.CancelAsync();
            bgw1Minute.CancelAsync();
            bgwTrending.CancelAsync();
            tmrStart.Enabled = false;
            tmrWait5Mins.Enabled = false;
            tmrTrending.Enabled = false;
        }


        private void checkTime()
        {
            DateTime now = DateTime.Now;


            totalmins = now.Minute % 5;
            totalsecs = now.Second % 60;
            if (totalmins % 5 == 0)
            {
                if (bgWorker5MinData.IsBusy != true)
                {
                    bgWorker5MinData.RunWorkerAsync();
                    tmrWait5Mins.Enabled = true;
                }
            }
            else
            {
                timetilstart = ((4 - totalmins) * 60) + (60 - totalsecs);
                tmrStart.Interval = timetilstart * 1000;
                tmrStart.Enabled = true;
            }
        }


        private void fillTable()
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
            
            if (numshifts == 2)
            {
                //setting the shift start and end times for the rest of the shifts
                firstshiftstarttime = stime.Substring(1, 4) + " AM";
                firstshiftendtime = stime.Substring(1, 4) + " PM";
                string[] hrarray = stime.Split(':');
                shifthr1 = Convert.ToInt32(hrarray[0]);
                shifthr2 = shifthr1 - 2;
                firstshifttotalendtimerange = Convert.ToString(shifthr2 + ":00 PM");
                secondshifttotalendtimerange = Convert.ToString(shifthr2 + ":00 AM");
                secondshiftstarttime = firstshiftendtime;
                secondshiftendtime = firstshiftstarttime;
            }
            else if (numshifts == 3)
            {
                //code to get shift starting and ending times
                //these will be used for the datagridview queries
                firstshiftstarttime = stime.Substring(1, 4) + " AM";
                string[] hrarray = stime.Split(':');
                shifthr1 = Convert.ToInt32(hrarray[0]);
                shifthr2 = shifthr1 + 8;
                firstshiftendtime = Convert.ToString(shifthr2 + ":00 PM");
                secondshiftstarttime = firstshiftendtime;
                shifthr3 = shifthr2 + 8;
                if(shifthr3 == 24)
                {
                    secondshiftendtime = Convert.ToString(shifthr3 + ":00 AM");
                }

                secondshiftendtime = Convert.ToString(shifthr3 + ":00 PM");
                thirdshiftstarttime = secondshiftendtime;
                thirdshiftendtime = firstshiftendtime;
            }

            //MessageBox.Show(firstshiftendtime);

            

            //instantiate everything needed for filling datagridview
            bs = new BindingSource();
            dh = new SQLDataHandler();
            dt = new DataTable();
            dm = new DownTimeMonitoring();
            ////dt1 = new DataTable();
            ////bs1 = new BindingSource();
            ////dh1 = new SQLDataHandler();
            adpt = new SqlDataAdapter();
            adpt1 = new SqlDataAdapter();
            adpt2 = new SqlDataAdapter();
            //getting todays date for use in sql query
            DateTime dtoday = DateTime.Today;
            string today;

            //convert datetime to string
            today = Convert.ToString(dtoday);
            string[] tday = today.Split(' ');
            //get substring of just the date
            today = Convert.ToString(today.Substring(0, 10));
           
            //split the date on the forward slash so that it can be formatted
            //to match ms sql date format
            string[] splitdate = today.Split('/');

            //formatting the date to match the ms sql date format ex.  2019-11-28
            //this is not necessary I have found out
            string newdate = splitdate[2] + "-" + splitdate[0] + "-" + splitdate[1];

            //doing the same thing for date but for the next day
            DateTime tomorrow = DateTime.Today.AddDays(1);
            string newtomorrow;

            //convert datetime to string
            newtomorrow = Convert.ToString(tomorrow);

            //substring to get the date only
            newtomorrow = Convert.ToString(newtomorrow.Substring(0, 10));

            //splitting the date on the forward slash
            string[] splittomorrow = newtomorrow.Split('/');

            //formatting the date to match ms sql date format
            string tmdate = splittomorrow[2] + "-" + splittomorrow[0] + "-" + splittomorrow[1];

            

            //setting the properties for the datagridview for visual appearance
            dataGridView1.RowTemplate.Height = 35;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;


            //setting the properties for the datagridview for visual appearance
            dataGridView2.RowTemplate.Height = 35;
            dataGridView2.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView2.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
            dataGridView2.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;


            //setting the properties for the datagridview for visual appearance
            dataGridView3.RowTemplate.Height = 35;
            dataGridView3.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView2.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
            dataGridView3.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView3.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;



            //creating a database connection and passing the connection string as a parameter
            cn = new SqlConnection(cstring);
            cn1 = new SqlConnection(cstring);
            cn2 = new SqlConnection(cstring);
            
            string today1 = DateTime.Today.ToShortDateString();
            DateTime tomorrow1 = Convert.ToDateTime(today1);
            tomorrow1 = tomorrow1.AddDays(1);
            string tmr1 = Convert.ToString(tomorrow1);
            string[] splittmr1 = tmr1.Split(' ');
            int edtm = Convert.ToInt32(endtime[0]);
            dh = new SQLDataHandler();

            ////get the shift start time
            dr = dh.getReader("SELECT currnumshifts from ConfigurationSettings WHERE plantname = 'Kanawha Eagle'");
            dr.Read();
            shiftnums = (int)dr["currnumshifts"];

            if (shiftnums == 2)
            {
                //hr < 7 needs to be hr < shiftstart hr
                if (hr == 12 && timesplit[1].Equals("AM") || hr < edtm && timesplit[1].Equals("AM"))
                {                   
                    //setting the sql adapter and passing the query string as the parameter
                    adpt = new SqlDataAdapter("SELECT RIGHT(CONVERT(VARCHAR, starttime, 100), 7) AS Start_Time, RIGHT(CONVERT(VARCHAR, endtime, 100), 7) AS End_Time, schrunmins AS Sch_Run_Mins, actrunhrs AS Act_Run_Hrs, " +
                        "rawtons AS Raw_Tons, cleantons AS Clean_Tons, yield AS Yield, rawtonsperhr AS Raw_TPH, cleantonsperhr AS Clean_TPH, downmins AS Downtime_Minutes, mandata AS Downtime_Description, magminutes AS Mag_Mins" +
                        " FROM DetailedReport WHERE starttime >= '" + prevday[0] + " " + firstshiftstarttime + "' AND starttime <= '" + prevday[0] + " " + firstshiftendtime + "'", cn);
                    dt = new DataTable();

                    //filling the data adapter with the datatable
                    adpt.Fill(dt);

                    //adding the data from the data table to the datagridview
                    dataGridView1.DataSource = dt;
                    dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
                    cn.Close();
                   
                    //setting the sql adapter and passing the query string as the parameter
                    adpt1 = new SqlDataAdapter("SELECT RIGHT(CONVERT(VARCHAR, starttime, 100), 7) AS Start_Time, RIGHT(CONVERT(VARCHAR, endtime, 100), 7) AS End_Time, schrunmins AS Sch_Run_Mins, actrunhrs AS Act_Run_Hrs, " +
                        "rawtons AS Raw_Tons, cleantons AS Clean_Tons, yield AS Yield, rawtonsperhr AS Raw_TPH, cleantonsperhr AS Clean_TPH, downmins AS Downtime_Minutes, mandata AS Downtime_Description, magminutes AS Mag_Mins" +
                        " FROM DetailedReport WHERE starttime >= '" + prevday[0] + " " + secondshiftstarttime + "' AND endtime <= '" + tday[0] + " " + secondshiftendtime + "'", cn1);
                    dt1 = new DataTable();

                    //filling the data adapter with the datatable
                    adpt1.Fill(dt1);

                    //adding the data from the data table to the datagridview
                    dataGridView2.DataSource = dt1;
                    dataGridView2.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
                    cn1.Close();

                    //added this to display totals under grid views....   
                    dh = new SQLDataHandler();
                    dr = dh.getReader("SELECT schrunmins, actrunhrs, rawtons, cleantons, downmins FROM DetailedReport WHERE actrunhrs IS NOT NULL AND starttime between '" + prevday[0] + " " + firstshiftstarttime +
                        "' AND '" + prevday[0] + " " + firstshifttotalendtimerange + "'");
                    while (dr.Read())
                    {
                        //getting all the totals data
                        schrunmins1 += Convert.ToInt32(dr["schrunmins"]);
                        lblactrunhrs += Convert.ToDouble(dr["actrunhrs"]);
                        lblrawtons += Convert.ToInt32(dr["rawtons"]);
                        lblcleantons += Convert.ToInt32(dr["cleantons"]);
                        lbldownmins += Convert.ToDouble(dr["downmins"]);
                    }

                    //calculating yield and availability
                    yield1 = ((double)lblcleantons / (double)lblrawtons) * 100;
                    yield1 = Math.Round(yield1, 2);
                    avail = (((double)schrunmins1 - (double)lbldownmins) / (double)schrunmins1) * 100;
                    avail = Math.Round(avail, 2);

                    //display with form labels
                    lblAvail1.Text = Convert.ToString(avail);
                    lblSchRunMins1.Text = Convert.ToString(schrunmins1);
                    lblRunHrs1.Text = Convert.ToString(lblactrunhrs);
                    lblRawTons1.Text = Convert.ToString(lblrawtons);
                    lblCleanTons1.Text = Convert.ToString(lblcleantons);
                    lblDowntime1.Text = Convert.ToString(lbldownmins);
                    lblYield1.Text = Convert.ToString(yield1);

                    //query from detailed report table for displaying totals
                    dr = dh.getReader("SELECT schrunmins, actrunhrs, rawtons, cleantons, downmins FROM DetailedReport WHERE actrunhrs IS NOT NULL AND starttime between '" + prevday[0] + " " + secondshiftstarttime +
                        "' AND '" + today1+ " " + secondshifttotalendtimerange + "'");
                    while (dr.Read())
                    {
                        //getting all the totals data for shift 2
                        schrunmins2 += Convert.ToInt32(dr["schrunmins"]);
                        lblactrunhrs2 += Convert.ToDouble(dr["actrunhrs"]);
                        lblrawtons2 += Convert.ToInt32(dr["rawtons"]);
                        lblcleantons2 += Convert.ToInt32(dr["cleantons"]);
                        lbldownmins2 += Convert.ToDouble(dr["downmins"]);
                    }

                    //calculating yield and availability
                    yield2 = ((double)lblcleantons2 / (double)lblrawtons2) * 100;
                    yield2 = Math.Round(yield2, 2);
                    avail2 = (((double)schrunmins2 - (double)lbldownmins2) / (double)schrunmins2) * 100;
                    avail2 = Math.Round(avail2, 2);

                    //display with form labels
                    lblAvail2.Text = Convert.ToString(avail2);
                    lblSchRunMins2.Text = Convert.ToString(schrunmins2);
                    lblRunHrs2.Text = Convert.ToString(lblactrunhrs2);
                    lblRawTons2.Text = Convert.ToString(lblrawtons2);
                    lblCleanTons2.Text = Convert.ToString(lblcleantons2);
                    lblDowntime2.Text = Convert.ToString(lbldownmins2);
                    lblYield2.Text = Convert.ToString(yield2);



                    lblrawtons = 0;
                    lblcleantons = 0;
                    lbldownmins = 0;
                    lblactrunhrs = 0.00;
                    schrunmins1 = 0;
                    avail = 0.00;
                    yield1 = 0.00;

                    lblrawtons2 = 0;
                    lblcleantons2 = 0;
                    lbldownmins2 = 0;
                    lblactrunhrs2 = 0.00;
                    schrunmins2 = 0;
                    avail2 = 0.00;
                    yield2 = 0.00;


                }
                else
                {
                    
                    //setting the sql adapter and passing the query string as the parameter
                    adpt = new SqlDataAdapter("SELECT RIGHT(CONVERT(VARCHAR, starttime, 100), 7) AS Start_Time, RIGHT(CONVERT(VARCHAR, endtime, 100), 7) AS End_Time, schrunmins AS Sch_Run_Mins, actrunhrs AS Act_Run_Hrs, " +
                        "rawtons AS Raw_Tons, cleantons AS Clean_Tons, yield AS Yield, rawtonsperhr AS Raw_TPH, cleantonsperhr AS Clean_TPH, downmins AS Downtime_Minutes, mandata AS Downtime_Description, magminutes AS Mag_Mins " +
                        "FROM DetailedReport WHERE starttime >= '" + today1 + " " + firstshiftstarttime + "' AND starttime <= '" + today1 + " " + firstshiftendtime + "'", cn);
                    dt = new DataTable();

                    //filling the data adapter with the datatable
                    adpt.Fill(dt);

                    //adding the data from the data table to the datagridview
                    dataGridView1.DataSource = dt;
                    dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
                    cn.Close();

                    //setting the sql adapter and passing the query string as the parameter
                    adpt1 = new SqlDataAdapter("SELECT RIGHT(CONVERT(VARCHAR, starttime, 100), 7) AS Start_Time, RIGHT(CONVERT(VARCHAR, endtime, 100), 7) AS End_Time, schrunmins AS Sch_Run_Mins, actrunhrs AS Act_Run_Hrs, " +
                        "rawtons AS Raw_Tons, cleantons AS Clean_Tons, yield AS Yield, rawtonsperhr AS Raw_TPH, cleantonsperhr AS Clean_TPH, downmins AS Downtime_Minutes, mandata AS Downtime_Description, magminutes AS Mag_Mins " +
                        "FROM DetailedReport WHERE starttime >= '" + today1 + " " + secondshiftstarttime + "' AND endtime <= '" + splittmr1[0] + " " + secondshiftendtime + "'", cn1);
                    dt1 = new DataTable();

                    //filling the data adapter with the datatable
                    adpt1.Fill(dt1);

                    //adding the data from the data table to the datagridview
                    dataGridView2.DataSource = dt1;
                    dataGridView2.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
                    cn1.Close();

                    

                    //added this to display totals under grid views
                    dh = new SQLDataHandler();
                    dr = dh.getReader("SELECT schrunmins, actrunhrs, rawtons, cleantons, downmins FROM DetailedReport WHERE actrunhrs IS NOT NULL AND starttime between '" + today1 + " " + firstshiftstarttime + 
                        "' AND '" + today1 + " " + firstshifttotalendtimerange + "'");
                    while (dr.Read())
                    {

                        schrunmins1 += Convert.ToInt32(dr["schrunmins"]);
                        lblactrunhrs += Convert.ToDouble(dr["actrunhrs"]);
                        lblrawtons += Convert.ToInt32(dr["rawtons"]);
                        lblcleantons += Convert.ToInt32(dr["cleantons"]);
                        lbldownmins += Convert.ToDouble(dr["downmins"]);
                    }

                    yield1 = ((double)lblcleantons / (double)lblrawtons) * 100;
                    yield1 = Math.Round(yield1, 2);
                    avail = (((double)schrunmins1 - (double)lbldownmins) / (double)schrunmins1) * 100;
                    avail = Math.Round(avail, 2);
                    lblAvail1.Text = Convert.ToString(avail);
                    lblSchRunMins1.Text = Convert.ToString(schrunmins1);
                    lblRunHrs1.Text = Convert.ToString(lblactrunhrs);
                    lblRawTons1.Text = Convert.ToString(lblrawtons);
                    lblCleanTons1.Text = Convert.ToString(lblcleantons);
                    lblDowntime1.Text = Convert.ToString(lbldownmins);
                    lblYield1.Text = Convert.ToString(yield1);


                    dr = dh.getReader("SELECT schrunmins, actrunhrs, rawtons, cleantons, downmins FROM DetailedReport WHERE actrunhrs IS NOT NULL AND starttime between '" + today1 + " " + secondshiftstarttime + 
                        "' AND '" + splittmr1[0] + " " + secondshifttotalendtimerange + "'");
                    while (dr.Read())
                    {
                        schrunmins2 += Convert.ToInt32(dr["schrunmins"]);
                        lblactrunhrs2 += Convert.ToDouble(dr["actrunhrs"]);
                        lblrawtons2 += Convert.ToInt32(dr["rawtons"]);
                        lblcleantons2 += Convert.ToInt32(dr["cleantons"]);
                        lbldownmins2 += Convert.ToDouble(dr["downmins"]);
                    }

                    yield2 = ((double)lblcleantons2 / (double)lblrawtons2) * 100;
                    yield2 = Math.Round(yield2, 2);
                    avail2 = (((double)schrunmins2 - (double)lbldownmins2) / (double)schrunmins2) * 100;
                    avail2 = Math.Round(avail2, 2);
                    lblAvail2.Text = Convert.ToString(avail2);
                    lblSchRunMins2.Text = Convert.ToString(schrunmins2);
                    lblRunHrs2.Text = Convert.ToString(lblactrunhrs2);
                    lblRawTons2.Text = Convert.ToString(lblrawtons2);
                    lblCleanTons2.Text = Convert.ToString(lblcleantons2);
                    lblDowntime2.Text = Convert.ToString(lbldownmins2);
                    lblYield2.Text = Convert.ToString(yield2);



                    lblrawtons = 0;
                    lblcleantons = 0;
                    lbldownmins = 0;
                    lblactrunhrs = 0.00;
                    schrunmins1 = 0;
                    avail = 0.00;
                    yield1 = 0.00;

                    lblrawtons2 = 0;
                    lblcleantons2 = 0;
                    lbldownmins2 = 0;
                    lblactrunhrs2 = 0.00;
                    schrunmins2 = 0;
                    avail2 = 0.00;
                    yield2 = 0.00;

                }
            

            }
            else if (shiftnums == 3)
            {

                //hr < 7 needs to be hr < shiftstart hr
                if (hr == 12 && timesplit[1].Equals("AM") || hr < edtm && timesplit[1].Equals("AM"))
                {
                    //setting the sql adapter and passing the query string as the parameter
                    adpt = new SqlDataAdapter("SELECT RIGHT(CONVERT(VARCHAR, starttime, 100), 7) AS Start_Time, RIGHT(CONVERT(VARCHAR, endtime, 100), 7) AS End_Time, schrunmins AS Sch_Run_Mins, actrunhrs AS Act_Run_Hrs, " +
                        "rawtons AS Raw_Tons, cleantons AS Clean_Tons, yield AS Yield, rawtonsperhr AS Raw_TPH, cleantonsperhr AS Clean_TPH, downmins AS Downtime_Minutes, mandata AS Downtime_Description, magminutes AS Mag_Mins " +
                        "FROM DetailedReport WHERE starttime >= '" + today1 + " " + firstshiftstarttime + "' AND starttime <= '" + today1 + " " + firstshiftendtime + "'", cn);
                    dt = new DataTable();

                    //filling the data adapter with the datatable
                    adpt.Fill(dt);

                    //adding the data from the data table to the datagridview
                    dataGridView1.DataSource = dt;
                    dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
                    cn.Close();

                    //setting the sql adapter and passing the query string as the parameter
                    adpt1 = new SqlDataAdapter("SELECT RIGHT(CONVERT(VARCHAR, starttime, 100), 7) AS Start_Time, RIGHT(CONVERT(VARCHAR, endtime, 100), 7) AS End_Time, schrunmins AS Sch_Run_Mins, actrunhrs AS Act_Run_Hrs, " +
                        "rawtons AS Raw_Tons, cleantons AS Clean_Tons, yield AS Yield, rawtonsperhr AS Raw_TPH, cleantonsperhr AS Clean_TPH, downmins AS Downtime_Minutes, mandata AS Downtime_Description, magminutes AS Mag_Mins " +
                        "FROM DetailedReport WHERE starttime >= '" + today1 + " " + secondshiftstarttime + "' AND endtime <= '" + today1 + " " + secondshiftendtime + "'", cn1);
                    dt1 = new DataTable();

                    //filling the data adapter with the datatable
                    adpt1.Fill(dt1);

                    //adding the data from the data table to the datagridview
                    dataGridView2.DataSource = dt1;
                    dataGridView2.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
                    cn1.Close();

                    //setting the sql adapter and passing the query string as the parameter
                    adpt2 = new SqlDataAdapter("SELECT RIGHT(CONVERT(VARCHAR, starttime, 100), 7) AS Start_Time, RIGHT(CONVERT(VARCHAR, endtime, 100), 7) AS End_Time, schrunmins AS Sch_Run_Mins, actrunhrs AS Act_Run_Hrs, " +
                        "rawtons AS Raw_Tons, cleantons AS Clean_Tons, yield AS Yield, rawtonsperhr AS Raw_TPH, cleantonsperhr AS Clean_TPH, downmins AS Downtime_Minutes, mandata AS Downtime_Description, magminutes AS Mag_Mins " +
                        "FROM DetailedReport WHERE starttime >= '" + today1 + " " + thirdshiftstarttime + "' AND endtime <= '" + splittmr1[0] + " " + thirdshiftendtime + "'", cn1);
                    dt2 = new DataTable();

                    //filling the data adapter with the datatable
                    adpt2.Fill(dt2);

                    //adding the data from the data table to the datagridview
                    dataGridView3.DataSource = dt2;
                    dataGridView3.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
                    cn2.Close();

                    //added this to display totals under grid views
                    dh = new SQLDataHandler();
                    dr = dh.getReader("SELECT schrunmins, actrunhrs, rawtons, cleantons, downmins FROM DetailedReport WHERE actrunhrs IS NOT NULL AND starttime between '" + today1 + " " + firstshiftstarttime +
                        "' AND '" + today1 + " " + firstshifttotalendtimerange + "'");
                    while (dr.Read())
                    {

                        schrunmins1 += Convert.ToInt32(dr["schrunmins"]);
                        lblactrunhrs += Convert.ToDouble(dr["actrunhrs"]);
                        lblrawtons += Convert.ToInt32(dr["rawtons"]);
                        lblcleantons += Convert.ToInt32(dr["cleantons"]);
                        lbldownmins += Convert.ToDouble(dr["downmins"]);
                    }

                    yield1 = ((double)lblcleantons / (double)lblrawtons) * 100;
                    yield1 = Math.Round(yield1, 2);
                    avail = (((double)schrunmins1 - (double)lbldownmins) / (double)schrunmins1) * 100;
                    avail = Math.Round(avail, 2);
                    lblAvail1.Text = Convert.ToString(avail);
                    lblSchRunMins1.Text = Convert.ToString(schrunmins1);
                    lblRunHrs1.Text = Convert.ToString(lblactrunhrs);
                    lblRawTons1.Text = Convert.ToString(lblrawtons);
                    lblCleanTons1.Text = Convert.ToString(lblcleantons);
                    lblDowntime1.Text = Convert.ToString(lbldownmins);
                    lblYield1.Text = Convert.ToString(yield1);


                    dr = dh.getReader("SELECT schrunmins, actrunhrs, rawtons, cleantons, downmins FROM DetailedReport WHERE actrunhrs IS NOT NULL AND starttime between '" + today1 + " " + secondshiftstarttime +
                        "' AND '" + splittmr1[0] + " " + secondshifttotalendtimerange + "'");
                    while (dr.Read())
                    {
                        schrunmins2 += Convert.ToInt32(dr["schrunmins"]);
                        lblactrunhrs2 += Convert.ToDouble(dr["actrunhrs"]);
                        lblrawtons2 += Convert.ToInt32(dr["rawtons"]);
                        lblcleantons2 += Convert.ToInt32(dr["cleantons"]);
                        lbldownmins2 += Convert.ToDouble(dr["downmins"]);
                    }

                    yield2 = ((double)lblcleantons2 / (double)lblrawtons2) * 100;
                    yield2 = Math.Round(yield2, 2);
                    avail2 = (((double)schrunmins1 - (double)lbldownmins) / (double)schrunmins1) * 100;
                    avail2 = Math.Round(avail2, 2);
                    lblAvail2.Text = Convert.ToString(avail2);
                    lblSchRunMins2.Text = Convert.ToString(schrunmins2);
                    lblRunHrs2.Text = Convert.ToString(lblactrunhrs2);
                    lblRawTons2.Text = Convert.ToString(lblrawtons2);
                    lblCleanTons2.Text = Convert.ToString(lblcleantons2);
                    lblDowntime2.Text = Convert.ToString(lbldownmins2);
                    lblYield2.Text = Convert.ToString(yield2);


                    dr = dh.getReader("SELECT schrunmins, actrunhrs, rawtons, cleantons, downmins FROM DetailedReport WHERE actrunhrs IS NOT NULL AND starttime between '" + today1 + " " + thirdshiftstarttime +
                        "' AND '" + today + " " + thirdshifttotalendtimerange + "'");
                    while (dr.Read())
                    {
                        schrunmins3 += Convert.ToInt32(dr["schrunmins"]);
                        lblactrunhrs3 += Convert.ToDouble(dr["actrunhrs"]);
                        lblrawtons3 += Convert.ToInt32(dr["rawtons"]);
                        lblcleantons3 += Convert.ToInt32(dr["cleantons"]);
                        lbldownmins3 += Convert.ToDouble(dr["downmins"]);
                    }

                    yield3 = ((double)lblcleantons3 / (double)lblrawtons3) * 100;
                    yield3 = Math.Round(yield3, 2);
                    avail3 = (((double)schrunmins3 - (double)lbldownmins3) / (double)schrunmins3) * 100;
                    avail3 = Math.Round(avail3, 2);
                    lblAvail3.Text = Convert.ToString(avail3);
                    lblSchRunMins3.Text = Convert.ToString(schrunmins3);
                    lblRunHrs3.Text = Convert.ToString(lblactrunhrs3);
                    lblRawTons3.Text = Convert.ToString(lblrawtons3);
                    lblCleanTons3.Text = Convert.ToString(lblcleantons3);
                    lblDowntime3.Text = Convert.ToString(lbldownmins3);
                    lblYield3.Text = Convert.ToString(yield3);



                    lblrawtons = 0;
                    lblcleantons = 0;
                    lbldownmins = 0;
                    lblactrunhrs = 0.00;
                    schrunmins1 = 0;
                    avail = 0.00;
                    yield1 = 0.00;

                    lblrawtons2 = 0;
                    lblcleantons2 = 0;
                    lbldownmins2 = 0;
                    lblactrunhrs2 = 0.00;
                    schrunmins2 = 0;
                    avail2 = 0.00;
                    yield2 = 0.00;

                    lblrawtons3 = 0;
                    lblcleantons3 = 0;
                    lbldownmins3 = 0;
                    lblactrunhrs3 = 0.00;
                    schrunmins3 = 0;
                    avail3 = 0.00;
                    yield3 = 0.00;


                }
                else
                {

                    //setting the sql adapter and passing the query string as the parameter
                    adpt = new SqlDataAdapter("SELECT RIGHT(CONVERT(VARCHAR, starttime, 100), 7) AS Start_Time, RIGHT(CONVERT(VARCHAR, endtime, 100), 7) AS End_Time, schrunmins AS Sch_Run_Mins, actrunhrs AS Act_Run_Hrs, " +
                        "rawtons AS Raw_Tons, cleantons AS Clean_Tons, yield AS Yield, rawtonsperhr AS Raw_TPH, cleantonsperhr AS Clean_TPH, downmins AS Downtime_Minutes, mandata AS Downtime_Description, magminutes AS Mag_Mins " +
                        "FROM DetailedReport WHERE starttime >= '" + today1 + " " + firstshiftstarttime + "' AND starttime <= '" + today1 + " " + firstshiftendtime + "'", cn);
                    dt = new DataTable();

                    //filling the data adapter with the datatable
                    adpt.Fill(dt);

                    //adding the data from the data table to the datagridview
                    dataGridView1.DataSource = dt;
                    dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
                    cn.Close();

                    //setting the sql adapter and passing the query string as the parameter
                    adpt1 = new SqlDataAdapter("SELECT RIGHT(CONVERT(VARCHAR, starttime, 100), 7) AS Start_Time, RIGHT(CONVERT(VARCHAR, endtime, 100), 7) AS End_Time, schrunmins AS Sch_Run_Mins, actrunhrs AS Act_Run_Hrs, " +
                        "rawtons AS Raw_Tons, cleantons AS Clean_Tons, yield AS Yield, rawtonsperhr AS Raw_TPH, cleantonsperhr AS Clean_TPH, downmins AS Downtime_Minutes, mandata AS Downtime_Description, magminutes AS Mag_Mins " +
                        "FROM DetailedReport WHERE starttime >= '" + today1 + " " + secondshiftstarttime + "' AND endtime <= '" + today1 + " " + secondshiftendtime + "'", cn1);
                    dt1 = new DataTable();

                    //filling the data adapter with the datatable
                    adpt1.Fill(dt1);

                    //adding the data from the data table to the datagridview
                    dataGridView2.DataSource = dt1;
                    dataGridView2.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
                    cn1.Close();

                    //setting the sql adapter and passing the query string as the parameter
                    adpt2 = new SqlDataAdapter("SELECT RIGHT(CONVERT(VARCHAR, starttime, 100), 7) AS Start_Time, RIGHT(CONVERT(VARCHAR, endtime, 100), 7) AS End_Time, schrunmins AS Sch_Run_Mins, actrunhrs AS Act_Run_Hrs, " +
                        "rawtons AS Raw_Tons, cleantons AS Clean_Tons, yield AS Yield, rawtonsperhr AS Raw_TPH, cleantonsperhr AS Clean_TPH, downmins AS Downtime_Minutes, mandata AS Downtime_Description, magminutes AS Mag_Mins " +
                        "FROM DetailedReport WHERE starttime >= '" + today1 + " " + thirdshiftstarttime + "' AND endtime <= '" + splittmr1[0] + " " + thirdshiftendtime + "'", cn1);
                    dt2 = new DataTable();

                    //filling the data adapter with the datatable
                    adpt2.Fill(dt2);

                    //adding the data from the data table to the datagridview
                    dataGridView3.DataSource = dt2;
                    dataGridView3.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
                    cn2.Close();

                    //added this to display totals under grid views
                    dh = new SQLDataHandler();
                    dr = dh.getReader("SELECT schrunmins, actrunhrs, rawtons, cleantons, downmins FROM DetailedReport WHERE actrunhrs IS NOT NULL AND starttime between '" + today1 + " " + firstshiftstarttime +
                        "' AND '" + today1 + " " + firstshifttotalendtimerange + "'");
                    while (dr.Read())
                    {

                        schrunmins1 += Convert.ToInt32(dr["schrunmins"]);
                        lblactrunhrs += Convert.ToDouble(dr["actrunhrs"]);
                        lblrawtons += Convert.ToInt32(dr["rawtons"]);
                        lblcleantons += Convert.ToInt32(dr["cleantons"]);
                        lbldownmins += Convert.ToDouble(dr["downmins"]);
                    }

                    yield1 = ((double)lblcleantons / (double)lblrawtons) * 100;
                    yield1 = Math.Round(yield1, 2);
                    avail = (((double)schrunmins1 - (double)lbldownmins) / (double)schrunmins1) * 100;
                    avail = Math.Round(avail, 2);
                    lblAvail1.Text = Convert.ToString(avail);
                    lblSchRunMins1.Text = Convert.ToString(schrunmins1);
                    lblRunHrs1.Text = Convert.ToString(lblactrunhrs);
                    lblRawTons1.Text = Convert.ToString(lblrawtons);
                    lblCleanTons1.Text = Convert.ToString(lblcleantons);
                    lblDowntime1.Text = Convert.ToString(lbldownmins);
                    lblYield1.Text = Convert.ToString(yield1);


                    dr = dh.getReader("SELECT schrunmins, actrunhrs, rawtons, cleantons, downmins FROM DetailedReport WHERE actrunhrs IS NOT NULL AND starttime between '" + today1 + " " + secondshiftstarttime +
                        "' AND '" + today1 + " " + secondshifttotalendtimerange + "'");
                    while (dr.Read())
                    {
                        schrunmins2 += Convert.ToInt32(dr["schrunmins"]);
                        lblactrunhrs2 += Convert.ToDouble(dr["actrunhrs"]);
                        lblrawtons2 += Convert.ToInt32(dr["rawtons"]);
                        lblcleantons2 += Convert.ToInt32(dr["cleantons"]);
                        lbldownmins2 += Convert.ToDouble(dr["downmins"]);
                    }

                    yield2 = ((double)lblcleantons2 / (double)lblrawtons2) * 100;
                    yield2 = Math.Round(yield2, 2);
                    avail2 = (((double)schrunmins1 - (double)lbldownmins) / (double)schrunmins1) * 100;
                    avail2 = Math.Round(avail2, 2);
                    lblAvail2.Text = Convert.ToString(avail2);
                    lblSchRunMins2.Text = Convert.ToString(schrunmins2);
                    lblRunHrs2.Text = Convert.ToString(lblactrunhrs2);
                    lblRawTons2.Text = Convert.ToString(lblrawtons2);
                    lblCleanTons2.Text = Convert.ToString(lblcleantons2);
                    lblDowntime2.Text = Convert.ToString(lbldownmins2);
                    lblYield2.Text = Convert.ToString(yield2);


                    dr = dh.getReader("SELECT schrunmins, actrunhrs, rawtons, cleantons, downmins FROM DetailedReport WHERE actrunhrs IS NOT NULL AND starttime between '" + today1 + " " + thirdshiftstarttime +
                        "' AND '" + splittmr1[0] + " " + thirdshifttotalendtimerange + "'");
                    while (dr.Read())
                    {
                        schrunmins3 += Convert.ToInt32(dr["schrunmins"]);
                        lblactrunhrs3 += Convert.ToDouble(dr["actrunhrs"]);
                        lblrawtons3 += Convert.ToInt32(dr["rawtons"]);
                        lblcleantons3 += Convert.ToInt32(dr["cleantons"]);
                        lbldownmins3 += Convert.ToDouble(dr["downmins"]);
                    }

                    yield3 = ((double)lblcleantons3 / (double)lblrawtons3) * 100;
                    yield3 = Math.Round(yield3, 2);
                    avail3 = (((double)schrunmins3 - (double)lbldownmins3) / (double)schrunmins3) * 100;
                    avail3 = Math.Round(avail3, 2);
                    lblAvail3.Text = Convert.ToString(avail3);
                    lblSchRunMins3.Text = Convert.ToString(schrunmins3);
                    lblRunHrs3.Text = Convert.ToString(lblactrunhrs3);
                    lblRawTons3.Text = Convert.ToString(lblrawtons3);
                    lblCleanTons3.Text = Convert.ToString(lblcleantons3);
                    lblDowntime3.Text = Convert.ToString(lbldownmins3);
                    lblYield3.Text = Convert.ToString(yield3);



                    lblrawtons = 0;
                    lblcleantons = 0;
                    lbldownmins = 0;
                    lblactrunhrs = 0.00;
                    schrunmins1 = 0;
                    avail = 0.00;
                    yield1 = 0.00;

                    lblrawtons2 = 0;
                    lblcleantons2 = 0;
                    lbldownmins2 = 0;
                    lblactrunhrs2 = 0.00;
                    schrunmins2 = 0;
                    avail2 = 0.00;
                    yield2 = 0.00;

                    lblrawtons3 = 0;
                    lblcleantons3 = 0;
                    lbldownmins3 = 0;
                    lblactrunhrs3 = 0.00;
                    schrunmins3 = 0;
                    avail3 = 0.00;
                    yield3 = 0.00;
                }
            }
        }


        private void checkStatus()
        {
            //Instantiate the PLCConnection object
            downTimeBit = new PLCConnection();
            dh = new SQLDataHandler();
            //creating a PLC connection and passing the memory address name, number of bytes to be retrieved
            //and how many memory addresses in a row that will be retrieved
            downTimeBit.createConnection("PLANT_STATUS", 2, 1);

            //setbool will store the downtimebit to be used to determine if the plant is running or not
            //or if a communication error has been detected
            int setbool = downTimeBit.getDownbit();

            dm = new DownTimeMonitoring();
            
            //if setbool equals 1 the plant is running
            if (setbool.Equals(1))
            {
                //set the downtimebit in the downtimemonitoring class using a setter
                dm.setdowntimeMonitor(true);

                //set the dashboard button text to running
                btnRunIndicator.Text = "Running";

                //Start the background worker thread
                bgwRetrieveData.RunWorkerAsync();
            }
            //if setbit equals -5 there has been a communication error
            else if (setbool.Equals(-5))
            {
                btnRunIndicator.Text = "No Comm";
                btnRunIndicator.BackColor = Color.Gray;
            }
            else
            {
                dm.setdowntimeMonitor(false);
                //Inserting the downtime date and time into the database
                dh.executeSql("Insert into Downtime(downtime, uptime) values (GETDATE(), NULL)");

                btnRunIndicator.Text = "Plant Down";
                //Start the background worker thread
                bgwRetrieveData.RunWorkerAsync();
            }
        }

        private void bgWorker5MinData_DoWork(object sender, DoWorkEventArgs e)
        {
            plcread = new FiveMinuteData();
            plcread.readData();
        }
 

        private void bgWorker5MinData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            fillTable();
            this.Refresh();
        }

        private void tmrStart_Tick(object sender, EventArgs e)
        {
            if (bgWorker5MinData.IsBusy != true)
            {
                bgWorker5MinData.RunWorkerAsync();
                tmrWait5Mins.Enabled = true;
                tmrStart.Enabled = false;
            }
            
        }

        private void tmrWait5Mins_Tick(object sender, EventArgs e)
        {
            if (bgWorker5MinData.IsBusy != true)
            {
                bgWorker5MinData.RunWorkerAsync();
            }
        }

        private void bgwTrending_DoWork(object sender, DoWorkEventArgs e)
        {
            trend = new OneMinuteData();
            trend.readdata();
        }

        private void bgwTrending_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void tmrTrending_Tick(object sender, EventArgs e)
        {
            if(bgwTrending.IsBusy != true)
            {
                bgwTrending.RunWorkerAsync();
            }
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            Manual_Data1 manData = new Manual_Data1();
            manData.Show();
        }

        private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Shift Configuration Under Construction!", "Under Construction", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //ConfigVerification configverify = new ConfigVerification();
            //configverify.Show();
        }



        public void desktopConfig()
        {
            dh = new SQLDataHandler();

            ////get the shift start time
            dr = dh.getReader("SELECT currnumshifts from ConfigurationSettings WHERE plantname = 'Kanawha Eagle'");
            dr.Read();
            shiftnums = (int)dr["currnumshifts"];

            if (shiftnums == 2)
            {
                dataGridView3.Visible = false;
                lblTotal3.Visible = false;
                lblSchRunMins3.Visible = false;
                lblRunHrs3.Visible = false;
                lblRawTons3.Visible = false;
                lblDowntime3.Visible = false;
                lblCleanTons3.Visible = false;
                lblYield3.Visible = false;
                lblYieldHeading3.Visible = false;
                lblDowntimeHeading3.Visible = false;
                lblRawHeading3.Visible = false;
                lblRunHeading3.Visible = false;
                lblSchHeading3.Visible = false;
                lblThirdShift.Visible = false;
                lblCleanHeading3.Visible = false;
                lblAvailHeading3.Visible = false;
                lblAvail3.Visible = false;


                dataGridView1.Location = new Point(345, 145);
                dataGridView1.Height = 232;
                lblTotal1.Location = new Point(364, 405);
                lblFirstShift.Location = new Point(221, 145);
                lblYield1.Location = new Point(989, 408);
                lblYieldHeading1.Location = new Point(989, 380);
                lblCleanHeading1.Location = new Point(886, 380);
                lblCleanTons1.Location = new Point(908, 408);
                lblDowntime1.Location = new Point(1278, 408);
                lblDowntimeHeading1.Location = new Point(1255, 380);
                lblRawHeading1.Location = new Point(792, 380);
                lblRawTons1.Location = new Point(808, 407);
                lblRunHeading1.Location = new Point(697, 380);
                lblRunHrs1.Location = new Point(707, 408);
                lblSchHeading1.Location = new Point(570, 380);
                lblSchRunMins1.Location = new Point(597, 408);
                lblAvailHeading1.Location = new Point(1087, 380);
                lblAvail1.Location = new Point(1104, 408);

                dataGridView2.Location = new Point(345, 605);
                dataGridView2.Height = 232;
                lblTotal2.Location = new Point(364, 863);
                lblSecondShift.Location = new Point(221, 605);
                lblYield2.Location = new Point(989, 866);
                lblYieldHeading2.Location = new Point(989, 840);
                lblCleanHeading2.Location = new Point(886, 840);
                lblCleanTons2.Location = new Point(908, 866);
                lblDowntime2.Location = new Point(1278, 863);
                lblDowntimeHeading2.Location = new Point(1255, 840);
                lblRawHeading2.Location = new Point(792, 840);
                lblRawTons2.Location = new Point(808, 866);
                lblRunHeading2.Location = new Point(697, 840);
                lblRunHrs2.Location = new Point(707, 863);
                lblSchHeading2.Location = new Point(570, 840);
                lblSchRunMins2.Location = new Point(597, 866);
                lblAvailHeading2.Location = new Point(1087, 840);
                lblAvail2.Location = new Point(1104, 866);


            }
            else if (shiftnums == 3)
            {
                dataGridView3.Visible = true;
                lblTotal3.Visible = true;
                lblSchRunMins3.Visible = true;
                lblRunHrs3.Visible = true;
                lblRawTons3.Visible = true;
                lblDowntime3.Visible = true;
                lblCleanTons3.Visible = true;
                lblYield3.Visible = true;
                lblYieldHeading3.Visible = true;
                lblDowntimeHeading3.Visible = true;
                lblRawHeading3.Visible = true;
                lblRunHeading3.Visible = true;
                lblSchHeading3.Visible = true;
                lblThirdShift.Visible = true;
                lblCleanHeading3.Visible = true;
                lblAvailHeading3.Visible = true;
                lblAvail3.Visible = true;


                dataGridView1.Location = new Point(345, 63);
                dataGridView1.Height = 161;
                lblTotal1.Location = new Point(364, 269);
                lblFirstShift.Location = new Point(221, 63);
                lblYield1.Location = new Point(989, 272);
                lblYieldHeading1.Location = new Point(989, 244);
                lblCleanHeading1.Location = new Point(886, 244);
                lblCleanTons1.Location = new Point(908, 272);
                lblDowntime1.Location = new Point(1278, 272);
                lblDowntimeHeading1.Location = new Point(1255, 244);
                lblRawHeading1.Location = new Point(792, 244);
                lblRawTons1.Location = new Point(808, 272);
                lblRunHeading1.Location = new Point(697, 244);
                lblRunHrs1.Location = new Point(707, 272);
                lblSchHeading1.Location = new Point(570, 244);
                lblSchRunMins1.Location = new Point(597, 272);
                lblAvailHeading1.Location = new Point(1087, 244);
                lblAvail1.Location = new Point(1104, 272);

                dataGridView2.Location = new Point(345, 369);
                dataGridView2.Height = 161;
                lblTotal2.Location = new Point(364, 576);
                lblSecondShift.Location = new Point(221, 369);
                lblYield2.Location = new Point(989, 579);
                lblYieldHeading2.Location = new Point(989, 553);
                lblCleanHeading2.Location = new Point(886, 553);
                lblCleanTons2.Location = new Point(908, 579);
                lblDowntime2.Location = new Point(1278, 579);
                lblDowntimeHeading2.Location = new Point(1255, 553);
                lblRawHeading2.Location = new Point(792, 553);
                lblRawTons2.Location = new Point(808, 579);
                lblRunHeading2.Location = new Point(697, 553);
                lblRunHrs2.Location = new Point(707, 579);
                lblSchHeading2.Location = new Point(570, 553);
                lblSchRunMins2.Location = new Point(597, 579);
                lblAvailHeading2.Location = new Point(1087, 553);
                lblAvail2.Location = new Point(1104, 579);


            }
        }

        private void editToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Edit_Scheduled_Maintenance editMaint = new Edit_Scheduled_Maintenance();
            editMaint.Show();
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
