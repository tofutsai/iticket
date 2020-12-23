using Newtonsoft.Json;
using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel
{
    public class CBackEndOrderDetail
    {

        [JsonIgnore]
        public Orders Orders { get; set; }

        public int OrderID { get { return this.Orders.OrderID; } }
        public DateTime OrderDate { get { return this.Orders.OrderDate; } }
        public string UserName { get { return this.Orders.Name; } }
        public string Email { get { return this.Orders.Email; } }
        public bool OrderStatus { get { return this.Orders.OrderStatus; } }
        public string OrderGuid { get { return this.Orders.OrderGuid; } }


        [JsonIgnore]
        public Order_Detail OrderDetail { get; set; }
        
        public int OrderDetialID { get { return this.OrderDetail.OrderDetailID; } }
        public int Quantity { get { return this.OrderDetail.Quantity; } }
        public decimal Discount { get { return this.OrderDetail.Discount; } }


        [JsonIgnore]
        public Activity Activity { get; set; }

        public string ActivityName { get { return this.Activity.ActivityName; } }

        [JsonIgnore]
        public Districts Districts { get; set; }

        public string DistrictName { get { return this.Districts.DistrictName; } }
        public string PostCode { get { return this.Districts.PostCode; } }

        [JsonIgnore]
        public Cities Cities { get; set; }

        public string CityName { get { return this.Cities.CityName; } }

        [JsonIgnore]
        public Tickets Tickets { get; set; }

        public int TicketPrice { get { return this.Tickets.Price; } }
       
        [JsonIgnore]
        public TicketCategory TicketCategory { get; set; }

        public string TicketCategoryName { get { return this.TicketCategory.TicketCategoryName; } }

        [JsonIgnore]
        public TicketTimes TicketTimes { get; set; }

        public DateTime TicketTime { get { return this.TicketTimes.TicketTime; } }


    }
}