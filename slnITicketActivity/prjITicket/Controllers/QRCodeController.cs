using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using prjITicket.Models;
using Newtonsoft.Json;

namespace prjITicket.Controllers
{
    public class QRCodeController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public ActivityData Get(string qrCodeContent)
        {
            TicketSysEntities db = new TicketSysEntities();
            ActivityData activityData = db.TicketQRCodes.AsEnumerable().Where(tqrc => tqrc.QRCode == qrCodeContent).
                Select(tqrc =>
                {
                    Order_Detail order_Detail = tqrc.Order_Detail;
                    return new ActivityData()
                    {
                        ActivityName = order_Detail.Tickets.Activity.ActivityName,
                        TicketCategoryName = order_Detail.Tickets.TicketCategory.TicketCategoryName,
                        TicketTime = order_Detail.Tickets.TicketTimes.TicketTime.ToString("yyyy/MM/dd HH:mm"),
                        MemberName = tqrc.Order_Detail.Orders.Name
                    };
                }).FirstOrDefault();
            return activityData;
        }
        public class ActivityData
        {
            public string ActivityName { get; set; }
            public string TicketCategoryName { get; set; }
            public string TicketTime { get; set; }
            public string MemberName { get; set; }

        }
        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}