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
    public partial class ConfigVerification : Form
    {
        string cstring = "Server=DESKTOP-QDF9A1Q\\BHPP_REPORTING;Database=BHCCReport;User id=sa;Password=BHReporting;";
        //string cstring = "Server=DESKTOP-CL05KBC;Database=BHCCReport;User id=sa;Password=BHReporting;";
        //string cstring = "Server=DESKTOP-VQBH5C1\\SQLEXPRESS;Database=KanawhaEagleDB;User id=sa;Password=BHReporting;";

        SQLDataHandler dh;
        SqlDataReader dr;
        SqlCommand cmd;
        SqlConnection con;
        String txtpasshash;
        String dbpass;
        int checkpw;
        Encryption en;
        int etype;

        public ConfigVerification()
        {
            InitializeComponent();
        }

        private void ConfigVerification_Load(object sender, EventArgs e)
        {
            lblErrorMsg.Visible = false;
            btnConfirm.Visible = false;
            btnConfirm.Enabled = false;
            dh = new SQLDataHandler();
            en = new Encryption();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            con = new SqlConnection(cstring);
            string uname = txtUName.Text;
            string pword = txtPword.Text;
            txtpasshash = en.hashPW(pword);


            SqlParameter param = new SqlParameter();
            param.ParameterName = "@usrname";
            param.Value = uname;
            cmd = new SqlCommand("Select * from Employee where usrname = @usrname", con);
            cmd.Parameters.AddWithValue("@usrname", uname);
            con.Open();
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                dbpass = (String)dr["ehashpw"];
                byte[] hashBytes = Convert.FromBase64String(dbpass);
                checkpw = en.checkpass(hashBytes, pword);
            }
            dr.Close();
            con.Close();

            if (!checkpw.Equals(1) || pword == "")
            {
                lblErrorMsg.Visible = true;
                btnConfirm.Visible = true;
                btnConfirm.Enabled = true;
                btnSubmit.Enabled = false;
                lblErrorMsg.Text = "Incorrect username or password";
                lblErrorMsg.BackColor = Color.Red;
                txtUName.Text = "";
                txtPword.Text = "";

                return;
            }


            cmd = new SqlCommand("Select * from Employee where usrname = @usrname", con);
            cmd.Parameters.AddWithValue("@usrname", uname);
            con.Open();
            dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                etype = Convert.ToInt32(dr["etype"]);
            }
            dr.Close();
            con.Close();
            switch (etype)
            {
                case 1:
                    Configuration_Form1 config = new Configuration_Form1();
                    config.Show();
                    this.Close();
                    break;
                case 2:
                    lblErrorMsg.Visible = true;
                    btnConfirm.Visible = true;
                    btnConfirm.Enabled = true;
                    btnSubmit.Enabled = false;
                    lblErrorMsg.Text = "You do not have privileges to make \n software configuration changes!";
                    lblErrorMsg.BackColor = Color.Red;
                    txtUName.Text = "";
                    txtPword.Text = "";
                    break;
                case 3:
                    lblErrorMsg.Visible = true;
                    btnConfirm.Visible = true;
                    btnConfirm.Enabled = true;
                    btnSubmit.Enabled = false;
                    lblErrorMsg.Text = "You do not have privileges to make \n software configuration changes!";
                    lblErrorMsg.BackColor = Color.Red;
                    txtUName.Text = "";
                    txtPword.Text = "";
                    break;
            }

        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            lblErrorMsg.Visible = false;
            btnConfirm.Visible = false;
            btnConfirm.Enabled = false;
            btnSubmit.Enabled = true;
            lblErrorMsg.Text = "";

        }
    }
}
