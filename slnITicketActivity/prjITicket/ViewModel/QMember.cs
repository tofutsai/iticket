using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel
{
    public class QMember
    {
        public int MemberID { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Name { get; set; }
        public string IDentityNumber { get; set; }
        public string Passport { get; set; }
        public string NickName { get; set; }
        public Nullable<System.DateTime> BirthDate { get; set; }
        [RegularExpression(@"^(?=.*\d).{10,10}$", ErrorMessage = "手機號碼10碼")]
        public string Phone { get; set; }
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).{8,12}$", ErrorMessage = "密碼請輸入8-12碼,至少一個英文及數字")]
        public string Password { get; set; }
        public Nullable<int> Point { get; set; }
        public string Address { get; set; }
        public int MemberRoleId { get; set; }
        public string Icon { get; set; }
        public Nullable<bool> Sex { get; set; }
        public Nullable<int> DistrictId { get; set; }
        public string RegisterCheckCode { get; set; }
        public bool agreeterm { get; set; }
        public string NPassword { get; set; }
    }
    
}