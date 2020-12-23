using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel
{
    public class CSeller
    {
        public Seller entity { get; set; }
        public int SellerID { get { return this.entity.SellerID; } }
        [DisplayName("公司名稱:")]
        public string CompanyName { get { return this.entity.CompanyName; } }
        //[Required(ErrorMessage ="請輸入")]
        [DisplayName("統一編號:")]
        public string TaxIDNumber { get { return this.entity.TaxIDNumber; } }
        public string SellerHomePage { get { return this.entity.SellerHomePage; } }
        [DisplayName("聯絡電話:")]
        public string SellerPhone { get { return this.entity.SellerPhone; } }
        public string SellerDeccription { get { return this.entity.SellerDeccription; } }
        public int MemberId { get { return this.entity.MemberId; } }

        public CMember CMember { get; set; }
        public virtual Member Member { get; set; }
    }
}