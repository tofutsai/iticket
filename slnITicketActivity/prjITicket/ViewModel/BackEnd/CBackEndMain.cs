using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel.BackEnd
{
    public class CBackEndMain
    {
        public int YesterdayOrderCount { get; set; }
        public int BeforeYesterdayOrderCount { get; set; }

        public int YesterDayTotalPrice { get; set; }
        public int BeforeYesterDayTotalPrice { get; set; }

        public int Member_Customer { get; set; }
        public int Member_Seller { get; set; }

        public int Seller_NoPass { get; set; }
        public int Activity_NoPass { get; set; }
        public int GroupTicket_NoPass { get; set; }

        public List<Take3_Forum> Take3_Forum { get; set; }
        public List<Take3_Article> Take3_Articles { get; set; }

        public int OrderCount_15 { get; set; }
        public int OrderCount_14 { get; set; }
        public int OrderCount_13 { get; set; }
        public int OrderCount_12 { get; set; }
        public int OrderCount_11 { get; set; }
        public int OrderCount_10 { get; set; }
        public int OrderCount_9 { get; set; }
        public int OrderCount_8 { get; set; }
        public int OrderCount_7 { get; set; }
        public int OrderCount_6 { get; set; }
        public int OrderCount_5 { get; set; }
        public int OrderCount_4 { get; set; }
        public int OrderCount_3 { get; set; }
        public int OrderCount_2 { get; set; }
        public int OrderCount_1 { get; set; }

        public int OrderPrice_15 { get; set; }
        public int OrderPrice_14 { get; set; }
        public int OrderPrice_13 { get; set; }
        public int OrderPrice_12 { get; set; }
        public int OrderPrice_11 { get; set; }
        public int OrderPrice_10 { get; set; }
        public int OrderPrice_9 { get; set; }
        public int OrderPrice_8 { get; set; }
        public int OrderPrice_7 { get; set; }
        public int OrderPrice_6 { get; set; }
        public int OrderPrice_5 { get; set; }
        public int OrderPrice_4 { get; set; }
        public int OrderPrice_3 { get; set; }
        public int OrderPrice_2 { get; set; }
        public int OrderPrice_1 { get; set; }

        public int MemberCount_1 { get; set; }
        public int MemberCount_2 { get; set; }
        public int MemberCount_3 { get; set; }
        public int MemberCount_4 { get; set; }
        public int MemberCount_5 { get; set; }
        public int MemberCount_6 { get; set; }
        public int MemberCount_7 { get; set; }
        public int MemberCount_8 { get; set; }
        public int MemberCount_9 { get; set; }
        public int MemberCount_10 { get; set; }
        public int MemberCount_11 { get; set; }
        public int MemberCount_12 { get; set; }
        public int MemberCount_13 { get; set; }
        public int MemberCount_14 { get; set; }
        public int MemberCount_15 { get; set; }

        public int SellerCount_1 { get; set; }
        public int SellerCount_2 { get; set; }
        public int SellerCount_3 { get; set; }
        public int SellerCount_4 { get; set; }
        public int SellerCount_5 { get; set; }
        public int SellerCount_6 { get; set; }
        public int SellerCount_7 { get; set; }
        public int SellerCount_8 { get; set; }
        public int SellerCount_9 { get; set; }
        public int SellerCount_10 { get; set; }
        public int SellerCount_11 { get; set; }
        public int SellerCount_12 { get; set; }
        public int SellerCount_13 { get; set; }
        public int SellerCount_14 { get; set; }
        public int SellerCount_15 { get; set; }

    }
    public class Take3_Forum
    {
        public string CategoryName { get; set; }
        public int CommentNum { get; set; }
    }

    public class Take3_Article
    {
        public string CategoryName { get; set; }
        public string ArticleTitle { get; set; }
        public int ArticleLike { get; set; }
        public int ArticleID { get; set; }
    }

    

}