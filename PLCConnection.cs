using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibplctagWrapper;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;

namespace BlackHawk_Reporting
{
    public class PLCConnection
    {

        private const int DataTimeout = 5000;
        Int16 downTimeBit = -5;
        DownTimeMonitoring dm;

        public void createConnection(string tagname, int bytesize, int numretrieved)
        {
            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();
            try
            {

                dm = new DownTimeMonitoring();
                // creates a tag to read B3:0, 1 item, from LGX ip address 192.168.0.100
                // The last entry in this new tag is the element count.  It is currently
                // set to 1
                //public Tag(string ipAddress, string path, CpuType cpuType, string name, int elementSize, int elementCount, int debugLevel = 0)
                //string name is the textual name of the tag in plc
                //elementSize is the size of the element in bytes
                //elementCount elements count: 1- single, n-array
                //public Tag(string ipAddress, string path, CpuType cpuType, string name, int elementSize, int elementCount, int debugLevel = 0)
                var tagstatus = new Tag("10.14.6.100", "1, 0", CpuType.LGX, tagname, bytesize, numretrieved);

                using (var client = new Libplctag())
                {
                    // add the tag
                    client.AddTag(tagstatus);

                    // check that the tag has been added, if it returns pending we have to retry
                    while (client.GetStatus(tagstatus) == Libplctag.PLCTAG_STATUS_PENDING)
                    {
                        Thread.Sleep(100);
                    }


                    // if the status is not ok, we have to handle the error
                    if (client.GetStatus(tagstatus) != Libplctag.PLCTAG_STATUS_OK)
                    {
                        setDownbit(-5);
                        return;
                    }



                    // Execute the read
                    var result = client.ReadTag(tagstatus, DataTimeout);

                    Int16 checkError;

                    // Check the read operation result
                    if (result != Libplctag.PLCTAG_STATUS_OK)
                    {
                        setDownbit(-5);
                        return;
                    }
                    else
                    {
                        checkError = client.GetInt16Value(tagstatus, 0 * tagstatus.ElementSize);
                        setDownbit(checkError);
                    }

                    client.RemoveTag(tagstatus);
                    
                }
            }
            catch(Exception ex)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"c:\Users\KE_PLCData\Report Error Logging\ErrorLog.txt", true))
                {
                    file.WriteLine("Error occurred with plc connection: " + DateTime.Now + " \r\n" + ex + "\r\n");
                }
                return;
            }
        }

        public Int16 getDownbit()
        {
            return downTimeBit;
        }

        public void setDownbit(Int16 value)
        {
            downTimeBit = value;
        }


    }
}
