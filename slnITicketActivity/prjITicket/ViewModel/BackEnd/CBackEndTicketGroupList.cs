using Newtonsoft.Json;
using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel.BackEnd
{
    public class CBackEndTicketGroupList
    {
        [JsonIgnore]
        public TicketGroups TicketGroups { get; set; }

        [DisplayName("套票名稱")]
        public string TicketGroupName { get { return this.TicketGroups.TicketGroupName; }  }//套票名稱
        [DisplayName("套票折扣")]
        public decimal TicketGroupDiscount { get { return this.TicketGroups.TicketGroupDiscount; } }//套票折扣
        [DisplayName("套票狀態")]
        public bool TicketGroupStatus { get { return this.TicketGroups.Status; } }//套票狀態
        [DisplayName("套票代碼")]
        public int TicketGroupID { get { return this.TicketGroups.TicketGroupId; } }

        [JsonIgnore]
        public Seller Seller { get; set; }
        [DisplayName("商家名稱")]
        public string CompanyName { get { return this.Seller.CompanyName; } }
    }
}