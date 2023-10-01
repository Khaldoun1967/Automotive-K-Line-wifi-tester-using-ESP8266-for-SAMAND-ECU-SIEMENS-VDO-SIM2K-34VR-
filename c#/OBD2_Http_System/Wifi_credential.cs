using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Easy_Queue_System
{
    public partial class Wifi_credential : Form
    {
        MainForm mainform;
        String SSID;
        String PSWD;
        bool EnableAbort = true;
        public Wifi_credential(MainForm F, String ssid)
        {
            mainform = F;
            SSID = ssid;
            InitializeComponent();
            this.ActiveControl = textBox2;
            textBox1.Text = SSID;
        }

        public void process(String str, int percent)
        {
    
            progressBar1.Value = percent;
            label1.Text = percent + "%";
            toolStripStatusLabel1.Text = str;
        }

        public void Finish()
        {
            EnableAbort = false;
            this.Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            PSWD = textBox2.Text;
            if ((PSWD.Length < 1) || (SSID.Length < 1))
            {
                MessageBox.Show("Bad Wifi LAN Credentials!", "Er", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string msg = "Are you sure you want the Easy Queuing System to be Connected to SSID: " + SSID;
            if (MessageBox.Show(msg, "Question",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                mainform.mWifiClient.Connect_to_Station_LAN(SSID, PSWD);
                button1.Visible = false;
                label1.Visible = true;
                progressBar1.Visible = true;
                textBox2.ReadOnly = true;
                this.ActiveControl = progressBar1;
            }
           
        }

        private void Wifi_credential_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(EnableAbort == true) mainform.mWifiClient.Abort_Connection();
        }
    }
}
