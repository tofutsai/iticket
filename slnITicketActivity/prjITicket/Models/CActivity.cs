using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.Models
{
    //上傳Activity用的裝B的類別
    public class CActivity
    {
        public int activityId { get; set; }
        public string activityName { get; set; }
        public int districtId { get; set; }
        public string address { get; set; }
        public string map { get; set; }
        public int subcategoryId { get; set; }
        public string hostwords { get; set; }
        public string description { get; set; }
        public List<string> ticketCategories { get; set; }
        public List<string> times { get; set; }
        public string information { get; set; }
        public string picture { get; set; }
    }
}