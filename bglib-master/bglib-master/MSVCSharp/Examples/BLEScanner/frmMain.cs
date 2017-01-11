﻿// Bluegiga BGLib C# interface library
// 2013-01-15 by Jeff Rowberg <jeff@rowberg.net>
// Updates should (hopefully) always be available at https://github.com/jrowberg/bglib

/* ============================================
BGLib C# interface library code is placed under the MIT license
Copyright (c) 2013 Jeff Rowberg

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
===============================================
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BLEScanner
{
    public partial class frmMain : Form
    {
        //BGlib is now instantiated as a referenced method database. 
        public Bluegiga.BGLib bglib = new Bluegiga.BGLib();

        // Typical startup for the initialization of the actual dongle, but is this uploaded to the actual device? 
        public void SystemBootEvent(object sender, Bluegiga.BLE.Events.System.BootEventArgs e)
        {
            String log = String.Format("ble_evt_system_boot:" + Environment.NewLine + "\tmajor={0}, minor={1}, patch={2}, build={3}, ll_version={4}, protocol_version={5}, hw={6}" + Environment.NewLine,
                e.major,
                e.minor,
                e.patch,
                e.build,
                e.ll_version,
                e.protocol_version,
                e.hw
                );
            Console.Write(log);
            ThreadSafeDelegate(delegate { txtLog.AppendText(log); });
        }

        //object is the sender which is from the RX line on the UART serial bus. 
        public void GAPScanResponseEvent(object sender, Bluegiga.BLE.Events.GAP.ScanResponseEventArgs e)
        {
            String log = String.Format("ble_evt_gap_scan_response:" + Environment.NewLine + "\trssi={0}, packet_type={1}, bd_addr=[ {2}], address_type={3}, bond={4}, data=[ {5}]" + Environment.NewLine,
               //These expose the data found from the occured event. We take the info an print it out. 
                (SByte)e.rssi,
                //Double check cast redundant type. Represent 8 bit signed integer
                (SByte)e.packet_type,
                ByteArrayToHexString(e.sender),
                (SByte)e.address_type,
                (SByte)e.bond,
                ByteArrayToHexString(e.data)
                );
            Console.Write(log);
            ThreadSafeDelegate(delegate { txtLog.AppendText(log); });
        }

        // Thread-safe operations from event handlers
    
        public void ThreadSafeDelegate(MethodInvoker method)
        {
            if (InvokeRequired)
                BeginInvoke(method);
            else
                method.Invoke();
        }
        
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //All data is sent through the acutal serial port. So the MCU on the Bluegiga acts as a transpararent data pipe right to the actual PC. 
            serialAPI.Handshake = System.IO.Ports.Handshake.RequestToSend;
            serialAPI.BaudRate = 115200;
            serialAPI.PortName = "COM7"; // Specify here what one you want depending on where the actual device is enumerated. Can make this moduluar but whatever, just a pain
            serialAPI.DataBits = 8;
            serialAPI.StopBits = System.IO.Ports.StopBits.One;
            serialAPI.Parity = System.IO.Ports.Parity.None;
            serialAPI.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(DataReceivedHandler);
            serialAPI.Open();
            Console.WriteLine("Port open");

            bglib.BLEEventSystemBoot += new Bluegiga.BLE.Events.System.BootEventHandler(this.SystemBootEvent);
            bglib.BLEEventGAPScanResponse += new Bluegiga.BLE.Events.GAP.ScanResponseEventHandler(this.GAPScanResponseEvent);

            // send system_hello()
            //serialAPI.Write(new Byte[] { 0, 0, 0, 1 }, 0, 4);
        }

        private void DataReceivedHandler(
                                object sender,
                                System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // Make the actual serial port resource. Kinda like a data struct.
            System.IO.Ports.SerialPort sp = (System.IO.Ports.SerialPort)sender;

            // retrieve the size of the bytes seen on the actual serial port and make a byte array of that size (LOOK INTO BUFFERING?????!!??!?!??!)
            Byte[] inData = new Byte[sp.BytesToRead];

            // read all available bytes from serial port in one chunk
            sp.Read(inData, 0, sp.BytesToRead);
            
            // DEBUG: display bytes read
            Console.WriteLine("<= RX ({0}) [ {1}]", inData.Length, ByteArrayToHexString(inData));
            
            // parse all bytes read through BGLib parser
            for (int i = 0; i < inData.Length; i++)
            {
                //YAY BUILT IN PARSER 
                bglib.Parse(inData[i]);
            }
        }

        private void btnStartScan_Click(object sender, EventArgs e)
        {
            // send gap_discover(mode: 1)
            //serialAPI.Write(new Byte[] { 0, 1, 6, 2, 1 }, 0, 5);
            bglib.SendCommand(serialAPI, bglib.BLECommandGAPDiscover(1));
        }

        private void btnStopScan_Click(object sender, EventArgs e)
        {
            // send gap_end_procedure()
            //serialAPI.Write(new Byte[] { 0, 0, 6, 4 }, 0, 4);
            bglib.SendCommand(serialAPI, bglib.BLECommandGAPEndProcedure());
        }

        public string ByteArrayToHexString(Byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2} ", b);
            return hex.ToString();
        }
    }
}
