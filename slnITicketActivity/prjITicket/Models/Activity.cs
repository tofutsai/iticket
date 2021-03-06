
//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------


namespace prjITicket.Models
{

using System;
    using System.Collections.Generic;
    
public partial class Activity
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Activity()
    {

        this.Comment = new HashSet<Comment>();

        this.TicketCategory = new HashSet<TicketCategory>();

        this.TicketGroupDetail = new HashSet<TicketGroupDetail>();

        this.Tickets = new HashSet<Tickets>();

        this.TicketTimes = new HashSet<TicketTimes>();

        this.ActivityFavourite = new HashSet<ActivityFavourite>();

        this.Ad_Article_Activity = new HashSet<Ad_Article_Activity>();

    }


    public int ActivityID { get; set; }

    public string ActivityName { get; set; }

    public int SellerID { get; set; }

    public string Description { get; set; }

    public string Address { get; set; }

    public string Picture { get; set; }

    public string Information { get; set; }

    public int SubCategoryId { get; set; }

    public string Hostwords { get; set; }

    public string Map { get; set; }

    public int ActivityStatusID { get; set; }

    public int DistrictId { get; set; }



    public virtual ActivityStatus ActivityStatus { get; set; }

    public virtual Districts Districts { get; set; }

    public virtual Seller Seller { get; set; }

    public virtual SubCategories SubCategories { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Comment> Comment { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<TicketCategory> TicketCategory { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<TicketGroupDetail> TicketGroupDetail { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Tickets> Tickets { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<TicketTimes> TicketTimes { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<ActivityFavourite> ActivityFavourite { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Ad_Article_Activity> Ad_Article_Activity { get; set; }

}

}
