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
    public partial class dashboard : Form
    {
        public dashboard()
        {
            InitializeComponent();
            setup();
        }

        void setup()
        {
            RPM_PB.Minimum = 0;
            RPM_PB.Maximum = 5000;

            Coolant_PB.Minimum = 0;
            Coolant_PB.Maximum = 250;

            IntakeAir_PB.Minimum = 0;
            IntakeAir_PB.Maximum = 250;

            
            Spark1PB.Minimum = 0;
            Spark1PB.Maximum = 1000;

            Spark2PB.Minimum = 0;
            Spark2PB.Maximum = 1000;

            Spark3PB.Minimum = 0;
            Spark3PB.Maximum = 1000;

            Spark4PB.Minimum = 0;
            Spark4PB.Maximum = 1000;

        }

        public void UpdateInjectorsTime(int t1, int t2, int t3, int t4, int t_avr)
        {
            String S;
            float tt1 = t1;
            float tt2 = t2;
            float tt3 = t3;
            float tt4 = t4;
            float tt_av = t_avr;
            int Max = 10;

            Injector1_t.Minimum = 0;
            Injector1_t.Maximum = Max;

            Injector2_t.Minimum = 0;
            Injector2_t.Maximum = Max;

            Injector3_t.Minimum = 0;
            Injector3_t.Maximum = Max;

            Injector4_t.Minimum = 0;
            Injector4_t.Maximum = Max;

            Injector_t.Minimum = 0;
            Injector_t.Maximum = Max;     

            tt1 /= 250; if (tt1 > Max) tt1 = Max; S = String.Format("{0:F3}", tt1);
            Injector1_t_value.Text = S;
            Injector1_t.Value = (int)tt1;
            tt2 /= 250; if (tt2 > Max) tt2 = Max; S = String.Format("{0:F3}", tt2);
            Injector2_t_value.Text = S;
            Injector2_t.Value = (int)tt2;
            tt3 /= 250; if (tt3 > Max) tt3 = Max; S = String.Format("{0:F3}", tt3);
            Injector3_t_value.Text = S;
            Injector3_t.Value = (int)tt3;
            tt4 /= 250; if (tt4 > Max) tt4 = Max; S = String.Format("{0:F3}", tt4);
            Injector4_t_value.Text = S;
            Injector4_t.Value = (int)tt4;
            tt_av /= 250; if (tt_av > Max) tt_av = Max; S = String.Format("{0:F3}", tt_av);
            Injector_t_value.Text = S;
            Injector_t.Value = (int)tt_av;
        }

        public void UpdateSparks(int Spark1, int Spark2, int Spark3, int Spark4)
        {
            String S;
            int Max = 100;
            int Min = 0;

            Spark1PB.Minimum = Min;
            Spark1PB.Maximum = Max;

            Spark2PB.Minimum = Min;
            Spark2PB.Maximum = Max;

            Spark3PB.Minimum = Min;
            Spark3PB.Maximum = Max;

            Spark4PB.Minimum = Min;
            Spark4PB.Maximum = Max;      

            float Sp1 = (float)(0xff - Spark1); Sp1 *= (float)0.42857; Sp1 -= (float)27.857;
            float Sp2 = (float)(0xff - Spark2); Sp2 *= (float)0.42857; Sp2 -= (float)27.857;
            float Sp3 = (float)(0xff - Spark3); Sp3 *= (float)0.42857; Sp3 -= (float)27.857;
            float Sp4 = (float)(0xff - Spark4); Sp4 *= (float)0.42857; Sp4 -= (float)27.857;

            S = String.Format("{0:F3}", Sp1);
            Spark1value.Text = S;
            Spark1PB.Value = (int)Sp1;

            S = String.Format("{0:F3}", Sp2);
            Spark2value.Text = S;
            Spark2PB.Value = (int)Sp2;

            S = String.Format("{0:F3}", Sp3);
            Spark3value.Text = S;
            Spark3PB.Value = (int)Sp3;

            S = String.Format("{0:F3}", Sp4);
            Spark4value.Text = S;
            Spark4PB.Value = (int)Sp4;
           
        }
        public void Update(int Vbat, int rpm, int coolant_T, int intake_T)
        {
            float Vb = Vbat; Vb /= 10;
            Vbat_value.Text = Vb.ToString();

            RPM_PB.Value = rpm;
            RPM_value.Text = rpm.ToString();

            float Tc = (float)(0xff - coolant_T); Tc *= (float)0.5; Tc -= (float)12.0;
            float Ta = (float)(0xff - intake_T); Ta *= (float)0.5; Ta -= (float)12.0;

            Coolant_value.Text = Tc.ToString();
            Coolant_PB.Value = (int)Tc;

            IntakeAir_value.Text = Ta.ToString();
            IntakeAir_PB.Value = (int)Ta;       
        }
    
    }
}
