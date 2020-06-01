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
    class OneMinuteData
    {
        SQLDataHandler dh;
        //SqlDataReader dr;
        private const int DataTimeout = 1000;

        public void readdata()
        {


            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();
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
                var tag = new Tag("10.14.6.100", "1, 0", CpuType.LGX, "REPORT_INT[0]", 2, 15, 0);
                var tag1 = new Tag("10.14.6.100", "1, 0", CpuType.LGX, "REPORT_FLOAT[0]", 4, 14, 0);

                using (var client = new Libplctag())
                {
                    // add the tag
                    client.AddTag(tag);
                    client.AddTag(tag1);
                    //client.AddTag(tag2);
                    // check that the tag has been added, if it returns pending we have to retry
                    while (client.GetStatus(tag) == Libplctag.PLCTAG_STATUS_PENDING)
                    {
                        Thread.Sleep(100);
                    }
                    // check that the tag has been added, if it returns pending we have to retry
                    while (client.GetStatus(tag1) == Libplctag.PLCTAG_STATUS_PENDING)
                    {
                        Thread.Sleep(100);
                    }


                    // if the status is not ok, we have to handle the error
                    if (client.GetStatus(tag) != Libplctag.PLCTAG_STATUS_OK)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                        {
                            file.WriteLine("Error occured at: " + DateTime.Now + Convert.ToString((client.GetStatus(tag))) + ("\n" + $"Five Minute Data tag 5 Read Error setting up tag internal state. Error{ client.DecodeError(client.GetStatus(tag))}\n"));
                        }
                        return;
                    }
                    // if the status is not ok, we have to handle the error
                    if (client.GetStatus(tag1) != Libplctag.PLCTAG_STATUS_OK)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                        {
                            file.WriteLine("Error occured at: " + DateTime.Now + Convert.ToString((client.GetStatus(tag1))) + ("\n" + $"Five Minute Data tag 5 Read Error setting up tag internal state. Error{ client.DecodeError(client.GetStatus(tag1))}\n"));
                        }
                        return;
                    }


                    // Execute the read
                    var result = client.ReadTag(tag, DataTimeout);
                    // Execute the read
                    var result1 = client.ReadTag(tag1, DataTimeout);
                    // Execute the read
                    //var result2 = client.ReadTag(tag2, DataTimeout);

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

                    dh = new SQLDataHandler();
                    //dr = dh.getReader("Select schrunmins, shiftstarttime FROM ConfigurationSettings Where plantname = 'Kanawha Eagle';");
                    //dr.Read();
                    //int schrunmins = (int)dr["schrunmins"];


                    // Convert the data
                    var plantairpressure = client.GetInt16Value(tag, 0 * tag.ElementSize);
                    var thickenerunderflowpsi = client.GetInt16Value(tag, 1 * tag.ElementSize);
                    var thickenerrotationtorque = client.GetInt16Value(tag, 2 * tag.ElementSize);
                    var thickenerunderflowgpm = client.GetInt16Value(tag, 3 * tag.ElementSize);
                    var screenbowl1amps = client.GetInt16Value(tag, 4 * tag.ElementSize);
                    var screenbowl1torque = client.GetInt16Value(tag, 5 * tag.ElementSize);
                    var screenbowl2amps = client.GetInt16Value(tag, 6 * tag.ElementSize);
                    var screenbowl2torque = client.GetInt16Value(tag, 7 * tag.ElementSize);
                    var plantfeedtph = client.GetInt16Value(tag, 8 * tag.ElementSize);
                    var cleancoaltph = client.GetInt16Value(tag, 9 * tag.ElementSize);
                    var stokertph = client.GetInt16Value(tag, 10 * tag.ElementSize);
                    var bypasstph = client.GetInt16Value(tag, 11 * tag.ElementSize);
                    var refusetph = client.GetInt16Value(tag, 12 * tag.ElementSize);
                    var scalpedtph = client.GetInt16Value(tag, 13 * tag.ElementSize);
                    var slurrytph = client.GetInt16Value(tag, 14 * tag.ElementSize);


                    var thickenerunderflowsg = client.GetFloat32Value(tag1, 0 * tag1.ElementSize);
                    var thickenerrotationamps = client.GetFloat32Value(tag1, 1 * tag1.ElementSize);
                    //var hmvsg = client.GetFloat32Value(tag1, 2 * tag1.ElementSize);
                    //var hmvsgsetpoint = client.GetFloat32Value(tag1, 3 * tag1.ElementSize);
                    var hmcyclonesg = client.GetFloat32Value(tag1, 4 * tag1.ElementSize);
                    var hmcyclone1sgsetpoint = client.GetFloat32Value(tag1, 5 * tag1.ElementSize);
                    var yield = client.GetFloat32Value(tag1, 6 * tag1.ElementSize);
                    var hmcyclone1psi = client.GetFloat32Value(tag1, 7 * tag1.ElementSize);
                    var hmcyclone2psi = client.GetFloat32Value(tag1, 8 * tag1.ElementSize);
                    var deslimecyclonepsi = client.GetFloat32Value(tag1, 9 * tag1.ElementSize);
                    var ccclasscyclonepsi = client.GetFloat32Value(tag1, 10 * tag1.ElementSize);
                    var rawcoalcyclonepsi = client.GetFloat32Value(tag1, 11 * tag1.ElementSize);
                    var magscrewtotalrunmins = client.GetFloat32Value(tag1, 12 * tag1.ElementSize);
                    var plantfeedtotalrunmins = client.GetFloat32Value(tag1, 13 * tag1.ElementSize);


                    try
                    {
                        dh.executeSql("Insert INTO TrendingData(created_at, plantair, thickenerunderpsi, thickenertorque, thickenerunderflow, thickenergravity, thickeneramps, screenbowl1amps, " +
                            "screenbowl1torque, screenbowl2amps, screenbowl2torque, hmcyclonesg1, hmcyclone1sgsetpoint, hmcyclone1psi, " +
                            "hmcyclone2psi, deslimecyclonepsi, ccclasscyclonepsi, rawcoalcyclonepsi, yield, plantfeedtph, cleantph, stokertph, bypasstph, reftph, scalpedtph, " +
                            "slurrytph) VALUES (GETDATE(), '" + plantairpressure + "', '" + thickenerunderflowpsi + "', '" + thickenerrotationtorque + "', '" + thickenerunderflowgpm + "', '" + thickenerunderflowsg + "', '" + thickenerrotationamps +
                            "', '" + screenbowl1amps + "', '" + screenbowl1torque + "', '" + screenbowl2amps + "', '" + screenbowl2torque + "', '" + hmcyclonesg + "', '" + hmcyclone1sgsetpoint +
                            "', '" + hmcyclone1psi + "', '" + hmcyclone2psi + "', '" + deslimecyclonepsi + "', '" + ccclasscyclonepsi + "', '" + rawcoalcyclonepsi + "', '" + yield +
                            "', '" + plantfeedtph + "', '" + cleancoaltph + "', '" + stokertph + "', '" + bypasstph + "', '" + refusetph + "', '" + scalpedtph + "', '" + slurrytph + "')");
                    }
                    catch (SqlException se)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                        {
                            file.WriteLine("Error occurred at one minute data insert to database on: " + DateTime.Now + " \r\n" + se + "\r\n");
                        }
                        return;
                    }
                    client.RemoveTag(tag);
                    client.RemoveTag(tag1);
                }
            }
            catch (Exception ex)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\KE_PLCData\Documents\ReportErrorLogging\ErrorLog.txt", true))
                {
                    file.WriteLine("Error occurred at one minute data plc connection on: " + DateTime.Now + " \r\n" + ex + "\r\n");
                }
                return;
            }

        }
    }
}
