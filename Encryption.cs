using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace BlackHawk_Reporting
{
    class Encryption
    {
        string cstring = "Server=DESKTOP-QDF9A1Q\\BHPP_REPORTING;Database=BHCCReport;User id=sa;Password=BHReporting;";
        //readonly string cstring = "Server=DESKTOP-CL05KBC;Database=BHCCReport;User id=sa;Password=BHReporting;";
        //readonly string cstring = "Data Source=DESKTOP-VQBH5C1\\SQLEXPRESS;Initial Catalog=KanawhaEagleDB;User id=sa;Password=BHReporting;";

        string savedPasswordHash;
        SqlConnection con;
        SqlCommand cmd;


        public string hashPW(string password)
        {
            con = new SqlConnection(cstring);
            //hash and salt
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            //hash and salt password
            var pdkdf2 = new Rfc2898DeriveBytes(password, salt, 1000);

            //place the string in the byte array
            byte[] hash = pdkdf2.GetBytes(20);

            //make new byte array to store the hashed password and salt
            byte[] hashBytes = new byte[36];

            //place the hash and salt in their respective places
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            //now convert the byte array to a string
            savedPasswordHash = Convert.ToBase64String(hashBytes);

            return savedPasswordHash;
        }

        public int checkpass(byte[] hbytes, string pass)
        {
            con = new SqlConnection(cstring);
            byte[] salt = new byte[16];
            Array.Copy(hbytes, 0, salt, 0, 16);
            var pbkdf2 = new Rfc2898DeriveBytes(pass, salt, 1000);
            byte[] hash = pbkdf2.GetBytes(20);

            int ok = 1;
            for (int i = 0; i < 20; i++)
            {
                if (hbytes[i + 16] != hash[i])
                {
                    ok = 0;
                }
            }
            return ok;
        }


        public int insertUser(string name, string pass, string email, string etype, string userid, string plant)
        {
            con = new SqlConnection(cstring);
            int attempts = 0;
            int islocked = 0;
            savedPasswordHash = hashPW("Rcyfers11");
            SqlParameter param1 = new SqlParameter();
            SqlParameter param2 = new SqlParameter();
            SqlParameter param3 = new SqlParameter();
            SqlParameter param4 = new SqlParameter();
            SqlParameter param5 = new SqlParameter();
            SqlParameter param6 = new SqlParameter();
            SqlParameter param7 = new SqlParameter();
            SqlParameter param8 = new SqlParameter();

            param1.ParameterName = "@Name";
            param1.Value = name;
            param2.ParameterName = "@Pass";
            param2.Value = pass;
            param4.ParameterName = "@email";
            param4.Value = email;
            param5.ParameterName = "@etype";
            param5.Value = etype;
            param3.ParameterName = "@usrname";
            param3.Value = userid;
            param6.ParameterName = "@attempts";
            param6.Value = attempts;
            param7.ParameterName = "@islocked";
            param7.Value = islocked;
            param8.ParameterName = "@plant";
            param8.Value = plant;

            cmd = new SqlCommand("Insert Into Employee (usrname, ename, etype, ehashpw, usremail, retryattempts, IsLocked, associatedplant) VALUES " +
                "(@usrname, @Name, @etype, @Pass, @email, @attempts, @islocked, @plant)", con);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Pass", pass);
            cmd.Parameters.AddWithValue("@usrname", userid);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@etype", etype);
            cmd.Parameters.AddWithValue("@attempts", attempts);
            cmd.Parameters.AddWithValue("@islocked", islocked);
            cmd.Parameters.AddWithValue("@plant", plant);
            con.Open();
            int RowsAffected = cmd.ExecuteNonQuery();
            con.Close();
            return RowsAffected;
        }
    }
}
