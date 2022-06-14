using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dataclient
{
    public partial class Best5_Form : Form
    {
        public int curr_x;
        public int curr_y;
        public bool isWndMove=false;
        public Best5_Form(Form1 Parentform)
        {
            InitializeComponent();
            this.Tag = Parentform;
        }

        public void F_AddListViewItems(String[] arr)
        {
            lv_Best5.Items.Clear();
            for (int i = 1; i < 6; i++)
            {
                ListViewItem lvi = new ListViewItem(arr[i]);
                lvi.SubItems.Add(arr[i + 5]);
                lvi.SubItems.Add(arr[i + 10]);
                lvi.SubItems.Add(arr[i + 15]);
                lv_Best5.Items.Add(lvi);
            }
        }


    }
}
