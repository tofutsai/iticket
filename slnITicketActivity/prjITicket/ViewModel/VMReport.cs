using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using prjITicket.Models;

namespace prjITicket.ViewModel
{
    public class VMReport
    {
        public Article Article { get; set; }
        public List<Report> Report { get; set; }
        public List<Activity> Activities{ get; set; }
        
    }
    public class VMforum_mainblock
    {
        public List<ArticleCategories> ArticleCategories { get; set; }
        public List<Article> Article { get; set; }
        public string searchWord { get; set; }
        public int page { get; set; }
        public int maxpage { get; set; }
        public string editor { get; set; }
        public string list { set; get; }
        public int ArticleCategoryID { get; set; }
        public List<Activity> activities { set; get; }
        public List<string> A權限 { set; get; }
        public List<Member> Memberlist { get; set; }

    }

}