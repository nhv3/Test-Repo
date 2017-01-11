// Declare all of our uses from outside libraries and namespaces
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Forms;

namespace BLEHeartRateCollector
{
    //Link to Form1 Designer 
    public partial class Form1 : Form
    {

        //Create an instance of BGLib 
        public Bluegiga.BGLib bglib = new Bluegiga.BGLib();
        public Hashtable myHT = new Hashtable();
        /*Define Global Variables */
        public Byte[] target_BLE_address = {0x7f, 0x7f, 0x6a, 0x11, 0x75, 0x13};
        public Boolean isAttached = false;
        public byte[] adv_data;
        public sbyte rssi;
        public string hex_data_input;
        private const string hexDigits = "0123456789ABCDEF";
        public Byte ble_connection_handle = 0;
        public int write_prepare_success = 0;
        public int write_queque_counter = 0;
        public int ready_to_write = 0;
        public int execute_write_success = 0;
        public int index_selector = 0;
        public byte[] hex_byte_array, temp_hex_byte_array_holder;
        public int MTU_less = 0;
        public byte ASIC_Status = 0;
        public int status_notified = 0;
        public Byte connection_handle = 0;              // connection handle (will always be 0 if only one connection happens at a time)
        public UInt16 att_handlesearch_start = 0;       // "start" handle holder during search
        public UInt16 att_handlesearch_end = 0;         // "end" handle holder during search
        public UInt16 att_handle_measurement = 0;       // heart rate measurement attribute handle
        public UInt16 att_handle_measurement_ccc = 0;   // heart rate measurement client characteristic configuration handle (to enable notifications)
        public UInt16 att_handle_ASIC_status = 0;
        public int attribute_handles_found = 0;
        //Plucks out the available COMS ports for the user to attach to
        public Dictionary<string, string> portDict = new Dictionary<string, string>();


        //State possibilities
        public const int STATE_STANDBY = 0;
        public const int STATE_SCANNING = 1;
        public const int STATE_CONNECTING = 2;
        public const int STATE_FINDING_SERVICES = 3;
        public const int STATE_FINDING_ATTR = 4;
        public const int PREPARING_WRITE_PDU = 5;
        public const int EXECUTE_WRITE = 6;
        public const int READING_GATT_SERVER = 7;
        public const int BEGIN_PREAPRE_WRITES = 8;
        public const int READY_TO_WRITE = 9;
        public const int READY_TO_READ_BACK = 10;
        public int app_state = STATE_STANDBY;        // current application state for the first run through 
    

        /* ================================================================ */
        /*                BEGIN MAIN EVENT-DRIVEN APP LOGIC                 */
        /* ================================================================ */

        public void GAPScanResponseEvent(object sender, Bluegiga.BLE.Events.GAP.ScanResponseEventArgs e)
        {
            //Print out Debug Data to the Serial Console
            String log = String.Format("ble_evt_gap_scan_response: rssi={0}, packet_type={1}, sender=[ {2}], address_type={3}, bond={4}, data=[ {5}]" + Environment.NewLine,
                (SByte)e.rssi,
                e.packet_type,
                ByteArrayToHexString(e.sender),
                e.address_type,
                e.bond,
                ByteArrayToHexString(e.data)
                );

            Console.Write(log);
            ThreadSafeDelegate(delegate { snif_log.AppendText(log); });

            //Compare the sender address with a hardcore fixed address. If the sender address is what we have target on the PC, its okay to connect.

            if (e.sender.SequenceEqual(target_BLE_address))
            {

                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("SAMB11 Device has been identified... Connecting") + Environment.NewLine); });
                adv_data = e.data;
                rssi = e.rssi;
                //Connect to the Samb11 device
                Byte[] cmd = bglib.BLECommandGAPConnectDirect(e.sender, e.address_type, 0x20, 0x30, 0x100, 0); // 125ms interval, 125ms window, active scanning
                bglib.SendCommand(serialAPI, cmd);

                // update state to connecting
                app_state = STATE_CONNECTING;

            }
        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */


        // Event is trigger whenever the a new connection occurs.
        public void ConnectionStatusEvent(object sender, Bluegiga.BLE.Events.Connection.StatusEventArgs e)
        {
            //Print debug data to the serial console
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

            Console.Write(log);
            ThreadSafeDelegate(delegate { txtLog.AppendText(log); });
            ThreadSafeDelegate(delegate
            {
                disconnected_led.BackColor = Color.White;
                connected_led.BackColor = Color.LightGreen; 
            });
            ThreadSafeDelegate(delegate { Device_Address.Text = ByteArrayToHexString(e.address); connection_interval.Text = e.conn_interval.ToString(); RSSI.Text = rssi.ToString(); latency.Text = e.latency.ToString(); });

            //If the connection flag is set, then we are connected
            if ((e.flags & 0x05) == 0x05)
            {
                /*Save the BLE connection handle. Since there is only one device connected at one time for this code we can use a unqie identifier. Later, we might need to change it to dynamic assignment to allow for 
                the user to connect and master read all devices within reach */
                ble_connection_handle = e.connection;

                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("Connected to {0}", ByteArrayToHexString(e.address)) + Environment.NewLine); });

                //Read the primary services which are denoted by 0x2800 and is search in little endian format! 
                Byte[] cmd = bglib.BLECommandATTClientReadByGroupType(e.connection, 0x0001, 0xFFFF, new Byte[] { 0x00, 0x28 }); 

                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
                bglib.SendCommand(serialAPI, cmd);

                app_state = STATE_FINDING_SERVICES;

            }
        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        //Event is triggered when a primary service is found. Check the UUID
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
 
                //Update the APP State
                app_state = STATE_FINDING_ATTR;
            }    
        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        //Event triggered when a attribute handle is found from the search process
        public void ATTClientFindInformationFoundEvent(object sender, Bluegiga.BLE.Events.ATTClient.FindInformationFoundEventArgs e)
        {
            String log = String.Format("ble_evt_attclient_find_information_found: connection={0}, chrhandle={1}, uuid=[ {2}]" + Environment.NewLine,
                e.connection,
                e.chrhandle,
                ByteArrayToHexString(e.uuid)
                );
            Console.Write(log);
            ThreadSafeDelegate(delegate { txtLog.AppendText(log); });

            // Parse out the UUIDs and store their handle for later access 
            if (e.uuid.SequenceEqual(new Byte[] { 0xf7, 0xed, 0xf0, 0x2b, 0xcb, 0xed, 0x4b, 0x15, 0x88, 0xe6, 0x75, 0x75, 0xed, 0xab, 0x98, 0x80 })) // Programming Data ATTR
            {
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("Found Programming Attribute : handle={0}", e.chrhandle) + Environment.NewLine); });
                att_handle_measurement_ccc = e.chrhandle;
                attribute_handles_found++;
            }
            else if(e.uuid.SequenceEqual(new Byte[] { 0xaa, 0xbb, 0xf0, 0x2b, 0xcb, 0xed, 0x4b, 0x15, 0x88, 0xe6, 0x75, 0x75, 0xed, 0xab, 0x98, 0x80 })) // ASIC Status ATTR
            {
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("Found ASIC Status Attribute : handle={0}", e.chrhandle) + Environment.NewLine); });
                att_handle_ASIC_status = e.chrhandle;
                attribute_handles_found++;

            }

            // Now that we have found the handles, we can write whatever programming data is loaded into the user input box
            if(attribute_handles_found >= 2)
            {
                app_state = READY_TO_WRITE;
            }
            else
            {
                app_state = STATE_FINDING_ATTR;
            }
            
        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        public void ATTClientAttributeValueEvent(object sender, Bluegiga.BLE.Events.ATTClient.AttributeValueEventArgs e)
        {
            String log = String.Format("ble_evt_attclient_attribute_value: connection={0}, atthandle={1}, type={2}, value=[ {3}]" + Environment.NewLine,
                e.connection,
                e.atthandle,
                e.type,
                ByteArrayToHexString(e.value)
                );
            GATT_Server_Read_Data_Recieved(e, log);
        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        public void BLEResponseATTClientPrepareWrite(object sender, Bluegiga.BLE.Responses.ATTClient.PrepareWriteEventArgs e)
        {
            if (e.result == 0)
            {
                write_prepare_success = 1;
                write_queque_counter++;
                ThreadSafeDelegate(delegate { txtLog.AppendText("Prepare Event Executed Sucessfully" + Environment.NewLine); });
            }

            else
            {
                write_prepare_success = 0;
                ThreadSafeDelegate(delegate { txtLog.AppendText("Prepare Event Failed" + Environment.NewLine); });
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("Result({0})", e.result) + Environment.NewLine); });

            }
        }


        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        public void BLEResponseATTClientExecuteWrite(object sender, Bluegiga.BLE.Responses.ATTClient.ExecuteWriteEventArgs e)
        {
            if (e.result == 0)
            {
                execute_write_success = 1;
                ThreadSafeDelegate(delegate { txtLog.AppendText("Execute Write Completed" + Environment.NewLine); });

            }

            else
            {

                ThreadSafeDelegate(delegate { txtLog.AppendText("Execute Write Failed" + Environment.NewLine); });
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("Result({0})", e.result) + Environment.NewLine); });
            }

        }


        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        // Event is triggered when any BLE is completed. It is reguarded as the Main() of the program. It will direct the program as each task is carried out. 
        public void ATTClientProcedureCompletedEvent(object sender, Bluegiga.BLE.Events.ATTClient.ProcedureCompletedEventArgs e)
        {
            //Debug the results of the completed events. This can help in identifiying why a procedure may have failed
            String log = String.Format("ble_evt_attclient_procedure_completed: connection={0}, result={1}, chrhandle={2}" + Environment.NewLine,
                e.connection,
                e.result,
                e.chrhandle
                );
            Console.Write(log);
            ThreadSafeDelegate(delegate { txtLog.AppendText(log); });
            switch (app_state)
            {
                case STATE_FINDING_ATTR:

                    if (att_handlesearch_end > 0)
                    {
                        // Search with the collected start and end parameters. The function will expose all attributes within the start and end handle range
                        Byte[] cmd = bglib.BLECommandATTClientFindInformation(e.connection, att_handlesearch_start, att_handlesearch_end);
                        // DEBUG: display bytes written
                        ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
                        bglib.SendCommand(serialAPI, cmd);

                    }

                    else
                    {
                        ThreadSafeDelegate(delegate { txtLog.AppendText("No Attributes were located within the GATT Server. Check Connection and SAMB11 Server Intialization" + Environment.NewLine); });
                    }

                    break;


                case READY_TO_WRITE:
                    Preapre_Write_Handler();
                    break;


                case PREPARING_WRITE_PDU:
                    Currently_Preparing_Write_PDU();
                    break;


                case EXECUTE_WRITE:
                    if (execute_write_success == 1)
                    {
                        execute_write_success = 0;
                        Read_Server(att_handle_measurement_ccc);
                    }
                    break;

                case READING_GATT_SERVER:
                    if (MTU_less == 1 && status_notified == 1 && ASIC_Status == 0)
                    {
                        //System_Reset();
                        ThreadSafeDelegate(delegate { txtLog.AppendText("Reading the Service handler" + Environment.NewLine); });
                        Read_Server(att_handle_ASIC_status);
                    }

                    else if (MTU_less == 1 && ASIC_Status == 1)
                    {
                        System_Reset();
                    }

                    break;

            }

            ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> APP STATE({0})", app_state) + Environment.NewLine); });
        }

        /* ================================================================ */
        /*                 END MAIN EVENT-DRIVEN APP LOGIC                  */
        /* ================================================================ */

        /*Define all secondary functions reference in MAIN Event Drive Logic. Also define process for the Form1 Linkers */

        public void GATT_Server_Read_Data_Recieved(Bluegiga.BLE.Events.ATTClient.AttributeValueEventArgs e, String log)
        {
            if (e.type == 1 & e.atthandle == att_handle_measurement_ccc)
            {
                ThreadSafeDelegate(delegate { txtLog.AppendText("Programming Data was Written" + Environment.NewLine); });

            }

            else if (e.type == 1 & e.atthandle == att_handle_ASIC_status)
            {
                status_notified = 1;
                ThreadSafeDelegate(delegate { txtLog.AppendText("ASIC Status ATTR was Notified" + Environment.NewLine); });
            }

            else if (e.type == 4 & e.atthandle == att_handle_ASIC_status)
            {
                Console.Write(log);
                ThreadSafeDelegate(delegate { txtLog.AppendText(log); });
                //print the ASIC Status
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("ASIC STATUS: {0} ", ByteArrayToHexString(e.value)) + Environment.NewLine);
                    asic_status_text.AppendText(String.Format("{0}", ByteArrayToHexString(e.value)));
                });
                ASIC_Status = 1;
            }

            else if (e.type == 4 & e.atthandle == att_handle_measurement_ccc)
            {
                if ((e.value.Length) != 22)
                {
                    MTU_less = 1;
                }

                Console.Write(log);
                ThreadSafeDelegate(delegate { txtLog.AppendText(log); });
                // print the Programming Data Written to the SAMB11 GATT Server
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("PROG DATA: {0} ", ByteArrayToHexString(e.value)) + Environment.NewLine);
                    asic_prog_data_text.AppendText(String.Format("{0}", ByteArrayToHexString(e.value)));
                });

            }

        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        public void Preapre_Write_Handler()
        {
                app_state = PREPARING_WRITE_PDU;
                hex_byte_array = HexStringToBytes(hex_data_input);
                temp_hex_byte_array_holder = new byte[18];

                Array.Copy(hex_byte_array, 0, temp_hex_byte_array_holder, 0, 18);
                index_selector = 0;

                Byte[] cmd_int = bglib.BLECommandATTClientPrepareWrite(ble_connection_handle, att_handle_measurement_ccc, Convert.ToUInt16(index_selector), temp_hex_byte_array_holder);
                bglib.SendCommand(serialAPI, cmd_int);

                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("First Packet has been Prepared for Write") + Environment.NewLine); }); 
        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        public void Currently_Preparing_Write_PDU()
        {
            if ((write_queque_counter > 0) && (write_queque_counter <= 5) && (write_prepare_success == 1))
            {
                write_prepare_success = 0;
                index_selector = index_selector + 18;

                //Not dynamic since the programming data will be a known value
                if (write_queque_counter != 5)
                {
                    Array.Copy(hex_byte_array, index_selector, temp_hex_byte_array_holder, 0, 18);
                }

                else
                {
                    byte[] last_cpy = new byte[hex_byte_array.Length - index_selector];
                    Array.Copy(hex_byte_array, index_selector, last_cpy, 0, hex_byte_array.Length - index_selector);
                    temp_hex_byte_array_holder = last_cpy;
                }

                Byte[] cmd_int = bglib.BLECommandATTClientPrepareWrite(ble_connection_handle, att_handle_measurement_ccc, Convert.ToUInt16(index_selector), temp_hex_byte_array_holder);
                bglib.SendCommand(serialAPI, cmd_int);

                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("Executing Another Prepare Write") + Environment.NewLine); });
            }

            else if (write_prepare_success == 1 && write_queque_counter == 6)
            {
                write_prepare_success = 0;

                Byte[] cmd = bglib.BLECommandATTClientExecuteWrite(ble_connection_handle, 1);
                ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
                bglib.SendCommand(serialAPI, cmd);
                app_state = EXECUTE_WRITE;
            }
        }
        
        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        public void System_Reset()
        {
            status_notified = 0;
            index_selector = 0;
            write_prepare_success = 0;
            execute_write_success = 0;
            write_queque_counter = 0;
            execute_write_success = 0;
            MTU_less = 0;
            ASIC_Status = 0;
            app_state = STATE_STANDBY;
            ThreadSafeDelegate(delegate { txtLog.AppendText("System Reset - Closing Connection" + Environment.NewLine); });
            Byte[] cmd;

            //Run a Hard Disconnect Event from the PC
            cmd = bglib.BLECommandConnectionDisconnect(ble_connection_handle);
            bglib.SendCommand(serialAPI, cmd);

            // stop scanning if scanning
            cmd = bglib.BLECommandGAPEndProcedure();
            // DEBUG: display bytes read
            ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
            bglib.SendCommand(serialAPI, cmd);

            // stop advertising if advertising
            cmd = bglib.BLECommandGAPSetMode(0, 0);
            // DEBUG: display bytes read
            ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
            bglib.SendCommand(serialAPI, cmd);

            // enable "engage" button to allow them to start again
            ThreadSafeDelegate(delegate
            {
                disconnected_led.BackColor = Color.Red;
                connected_led.BackColor = Color.White;
                btnGo.Enabled = true;
                write_prog_data.Checked = false;
            });
        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        public void Read_Server(UInt16 attrib_handle)
        {
            Byte[] cmd = bglib.BLECommandATTClientReadLong(connection_handle, attrib_handle);
            ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
            bglib.SendCommand(serialAPI, cmd);
            app_state = READING_GATT_SERVER;
        }


        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        // Thread-safe operations from event handlers
        public void ThreadSafeDelegate(MethodInvoker method)
        {
            if (InvokeRequired)
                BeginInvoke(method);

            else
                method.Invoke();
        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        // Convert byte hex_byte_array to "00 11 22 33 44 55 " string FOR PRINTING PURPOSES
        public string ByteArrayToHexString(Byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2} ", b);
            return hex.ToString();
        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        // Serial port event handler for a nice event-driven architecture
        private void DataReceivedHandler(object sender,System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            System.IO.Ports.SerialPort sp = (System.IO.Ports.SerialPort)sender;
            Byte[] inData = new Byte[sp.BytesToRead];
            if (!(inData.Length == 0))
            {
                sp.Read(inData, 0, sp.BytesToRead);
            }
            // read all available bytes from serial port in one chunk
            

            // DEBUG: display bytes read
           // ThreadSafeDelegate(delegate { txtLog.AppendText(String.Format("<= RX ({0}) [ {1}]", inData.Length, ByteArrayToHexString(inData)) + Environment.NewLine); });

            // parse all bytes read through BGLib parser to trigger the event
            for (int i = 0; i < inData.Length; i++)
            {
                bglib.Parse(inData[i]);
            }
        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */
        public void Load_Hash_Table()
        {
            byte[] select_BLE_address_1 = { 0x7f, 0x7f, 0x6a, 0x11, 0x75, 0x13 };
            byte[] select_BLE_address_2 = { 0xaf, 0x7f, 0x6a, 0x11, 0x75, 0x13 };
            byte[] select_BLE_address_3 = { 0xcd, 0x7f, 0x6a, 0x11, 0x75, 0x13 };

            myHT.Add(0, select_BLE_address_1);
            myHT.Add(1, select_BLE_address_2);
            myHT.Add(2, select_BLE_address_3);

            foreach(DictionaryEntry entry in myHT)
            {
                ble_target_address_drop_dwn.Items.Add(ByteArrayToHexString((byte[])entry.Value));
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
            Load_Hash_Table();
            

            // initialize COM port combobox with list of portsl
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

            // initialize BGLib events we'll need for this script within the Form 1 Context 
            bglib.BLEEventGAPScanResponse += new Bluegiga.BLE.Events.GAP.ScanResponseEventHandler(this.GAPScanResponseEvent);
            bglib.BLEEventConnectionStatus += new Bluegiga.BLE.Events.Connection.StatusEventHandler(this.ConnectionStatusEvent);
            bglib.BLEEventATTClientGroupFound += new Bluegiga.BLE.Events.ATTClient.GroupFoundEventHandler(this.ATTClientGroupFoundEvent);
            bglib.BLEEventATTClientFindInformationFound += new Bluegiga.BLE.Events.ATTClient.FindInformationFoundEventHandler(this.ATTClientFindInformationFoundEvent);
            bglib.BLEEventATTClientProcedureCompleted += new Bluegiga.BLE.Events.ATTClient.ProcedureCompletedEventHandler(this.ATTClientProcedureCompletedEvent);
            bglib.BLEEventATTClientAttributeValue += new Bluegiga.BLE.Events.ATTClient.AttributeValueEventHandler(this.ATTClientAttributeValueEvent);
            bglib.BLEResponseATTClientPrepareWrite += new Bluegiga.BLE.Responses.ATTClient.PrepareWriteEventHandler(this.BLEResponseATTClientPrepareWrite);
            bglib.BLEResponseATTClientExecuteWrite += new Bluegiga.BLE.Responses.ATTClient.ExecuteWriteEventHandler(this.BLEResponseATTClientExecuteWrite);

        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

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

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        private void btnAttach_Click(object sender, EventArgs e)
        {
            if (!isAttached)
            {
                snif_log.AppendText("Opening serial port '" + comboPorts.SelectedValue.ToString() + "'..." + Environment.NewLine);
                serialAPI.PortName = comboPorts.SelectedValue.ToString();
                serialAPI.Open();
                snif_log.AppendText("Port opened" + Environment.NewLine);
                isAttached = true;
                btnAttach.Text = "Detach";
                btnGo.Enabled = true;
                btnReset.Enabled = true;
                snif_log.AppendText("Ready to begin BLE Process" + Environment.NewLine);
            }
            else
            {
                snif_log.AppendText("Closing serial port..." + Environment.NewLine);
                serialAPI.Close();
                snif_log.AppendText("Port closed" + Environment.NewLine);
                isAttached = false;
                btnAttach.Text = "Attach";
                btnGo.Enabled = false;
                btnReset.Enabled = false;
            }
        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        private void btnGo_Click(object sender, EventArgs e)
        {
            asic_prog_data_text.Text = "";
            asic_status_text.Text = "";
            

            if (write_prog_data.Checked)
            {
                //Grab the Programming Data from the user input text box
                hex_data_input = richTextBox1.Text;
                int Select = ble_target_address_drop_dwn.SelectedIndex;
                if (Select == 2)
                {
                    Select = 0;
                }
                else if(Select == 0)
                {
                    Select = 2;
                }
                target_BLE_address = (byte[])myHT[Select];
                ThreadSafeDelegate(delegate { snif_log.AppendText(String.Format("addr chosen: {0}", ByteArrayToHexString(target_BLE_address)) + Environment.NewLine); });
                // start the scan/connect process now
                Byte[] cmd;

                // set scan parameters
                cmd = bglib.BLECommandGAPSetScanParameters(0xC8, 0xC8, 1);
                ThreadSafeDelegate(delegate { snif_log.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
                bglib.SendCommand(serialAPI, cmd);

                // begin scanning for BLE peripherals
                cmd = bglib.BLECommandGAPDiscover(1); // generic discovery mode
                ThreadSafeDelegate(delegate { snif_log.AppendText(String.Format("=> TX ({0}) [ {1}]", cmd.Length, ByteArrayToHexString(cmd)) + Environment.NewLine); });
                bglib.SendCommand(serialAPI, cmd);


                // disable "Engage" button 
                btnGo.Enabled = false;
                app_state = STATE_SCANNING;
            }
            
        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        public static byte[] HexStringToBytes(string str)
        {    
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


        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

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
            disconnected_led.BackColor = Color.Red;
            connected_led.BackColor = Color.White;

        }


        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

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

                ThreadSafeDelegate(delegate { txtLog.AppendText("ECIT" + Environment.NewLine); });
            }

        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

        private void button2_Click(object sender, EventArgs e)
        {
            // stop everything we're doing, if possible
            Byte[] cmd;

            // disconnect if connected
            cmd = bglib.BLECommandConnectionDisconnect(ble_connection_handle);
            bglib.SendCommand(serialAPI, cmd);
        }

        /* ================================================================================================================================================================= */
        /* ================================================================================================================================================================= */

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

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void RSSI_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

    }
}