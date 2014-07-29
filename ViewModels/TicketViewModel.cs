using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BuggerOff.ViewModels
{
    public class TicketViewModelShort
    {
        public int id { get; set; }
        
        public string Title { get; set; }

        public int StatusId { get; set; }

        public System.DateTimeOffset Created { get; set; }
        
        public string CreatedBy { get; set; }

        public bool isCompleted { get; set; }
        
        public string AssignedTo { get; set; }

        public Nullable<int> ProjectId { get; set; }

        public string ProjectName { get; set; }
        
        public int PriorityId { get; set; }

        public string AssignedToUserId { get; set; }
    }


    public class TicketViewModelFull : TicketViewModelShort
    {
        public string Description { get; set; }
        public Nullable<System.DateTimeOffset> Updated { get; set; }
        public Nullable<System.DateTimeOffset> Completed { get; set; }

        //Attachments
        //History
        //Comments

    }
}