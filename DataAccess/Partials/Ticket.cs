using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BuggerOff.DataAccess
{
    [MetadataType(typeof(TicketMetaData))]  
    public partial class Ticket
    {
        //public Ticket()
        //{
        //    this.TicketAttachments = new HashSet<TicketAttachment>();
        //    this.TicketHistories = new HashSet<TicketHistory>();

        //    this.Created = System.DateTime.Now;
        //}
    }
    
    public class TicketMetaData
    {
        [Display(Name = "Project Id")]
        public Nullable<int> ProjectId { get; set; }

        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; }

        [Display(Name = "Created By")]
        public virtual AspNetUser CreatedByUser { get; set; }
        //public System.DateTime Created { get; set; }
        //public System.DateTime Updated { get; set; }
    }
}

