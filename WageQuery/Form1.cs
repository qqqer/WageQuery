using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;


namespace WageQuery
{
    public partial class Form1 : Form
    {
        bool ClosesByBtn = false;
        int remaining_time; 

        System.Windows.Forms.Timer T = new System.Windows.Forms.Timer{  Interval = 1000  };

        DataTable dt = new DataTable();
        public Form1()
        {
            InitializeComponent();

            this.ControlBox = false;
            dataGridView1.DataSource = dt;
            dataGridView1.RowsDefaultCellStyle.Font = new Font("宋体", 19, FontStyle.Regular);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 19, FontStyle.Regular);
            T.Tick += new EventHandler(T_Tick);
            DevClass.Init();
        }

        
        void T_Tick(object sender, EventArgs e)
        {
            button1.Text = "退出并注销(" + remaining_time-- + "秒)";
        }


        private int getUserId()
        {
            int userid = 0;
            while (true)
            {
                DevClass.Capture();

                label1.Text = "正在验证中...";
                label1.Refresh();

                userid = DevClass.Verify();

                if (userid == 0)
                {
                    label1.Text = "验证失败，请重新按入指纹";
                    label1.Refresh();
                }
                else break;
            }

            return userid;
        }


        private void Show(int userid)
        {
            string sql = "select top 1 * from userfile ";
            dt = SQLRepository.ExecuteQueryToDataTable(SQLRepository.test_strConn, sql);
            dataGridView1.DataSource = dt;


            remaining_time = int.Parse(ConfigurationManager.AppSettings["KEEP_TIME_SEC"]);
            button1.Text = "退出并注销(" + remaining_time-- + "秒)";


            panel2.Visible = true;


            T.Enabled = false;
            T.Stop();
            T.Enabled = true;
            T.Start();


            Thread Thread = new Thread(new ThreadStart(detect));
            Thread.IsBackground = true;
            Thread.Start();
        }


        private void detect()
        {
            ClosesByBtn = false;

            DateTime StartDate = DateTime.Now;
            int KEEP_TIME_SEC = int.Parse(ConfigurationManager.AppSettings["KEEP_TIME_SEC"]);

            while ((DateTime.Now - StartDate).Seconds < KEEP_TIME_SEC && ClosesByBtn == false) ;

            if (ClosesByBtn == false)
            {
                Action act = () => { button1_Click(null, null); };

                this.Invoke(act);
            }
        }


        private void panel2_VisibleChanged(object sender, EventArgs e)
        {
            if (panel2.Visible == false)
            {
                this.label1.Text = "请在指纹机上按入指纹";
                this.Refresh();

                int userid = getUserId();

                if (userid > 0)
                {
                    Show(userid);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            //拦截双击标题栏、移动窗体的系统消息  
            if (m.Msg != 0xA3 && m.Msg != 0x0003 && m.WParam != (IntPtr)0xF012)
            {
                base.WndProc(ref m);
            }
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            int userid = getUserId();

            if (userid > 0)
            {
                Show(userid);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {           
            ClosesByBtn = true;
            panel2.Visible = false;
        }
    }
}
