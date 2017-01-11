namespace BLEHeartRateCollector
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblPorts = new System.Windows.Forms.Label();
            this.btnGo = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnAttach = new System.Windows.Forms.Button();
            this.comboPorts = new System.Windows.Forms.ComboBox();
            this.serialAPI = new System.IO.Ports.SerialPort(this.components);
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.ble_target_address_drop_dwn = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnReset = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.asic_status_readback = new System.Windows.Forms.GroupBox();
            this.asic_status_text = new System.Windows.Forms.TextBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.asic_prog_data_text = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.write_prog_data = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.latency = new System.Windows.Forms.TextBox();
            this.disconnected_led = new System.Windows.Forms.Button();
            this.connected_led = new System.Windows.Forms.Button();
            this.RSSI = new System.Windows.Forms.TextBox();
            this.connection_interval = new System.Windows.Forms.TextBox();
            this.Device_Address = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.snif_log = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.asic_status_readback.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPorts
            // 
            this.lblPorts.AutoSize = true;
            this.lblPorts.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPorts.Location = new System.Drawing.Point(8, 15);
            this.lblPorts.Name = "lblPorts";
            this.lblPorts.Size = new System.Drawing.Size(78, 20);
            this.lblPorts.TabIndex = 0;
            this.lblPorts.Text = "COM Port";
            this.lblPorts.Click += new System.EventHandler(this.lblPorts_Click);
            // 
            // btnGo
            // 
            this.btnGo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnGo.Enabled = false;
            this.btnGo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGo.Location = new System.Drawing.Point(10, 23);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(204, 29);
            this.btnGo.TabIndex = 2;
            this.btnGo.Text = "Engage";
            this.btnGo.UseVisualStyleBackColor = false;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.White;
            this.txtLog.ForeColor = System.Drawing.Color.Black;
            this.txtLog.Location = new System.Drawing.Point(12, 341);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(683, 272);
            this.txtLog.TabIndex = 3;
            this.txtLog.TextChanged += new System.EventHandler(this.txtLog_TextChanged);
            // 
            // btnAttach
            // 
            this.btnAttach.BackColor = System.Drawing.SystemColors.Info;
            this.btnAttach.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAttach.Location = new System.Drawing.Point(613, 43);
            this.btnAttach.Name = "btnAttach";
            this.btnAttach.Size = new System.Drawing.Size(82, 28);
            this.btnAttach.TabIndex = 5;
            this.btnAttach.Text = "Attach";
            this.btnAttach.UseVisualStyleBackColor = false;
            this.btnAttach.Click += new System.EventHandler(this.btnAttach_Click);
            // 
            // comboPorts
            // 
            this.comboPorts.FormattingEnabled = true;
            this.comboPorts.Location = new System.Drawing.Point(92, 14);
            this.comboPorts.Name = "comboPorts";
            this.comboPorts.Size = new System.Drawing.Size(513, 21);
            this.comboPorts.TabIndex = 6;
            this.comboPorts.SelectedIndexChanged += new System.EventHandler(this.comboPorts_SelectedIndexChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(611, 12);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(82, 25);
            this.btnRefresh.TabIndex = 7;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 20);
            this.label1.TabIndex = 8;
            this.label1.Text = "BLE Packet Sniffer";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(9, 42);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(653, 66);
            this.richTextBox1.TabIndex = 10;
            this.richTextBox1.Text = "";
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.Yellow;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(220, 23);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(223, 29);
            this.button2.TabIndex = 16;
            this.button2.Text = "Master Disconnect";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // ble_target_address_drop_dwn
            // 
            this.ble_target_address_drop_dwn.FormattingEnabled = true;
            this.ble_target_address_drop_dwn.Location = new System.Drawing.Point(808, 17);
            this.ble_target_address_drop_dwn.Name = "ble_target_address_drop_dwn";
            this.ble_target_address_drop_dwn.Size = new System.Drawing.Size(561, 21);
            this.ble_target_address_drop_dwn.TabIndex = 17;
            this.ble_target_address_drop_dwn.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(699, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 20);
            this.label3.TabIndex = 19;
            this.label3.Text = "BLE Address";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // btnReset
            // 
            this.btnReset.BackColor = System.Drawing.Color.Red;
            this.btnReset.Enabled = false;
            this.btnReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReset.Location = new System.Drawing.Point(449, 23);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(203, 29);
            this.btnReset.TabIndex = 4;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = false;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnGo);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.btnReset);
            this.groupBox1.Location = new System.Drawing.Point(701, 54);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(668, 64);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "System Functions ";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.asic_status_readback);
            this.groupBox2.Controls.Add(this.groupBox6);
            this.groupBox2.Controls.Add(this.groupBox4);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.richTextBox1);
            this.groupBox2.Location = new System.Drawing.Point(701, 189);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(668, 425);
            this.groupBox2.TabIndex = 23;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "BLE Functions";
            // 
            // asic_status_readback
            // 
            this.asic_status_readback.Controls.Add(this.asic_status_text);
            this.asic_status_readback.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.asic_status_readback.Location = new System.Drawing.Point(12, 325);
            this.asic_status_readback.Name = "asic_status_readback";
            this.asic_status_readback.Size = new System.Drawing.Size(107, 39);
            this.asic_status_readback.TabIndex = 29;
            this.asic_status_readback.TabStop = false;
            this.asic_status_readback.Text = "ASIC Status Byte";
            // 
            // asic_status_text
            // 
            this.asic_status_text.BackColor = System.Drawing.Color.White;
            this.asic_status_text.ForeColor = System.Drawing.Color.Black;
            this.asic_status_text.Location = new System.Drawing.Point(6, 14);
            this.asic_status_text.Multiline = true;
            this.asic_status_text.Name = "asic_status_text";
            this.asic_status_text.ReadOnly = true;
            this.asic_status_text.Size = new System.Drawing.Size(95, 19);
            this.asic_status_text.TabIndex = 26;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.asic_prog_data_text);
            this.groupBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox6.Location = new System.Drawing.Point(12, 220);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(650, 99);
            this.groupBox6.TabIndex = 28;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "ASIC Programming Data Readback";
            // 
            // asic_prog_data_text
            // 
            this.asic_prog_data_text.BackColor = System.Drawing.Color.White;
            this.asic_prog_data_text.ForeColor = System.Drawing.Color.Black;
            this.asic_prog_data_text.Location = new System.Drawing.Point(6, 19);
            this.asic_prog_data_text.Multiline = true;
            this.asic_prog_data_text.Name = "asic_prog_data_text";
            this.asic_prog_data_text.ReadOnly = true;
            this.asic_prog_data_text.Size = new System.Drawing.Size(638, 74);
            this.asic_prog_data_text.TabIndex = 25;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.checkBox6);
            this.groupBox4.Controls.Add(this.checkBox3);
            this.groupBox4.Controls.Add(this.checkBox5);
            this.groupBox4.Controls.Add(this.checkBox2);
            this.groupBox4.Controls.Add(this.checkBox4);
            this.groupBox4.Location = new System.Drawing.Point(12, 114);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(650, 47);
            this.groupBox4.TabIndex = 26;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Read Functions";
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Location = new System.Drawing.Point(517, 19);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(106, 17);
            this.checkBox6.TabIndex = 31;
            this.checkBox6.Text = "Stimulation Rate ";
            this.checkBox6.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(265, 19);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(140, 17);
            this.checkBox3.TabIndex = 28;
            this.checkBox3.Text = "Backpack Battery Level";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Location = new System.Drawing.Point(411, 19);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(100, 17);
            this.checkBox5.TabIndex = 30;
            this.checkBox5.Text = "Site Impedance";
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(6, 19);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(140, 17);
            this.checkBox2.TabIndex = 27;
            this.checkBox2.Text = "ASIC Programming Data";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(152, 19);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(107, 17);
            this.checkBox4.TabIndex = 29;
            this.checkBox4.Text = "ASIC Status Byte";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.write_prog_data);
            this.groupBox3.Location = new System.Drawing.Point(12, 167);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(650, 47);
            this.groupBox3.TabIndex = 25;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Write Functions";
            // 
            // write_prog_data
            // 
            this.write_prog_data.AutoSize = true;
            this.write_prog_data.Location = new System.Drawing.Point(6, 19);
            this.write_prog_data.Name = "write_prog_data";
            this.write_prog_data.Size = new System.Drawing.Size(141, 17);
            this.write_prog_data.TabIndex = 27;
            this.write_prog_data.Text = "Write Programming Data";
            this.write_prog_data.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(9, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 16);
            this.label2.TabIndex = 24;
            this.label2.Text = "PROG Data Input";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label7);
            this.groupBox5.Controls.Add(this.latency);
            this.groupBox5.Controls.Add(this.disconnected_led);
            this.groupBox5.Controls.Add(this.connected_led);
            this.groupBox5.Controls.Add(this.RSSI);
            this.groupBox5.Controls.Add(this.connection_interval);
            this.groupBox5.Controls.Add(this.Device_Address);
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.label4);
            this.groupBox5.Location = new System.Drawing.Point(701, 123);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(668, 60);
            this.groupBox5.TabIndex = 24;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "BLE Connection Information";
            this.groupBox5.Enter += new System.EventHandler(this.groupBox5_Enter);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(493, 33);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 13);
            this.label7.TabIndex = 30;
            this.label7.Text = "Latency:";
            // 
            // latency
            // 
            this.latency.Location = new System.Drawing.Point(547, 30);
            this.latency.Name = "latency";
            this.latency.Size = new System.Drawing.Size(65, 20);
            this.latency.TabIndex = 29;
            // 
            // disconnected_led
            // 
            this.disconnected_led.BackColor = System.Drawing.Color.Red;
            this.disconnected_led.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.disconnected_led.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.disconnected_led.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.disconnected_led.Location = new System.Drawing.Point(641, 10);
            this.disconnected_led.Name = "disconnected_led";
            this.disconnected_led.Size = new System.Drawing.Size(21, 10);
            this.disconnected_led.TabIndex = 28;
            this.disconnected_led.UseVisualStyleBackColor = false;
            // 
            // connected_led
            // 
            this.connected_led.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.connected_led.Location = new System.Drawing.Point(614, 10);
            this.connected_led.Name = "connected_led";
            this.connected_led.Size = new System.Drawing.Size(21, 10);
            this.connected_led.TabIndex = 25;
            this.connected_led.UseVisualStyleBackColor = true;
            // 
            // RSSI
            // 
            this.RSSI.Location = new System.Drawing.Point(407, 30);
            this.RSSI.Name = "RSSI";
            this.RSSI.Size = new System.Drawing.Size(76, 20);
            this.RSSI.TabIndex = 27;
            this.RSSI.TextChanged += new System.EventHandler(this.RSSI_TextChanged);
            // 
            // connection_interval
            // 
            this.connection_interval.Location = new System.Drawing.Point(316, 30);
            this.connection_interval.Name = "connection_interval";
            this.connection_interval.Size = new System.Drawing.Size(44, 20);
            this.connection_interval.TabIndex = 26;
            // 
            // Device_Address
            // 
            this.Device_Address.Location = new System.Drawing.Point(125, 30);
            this.Device_Address.Name = "Device_Address";
            this.Device_Address.Size = new System.Drawing.Size(141, 20);
            this.Device_Address.TabIndex = 25;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(375, 33);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "RSSI:";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(283, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(36, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "CONI:";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(119, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Device Target Address:";
            // 
            // snif_log
            // 
            this.snif_log.BackColor = System.Drawing.Color.White;
            this.snif_log.ForeColor = System.Drawing.Color.Black;
            this.snif_log.Location = new System.Drawing.Point(12, 77);
            this.snif_log.Multiline = true;
            this.snif_log.Name = "snif_log";
            this.snif_log.ReadOnly = true;
            this.snif_log.Size = new System.Drawing.Size(683, 238);
            this.snif_log.TabIndex = 25;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(8, 318);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(156, 20);
            this.label8.TabIndex = 26;
            this.label8.Text = "BLE Connection Log";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1381, 626);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.snif_log);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ble_target_address_drop_dwn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.comboPorts);
            this.Controls.Add(this.btnAttach);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.lblPorts);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "BluView";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.asic_status_readback.ResumeLayout(false);
            this.asic_status_readback.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPorts;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnAttach;
        private System.Windows.Forms.ComboBox comboPorts;
        private System.IO.Ports.SerialPort serialAPI;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox ble_target_address_drop_dwn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox Device_Address;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox RSSI;
        private System.Windows.Forms.TextBox connection_interval;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox write_prog_data;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.Button disconnected_led;
        private System.Windows.Forms.Button connected_led;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox latency;
        private System.Windows.Forms.GroupBox asic_status_readback;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.TextBox asic_status_text;
        private System.Windows.Forms.TextBox asic_prog_data_text;
        private System.Windows.Forms.TextBox snif_log;
        private System.Windows.Forms.Label label8;
    }
}
