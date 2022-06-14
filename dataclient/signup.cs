using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dataclient
{
    public partial class signup : Form
    {
        ConnectDatabase connectdb = new ConnectDatabase();
        public signup()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                connectdb.F_SQL_Add("user_acc", "login_name,login_pwd,user_name_true", "'"+textBox1.Text+"',"+"'"+textBox2.Text+"',"+"'"+textBox3.Text+"'");
                MessageBox.Show("註冊成功!");
                this.Close();
            }
            catch(Exception ee)
            {
                MessageBox.Show("註冊失敗"+ee.ToString());
            }
            
        }
    }
}
