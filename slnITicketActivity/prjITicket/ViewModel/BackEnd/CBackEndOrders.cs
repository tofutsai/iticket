using Newtonsoft.Json;
using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel
{
    public class CBackEndOrders
    {
        [JsonIgnore]
        public Orders Orders { get; set; }

        public int OrderID { get { return this.Orders.OrderID; } }
        public DateTime OrderDate { get { return this.Orders.OrderDate; } }
        public string UserName { get { return this.Orders.Name; } }
        public string Email { get { return this.Orders.Email; } }
        public bool OrderStatus { get { return this.Orders.OrderStatus; } }
        public string OrderGuid { get { return this.Orders.OrderGuid; } }
        public int PayPoint { get { return this.Orders.PayPoint; } }
        
        public int OrderPrice { get; set; }
    }
}