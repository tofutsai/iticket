
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
    
public partial class Order_Detail
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Order_Detail()
    {

        this.TicketQRCodes = new HashSet<TicketQRCodes>();

    }


    public int OrderDetailID { get; set; }

    public int TicketId { get; set; }

    public int OrderID { get; set; }

    public int Quantity { get; set; }

    public decimal Discount { get; set; }



    public virtual Orders Orders { get; set; }

    public virtual Tickets Tickets { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<TicketQRCodes> TicketQRCodes { get; set; }

}

}
