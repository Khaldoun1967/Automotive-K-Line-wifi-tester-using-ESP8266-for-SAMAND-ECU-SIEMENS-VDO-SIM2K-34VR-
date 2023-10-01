using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using k_line_LIB;

//using System.Net.NetworkInformation;

namespace Easy_Queue_System
{
    public partial class MainForm : Form
    {
        dashboard mdashboard;
        public Wifi_credential mWifi_credential;

        public WifiClient mWifiClient;
        public k_line_class mk_line_class;

        
        public MainForm()
        {
            InitializeComponent();
    
 
            mWifiClient = new WifiClient();                       
            mWifiClient.ExceptionEvent += mWifiClient_ExceptionEvent;
            mWifiClient.Connect_Event += AP_WifiClient_Connect_Event;
            mWifiClient.Connected_Event += mWifiClient_Connected_Event;
            mWifiClient.K_Line += mWifiClient_K_Line;

            mk_line_class = new k_line_class(0x11, 0xF1);          
        }

        void mWifiClient_K_Line(object sender, MSG_EventArgs e)
        {
            string str = e.MSG.ToUpper();

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    richTextBox1.Text += str + "\n";
                    mk_line_class.decoding(str);
                    StreamShow();
                }));
            }
            else
            {
                richTextBox1.Text += str + "\n";
                mk_line_class.decoding(str);
                StreamShow();
            }

            //ASCIIEncoding ascii = new ASCIIEncoding();
            //byte[] array = ascii.GetBytes(str);    
        }

     
        void mWifiClient_Connected_Event(object sender, MSG_EventArgs e)
        {
            if (e.MSG.Length == 0) toolStripStatusLabel1.Text = "Not Connected";
            else toolStripStatusLabel1.Text = "Connected to: "+ e.MSG;
        }

       
     
     
        void mWifiClient_ExceptionEvent(object sender, MSG_EventArgs e)
        {
            MessageBox.Show(e.MSG, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
  
            
        void AP_WifiClient_Connect_Event(object sender, Connect_EventArgs e)
        {         
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    Show(e.STR, e.Progress, e.STATUS);
                }));
            }
            else
            {
                Show(e.STR, e.Progress, e.STATUS);
            }           
        }

 
        void Show(string msg, int progress, bool status)
        {
            if (progress > 100)
            {
                if (status == true)
                {
                    MessageBox.Show("Connection To the " + msg + " Access point has been Established Successfully", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to Establish a Connection To the " + msg + " Access point!. \n Make sure its powered on !", "Er",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                if (mWifi_credential != null)
                {
                    if (status == true)
                    {
                        if (progress == 100) MessageBox.Show(msg, "done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else MessageBox.Show(msg, "Eror", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mWifi_credential.Finish();
                    }
                    else { mWifi_credential.process(msg, progress); }
                }
            }
        }

     
 


       
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Would you like to Quit?  ", "Quit",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
                System.Environment.Exit(System.Environment.ExitCode);
            }
        }

      
 
      

        

        private void usingEasyQueueAccessPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mWifiClient.Connect_EasyQueue_AP();                         
        }
        private void toolStripComboBox1_DropDown(object sender, EventArgs e)
        {
            toolStripComboBox1.Items.Clear();
            toolStripComboBox1.Text = "Select Wifi LAN";

            List<string> mList = mWifiClient.get_station_list();

            for(int i=0; i < mList.Count; i++)
            {
                toolStripComboBox1.Items.Add(mList[i]);
            }           
        }

        private void usingAvialableRouterLANToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            toolStripComboBox1.Text = "Select Wifi LAN";
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {     
            string ssid = toolStripComboBox1.SelectedItem.ToString();
            mWifi_credential = new Wifi_credential(this, ssid);
            mWifi_credential.Owner = this;
            mWifi_credential.Show();
        }

   
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mWifiClient.Open_Wifi_Networks_Flyout();        //update Avilable Wifi networks
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mWifiClient.DisConnect();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
        }

   

        private void button18_Click(object sender, EventArgs e)
        {
            //initialization
            //81,11,F1,81,4
            //83,F1,11,C1,EF,8F,C4
 
            mWifiClient.Send_CMD("FASTKWP");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //nDisconnect
            //81 11 F1 82 05  
            //81 F1 11 C2 45

            mWifiClient.Send_CMD(mk_line_class.Disconnect);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            mWifiClient.Send_CMD(mk_line_class.Read_DTC);
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            //81 11 F1 3E C1    keep alive
            //81 F1 11 7E 01
            mWifiClient.Send_CMD("8111F13EC1");
        }

      

        private void button15_Click(object sender, EventArgs e)
        {
            string s = mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.Hi_Fan);
            mWifiClient.Send_CMD(s);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            string s = mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.Low_Fan);
            mWifiClient.Send_CMD(s);            
        }

        private void button14_Click(object sender, EventArgs e)
        {          
            string s = mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.All_off);
            mWifiClient.Send_CMD(s);    
        }

        private void button20_Click(object sender, EventArgs e)
        {
            //Dignostic Light test           
            //mWifiClient.Send_CMD("8411F1701007FF0C");
            string s = mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.dig_light);
            mWifiClient.Send_CMD(s);     
        }

        private void button11_Click(object sender, EventArgs e)
        {
            mWifiClient.Send_CMD(mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.AC));     
        }

        private void button10_Click(object sender, EventArgs e)
        {   
            mWifiClient.Send_CMD(mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.Main_relay));     
        }

        private void button17_Click(object sender, EventArgs e)
        {      
            mWifiClient.Send_CMD(mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.Cansiter));     
        }

        private void button16_Click(object sender, EventArgs e)
        {
            mWifiClient.Send_CMD(mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.exhanst));     
        }

        private void button19_Click(object sender, EventArgs e)
        {
            mWifiClient.Send_CMD(mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.Stepper));     
        }

        private void button22_Click(object sender, EventArgs e)
        {
            mWifiClient.Send_CMD(mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.Ignition_Coil1));     
        }

        private void button21_Click(object sender, EventArgs e)
        {
            mWifiClient.Send_CMD(mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.Ignition_Coil2));     
        }

        private void button24_Click(object sender, EventArgs e)
        {
            mWifiClient.Send_CMD(mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.Injector1));     
        }

        private void button25_Click(object sender, EventArgs e)
        {      
            mWifiClient.Send_CMD(mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.Injector2));     
        }

        private void button26_Click(object sender, EventArgs e)
        {
            mWifiClient.Send_CMD(mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.Injector3));     
        }

        private void button23_Click(object sender, EventArgs e)
        {
            mWifiClient.Send_CMD(mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.Injector4));     
        }

        private void button35_Click(object sender, EventArgs e)
        {
            string s = mk_line_class.Build_ReadECUId((byte)ReadECUId.VIN);
            mWifiClient.Send_CMD(s);     
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string s = mk_line_class.Build_IOControlByLocalId((byte)IOControlByLocalId.Fuel_pump);
            mWifiClient.Send_CMD(s);     
        }

        private void button33_Click(object sender, EventArgs e)
        {
            string s = mk_line_class.Build_ReadECUId((byte)ReadECUId.Supplier_ECU_Hardware_Number);
            mWifiClient.Send_CMD(s);     
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string s = mk_line_class.Build_ReadECUId((byte)ReadECUId.ECU_Software_Number);
            mWifiClient.Send_CMD(s);     
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string s = mk_line_class.Build_ReadECUId((byte)ReadECUId.Calibration_Number);
            mWifiClient.Send_CMD(s);     
        }

        private void button36_Click(object sender, EventArgs e)
        {
            string s = mk_line_class.Build_ReadECUId((byte)ReadECUId.Boot_Software_Number);
            mWifiClient.Send_CMD(s);     
        }

        private void button34_Click(object sender, EventArgs e)
        {
            string s = mk_line_class.Build_ReadECUId((byte)ReadECUId.Programming_Date);
            mWifiClient.Send_CMD(s);     
        }

        private void button7_Click(object sender, EventArgs e)
        {

            mWifiClient.Send_CMD(mk_line_class.ClearDTC);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string s = mk_line_class.Build_ReadDataByLocalId((byte)Commands.Stream_Cmd);
            mWifiClient.Send_CMD(s); 
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string s = mk_line_class.Build_ReadDataByLocalId((byte)Commands.Stream_Cmd);
            mWifiClient.Send_CMD(s); 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void button27_Click(object sender, EventArgs e)
        {
            string str = "80,F1,11,53,61,1,78,3D,A3,6D,A5,6C,11,0,0,11,CB,0,0,82,2B,0,0,75,0,0,0,37,4,FF,0,57,0,0,0,B0,B0,B0,B0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1D,0,80,FA,0,0,D1,96,D6,6,E1,F,7F,0,0,0,0,9A,9,0,7F,1,EB,3,0,39,DB,F,0,0,0,EA";

            mk_line_class.decoding(str);
            StreamShow();
        }

        void StreamShow()
        {
            label1.Text = "Vb=" + mk_line_class.par[1].ToString();//Vbat
            label2.Text = mk_line_class.par[2].ToString();
            label3.Text = "Tc=" + mk_line_class.par[3].ToString();//coolant_T
            label4.Text = mk_line_class.par[4].ToString();
            label5.Text = "Ta=" + mk_line_class.par[5].ToString();//intake_T
            label6.Text = mk_line_class.par[6].ToString();
            label7.Text = mk_line_class.par[7].ToString();
            label8.Text = mk_line_class.par[8].ToString();
            label9.Text = mk_line_class.par[9].ToString();
            label10.Text = mk_line_class.par[10].ToString();
            label11.Text = mk_line_class.par[11].ToString();
            label12.Text = mk_line_class.par[12].ToString();
            label13.Text = mk_line_class.par[13].ToString();
            label14.Text = mk_line_class.par[14].ToString();
            label15.Text = mk_line_class.par[15].ToString();

            label30.Text = mk_line_class.par[16].ToString();
            label29.Text = mk_line_class.par[17].ToString();
            label28.Text = mk_line_class.par[18].ToString();
            label27.Text = mk_line_class.par[19].ToString();
            label26.Text = mk_line_class.par[20].ToString(); //\
            label25.Text = mk_line_class.par[21].ToString(); ///  RPM
            label24.Text = mk_line_class.par[22].ToString();
            label23.Text = mk_line_class.par[23].ToString();
            label22.Text = mk_line_class.par[24].ToString();
            label21.Text = mk_line_class.par[25].ToString();

            //float MP = (float)par[26]; MP *= 0x5e; MP /= (float)255.0; MP += (float)10.4; label20.Text = MP.ToString();//Manfold Presure
            label20.Text = mk_line_class.par[26].ToString();
            label19.Text = mk_line_class.par[27].ToString();
            label18.Text = mk_line_class.par[28].ToString();
            label17.Text = mk_line_class.par[29].ToString();
            label16.Text = "sp1=" + mk_line_class.par[30].ToString();//Spark1

            label45.Text = "sp2=" + mk_line_class.par[31].ToString();//Spark2
            label44.Text = "sp3=" + mk_line_class.par[32].ToString();//Spark3
            label43.Text = "sp4=" + mk_line_class.par[33].ToString();//Spark4
            label42.Text = mk_line_class.par[34].ToString();//\
            label41.Text = mk_line_class.par[35].ToString();///Injector1 Time
            label40.Text = mk_line_class.par[36].ToString();//\
            label39.Text = mk_line_class.par[37].ToString();///Injector2 Time
            label38.Text = mk_line_class.par[38].ToString();//\
            label37.Text = mk_line_class.par[39].ToString();///Injector3 Time
            label36.Text = mk_line_class.par[40].ToString();//\
            label35.Text = mk_line_class.par[41].ToString();///Injector4 Time
            label34.Text = mk_line_class.par[42].ToString();//\
            label33.Text = mk_line_class.par[43].ToString();///Injector Avr Time
            label32.Text = mk_line_class.par[44].ToString();
            label31.Text = mk_line_class.par[45].ToString();

            label60.Text = mk_line_class.par[46].ToString();
            label59.Text = mk_line_class.par[47].ToString();
            label58.Text = mk_line_class.par[48].ToString();
            label57.Text = mk_line_class.par[49].ToString();
            label56.Text = mk_line_class.par[50].ToString();
            label55.Text = mk_line_class.par[51].ToString();
            label54.Text = mk_line_class.par[52].ToString();
            label53.Text = mk_line_class.par[53].ToString();
            label52.Text = mk_line_class.par[54].ToString();
            label51.Text = mk_line_class.par[55].ToString();
            label50.Text = mk_line_class.par[56].ToString();
            label49.Text = mk_line_class.par[57].ToString();
            label48.Text = mk_line_class.par[58].ToString();
            label47.Text = mk_line_class.par[59].ToString();
            label46.Text = mk_line_class.par[60].ToString();

            label75.Text = mk_line_class.par[61].ToString();
            label74.Text = mk_line_class.par[62].ToString();
            label73.Text = mk_line_class.par[63].ToString();
            label72.Text = mk_line_class.par[64].ToString();
            label71.Text = mk_line_class.par[65].ToString();
            label70.Text = mk_line_class.par[66].ToString();
            label69.Text = mk_line_class.par[67].ToString();
            label68.Text = mk_line_class.par[68].ToString();
            label67.Text = mk_line_class.par[69].ToString();
            label66.Text = mk_line_class.par[70].ToString();
            label65.Text = mk_line_class.par[71].ToString();
            label64.Text = mk_line_class.par[72].ToString();
            label63.Text = mk_line_class.par[73].ToString();
            label62.Text = mk_line_class.par[74].ToString();
            label61.Text = mk_line_class.par[75].ToString();

            label90.Text = mk_line_class.par[76].ToString();
            label89.Text = mk_line_class.par[77].ToString();
            label88.Text = mk_line_class.par[78].ToString();
            label87.Text = mk_line_class.par[79].ToString();
            label86.Text = mk_line_class.par[80].ToString();
            label85.Text = mk_line_class.par[81].ToString();
            label84.Text = mk_line_class.par[82].ToString();
            label83.Text = mk_line_class.par[83].ToString();
            label82.Text = mk_line_class.par[84].ToString();
            label81.Text = mk_line_class.par[85].ToString();
            label80.Text = mk_line_class.par[86].ToString();
            label79.Text = mk_line_class.par[87].ToString();
            label78.Text = mk_line_class.par[88].ToString();
            label77.Text = mk_line_class.par[89].ToString();
            label76.Text = mk_line_class.par[90].ToString();

            int p1 = mk_line_class.par[21]; p1 *= 0xff; p1 += mk_line_class.par[20]; label92.Text = "RPM=" + p1.ToString();//RPM            
            int p2 = mk_line_class.par[35]; p2 *= 0xff; p2 += mk_line_class.par[34]; label93.Text = "J1=" + p2.ToString();//Injector1 Time
            int p3 = mk_line_class.par[37]; p3 *= 0xff; p3 += mk_line_class.par[36]; label94.Text = "J2=" + p3.ToString();//Injector2 Time
            int p4 = mk_line_class.par[39]; p4 *= 0xff; p4 += mk_line_class.par[38]; label95.Text = "J3=" + p4.ToString();//Injector3 Time
            int p5 = mk_line_class.par[41]; p5 *= 0xff; p5 += mk_line_class.par[40]; label96.Text = "J4=" + p5.ToString();//Injector4 Time
            int p6 = mk_line_class.par[43]; p6 *= 0xff; p6 += mk_line_class.par[42]; label97.Text = "Jv=" + p6.ToString();//Injector Avr Time

            int p7 = mk_line_class.par[17]; p7 *= 0xff; p7 += mk_line_class.par[16]; label98.Text = p7.ToString();
            int p8 = mk_line_class.par[60]; p8 *= 0xff; p8 += mk_line_class.par[59]; label99.Text = p8.ToString();
            int p9 = mk_line_class.par[58]; p9 *= 0xff; p9 += mk_line_class.par[57]; label100.Text = p9.ToString();
            int p10 = mk_line_class.par[74]; p10 *= 0xff; p10 += mk_line_class.par[73]; label101.Text = p10.ToString();
            int p11 = mk_line_class.par[23]; p11 *= 0xff; p11 += mk_line_class.par[22]; label102.Text = p11.ToString();
            int p12 = mk_line_class.par[13]; p12 *= 0xff; p12 += mk_line_class.par[12]; label103.Text = p12.ToString();

            if (mdashboard != null)
            {
                mdashboard.Update(mk_line_class.par[1], p1, mk_line_class.par[3], mk_line_class.par[5]);
                mdashboard.UpdateInjectorsTime(p2, p3, p4, p5, p6);
                mdashboard.UpdateSparks(mk_line_class.par[30], mk_line_class.par[31], mk_line_class.par[32], mk_line_class.par[33]);
            }
        }

        private void button28_Click(object sender, EventArgs e)
        {
            string str = "80,F1,11,53,61,1,81,BD,A3,6D,A6,6C,11,0,0,11,4F,7B,2,94,11,8F,19,75,0,A3,4,37,4,FF,0,59,0,0,0,9F,9F,9F,9F,26,5,1F,5,1D,5,1F,5,86,5,4C,0,0,0,0,0,0,1D,0,80,FA,6C,0,69,72,EA,5,E1,F,6A,0,0,0,0,9A,9,0,6A,4,8B,3,0,39,DB,F,0,0,0,52";
            mk_line_class.decoding(str);
            StreamShow();
        }

        private void showDashboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mdashboard == null)
            {
                mdashboard = new dashboard();
                mdashboard.Owner = this;
                mdashboard.FormClosed += mdashboard_FormClosed;
                mdashboard.Show();
            }
            else
            {
                if (mdashboard.WindowState == FormWindowState.Minimized)
                {
                    mdashboard.WindowState = FormWindowState.Normal;
                    mdashboard.Focus();
                }
            }
        }

        void mdashboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            mdashboard = null;
        }
        
       
 
    }
}

 