// Declare all of our uses from outside libraries and namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Forms;

//Define our own namespace and populate it with BGLib Events
namespace BLEHeartRateCollector
{
    //Link to Form1 Designer 
    public partial class Form1 : Form
    {


        //Create an instance of BGLib 


        //Various identifiers to initialize
        public Boolean isAttached = false;
        //Text box input from the GUI
        public string user_input;
        private const string hexDigits = "0123456789ABCDEF";
        //Save the handler to make it globally accessible
        public Byte connection_handler = 0;
        //Flag for determining if our prepare wrtie was executed
        public int prepare_flag = 0;
        //This is a counter to keep track of when we are able to run an Execute write 
        public int route_counter = 0;
        public int write_GO = 0;
        public int execute_write, execute_done = 0;
        public int started = 0;
        public int selector = 0;
        public UInt16 T = 0;
        public byte[] array, newArray;
        public int MTU_less = 0;
        public byte ASIC_Status = 0;
        public int status_notified = 0;


        //Plucks out the available COMS ports for the user to attach to
        public Dictionary<string, string> portDict = new Dictionary<string, string>();



        /* ================================================================ */
        /*                BEGIN MAIN EVENT-DRIVEN APP LOGIC                 */
        /* ================================================================ */

        //State possibilities
        public const UInt16 STATE_STANDBY = 0;
        public const UInt16 STATE_SCANNING = 1;
        public const UInt16 STATE_CONNECTING = 2;
        public const UInt16 STATE_FINDING_SERVICES = 3;
        public const UInt16 STATE_FINDING_ATTRIBUTES = 4;
        public const UInt16 STATE_LISTENING_MEASUREMENTS = 5;
        public const UInt16 STATE_WAITING_ACTION = 6;
        public const UInt16 STATE_WRITING_DATA = 7;
        public const UInt16 BEGIN_PREAPRE_WRITES = 8;
        public const UInt16 READY_TO_WRITE = 9;
        public const UInt16 READY_TO_READ_BACK = 10;

        public UInt16 app_state = STATE_STANDBY;        // current application state for the first run through 
        public Byte connection_handle = 0;              // connection handle (will always be 0 if only one connection happens at a time)
        public UInt16 att_handlesearch_start = 0;       // "start" handle holder during search
        public UInt16 att_handlesearch_end = 0;         // "end" handle holder during search
        public UInt16 att_handle_measurement = 0;       // heart rate measurement attribute handle
        public UInt16 att_handle_measurement_ccc = 0;   // heart rate measurement client characteristic configuration handle (to enable notifications)
        public UInt16 att_handle_ASIC_status = 0;
       

        // for master/scanner devices, the "gap_scan_response" event is a common entry-like point
        // this filters ad packets to find devices which advertise the Heart Rate service
        public void GAPScanResponseEvent(object sender, Bluegiga.BLE.Events.GAP.ScanResponseEventArgs e)
        {
            String log = String.Format("ble_evt_gap_scan_response: rssi={0}, packet_type={1}, sender=[ {2}], address_type={3}, bond={4}, data=[ {5}]" + Environment.NewLine,
                (SByte)e.rssi,
                e.packet_type,
                ByteArrayToHexString(e.sender),
                e.address_type,
                e.bond,
                ByteArrayToHexString(e.data)
                );

            Console.Write(log);
            ThreadSafeDelegate(delegate { txtLog.AppendText(log); });

            // pull all advertised service info from ad packet
            List<Byte[]> ad_services = new List<Byte[]>();
            Byte[] this_field = { };
            int bytes_left = 0;
            int field_offset = 0;
            for (int i = 0; i < e.data.Length; i++)
            {
                if (bytes_left == 0)
                {
                    bytes_left = e.data[i];
                    this_field = new Byte[e.data[i]];
                    field_offset = i + 1;
                }
                else
                {
                    this_field[i - field_offset] = e.data[i];
                    bytes_left--;
                    if (bytes_left == 0)
                    {
                        if (this_field[0] == 0x02 || this_field[0] == 0x03)
                        {
                            // partial or complete list of 16-bit UUIDs
                            ad_services.Add(this_field.Skip(1).Take(2).Reverse().ToArray());
                        }
                        else if (this_field[0] == 0x04 || this_field[0] == 0x05)
                        {
                            // partial or complete list of 32-bit UUIDs
                            ad_services.Add(this_field.Skip(1).Take(4).Reverse().ToArray());
                        }
                        else if (this_field[0] == 0x06 || this_field[0] == 0x07)
                        {
                            // partial or complete list of 128-bit UUIDs
                            ad_services.Add(this_field.Skip(1).Take(16).Reverse().ToArray());
                        }
                    }
                }
            }
            //Now we check if the sender address is equal to the Device that we want. The device address is set on the ATMEL SAMB11 side. ENSURE THESE TWO ARE THE SAME
            //It is in little endian format

            if (e.sender.SequenceEqual(new Byte[] { 0x7f, 0x7f, 0x6a, 0x11, 0x75, 0x13 }))
            {

                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("Found the SAMB11 Device!") + Environment.NewLine); });
                //Connect to the atmel device
                Byte[] cmd = bglib.BLECommandGAPConnectDirect(e.sender, e.address_type, 0x20, 0x30, 0x100, 0); // 125ms interval, 125ms window, active scanning
                                                                                                               // We send the connection request, no key pair, generic trust pair authentication occurs underneath user interface. Ghost write, handshake and return success.
                bglib.SendCommand(serialAPI, cmd);
                // update state to connecting
                app_state = STATE_CONNECTING;

            }
        }

        // the "connection_status" event occurs when a new connection is established
        public void ConnectionStatusEvent(object sender, Bluegiga.BLE.Events.Connection.StatusEventArgs e)
        {
            String log = String.Format("ble_evt_connection_status: connection={0}, flags={1}, address=[ {2}], address_type={3}, conn_interval={4}, timeout={5}, latency={6}, bonding={7}" + Environment.NewLine,
                e.connection,
                e.flags,
                ByteArrayToHexString(e.address),
                e.address_type,
                e.conn_interval,
                e.timeout,
                e.latency,
                e.bonding
                );

            //Save the connection handler for future use in executing events quickly. Since we will only be connected to one device at a time, we can rely on having a static connection identifier. It will usually be 0 if only one device is connected 
            connection_handler = e.connection;
            Console.Write(log);
            ThreadSafeDelegate(delegate { txtLog.AppendText(log); });

            //Check if the flags are indicative of connection formed. This is a BLE set flag.
            if ((e.flags & 0x05) == 0x05)
            {
                // connected, now perform service discovery
                connection_handle = e.connection;
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("Connected to {0}", ByteArrayToHexString(e.address)) + Environment.NewLine); });
                //Find the primary services
                Byte[] cmd = bglib.BLECommandATTClientReadByGroupType(e.connection, 0x0001, 0xFFFF, new Byte[] { 0x00, 0x28 }); // "service" UUID is 0x2800 (little-endian for UUID uint8array)
                                                                                                                                // DEBUG: display bytes written
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
                bglib.SendCommand(serialAPI, cmd);
                //while (bglib.IsBusy()) ;

                // update state
                app_state = STATE_FINDING_SERVICES;
            }
        }

        public void ATTClientGroupFoundEvent(object sender, Bluegiga.BLE.Events.ATTClient.GroupFoundEventArgs e)
        {
            String log = String.Format("ble_evt_attclient_group_found: connection={0}, start={1}, end={2}, uuid=[ {3}]" + Environment.NewLine,
                e.connection,
                e.start,
                e.end,
                ByteArrayToHexString(e.uuid)
                );

            Console.Write(log);
            ThreadSafeDelegate(delegate { txtLog.AppendText(log); });

            // Check for our GATT programming database. See if it is there.
            if (e.uuid.SequenceEqual(new Byte[] { 0xf7, 0xed, 0xf0, 0x2b, 0xcb, 0xed, 0x4b, 0x15, 0x88, 0xe6, 0x75, 0x75, 0xed, 0xab, 0x98, 0x80 }))
            {
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("Found attribute group for Primary service: start={0}, end=%d", e.start, e.end) + Environment.NewLine); });
                att_handlesearch_start = e.start;
                att_handlesearch_end = e.end;
            }    
        }

        public void ATTClientFindInformationFoundEvent(object sender, Bluegiga.BLE.Events.ATTClient.FindInformationFoundEventArgs e)
        {
            String log = String.Format("ble_evt_attclient_find_information_found: connection={0}, chrhandle={1}, uuid=[ {2}]" + Environment.NewLine,
                e.connection,
                e.chrhandle,
                ByteArrayToHexString(e.uuid)
                );
            Console.Write(log);
            ThreadSafeDelegate(delegate { txtLog.AppendText(log); });

            // Found the programming database. Grab the handle for the acctual characterisitic bin. 
            if (e.uuid.SequenceEqual(new Byte[] { 0xf7, 0xed, 0xf0, 0x2b, 0xcb, 0xed, 0x4b, 0x15, 0x88, 0xe6, 0x75, 0x75, 0xed, 0xab, 0x98, 0x80 }))
            {
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("Found Programming Attribute : handle={0}", e.chrhandle) + Environment.NewLine); });
                att_handle_measurement_ccc = e.chrhandle;
            }
            if(e.uuid.SequenceEqual(new Byte[] { 0xaa, 0xbb, 0xf0, 0x2b, 0xcb, 0xed, 0x4b, 0x15, 0x88, 0xe6, 0x75, 0x75, 0xed, 0xab, 0x98, 0x80 }))
            {
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("Found ASIC Status Attribute : handle={0}", e.chrhandle) + Environment.NewLine); });
                att_handle_ASIC_status = e.chrhandle;

            }
            app_state = READY_TO_WRITE;
        }

        public void ATTClientProcedureCompletedEvent(object sender, Bluegiga.BLE.Events.ATTClient.ProcedureCompletedEventArgs e)
        {
            String log = String.Format("ble_evt_attclient_procedure_completed: connection={0}, result={1}, chrhandle={2}" + Environment.NewLine,
                e.connection,
                e.result,
                e.chrhandle
                );
            Console.Write(log);
            ThreadSafeDelegate(delegate { txtLog.AppendText(log); });

            if(app_state == READY_TO_WRITE & started == 0)
            {
                started = 1;
                array = HexStringToBytes(user_input);
                newArray = new byte[18];
                Array.Copy(array, 0, newArray, 0, 18);
                selector = 0;
                T = Convert.ToUInt16(selector);
                Byte[] cmd_int = bglib.BLECommandATTClientPrepareWrite(connection_handler, att_handle_measurement_ccc, T, newArray);
                bglib.SendCommand(serialAPI, cmd_int);
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("First Section Run") + Environment.NewLine); });
 
            }

            if(started == 1 && prepare_flag ==1 && route_counter <=5)
            {
                prepare_flag = 0;
                //Increae offset by 0x14, dec = 20 bytes

                selector = selector + 18;
             
                T = Convert.ToUInt16(selector);
                if (route_counter != 5)
                {
                    Array.Copy(array, selector, newArray, 0, 18);
                }
                else
                {
                    byte[] last_cpy = new byte[array.Length - selector];
                    Array.Copy(array, selector, last_cpy, 0, array.Length  - selector);
                    newArray = last_cpy;
                }
                Byte[] cmd_int = bglib.BLECommandATTClientPrepareWrite(connection_handler, att_handle_measurement_ccc, T, newArray);
                bglib.SendCommand(serialAPI, cmd_int);
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("Running Sequences") + Environment.NewLine); });
            }

            if(started == 1 && prepare_flag == 1 && route_counter == 6)
            {
                prepare_flag = 0;
                Byte[] cmd = bglib.BLECommandATTClientExecuteWrite(connection_handler, 1);
                while (bglib.IsBusy()) ;
                // DEBUG: display bytes written
                //ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
                // bglib.SendCommand(serialAPI, cmd);
               // cmd = bglib.BLECommandATTClientReadLong(connection_handle, att_handle_measurement_ccc);
                // DEBUG: display bytes written
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
                bglib.SendCommand(serialAPI, cmd);
            }

            if(execute_write == 1)
            {
                execute_write = 0;
                Read_Server(att_handle_measurement_ccc);
            }
        
            if(MTU_less == 1 && status_notified == 1)
            {
                //Reset();
                ThreadSafeDelegate(delegate { txtLog.AppendText("Reading the Service handler" + Environment.NewLine); });
                Read_Server(att_handle_ASIC_status);
            }

            if(MTU_less == 1 && ASIC_Status == 1)
            {
                Reset();
            }

            ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> APP+STATE({0})", app_state) + Environment.NewLine); });

            // check if we just finished searching for services
            if (app_state == STATE_FINDING_SERVICES)
            {
                if (att_handlesearch_end > 0)
                {
                    // found the Heart Rate service, so now search for the attributes inside
                    Byte[] cmd = bglib.BLECommandATTClientFindInformation(e.connection, att_handlesearch_start, att_handlesearch_end);
                    // DEBUG: display bytes written
                    ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
                    bglib.SendCommand(serialAPI, cmd);
                    //while (bglib.IsBusy()) ;

                    // update state
                    app_state = STATE_FINDING_ATTRIBUTES;
                }
                else
                {
                    ThreadSafeDelegate(delegate { txtLog.AppendText("Could not find 'Heart Rate' service with UUID 0x180D" + Environment.NewLine); });
                }
            }

        }

        public void BLEResponseATTClientPrepareWrite(object sender, Bluegiga.BLE.Responses.ATTClient.PrepareWriteEventArgs e)
        {
            if (e.result == 0)
            {
                prepare_flag = 1;
                route_counter++;
                ThreadSafeDelegate(delegate { txtLog.AppendText("Prepare Event Executed Sucessfully" + Environment.NewLine); });
            }

            else
            {
                prepare_flag = 0;
                ThreadSafeDelegate(delegate { txtLog.AppendText("Something is not right with the Prepare sequence" + Environment.NewLine); });

            }
        }

        public void BLEResponseATTClientExecuteWrite(object sender, Bluegiga.BLE.Responses.ATTClient.ExecuteWriteEventArgs e)
        {
            if (e.result == 0)
            {
                execute_write = 1;
                ThreadSafeDelegate(delegate { txtLog.AppendText("Execute Write Completed" + Environment.NewLine); });

            }

            else
            {
                
                ThreadSafeDelegate(delegate { txtLog.AppendText("ERROR! Execute Write Failed!" + Environment.NewLine); });
            }

        }

        public void ATTClientAttributeValueEvent(object sender, Bluegiga.BLE.Events.ATTClient.AttributeValueEventArgs e)
        {
            String log = String.Format("ble_evt_attclient_attribute_value: connection={0}, atthandle={1}, type={2}, value=[ {3}]" + Environment.NewLine,
                e.connection,
                e.atthandle,
                e.type,
                ByteArrayToHexString(e.value)
                );
                 GATT_Server_Read_Data_Recieved(e,log);
        }

        //  public void BLEResponseATTClientReadByHandle(object sender, Bluegiga.BLE.Responses.ATTClient.ReadByHandleEventArgs e)
        // {
        // ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("GATT SERVER is accessed ") + Environment.NewLine); });
        //  }

        /* ================================================================ */
        /*                 END MAIN EVENT-DRIVEN APP LOGIC                  */
        /* ================================================================ */
        public void GATT_Server_Read_Data_Recieved(Bluegiga.BLE.Events.ATTClient.AttributeValueEventArgs e, String log)
        {
            if (e.type == 1 & e.atthandle == att_handle_measurement_ccc)
            {
                ThreadSafeDelegate(delegate { txtLog.AppendText("Programming Data was Written -- TX SAMB11!" + Environment.NewLine); });

            }

            else if (e.type == 1 & e.atthandle == att_handle_ASIC_status)
            {
                status_notified = 1;
                ThreadSafeDelegate(delegate { txtLog.AppendText("Notification on the ASIC Programming UART" + Environment.NewLine); });
            }
            else if (e.type == 4 & e.atthandle == att_handle_ASIC_status)
            {
                Console.Write(log);
                ThreadSafeDelegate(delegate { txtLog.AppendText(log); });
                // display actual measurement
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("ASIC STATUS: {0} ", ByteArrayToHexString(e.value)) + Environment.NewLine); });

               // if (e.value.SequenceEqual(new Byte[] { 0x00,0x00,0x00,0x01}) )
               // {
                    ASIC_Status = 1;
                //  }
                // if (e.value.SequenceEqual(new Byte[] { 0x00, 0x00, 0x00, 0x0E }))
                // {
                //    ASIC_Status = 1;
                ThreadSafeDelegate(delegate { txtLog.AppendText("ASIC READ" + Environment.NewLine); });
                //  }
            }

            else if (e.type == 4 & e.atthandle == att_handle_measurement_ccc)
            {
                if ((e.value.Length) != 22)
                {
                    MTU_less = 1;
                }

                Console.Write(log);
                ThreadSafeDelegate(delegate { txtLog.AppendText(log); });
                // display actual measurement
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("PROG DATA: {0} ", ByteArrayToHexString(e.value)) + Environment.NewLine); });
            }

        }
    



        public void Reset()
        {
            status_notified = 0;
            selector = 0;
            prepare_flag = 0;
            execute_write = 0;
            started = 0;
            route_counter = 0;
            execute_write = 0;
            MTU_less = 0;
            ASIC_Status = 0;
            app_state = STATE_STANDBY;
            ThreadSafeDelegate(delegate { txtLog.AppendText("Closing Connection" + Environment.NewLine); });
            Byte[] cmd;

            // disconnect if connected
            cmd = bglib.BLECommandConnectionDisconnect(connection_handler);
            bglib.SendCommand(serialAPI, cmd);
        }

        public void Read_Server(UInt16 attrib_handle)
        {
            Byte[] cmd = bglib.BLECommandATTClientReadLong(connection_handle, attrib_handle);
            ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
            bglib.SendCommand(serialAPI, cmd);
        }

        // Thread-safe operations from event handlers
        public void ThreadSafeDelegate(MethodInvoker method)
        {
            if (InvokeRequired)
                BeginInvoke(method);

            else
                method.Invoke();
        }

        // Convert byte array to "00 11 22 33 44 55 " string
        public string ByteArrayToHexString(Byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2} ", b);
            return hex.ToString();
        }

        // Serial port event handler for a nice event-driven architecture
        private void DataReceivedHandler(
                                object sender,
                                System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            System.IO.Ports.SerialPort sp = (System.IO.Ports.SerialPort)sender;
            Byte[] inData = new Byte[sp.BytesToRead];

            // read all available bytes from serial port in one chunk
            sp.Read(inData, 0, sp.BytesToRead);

            // DEBUG: display bytes read
            ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("<= RX ({0}) [ {1}]", inData.Length, ByteArrayToHexString(inData)) + Environment.NewLine); });

            // parse all bytes read through BGLib parser
            for (int i = 0; i < inData.Length; i++)
            {
                bglib.Parse(inData[i]);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // initialize list of ports
            btnRefresh_Click(sender, e);

            // initialize COM port combobox with list of ports
            comboPorts.DataSource = new BindingSource(portDict, null);
            comboPorts.DisplayMember = "Value";
            comboPorts.ValueMember = "Key";

            // initialize serial port with all of the normal values (should work with BLED112 on USB)
            serialAPI.Handshake = System.IO.Ports.Handshake.RequestToSend;
            serialAPI.BaudRate = 115200;
            serialAPI.DataBits = 8;
            serialAPI.StopBits = System.IO.Ports.StopBits.One;
            serialAPI.Parity = System.IO.Ports.Parity.None;
            serialAPI.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(DataReceivedHandler);

            // initialize BGLib events we'll need for this script
            bglib.BLEEventGAPScanResponse += new Bluegiga.BLE.Events.GAP.ScanResponseEventHandler(this.GAPScanResponseEvent);
            bglib.BLEEventConnectionStatus += new Bluegiga.BLE.Events.Connection.StatusEventHandler(this.ConnectionStatusEvent);
            bglib.BLEEventATTClientGroupFound += new Bluegiga.BLE.Events.ATTClient.GroupFoundEventHandler(this.ATTClientGroupFoundEvent);
            bglib.BLEEventATTClientFindInformationFound += new Bluegiga.BLE.Events.ATTClient.FindInformationFoundEventHandler(this.ATTClientFindInformationFoundEvent);
            bglib.BLEEventATTClientProcedureCompleted += new Bluegiga.BLE.Events.ATTClient.ProcedureCompletedEventHandler(this.ATTClientProcedureCompletedEvent);
            bglib.BLEEventATTClientAttributeValue += new Bluegiga.BLE.Events.ATTClient.AttributeValueEventHandler(this.ATTClientAttributeValueEvent);
            bglib.BLEResponseATTClientPrepareWrite += new Bluegiga.BLE.Responses.ATTClient.PrepareWriteEventHandler(this.BLEResponseATTClientPrepareWrite);
            bglib.BLEResponseATTClientExecuteWrite += new Bluegiga.BLE.Responses.ATTClient.ExecuteWriteEventHandler(this.BLEResponseATTClientExecuteWrite);

        }
        
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // get a list of all available ports on the system
            portDict.Clear();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SerialPort");
                //string[] ports = System.IO.Ports.SerialPort.GetPortNames();
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    portDict.Add(String.Format("{0}", queryObj["DeviceID"]), String.Format("{0} - {1}", queryObj["DeviceID"], queryObj["Caption"]));
                }
            }
            catch (ManagementException ex)
            {
                portDict.Add("0", "Error " + ex.Message);
            }
        }

        private void btnAttach_Click(object sender, EventArgs e)
        {
            if (!isAttached)
            {
                txtLog.AppendText("Opening serial port '" + comboPorts.SelectedValue.ToString() + "'..." + Environment.NewLine);
                serialAPI.PortName = comboPorts.SelectedValue.ToString();
                serialAPI.Open();
                txtLog.AppendText("Port opened" + Environment.NewLine);
                isAttached = true;
                btnAttach.Text = "Detach";
                btnGo.Enabled = true;
                btnReset.Enabled = true;
            }
            else
            {
                txtLog.AppendText("Closing serial port..." + Environment.NewLine);
                serialAPI.Close();
                txtLog.AppendText("Port closed" + Environment.NewLine);
                isAttached = false;
                btnAttach.Text = "Attach";
                btnGo.Enabled = false;
                btnReset.Enabled = false;
            }
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            user_input = richTextBox1.Text;
            // start the scan/connect process now
            Byte[] cmd;
            app_state = STATE_FINDING_SERVICES;
            // set scan parameters
            cmd = bglib.BLECommandGAPSetScanParameters(0xC8, 0xC8, 1); // 125ms interval, 125ms window, active scanning
                                                                       // DEBUG: display bytes read
            ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
            bglib.SendCommand(serialAPI, cmd);
            //while (bglib.IsBusy()) ;

            // begin scanning for BLE peripherals
            cmd = bglib.BLECommandGAPDiscover(1); // generic discovery mode
                                                  // DEBUG: display bytes read
            ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
            bglib.SendCommand(serialAPI, cmd);
            //while (bglib.IsBusy()) ;


            // disable "GO" button since we already started, and sending the same commands again sill not work right
            btnGo.Enabled = false;

   
            
        }

        public static byte[] HexStringToBytes(string str)
        {
            // Determine how many bytes there are.     


            byte[] bytes = new byte[str.Length >> 1];
            for (int i = 0; i < str.Length; i += 2)
            {
                int highDigit = hexDigits.IndexOf(Char.ToUpperInvariant(str[i]));
                int lowDigit = hexDigits.IndexOf(Char.ToUpperInvariant(str[i + 1]));
                if (highDigit == -1 || lowDigit == -1)
                {
                    throw new ArgumentException("The string contains an invalid digit.", "s");
                }
                bytes[i >> 1] = (byte)((highDigit << 4) | lowDigit);
            }
            return bytes;
        }

        

        private void btnReset_Click(object sender, EventArgs e)
        {
            // stop everything we're doing, if possible
            Byte[] cmd;
            // stop scanning if scanning
            cmd = bglib.BLECommandGAPEndProcedure();
            // DEBUG: display bytes read
            ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
            bglib.SendCommand(serialAPI, cmd);
            //while (bglib.IsBusy()) ;

            // stop advertising if advertising
            cmd = bglib.BLECommandGAPSetMode(0, 0);
            // DEBUG: display bytes read
            ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
            bglib.SendCommand(serialAPI, cmd);
            //while (bglib.IsBusy()) ;

            // enable "GO" button to allow them to start again
            btnGo.Enabled = true;

            // update state
            app_state = STATE_STANDBY;
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {

        }

        private void lblPorts_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboPorts_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void send_data_Click(object sender, EventArgs e)
        {


        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ThreadSafeDelegate(delegate { txtLog.AppendText("State = Finding Attr." + Environment.NewLine); });
            if (att_handle_measurement_ccc > 0)
            {
                // found the measurement + client characteristic configuration, so enable notifications
                // (this is done by writing 0x0001 to the client characteristic configuration attribute)
                Byte[] cmd = bglib.BLECommandATTClientReadLong(connection_handle, att_handle_measurement_ccc);
                // DEBUG: display bytes written
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
                bglib.SendCommand(serialAPI, cmd);
                //while (bglib.IsBusy()) ;

                // update state
                app_state = STATE_WAITING_ACTION;
                ThreadSafeDelegate(delegate { txtLog.AppendText("ECIT" + Environment.NewLine); });
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // stop everything we're doing, if possible
            Byte[] cmd;

            // disconnect if connected
            cmd = bglib.BLECommandConnectionDisconnect(connection_handler);
            bglib.SendCommand(serialAPI, cmd);
        }
    }
}
