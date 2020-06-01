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
    class SQLDataHandler
    {
        readonly string cstring = "Data Source=DESKTOP-QDF9A1Q\\BHPP_REPORTING;Initial Catalog=BHCCReport;User id=sa;Password=BHReporting;";
        //readonly string cstring = "Server=DESKTOP-CL05KBC;Database=BHCCReport;User id=sa;Password=BHReporting;";
        //readonly string cstring = "Data Source=192.168.1.23, 1433;Network Library=DBMSSOCN;Initial Catalog=KanawhaEagleDB;User id=sa;Password=BHReporting;";
        //readonly string cstring = "Data Source=DESKTOP-VQBH5C1\\SQLEXPRESS;Initial Catalog=KanawhaEagleDB;User id=sa;Password=BHReporting;";
        //readonly string cstring = "Data Source=LAPTOP\\BHREPORTING;Initial Catalog=BHCCReport;User id=sa;Password=BHReporting;";

        private SqlDataAdapter adpt;
        private SqlConnection cn;
        private SqlCommand cmd;
        private DataTable dt;

        //public String connect()
        //{
        //    String connect = "";
        //    try
        //    {
        //        string startupPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)));
        //        string subpath = startupPath.Substring(6, startupPath.Length - 6);
        //        string path = subpath + "\\connection.txt";
        //        StreamReader reader = new StreamReader(path);
        //        connect = cstring;
        //    }
        //    catch (Exception ee)
        //    { }
        //    return connect;
        //}

        public DataTable getTable(String s)
        {
            DataTable dt;
            SqlDataAdapter da;
            //cstring = connect();
            cn = new SqlConnection(cstring);
            cmd = new SqlCommand(s, cn);
            dt = new DataTable();
            da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public SqlDataReader getReader(String s)
        {
            SqlDataReader dr;
            //cstring = connect();
            cn = new SqlConnection(cstring);
            cmd = new SqlCommand(s, cn);
            cn.Open();
            dr = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection); //closes SqlConnection after execution
            return dr;
        }

        public Object getScalar(String s)
        {
            Object o;
            //cstring = connect();
            cn = new SqlConnection(cstring);
            cmd = new SqlCommand(s, cn);
            cn.Open();
            o = cmd.ExecuteScalar();
            cn.Close();
            return o;
        }

        public void executeSql(String s)
        {
            //cstring = connect();
            cn = new SqlConnection(cstring);
            cmd = new SqlCommand(s, cn);
            cn.Open();
            cmd.ExecuteNonQuery();
            cn.Close();
        }

        public void showData(DataGridView dv, String s)
        {
            cn = new SqlConnection(cstring);
            adpt = new SqlDataAdapter(s, cn);
            dt = new DataTable();
            adpt.Fill(dt);
            dv.DataSource = dt;
            cn.Close();
        }
    }
}

