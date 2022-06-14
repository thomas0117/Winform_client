using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dataclient
{
    public class Order_Entrust
    {
        public int order_id;
        public int sell_id;
        public int user_id;
        public int future_id;
        public String future_source;
        public String future_name;
        public int cost_type;
        public int cost_trade;
        public int cost_stay;
        public int lowest_price;
        public int lowest_cost;
        public Double order_price;
        public int order_num;
        public int price_stop_loss;
        public Double price_profit;
        public int price_hope;
        public int profit_count;
        public int type_buy;
        public int type_order;
        public int order_status;
        public String order_time;
        public Double sell_price;
        public int sell_num;
        public String sell_time;
        public Double user_profit;
        public int agreement_day_num;
        public int is_day_trade;
        public int is_today;

        public Order_Entrust(String[] arr)
        {

            order_id = Convert.ToInt32(arr[0]);
            sell_id = Convert.ToInt32(arr[1]);
            user_id = Convert.ToInt32(arr[2]);
            future_id = Convert.ToInt32(arr[3]);
            future_source = arr[4];
            future_name = arr[5];
            cost_type = Convert.ToInt32(arr[6]);
            cost_trade = Convert.ToInt32(arr[7]);
            cost_stay = Convert.ToInt32(arr[8]);
            lowest_price = Convert.ToInt32(arr[9]);
            lowest_cost = Convert.ToInt32(arr[10]);
            order_price = Convert.ToDouble(arr[11]);
            order_num = Convert.ToInt32(arr[12]);
            price_stop_loss = Convert.ToInt32(arr[13]);
            price_profit = Convert.ToDouble(arr[14]);
            price_hope = Convert.ToInt32(arr[15]);
            profit_count = Convert.ToInt32(arr[16]);
            type_buy = Convert.ToInt32(arr[17]);
            type_order = Convert.ToInt32(arr[18]);
            order_status = Convert.ToInt32(arr[19]);
            order_time = arr[20];
            sell_price = Convert.ToDouble(arr[21]);
            sell_num = Convert.ToInt32(arr[22]);
            sell_time = arr[23];
            user_profit = Convert.ToDouble(arr[24]);
            agreement_day_num = Convert.ToInt32(arr[25]);
            is_day_trade = Convert.ToInt32(arr[26]);
            is_today = Convert.ToInt32(arr[27]);
        }

    }
}
