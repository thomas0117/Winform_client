using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using dataserver;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading;
using System.Globalization;
using MySql.Data.MySqlClient;


namespace dataclient
{
    public delegate void delegate_OE(Order_Entrust OE);
    public delegate void delegate_void();
    public delegate void delegate_PO(Pre_Order PO);
    public partial class Form1 : Form
    {
        //UserData userdata;
        String serverIp = "203.124.11.59";
        DateTime ServerTime;
        bool gettime = true;
        int port = 3766;
        ChatSocket client;
        User_acc user_acc;
        StrHandler msgHandler;
        ConnectDatabase connectdb = new ConnectDatabase();
        ReadWriteIni rw = new ReadWriteIni();
        DataTable m_dt_Stocks;
        public DataTable m_dt_Entrust;
        public DataTable m_dt_OPint;
        Best5_Form BF;
        Future_base future_base;
        int Order_type_int = 0;
        Pre_Order pre_order;
        String Order_type_string = "市價單";
        List<Order_Entrust> Order_List_OPint = new List<Order_Entrust>();
        List<Order_Entrust> Order_List_Closing = new List<Order_Entrust>();
        List<Pre_Order> Order_List_Pre_Order = new List<Pre_Order>();
        bool msflag = true;
        int ms = 10;
        int inputms = 10;
        #region 表單相關

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            CreateQuotationDataGridView();
            CreateEntrustDataGridView();
            F_CreateOPintDataGridView();
            btn_logout.Enabled = false;
            label8.Text = inputms.ToString();

            timer1.Start();
            timer2.Start();
        }

        public Form1()
        {
            InitializeComponent();
            //F_Build_user_future();
            msgHandler = this.addMsg;
        }

        //關閉form執行
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                //if (e.CloseReason != CloseReason.WindowsShutDown)
                //{
                //    if (MessageBox.Show("是否確定要關閉程式", "關閉程式", MessageBoxButtons.YesNo) == DialogResult.No)
                //    {
                //        e.Cancel = true;
                //    }
                if (client != null)
                {
                    client.socket.Close();
                    client = null;
                }
                timer1.Stop();
                //}

            }
            catch
            {

            }
        }

        //每秒執行的timer
        private void timer1_Tick(object sender, EventArgs e)
        {
            ServerTime = ServerTime.AddSeconds(1);
            label18.Text = ServerTime.ToString("HH:mm:ss");
            //ChangRowColor();

        }



        #region 按鈕事件

        //按下登入按鈕
        private void btn_login_Click(object sender, EventArgs e)
        {
            sendMsg();
        }

        //在MessageText按下按鍵事件
        private void messageText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')                                                          //按下Enter
                sendMsg();
        }

        //登出按鈕
        private void logout_Click(object sender, EventArgs e)
        {
            try
            {
                F_Logout();
                //client.send("logout_logout");
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }

        #endregion

        #region DataGridView相關

        //建立DataGridView
        private void CreateQuotationDataGridView()
        {
            m_dt_Stocks = CreateQuotationDataTable();

            dgv_Quotation.DataSource = m_dt_Stocks;
            dgv_Quotation.Columns[0].Width = 110;
            dgv_Quotation.Columns[1].Width = 160;
            dgv_Quotation.Columns[2].Width = 80;
            dgv_Quotation.Columns[3].Width = 80;
            dgv_Quotation.Columns[4].Width = 80;
            dgv_Quotation.Columns[5].Width = 80;
            dgv_Quotation.Columns[6].Width = 60;
            dgv_Quotation.Columns[7].Width = 80;
            dgv_Quotation.Columns[8].Width = 60;
            dgv_Quotation.Columns[9].Width = 80;
            for (int i = 2; i < 15; i++)
            {
                dgv_Quotation.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            dgv_Quotation.Columns[8].DefaultCellStyle.Format = "0.00";
            dgv_Quotation.DefaultCellStyle.Font = new Font("Tahoma", 12);
            dgv_Quotation.Columns[10].Visible = false;
            dgv_Quotation.Columns[11].Visible = false;
            dgv_Quotation.Columns[12].Visible = false;
            dgv_Quotation.Columns[13].Visible = false;
            dgv_Quotation.Columns[15].Visible = false;
            //  dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);
            dgv_Quotation.ClearSelection();

        }

        //建立QuotationDataTable
        private DataTable CreateQuotationDataTable()
        {
            DataTable myDataTable = new DataTable();

            DataColumn myDataColumn;


            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "代碼";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "股名";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Double");
            myDataColumn.ColumnName = "開盤";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Double");
            myDataColumn.ColumnName = "最高";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Double");
            myDataColumn.ColumnName = "最低";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Double");
            myDataColumn.ColumnName = "最新";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Int32");
            myDataColumn.ColumnName = "現量";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Double");
            myDataColumn.ColumnName = "漲跌";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Double");
            myDataColumn.ColumnName = "漲跌幅";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Double");
            myDataColumn.ColumnName = "昨收";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Double");
            myDataColumn.ColumnName = "買價";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Int32");
            myDataColumn.ColumnName = "買量";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Double");
            myDataColumn.ColumnName = "賣價";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Int32");
            myDataColumn.ColumnName = "賣量";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Int32");
            myDataColumn.ColumnName = "總量";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "內部序號";
            myDataTable.Columns.Add(myDataColumn);

            myDataTable.PrimaryKey = new DataColumn[] { myDataTable.Columns["代碼"] };


            return myDataTable;
        }


        //更新資料表
        private void OnUpDateDataRow(string[] arr)
        {
            string strStockNo = arr[1];

            DataRow drFind = m_dt_Stocks.Rows.Find(strStockNo);
            if (drFind == null)
            {
                try
                {
                    DataRow myDataRow = m_dt_Stocks.NewRow();

                    myDataRow["代碼"] = arr[1];
                    myDataRow["股名"] = arr[2];
                    myDataRow["開盤"] = Convert.ToInt32(arr[6]) / Math.Pow(10, Convert.ToDouble(arr[15]));
                    myDataRow["最高"] = Convert.ToInt32(arr[7]) / Math.Pow(10, Convert.ToDouble(arr[15]));
                    myDataRow["最低"] = Convert.ToInt32(arr[8]) / Math.Pow(10, Convert.ToDouble(arr[15]));
                    myDataRow["最新"] = Convert.ToInt32(arr[9]) / Math.Pow(10, Convert.ToDouble(arr[15]));
                    myDataRow["現量"] = Convert.ToInt32(arr[4]);
                    if (Convert.ToDouble(arr[9]) == 0)
                    {
                        myDataRow["漲跌"] = 0;
                    }
                    else
                    {
                        myDataRow["漲跌"] = (Convert.ToInt32(arr[9]) - Convert.ToInt32(arr[3])) / Math.Pow(10, Convert.ToDouble(arr[15]));
                    }

                    if (Convert.ToDouble(arr[9]) == 0)
                    {
                        myDataRow["漲跌幅"] = 0;
                    }
                    else
                    {
                        myDataRow["漲跌幅"] = Math.Round(Convert.ToDouble((Convert.ToDouble(arr[9]) - Convert.ToDouble(arr[3])) / Convert.ToDouble(arr[3])) * 100, 2, MidpointRounding.AwayFromZero);
                    }
                    myDataRow["昨收"] = Convert.ToInt32(arr[3]) / Math.Pow(10, Convert.ToDouble(arr[15]));
                    myDataRow["買價"] = Convert.ToInt32(arr[11]) / Math.Pow(10, Convert.ToDouble(arr[15]));
                    myDataRow["買量"] = Convert.ToInt32(arr[12]);
                    myDataRow["賣價"] = Convert.ToInt32(arr[13]) / Math.Pow(10, Convert.ToDouble(arr[15]));
                    myDataRow["賣量"] = Convert.ToInt32(arr[14]);
                    myDataRow["總量"] = Convert.ToInt32(arr[10]);
                    myDataRow["內部序號"] = arr[16];

                    m_dt_Stocks.Rows.Add(myDataRow);

                }
                catch (Exception ex)
                {
                    myLog.Write(ex.ToString());
                }
            }
            else
            {

                drFind["代碼"] = arr[1];
                drFind["股名"] = arr[2];
                drFind["開盤"] = Convert.ToInt32(arr[6]) / Math.Pow(10, Convert.ToDouble(arr[15]));
                drFind["最高"] = Convert.ToInt32(arr[7]) / Math.Pow(10, Convert.ToDouble(arr[15]));
                drFind["最低"] = Convert.ToInt32(arr[8]) / Math.Pow(10, Convert.ToDouble(arr[15]));
                drFind["最新"] = Convert.ToInt32(arr[9]) / Math.Pow(10, Convert.ToDouble(arr[15]));
                drFind["現量"] = Convert.ToInt32(arr[4]);
                if (Convert.ToDouble(arr[9]) == 0)
                {
                    drFind["漲跌"] = 0;
                }
                else
                {
                    drFind["漲跌"] = (Convert.ToInt32(arr[9]) - Convert.ToInt32(arr[3])) / Math.Pow(10, Convert.ToDouble(arr[15]));
                }
                if (Convert.ToDouble(arr[5]) == 0)
                {
                    drFind["漲跌幅"] = 0;
                }
                else
                {
                    drFind["漲跌幅"] = Math.Round(Convert.ToDouble((Convert.ToDouble(arr[9]) - Convert.ToDouble(arr[3])) / Convert.ToDouble(arr[3])) * 100, 2, MidpointRounding.AwayFromZero);
                }
                drFind["昨收"] = Convert.ToInt32(arr[3]) / Math.Pow(10, Convert.ToDouble(arr[15]));
                drFind["買價"] = Convert.ToInt32(arr[11]) / Math.Pow(10, Convert.ToDouble(arr[15]));
                drFind["買量"] = Convert.ToInt32(arr[12]);
                drFind["賣價"] = Convert.ToInt32(arr[13]) / Math.Pow(10, Convert.ToDouble(arr[15]));
                drFind["賣量"] = Convert.ToInt32(arr[14]);
                drFind["總量"] = Convert.ToInt32(arr[10]);
                drFind["內部序號"] = arr[16];

                //當更新報價的資料 剛好和使用者選擇的是同一個商品則新增商品側邊欄位
                if (strStockNo == dgv_Quotation.SelectedRows[0].Cells[0].Value.ToString())
                {
                    F_Add_lv_detail_item(arr);
                    lb_price.Text = dgv_Quotation.SelectedRows[0].Cells[5].Value.ToString();
                }
            }
        }

        private void updateStock(string[] arr)
        {
            string strStockNo = arr[0];

            DataRow drFind = m_dt_Stocks.Rows.Find(strStockNo);
            if (drFind == null)
            {
                try
                {
                    DataRow myDataRow = m_dt_Stocks.NewRow();

                    myDataRow["代碼"] = arr[0];
                    myDataRow["股名"] = arr[1];
                    myDataRow["最新"] = Convert.ToInt32(arr[2]);
                    myDataRow["漲跌幅"] = arr[3];

                    m_dt_Stocks.Rows.Add(myDataRow);
                }
                catch (Exception ex)
                {
                    myLog.Write(ex.ToString());
                }
            }
            else
            {
                drFind["代碼"] = arr[0];
                drFind["股名"] = arr[1];
                drFind["最新"] = arr[2];
                drFind["漲跌幅"] = arr[3];
            }
        }

        //解決DataGridView閃爍
        class DoubleBufferDataGridView : DataGridView
        {
            public DoubleBufferDataGridView()
            {
                SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
                UpdateStyles();
            }
        }

        //根據漲跌改變顏色(漲紅跌綠)
        private void ChangRowColor()
        {
            if (dgv_Quotation.Rows.Count < 1) return;

            foreach (DataGridViewRow Row in dgv_Quotation.Rows)
            {
                if ((double)(Row.Cells[7].Value) > 0)
                {
                    Row.DefaultCellStyle.ForeColor = Color.Red;
                }
                else if ((double)(Row.Cells[7].Value) < 0)
                {
                    Row.DefaultCellStyle.ForeColor = Color.Green;
                }
                else
                {
                    Row.DefaultCellStyle.ForeColor = Color.Black;
                }
            }
        }

        //當選擇的row改變的事件
        private void dgv_Quotation_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                //lv_detail.Items.Clear();
                //lv_best5.Items.Clear();

                //lb_stockname.Text = dgv_Quotation.SelectedRows[0].Cells[1].Value.ToString();
                //textBox1.Text = dgv_Quotation.SelectedRows[0].Cells[5].Value.ToString();
                //lb_price.Text = dgv_Quotation.SelectedRows[0].Cells[5].Value.ToString();

                //client.send("Request_Best5," + dgv_Quotation.SelectedRows[0].Cells[0].Value.ToString());
                //String[] future_data = connectdb.F_SQL_Select_ReturnData("future_base", "future_source", "=", "'" + dgv_Quotation.SelectedRows[0].Cells[0].Value.ToString() + "'");
                //future_base = new Future_base(future_data);
            }
            catch
            {

            }
        }

        //建立EntrustDataTable
        private DataTable CreateEntrustDataTable()
        {
            DataTable myDataTable = new DataTable();

            DataColumn myDataColumn;

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "委託書號";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "商品";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "買賣別";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "數量";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "型別";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "委託價";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "委託時間";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "成交價";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "成交時間";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "狀態";
            myDataTable.Columns.Add(myDataColumn);

            myDataTable.PrimaryKey = new DataColumn[] { myDataTable.Columns["委託書號"] };


            return myDataTable;
        }

        public void NewEntrsutDataGridView(Pre_Order pre)
        {
            try
            {
                DataRow myDataRow = m_dt_Entrust.NewRow();

                myDataRow["委託書號"] = pre.pre_id.ToString();
                myDataRow["商品"] = pre.futrue_name;
                if (pre.buy_type == 0)
                {
                    myDataRow["買賣別"] = "多";
                }
                else
                {
                    myDataRow["買賣別"] = "空";
                }
                myDataRow["委託價"] = pre.order_price;
                myDataRow["數量"] = pre.order_num.ToString();
                myDataRow["成交價"] = "";
                myDataRow["委託時間"] = DateTime.Now.ToString("HH:mm:ss");
                if (pre.type_order == 0)
                {
                    myDataRow["型別"] = "市價單";
                }
                else if (pre.type_order == 2)
                {
                    myDataRow["型別"] = "限價單";
                }
                myDataRow["成交時間"] = "";
                myDataRow["狀態"] = "等待中";

                m_dt_Entrust.Rows.Add(myDataRow);

            }
            catch (Exception ex)
            {
                myLog.Write(ex.ToString());
            }
        }

        //建立Entrust DGV
        private void CreateEntrustDataGridView()
        {
            m_dt_Entrust = CreateEntrustDataTable();

            DataGridViewButtonColumn myDataButtonColumn = new DataGridViewButtonColumn();

            dgv_entrust.DataSource = m_dt_Entrust;

            myDataButtonColumn.HeaderText = "操作";
            myDataButtonColumn.Text = "取消";
            myDataButtonColumn.UseColumnTextForButtonValue = true;
            dgv_entrust.Columns.Add(myDataButtonColumn);

            dgv_entrust.Columns[0].Width = 80;
            dgv_entrust.Columns[1].Width = 160;
            dgv_entrust.Columns[2].Width = 70;
            dgv_entrust.Columns[3].Width = 80;
            dgv_entrust.Columns[4].Width = 80;
            dgv_entrust.Columns[5].Width = 80;
            dgv_entrust.Columns[6].Width = 100;
            dgv_entrust.Columns[7].Width = 80;
            dgv_entrust.Columns[8].Width = 100;
            dgv_entrust.Columns[9].Width = 80;
            dgv_entrust.Columns[10].Width = 60;
            //for (int i = 0; i < 11; i++)
            //{
            //    dgv_entrust.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            //}
            //dgv_entrust.Columns[8].DefaultCellStyle.Format = "0.00";
            dgv_entrust.DefaultCellStyle.Font = new Font("Tahoma", 11);

            //  dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);
            dgv_entrust.ClearSelection();

        }

        public void F_Update_Entrust_DataGridView()
        {
            if (this.InvokeRequired)
            {
                delegate_void Update_Entrust_DataGridView = new delegate_void(F_Update_Entrust_DataGridView);
                this.Invoke(Update_Entrust_DataGridView);
            }
            else
            {
                try
                {
                    foreach (Pre_Order pre in Order_List_Pre_Order)
                    {

                        DataRow DrFind = m_dt_Entrust.Rows.Find(pre.pre_id);

                        if (DrFind != null)
                        {
                            DrFind["委託書號"] = pre.pre_id.ToString();
                            DrFind["商品"] = pre.futrue_name;
                            if (pre.buy_type == 0)
                            {
                                DrFind["買賣別"] = "多";
                            }
                            else
                            {
                                DrFind["買賣別"] = "空";
                            }
                            DrFind["委託價"] = pre.order_price;
                            DrFind["數量"] = pre.order_num.ToString();
                            DrFind["成交價"] = "";
                            DrFind["委託時間"] = DateTime.Now.ToString("HH:mm:ss");
                            if (pre.type_order == 0)
                            {
                                DrFind["型別"] = "市價單";
                            }
                            else if (pre.type_order == 2)
                            {
                                DrFind["型別"] = "限價單";
                            }
                            DrFind["成交時間"] = "";
                            DrFind["狀態"] = "等待中";


                        }
                        else
                        {
                            DataRow myDataRow = m_dt_Entrust.NewRow();

                            myDataRow["委託書號"] = pre.pre_id.ToString();
                            myDataRow["商品"] = pre.futrue_name;
                            if (pre.buy_type == 0)
                            {
                                myDataRow["買賣別"] = "多";
                            }
                            else
                            {
                                myDataRow["買賣別"] = "空";
                            }
                            myDataRow["委託價"] = pre.order_price;
                            myDataRow["數量"] = pre.order_num.ToString();
                            myDataRow["成交價"] = "";
                            myDataRow["委託時間"] = DateTime.Now.ToString("HH:mm:ss");
                            if (pre.type_order == 0)
                            {
                                myDataRow["型別"] = "市價單";
                            }
                            else if (pre.type_order == 2)
                            {
                                myDataRow["型別"] = "限價單";
                            }
                            myDataRow["成交時間"] = "";
                            myDataRow["狀態"] = "等待中";

                            //Thread DoEntrust_Thread = new Thread(new ParameterizedThreadStart(F_DoEntrust));              //如果這筆委託是新的就執行成訂單
                            //DoEntrust_Thread.Start(pre);
                            //DoEntrust_Thread.IsBackground = true;

                            m_dt_Entrust.Rows.Add(myDataRow);
                        }
                    }

                    //F_remove_Pre_Order_dgvRow();
                }
                catch (Exception ex)
                {
                    myLog.Write(ex.ToString());
                }
            }
        }

        //public void F_remove_Pre_Order_dgvRow()
        //{
        //    bool remove_row = true;

        //    foreach (DataRow dr in m_dt_OPint.Rows)
        //    {
        //        remove_row = true;
        //        foreach (Pre_Order pre in Order_List_Pre_Order)
        //        {
        //            if (pre.pre_id.ToString() == dr["委託書號"].ToString())
        //            {
        //                remove_row = false;                              //如果Order_List裡面有和m_dt_OPint一樣的訂單編號則不移除
        //                break;
        //            }
        //        }

        //        if (remove_row)
        //        {
        //            m_dt_Entrust.Rows.Remove(dr);
        //        }
        //    }
        //}


        private DataTable F_CreateOPintDataTable()
        {
            DataTable myDataTable = new DataTable();

            DataColumn myDataColumn;

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "訂單編號";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "商品";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "買賣別";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "口數";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "未平口數";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "型別";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "成交價";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "手續費";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "虧損點數";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "倒限(利)";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "未平損益";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "點數";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "天數";
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "狀態";
            myDataTable.Columns.Add(myDataColumn);

            myDataTable.PrimaryKey = new DataColumn[] { myDataTable.Columns["訂單編號"] };


            return myDataTable;
        }

        private void F_CreateOPintDataGridView()
        {
            m_dt_OPint = F_CreateOPintDataTable();

            DataGridViewButtonColumn myDataButtonColumn = new DataGridViewButtonColumn();

            dgv_OPint.DataSource = m_dt_OPint;

            myDataButtonColumn.HeaderText = "操作";
            myDataButtonColumn.Text = "平倉";
            myDataButtonColumn.UseColumnTextForButtonValue = true;
            dgv_OPint.Columns.Add(myDataButtonColumn);


            dgv_OPint.Columns[0].Width = 80;
            dgv_OPint.Columns[1].Width = 160;
            dgv_OPint.Columns[2].Width = 80;
            dgv_OPint.Columns[3].Width = 60;
            dgv_OPint.Columns[4].Width = 80;
            dgv_OPint.Columns[5].Width = 80;
            dgv_OPint.Columns[6].Width = 80;
            dgv_OPint.Columns[7].Width = 80;
            dgv_OPint.Columns[8].Width = 80;
            dgv_OPint.Columns[9].Width = 80;
            dgv_OPint.Columns[10].Width = 80;
            dgv_OPint.Columns[11].Width = 80;
            dgv_OPint.Columns[12].Width = 80;
            dgv_OPint.Columns[13].Width = 80;
            dgv_OPint.Columns[14].Width = 60;

            dgv_OPint.DefaultCellStyle.Font = new Font("Tahoma", 11);
            dgv_OPint.ClearSelection();

        }

        //更新未平倉的內容
        public void F_Update_OPint_DataGridView()
        {
            if (this.InvokeRequired)
            {
                delegate_void Update_OPint_DataGridView = new delegate_void(F_Update_OPint_DataGridView);
                this.Invoke(Update_OPint_DataGridView);
            }
            else
            {
                try
                {
                    foreach (Order_Entrust OE in Order_List_OPint)
                    {

                        DataRow DrFind = m_dt_OPint.Rows.Find(OE.order_id);
                        DataRow DrStock = m_dt_Stocks.Rows.Find(OE.future_source);
                        double profit_now=0;

                        if (DrStock != null)
                        {
                            if (OE.type_buy == 0)
                            {
                                profit_now = (Convert.ToDouble(DrStock["最新"]) - OE.order_price)*OE.sell_num;
                            }
                            else
                            {
                                profit_now = OE.order_price - Convert.ToDouble(DrStock["最新"]) * OE.sell_num;
                            }
                        }


                        if (DrFind != null)
                        {

                            DrFind["訂單編號"] = OE.order_id.ToString();
                            DrFind["商品"] = OE.future_name;
                            if (OE.type_buy == 0)
                            {
                                DrFind["買賣別"] = "多";
                            }
                            else
                            {
                                DrFind["買賣別"] = "空";
                            }
                            DrFind["口數"] = OE.order_num.ToString();
                            DrFind["未平口數"] = OE.sell_num.ToString();
                            if (OE.type_order == 0)
                            {
                                DrFind["型別"] = "市價";
                            }
                            else
                            {
                                DrFind["型別"] = "限價";
                            }
                            DrFind["成交價"] = OE.order_price.ToString();
                            DrFind["手續費"] = OE.cost_stay.ToString();
                            DrFind["虧損點數"] = "";
                            DrFind["倒限(利)"] = "";
                            DrFind["未平損益"] = profit_now.ToString();
                            DrFind["點數"] = "";
                            DrFind["狀態"] = "等待中";

                        }
                        else
                        {
                            DataRow myDataRow = m_dt_OPint.NewRow();
             
                            myDataRow["訂單編號"] = OE.order_id.ToString();
                            myDataRow["商品"] = OE.future_name;
                            if (OE.type_buy == 0)
                            {
                                myDataRow["買賣別"] = "多";
                            }
                            else
                            {
                                myDataRow["買賣別"] = "空";
                            }
                            myDataRow["口數"] = OE.order_num.ToString();
                            myDataRow["未平口數"] = OE.sell_num.ToString();
                            if (OE.type_order == 0)
                            {
                                myDataRow["型別"] = "市價";
                            }
                            else
                            {
                                myDataRow["型別"] = "限價";
                            }
                            myDataRow["成交價"] = OE.order_price.ToString();
                            myDataRow["手續費"] = OE.cost_stay.ToString();
                            myDataRow["虧損點數"] = "";
                            myDataRow["倒限(利)"] = "";
                            myDataRow["未平損益"] = profit_now.ToString();
                            myDataRow["點數"] = "";
                            myDataRow["狀態"] = "等待中";

                            m_dt_OPint.Rows.Add(myDataRow);
                        }
                    }

                    F_remove_OPint_dgvRow();
                }
                catch (Exception ex)
                {
                    myLog.Write(ex.ToString());
                }
            }
        }

        //找到未平倉裡面已經平倉的並移除
        public void F_remove_OPint_dgvRow()
        {
            bool remove_row = true;

            foreach (DataRow dr in m_dt_OPint.Rows)
            {
                remove_row = true;
                foreach (Order_Entrust OE in Order_List_OPint)
                {
                    if (OE.order_id.ToString() == dr["訂單編號"].ToString())
                    {
                        remove_row = false;                              //如果Order_List裡面有和m_dt_OPint一樣的訂單編號則不移除
                        break;
                    }
                }

                if (remove_row)
                {
                    m_dt_OPint.Rows.Remove(dr);
                }
            }
        }

        #endregion

        #region ListView相關

        public void F_Add_lv_detail_item(String[] arr)
        {
            ListViewItem lvi = new ListViewItem(arr[0].Substring(arr[0].IndexOf('-') + 1));
            lvi.SubItems.Add((Convert.ToInt32(arr[9]) / Math.Pow(10, Convert.ToDouble(arr[15]))).ToString());
            lvi.SubItems.Add(arr[4]);
            lv_detail.Items.Add(lvi);
        }

        public void F_Add_lv_best5_item(String[] arr)
        {
            lv_best5.Items.Clear();
            for (int i = 1; i < 6; i++)
            {
                ListViewItem lvi = new ListViewItem(arr[i]);
                lvi.SubItems.Add(arr[i + 5]);
                lvi.SubItems.Add(arr[i + 10]);
                lvi.SubItems.Add(arr[i + 15]);
                lv_best5.Items.Add(lvi);
            }
        }

        public void F_Add_lv_closing(Order_Entrust OE)
        {
            if (this.InvokeRequired)
            {
                delegate_OE Add_lv_closing = new delegate_OE(F_Add_lv_closing);
                this.Invoke(Add_lv_closing, OE);
            }
            else
            {
                ListViewItem lvi = new ListViewItem(OE.future_name);
                lvi.SubItems.Add(OE.order_id.ToString());
                lvi.SubItems.Add(OE.sell_id.ToString());
                if (OE.type_order == 0)
                {
                    lvi.SubItems.Add("市價單");
                }
                else
                {
                    lvi.SubItems.Add("限價單");
                }
                lvi.SubItems.Add("0");
                lvi.SubItems.Add(OE.order_num.ToString());
                if (OE.type_buy == 0)
                {
                    lvi.SubItems.Add("多");
                }
                else
                {
                    lvi.SubItems.Add("空");
                }
                lvi.SubItems.Add(OE.order_price.ToString());
                lvi.SubItems.Add(OE.sell_price.ToString());
                lvi.SubItems.Add(OE.order_time);
                lvi.SubItems.Add(OE.sell_time);
                lvi.SubItems.Add("0");
                lvi.SubItems.Add("0");
                lvi.SubItems.Add(OE.cost_trade.ToString());
                lvi.SubItems.Add(OE.price_profit.ToString());
                lv_closing.Items.Add(lvi);
            }
        }

        private void F_Refresh_lv_closing()
        {
            lv_closing.Items.Clear();                   //清空lv_closing

            foreach (Order_Entrust OE in Order_List_Closing)
            {
                F_Add_lv_closing(OE);                   //新增已平倉
            }
        }

        class DoubleBufferListView : ListView
        {
            public DoubleBufferListView()
            {
                SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
                UpdateStyles();
            }
        }


        #endregion


        #endregion



        //使用者帳號密碼處理
        public string user()
        {
            if (port == 9002)
            {
                return "Test1" + ":" + "qazxsw!@";
            }
            return '$' + nameTextbox.Text.Trim() + "@" + passwordTextbox.Text.Trim();
        }

        //送出訊息判斷
        public void sendMsg()
        {
            if (nameTextbox.Text.Length == 0)                                 //ID沒填入東西時    
            {
                MessageBox.Show("請輸入使用者名稱!");
            }
            else if (passwordTextbox.Text.Length == 0)
            {
                MessageBox.Show("請輸入密碼!");
            }
            else
            {
                try
                {
                    //if ((connectdb.F_SQL_Login("SELECT * FROM user_acc  WHERE   login_name = '" + nameTextbox.Text + "'and login_pwd ='" + passwordTextbox.Text + "'")))
                    //{
                        //跟server連線
                        client = ChatSocket.connect(serverIp, port);
                        if(client == null)
                        {
                            MessageBox.Show("伺服器未啟動 請更換伺服器");
                        }
                        else
                        {
                            client.newListener(processMsgComeIn);
                            client.send(user());
                        }


                        //F_Login_Success();
                    //}
                    //else
                    //{
                    //    MessageBox.Show("帳號或密碼錯誤");
                    //}
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("伺服器未啟動 請更換伺服器" + ex.Number);
                }
            }
        }

        //使用invoke讓有視窗控制的addMsg可以正常執行
        public string processMsgComeIn(string msg)
        {
            this.Invoke(msgHandler, new Object[] { msg });
            return "OK";
        }

        //當收到訊息時做的處理
        public string addMsg(string msg)
        {
            string[] arr;
            if (msg != null)
            {
                arr = msg.Split(',');
                if (arr.Length <= 1)
                {
                    try
                    {
                        if (String.Compare(arr[0], "connect_success") == 0)
                        {

                        }
                        else if (String.Compare(arr[0], "double_connect") == 0)
                        {
                            F_Logout();
                            MessageBox.Show("有相同的帳號登入,您已被斷開連線");
                        }
                    }
                    catch (Exception ee)
                    {
                        myLog.Write(ee.ToString());
                    }
                }
                else if (arr.Length == 18)
                {
                    try
                    {
                        OnUpDateDataRow(arr);

                        if (gettime == true)
                        {
                            gettime = false;
                            DateTime.TryParseExact(arr[0], "yyyyMMdd-HH:mm:ss", null, DateTimeStyles.None, out ServerTime);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                else if (arr.Length == 22)
                {
                    F_Add_lv_best5_item(arr);
                }
                else if (arr.Length == 4)
                {
                    try
                    {
                        if (msflag)
                        {
                            updateStock(arr);
                            msflag = false;
                        }             
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }

            }
            return "OK";
        }


        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (radioButton1.Checked)
                {
                    textBox1.Visible = false;
                    Order_type_int = 0;
                    Order_type_string = "市價單";
                    lb_cost.Text = (Convert.ToDouble(dgv_Quotation.SelectedRows[0].Cells[5].Value) * Convert.ToInt32(numericUpDown1.Value)).ToString();
                }
                else
                {
                    textBox1.Visible = true;
                    Order_type_int = 2;
                    Order_type_string = "限價單";
                    lb_cost.Text = (Convert.ToDouble(textBox1.Text) * Convert.ToInt32(numericUpDown1.Value)).ToString();
                }
            }
            catch
            {

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            signup s = new signup();
            s.Show();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            serverIp = comboBox1.Text.Substring(0, comboBox1.Text.IndexOf(':'));
            port = Convert.ToInt32(comboBox1.Text.Substring(comboBox1.Text.IndexOf(':') + 1));
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 | (int)e.KeyChar > 57) & (int)e.KeyChar != 8)
            {
                e.Handled = true;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                lb_cost.Text = (Convert.ToDouble(textBox1.Text) * Convert.ToInt32(numericUpDown1.Value)).ToString();
            }
            catch
            {

            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                lb_cost.Text = (Convert.ToDouble(textBox1.Text) * Convert.ToInt32(numericUpDown1.Value)).ToString();
            }
            catch
            {

            }
        }

        public void F_Login_Success()
        {
            String[] UserData = connectdb.F_SQL_Select_ReturnData("user_acc", "login_name", "=", "'" + nameTextbox.Text + "'");  //從資料庫撈此ID的資料
            user_acc = new User_acc(UserData);                                                                                   //把資料丟到class user_acc裡

            if (user_acc != null)
            {
                this.backgroundWorker1.RunWorkerAsync();
            }

            if (user_acc != null)
            {
                Thread Refresh_Enstruct = new Thread(F_Refresh_Enstruct);
                Refresh_Enstruct.IsBackground = true;
                Refresh_Enstruct.Start();
            }

            //改變畫面
            lb_username.Text = user_acc.login_name;
            lb_service_staff.Text = user_acc.service_staff;
            lb_service_hotline.Text = user_acc.service_hotline;
            lb_money_default.Text = user_acc.money_default.ToString();
            lb_money.Text = user_acc.money_balance.ToString();
            nameTextbox.Enabled = false;
            passwordTextbox.Enabled = false;
            comboBox1.Enabled = false;
            btn_login.Enabled = false;
            btn_logout.Enabled = true;
            btn_signup.Enabled = false;

        }

        public void F_Refresh_OPint(BackgroundWorker worker, DoWorkEventArgs e)
        {
            ConnectDatabase CDB = new ConnectDatabase();

            try
            {
                while (true)
                {
                    Order_List_OPint = CDB.F_SQL_SelectEntrust(user_acc.user_id, 1);                                                       //抓取用戶的未平倉訂單        

                    F_Update_OPint_DataGridView();                                                                                  //更新未平倉欄位

                    Thread.Sleep(5000);
                }
            }
            catch (Exception ee)
            {
                myLog.Write(ee.ToString());
            }
        }

        public void F_Logout()
        {
            client.socket.Close();
            client = null;

            MessageBox.Show("登出成功");

            nameTextbox.Enabled = true;
            passwordTextbox.Enabled = true;
            comboBox1.Enabled = true;
            btn_login.Enabled = true;
            btn_logout.Enabled = false;
            btn_signup.Enabled = true;
            lb_username.Text = "";
            lb_money.Text = "";
            lb_service_staff.Text = "";
            lb_service_hotline.Text = "";

            lb_money_default.Text = "";

            lv_best5.Items.Clear();
            lv_detail.Items.Clear();
            DataTable dt = (DataTable)dgv_Quotation.DataSource;
            dt.Rows.Clear();
            dgv_Quotation.DataSource = dt;

            dt = (DataTable)dgv_entrust.DataSource;
            dt.Rows.Clear();
            dgv_entrust.DataSource = dt;

        }

        //執行委託
        public void F_DoEntrust(object ob)
        {
            Future_base s = future_base;
            ConnectDatabase CDB = new ConnectDatabase();
            Thread.Sleep(3000);
            Pre_Order pre = (Pre_Order)ob;
            DataRow EntrustFind = m_dt_Entrust.Rows.Find(pre.pre_id.ToString());
            Order_Entrust OE;
            int id;

            while (true)
            {
                DataRow StocksFind = m_dt_Stocks.Rows.Find(pre.futrue_source);

                if (EntrustFind == null)        //取消委託就退出
                {
                    break;
                }

                //市價單
                if (pre.type_order == 0)
                {
                    if (EntrustFind != null && StocksFind != null)
                    {
                        Double Now_Price = Convert.ToDouble(StocksFind["最新"]);

                        EntrustFind["狀態"] = "交易成功";
                        EntrustFind["成交價"] = StocksFind["最新"];
                        EntrustFind["成交時間"] = DateTime.Now.ToString("HH:mm:ss");
                        CDB.F_SQL_command("Update  pre_order  SET  entry_price  =" + EntrustFind["成交價"].ToString() + ", entry_time ='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',state  = 1  WHERE  pre_id  =" + pre.pre_id.ToString());
                        pre.entry_price = Convert.ToDouble(StocksFind["最新"]);

                        id = CDB.F_SQL_Add_ReturnID("order_entrust", "user_id,future_id,future_source,future_name,cost_trade,cost_stay,order_num,order_status,order_time,sell_num,type_buy,type_order,order_price,is_day_trade",
                             pre.user_id.ToString() + "," + pre.future_id.ToString() + ",'" + pre.futrue_source + "','" + pre.futrue_name + "'," + future_base.trade_cost.ToString() + "," + future_base.stay_cost.ToString()
                             + "," + pre.order_num.ToString() + "," + "1" + ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + pre.order_num.ToString() + "," + pre.buy_type.ToString() + "," + pre.type_order.ToString() + "," + pre.entry_price.ToString() + "," + pre.is_day_trade.ToString());
                        F_Closing(pre, id);
                        String[] arr = CDB.F_SQL_Select_ReturnData("order_entrust", "order_id", "=", id.ToString());
                        OE = new Order_Entrust(arr);
                        //if (OE.order_status == 1)
                        //{
                        //    F_Update_OPint_DataGridView(OE);
                        //}
                        MessageBox.Show("交易成功");
                        break;
                    }
                }
                else     //限價單
                {
                    
                    if (EntrustFind != null && StocksFind != null)
                    {
                        Double Now_Price = Convert.ToDouble(StocksFind["最新"]);
                        if (pre.buy_type == 0)  //多單
                        {
                            if (Now_Price <= pre.order_price)
                            {
                                id = CDB.F_SQL_Add_ReturnID("order_entrust", "user_id,future_id,future_source,future_name,cost_trade,cost_stay,order_num,order_status,order_time,sell_num,type_buy,type_order,order_price,is_day_trade",
                                    pre.user_id.ToString() + "," + pre.future_id.ToString() + ",'" + pre.futrue_source + "','" + pre.futrue_name + "'," + future_base.trade_cost.ToString() + "," + future_base.stay_cost.ToString()
                                    + "," + pre.order_num.ToString() + "," + "1" + ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + pre.order_num.ToString() + "," + pre.buy_type.ToString() + "," + pre.type_order.ToString() + "," + Now_Price.ToString() + "," + pre.is_day_trade.ToString());
                                EntrustFind["狀態"] = "交易成功";
                                EntrustFind["成交價"] = Now_Price.ToString();
                                EntrustFind["成交時間"] = DateTime.Now.ToString("HH:mm:ss");
                                CDB.F_SQL_command("Update  pre_order  SET  entry_price  =" + Now_Price.ToString() + ", entry_time ='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',state  = 1  WHERE  pre_id  =" + pre.pre_id.ToString());
                                pre.entry_price = Now_Price;
                                F_Closing(pre, id);
                                String[] arr = CDB.F_SQL_Select_ReturnData("order_entrust", "order_id", "=", id.ToString());
                                OE = new Order_Entrust(arr);
                                //if (OE.order_status == 1)
                                //{
                                //    F_Update_OPint_DataGridView(OE);
                                //}
                                MessageBox.Show("交易成功");
                                break;
                            }
                        }
                        else if (pre.buy_type == 1)  //空單
                        {
                            if (Now_Price >= pre.order_price)
                            {
                                id = CDB.F_SQL_Add_ReturnID("order_entrust", "user_id,future_id,future_source,future_name,cost_trade,cost_stay,order_num,order_status,order_time,sell_num,type_buy,type_order,order_price,is_day_trade",
                                    pre.user_id.ToString() + "," + pre.future_id.ToString() + ",'" + pre.futrue_source + "','" + pre.futrue_name + "'," + future_base.trade_cost.ToString() + "," + future_base.stay_cost.ToString()
                                    + "," + pre.order_num.ToString() + "," + "1" + ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + pre.order_num.ToString() + "," + pre.buy_type.ToString() + "," + pre.type_order.ToString() + "," + Now_Price.ToString() + "," + pre.is_day_trade.ToString());
                                EntrustFind["狀態"] = "交易成功";
                                EntrustFind["成交價"] = Now_Price.ToString();
                                EntrustFind["成交時間"] = DateTime.Now.ToString("HH:mm:ss");
                                CDB.F_SQL_command("Update  pre_order  SET  entry_price  =" + Now_Price.ToString() + ", entry_time ='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',state  = 1  WHERE  pre_id  =" + pre.pre_id.ToString());
                                pre.entry_price = Now_Price;
                                F_Closing(pre, id);
                                String[] arr = CDB.F_SQL_Select_ReturnData("order_entrust", "order_id", "=", id.ToString());
                                OE = new Order_Entrust(arr);
                                //if (OE.order_status == 1)
                                //{
                                //    F_Update_OPint_DataGridView(OE);
                                //}
                                //MessageBox.Show("交易成功");
                                break;
                            }
                        }
                    }
                    Thread.Sleep(1);
                }
            }

        }

        //檢查有無平倉
        public void F_Closing(Pre_Order pre, int id)
        {
            String[] entrust;
            while (pre.order_num != 0)
            {
                //找尋反向單
                entrust = connectdb.F_SQL_Select_ReturnData("order_entrust", "user_id", "=", pre.user_id.ToString() + " and future_id =" + pre.future_id.ToString() + " and type_buy != " + pre.buy_type.ToString() + " and order_status != 2");

                if (entrust != null)
                {
                    //如果委託的口數大於反向的未平倉口數
                    if (pre.order_num > Convert.ToInt32(entrust[22]))
                    {
                        double profit = Convert.ToDouble(entrust[22]) * (pre.entry_price - Convert.ToDouble(entrust[11]));
                        pre.order_num -= Convert.ToInt32(entrust[22]);
                        connectdb.F_SQL_command("Update  order_entrust  SET  sell_num  = 0 , order_status =  2,sell_price = " + pre.entry_price.ToString() + ", sell_time= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',price_profit=" + (profit + Convert.ToDouble(entrust[24])).ToString() + ",sell_id=" + id.ToString() + " WHERE  order_id  =" + entrust[0]);                  //把過去的舊單平倉
                        connectdb.F_SQL_command("Update  order_entrust  SET  sell_num  =" + pre.order_num.ToString() + ",price_profit=" + profit.ToString() + " WHERE  order_id  =" + id.ToString());      //扣掉新單的未平倉口數
                        //F_remove_OPint_dgvRow(Convert.ToInt32(entrust[0]));
                    }
                    else if (pre.order_num < Convert.ToInt32(entrust[22]))                //如果委託的口數小於反向的未平倉口數
                    {
                        double profit = pre.order_num * (pre.entry_price - Convert.ToDouble(entrust[11]));
                        entrust[22] = (Convert.ToInt32(entrust[22]) - pre.order_num).ToString();
                        pre.order_num = 0;
                        connectdb.F_SQL_command("Update  order_entrust  SET  sell_num  = " + entrust[22] + ",price_profit=" + (profit + Convert.ToDouble(entrust[24])).ToString() + " WHERE  order_id  =" + entrust[0]);                     //扣掉舊單的未平倉口數
                        connectdb.F_SQL_command("Update  order_entrust  SET  sell_num  = 0 , order_status =  2,sell_price = " + entrust[11] + ", sell_time= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',price_profit=" + profit.ToString() + ",sell_id=" + entrust[0] + "  WHERE  order_id  =" + id.ToString());               //把新單平倉
                        //F_remove_OPint_dgvRow(id);
                    }
                    else                                                                  //如果委託的口數等於反向的未平倉口數
                    {
                        double profit = Convert.ToDouble(pre.order_num) * (pre.entry_price - Convert.ToDouble(entrust[11]));
                        pre.order_num = 0;
                        connectdb.F_SQL_command("Update  order_entrust  SET  sell_num  = 0 , order_status =  2,sell_price = " + pre.order_price.ToString() + ", sell_time= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',price_profit=" + (profit + Convert.ToDouble(entrust[24])).ToString() + ",sell_id=" + id.ToString() + "  WHERE  order_id  =" + entrust[0]);               //把舊單平倉
                        connectdb.F_SQL_command("Update  order_entrust  SET  sell_num  = 0 , order_status =  2,sell_price = " + entrust[11] + ", sell_time= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',price_profit=" + profit.ToString() + ",sell_id=" + entrust[0] + "  WHERE  order_id  =" + id.ToString());               //把新單平倉
                        //F_remove_OPint_dgvRow(Convert.ToInt32(entrust[0]));
                        //F_remove_OPint_dgvRow(id);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        //按下委託取消按鈕事件
        private void dgv_buttonclick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                string buttonText = dgv_entrust.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                if (buttonText == "取消" && dgv_entrust.Rows[e.RowIndex].Cells[e.ColumnIndex - 1].Value.ToString() == "等待中")
                {
                    connectdb.F_SQL_command("Update  pre_order  SET  state  = 2  WHERE  pre_id  = " + dgv_entrust.SelectedRows[0].Cells[0].Value.ToString());
                    dgv_entrust.Rows.Remove(dgv_entrust.SelectedRows[0]);
                }
            }
        }

        private void btn_buy_Click(object sender, EventArgs e)
        {
            int id = 0;
            if (MessageBox.Show(Order_type_string + "\n" + lb_price.Text.ToString() + "\n" + numericUpDown1.Value.ToString() + "口\n多單\n" +
                "是否下單?", future_base.future_name.ToString(), MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (Order_type_int == 0)                                                        //市價單
                {
                    id = connectdb.F_SQL_Add_ReturnID("pre_order", "user_id,future_id,future_source,future_name,buy_type,order_price,order_num,order_time,type_order,state", user_acc.user_id.ToString()
                        + "," + future_base.future_id.ToString() + ",'" + future_base.future_source + "'," + "'" + future_base.future_name + "',0," + lb_price.Text.ToString() + "," + numericUpDown1.Value.ToString()
                        + ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + Order_type_int.ToString() + ",0");
                }
                else if (Order_type_int == 2)                                                   //限價單
                {
                    id = connectdb.F_SQL_Add_ReturnID("pre_order", "user_id,future_id,future_source,future_name,buy_type,order_price,order_num,order_time,type_order,state", user_acc.user_id.ToString()
                        + "," + future_base.future_id.ToString() + ",'" + future_base.future_source + "'," + "'" + future_base.future_name + "',0," + textBox1.Text.ToString() + "," + numericUpDown1.Value.ToString()
                        + ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + Order_type_int.ToString() + ",0");
                }

                //if (id != 0)
                //{
                //    //String[] pre_arr = connectdb.F_SQL_Select_ReturnData("pre_order", "pre_id", "=", id.ToString());
                //    //pre_order = new Pre_Order(pre_arr);
                //    NewEntrsutDataGridView(pre_order);
                //    //Thread DoEntrust_Thread = new Thread(new ParameterizedThreadStart(F_DoEntrust));
                //    //DoEntrust_Thread.Start(pre_order);
                //    //DoEntrust_Thread.IsBackground = true;
                //}
            }
        }

        private void btn_sell_Click(object sender, EventArgs e)
        {
            int id = 0;
            if (MessageBox.Show(Order_type_string + "\n" + lb_price.Text.ToString() + "\n" + numericUpDown1.Value.ToString() + "口\n空單\n" +
                "是否下單?", future_base.future_name.ToString(), MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (Order_type_int == 0)                                                        //市價單
                {
                    id = connectdb.F_SQL_Add_ReturnID("pre_order", "user_id,future_id,future_source,future_name,buy_type,order_price,order_num,order_time,type_order,state", user_acc.user_id.ToString()
                        + "," + future_base.future_id.ToString() + ",'" + future_base.future_source + "'," + "'" + future_base.future_name + "',1," + lb_price.Text.ToString() + "," + numericUpDown1.Value.ToString()
                        + ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + Order_type_int.ToString() + ",0");
                }
                else if (Order_type_int == 2)                                                   //限價單
                {
                    id = connectdb.F_SQL_Add_ReturnID("pre_order", "user_id,future_id,future_source,future_name,buy_type,order_price,order_num,order_time,type_order,state", user_acc.user_id.ToString()
                        + "," + future_base.future_id.ToString() + ",'" + future_base.future_source + "'," + "'" + future_base.future_name + "',1," + textBox1.Text.ToString() + "," + numericUpDown1.Value.ToString()
                        + ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + Order_type_int.ToString() + ",0");
                }

                //if (id != 0)
                //{
                //    //String[] pre_arr = connectdb.F_SQL_Select_ReturnData("pre_order", "pre_id", "=", id.ToString());
                //    //pre_order = new Pre_Order(pre_arr);
                //    NewEntrsutDataGridView(pre_order);
                //    //Thread DoEntrust_Thread = new Thread(new ParameterizedThreadStart(F_DoEntrust));
                //    //DoEntrust_Thread.Start(pre_order);
                //    //DoEntrust_Thread.IsBackground = true;
                //}
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (backgroundWorker1.CancellationPending) //如果被中斷...
                e.Cancel = true;

            BackgroundWorker worker = (BackgroundWorker)sender;
            this.F_Refresh_OPint(worker, e); //欲背景執行的function
        }


        private void dgv_OPint_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                ConnectDatabase CDB = new ConnectDatabase();
                String[] arr = CDB.F_SQL_Select_ReturnData("order_entrust", "order_id", "=", dgv_OPint.Rows[e.RowIndex].Cells[1].Value.ToString());
                Order_Entrust OE = new Order_Entrust(arr);
                int id = 0;


                string buttonText = dgv_OPint.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                if (buttonText == "平倉")
                {
                    MessageBox.Show("委託反向單中");
                    DataRow StocksFind = m_dt_Stocks.Rows.Find(arr[4]);
                    if (StocksFind != null)
                    {
                        if (dgv_OPint.Rows[e.RowIndex].Cells[3].Value.ToString() == "多")
                        {
                            id = connectdb.F_SQL_Add_ReturnID("pre_order", "user_id,future_id,future_source,future_name,buy_type,order_price,order_num,order_time,type_order,state", arr[2]
                            + "," + arr[3] + ",'" + arr[4] + "'," + "'" + arr[5] + "',1," + StocksFind["最新"] + "," + arr[22] + ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + "0" + ",0");

                        }
                        else
                        {
                            id = connectdb.F_SQL_Add_ReturnID("pre_order", "user_id,future_id,future_source,future_name,buy_type,order_price,order_num,order_time,type_order,state", arr[2]
                            + "," + arr[3] + ",'" + arr[4] + "'," + "'" + arr[5] + "',0," + StocksFind["最新"] + "," + arr[22] + ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + "0" + ",0");
                        }

                        //if (id != 0)
                        //{
                        //    String[] pre_arr = connectdb.F_SQL_Select_ReturnData("pre_order", "pre_id", "=", id.ToString());
                        //    pre_order = new Pre_Order(pre_arr);
                        //    NewEntrsutDataGridView(pre_order);
                        //    Thread DoEntrust_Thread = new Thread(new ParameterizedThreadStart(F_DoEntrust));
                        //    DoEntrust_Thread.Start(pre_order);
                        //    DoEntrust_Thread.IsBackground = true;
                        //}
                    }
                }
            }
        }

        //點選已平倉時更新已平倉
        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage3)
            {
                ConnectDatabase CDB = new ConnectDatabase();

                Order_List_Closing = CDB.F_SQL_SelectEntrust(user_acc.user_id, 2);              //找出所有已平倉訂單
                F_Refresh_lv_closing();
            }
        }


        private void F_Refresh_Enstruct()
        {
            ConnectDatabase CDB = new ConnectDatabase();

            try
            {
                while (true)
                {
                    Order_List_Pre_Order = CDB.F_SQL_SelectPre_Order(user_acc.user_id);                                             //抓取用戶的未成交委託  

                    F_Update_Entrust_DataGridView();                                                                                  //更新未平倉欄位

                    Thread.Sleep(5000);
                }
            }
            catch (Exception ee)
            {
                myLog.Write(ee.ToString());
            }
        }

        //建立user_future
        private void F_Build_user_future()
        {
            ConnectDatabase CDB = new ConnectDatabase();
            String[] userdata = CDB.F_SQL_Select_ReturnData("user_acc", "user_id", "=", "8");
            User_acc user = new User_acc(userdata);
            for (int i = 1; i < 19; i++)
            {
                String[] future_data = CDB.F_SQL_Select_ReturnData("future_base", "future_id", "=", i.ToString());
                Future_base fb = new Future_base(future_data);
                CDB.F_SQL_Add("user_future", "user_id,future_id,future_num,future_name,trade_cost,out_cost,stay_cost,future_price,single_order_max_num,"
                +"total_order_max_num,max_stay_day,max_stay_order_num,max_stay_future_num,enable_trade_type,enable_buy_type,future_status,status_change_time,"
                +"stop_trade_percent,trade_percent,auto_sell_percent,sell_wait_time,lowest_price,cost_type,cost_base", user.user_id.ToString()+","+
                fb.future_id.ToString()+",'"+fb.future_source+"','"+fb.future_name+"',"+fb.trade_cost.ToString()+","+"0"+","+fb.stay_cost.ToString()+","+
                fb.future_price.ToString()+","+fb.single_order_max_num.ToString()+","+fb.total_order_max_num.ToString()+","+fb.max_stay_day.ToString()+","+
                fb.max_stay_order_num.ToString()+","+fb.max_stay_future.ToString()+",'"+fb.enable_trade_type.ToString()+"',"+fb.enable_buy_type.ToString()+
                "," + "0" + ",'" + "2016-02-25 00:00:00"+"',"+fb.stop_trade_precent.ToString()+","+fb.trade_precent.ToString()+","+fb.auto_sell_precent.ToString()+
                ","+fb.sell_wait_time.ToString()+","+fb.lowest_price.ToString()+","+"0"+","+fb.future_price.ToString());
            }
        }

        //每十毫秒做一次
        private void timer2_Tick(object sender, EventArgs e)
        {
            ms -= 10;
            if(ms <= 0)
            {
                ms = inputms;
                msflag = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            inputms = Convert.ToInt32(textBox2.Text);
            label8.Text = textBox2.Text;
        }
    }

}
