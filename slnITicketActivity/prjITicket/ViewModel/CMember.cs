using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel
{
    //public class CMember
    //{
    //    public Member entity { get; set; }
    //    public int MemberID { get { return this.entity.MemberID; } }
    //    [Required(ErrorMessage ="非Email格式")]
    //    [DisplayName("電子信箱:")]
    //    public string Email { get { return this.entity.Email; } }
    //    public string Name { get { return this.entity.Name; } }
    //    public string IDentityNumber { get { return this.entity.IDentityNumber; } }
    //    public string Passport { get { return this.entity.Passport; } }
    //    public string NickName { get { return this.entity.NickName; } }
    //    public Nullable<System.DateTime> BirthDate { get { return this.entity.BirthDate; } }
    //    public string Phone { get { return this.entity.Phone; } }
    //    [Required(ErrorMessage ="請輸入8~12位英數混合")]
    //    [DisplayName("密碼:")]
    //    public string Password { get { return this.entity.Password; } }
    //    [Required(ErrorMessage ="與密碼不一致")]
    //    [DisplayName("請再輸入一次密碼:")]
    //    public string PasswordCheck { get; set; }
    //    public Nullable<int> Point { get { return this.entity.Point; } }
    //    public string Address { get { return this.entity.Address; } }

    //    public int MemberRoleId { get { return this.entity.MemberRoleId; } }
    //    public string Icon { get { return this.entity.Icon; } }
    //    public Nullable<bool> Sex { get { return this.entity.Sex; } }

    //}
    public class CMember
    {
        //public int MemberID { get; set; }
        //[Required]
        //[EmailAddress]
        //public string Email { get; set; }
        //public string Name { get; set; }
        //public string IDentityNumber { get; set; }
        //public string Passport { get; set; }
        //public string NickName { get; set; }
        //public Nullable<System.DateTime> BirthDate { get; set; }
        //public string Phone { get; set; }
        //[RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).{8,12}$", ErrorMessage = "密碼請輸入8-12碼,至少一個英文及數字")]
        //public string Password { get; set; }
        //public Nullable<int> Point { get; set; }
        //public string Address { get; set; }
        //public int MemberRoleId { get; set; }
        //public string Icon { get; set; }
        //public Nullable<bool> Sex { get; set; }
        //public Nullable<int> DistrictId { get; set; }
        //public string RegisterCheckCode { get; set; }
        //public bool agreeterm { get; set; }

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
        [DisplayName("聯絡電話:")]
        public string Phone { get { return this.entity.Phone; } }
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).{8,12}$", ErrorMessage = "密碼請輸入8~12碼, 至少一個英文及數字")]
        [Required(ErrorMessage = "密碼請輸入8~12碼, 至少一個英文及數字")]
        public string Password { get { return this.entity.Password; } }
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).{8,12}$", ErrorMessage = "密碼請輸入8~12碼, 至少一個英文及數字")]
        [Required(ErrorMessage = "密碼請輸入8~12碼, 至少一個英文及數字")]
        public string NewPassword { get; set; }
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "與新密碼不一致")]
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

    }
}