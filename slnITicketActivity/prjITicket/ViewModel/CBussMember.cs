using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel
{
    public class CBussMember
    {
        public Member entity { get; set; }
        public int MemberID { get { return this.entity.MemberID; } }
        [Required(ErrorMessage = "非Email格式")]
        [DisplayName("電子信箱:")]
        public string Email { get { return this.entity.Email; } }
        [DisplayName("姓名:")]
        public string Name { get { return this.entity.Name; } }
        [DisplayName("身分證字號:")]
        public string IDentityNumber { get { return this.entity.IDentityNumber; } }
        [DisplayName("護照號碼:")]
        public string Passport { get { return this.entity.Passport; } }
        [DisplayName("暱稱:")]
        public string NickName { get { return this.entity.NickName; } }
        [DisplayName("出生日期:")]
        public Nullable<System.DateTime> BirthDate { get { return this.entity.BirthDate; } }
        [DisplayName("電話號碼:")]
        public string Phone { get { return this.entity.Phone; } }
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).{8,12}$", ErrorMessage = "密碼請輸入8-12碼,至少一個英文及數字")]
        [DisplayName("密碼:")]
        public string Password { get { return this.entity.Password; } }
        [Required(ErrorMessage = "與密碼不一致")]
        [DisplayName("請再輸入一次密碼:")]
        public string PasswordCheck { get; set; }
        [DisplayName("iTicket Points:")]
        public Nullable<int> Point { get { return this.entity.Point; } }
        [DisplayName("居住地址:")]
        public string Address { get { return this.entity.Address; } }
        public int MemberRoleId { get { return this.entity.MemberRoleId; } }
        public string Icon { get { return this.entity.Icon; } }
        [DisplayName("性別:")]
        public Nullable<bool> Sex { get { return this.entity.Sex; } }
        public Nullable<int> DistrictId { get { return this.entity.DistrictId; } }
        public string RegisterCheckCode { get { return this.entity.RegisterCheckCode; } }

        public Seller BussEntity { get; set; }
        public int SellerID { get { return this.BussEntity.SellerID; } }
        public string CompanyName { get { return this.BussEntity.CompanyName; } }
        [RegularExpression(@"^(?=.*\d).{8,8}$", ErrorMessage = "統編8碼")]
        public string TaxIDNumber { get { return this.BussEntity.TaxIDNumber; } }
        public string SellerHomePage { get { return this.BussEntity.SellerHomePage; } }
        public string SellerPhone { get { return this.BussEntity.SellerPhone; } }
        public string SellerDeccription { get { return this.BussEntity.SellerDeccription; } }
        public Nullable<bool> fPass { get { return this.BussEntity.fPass; } }
    }
}