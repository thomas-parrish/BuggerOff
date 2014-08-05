using System;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using Mvc.JQuery.Datatables;
using BuggerOff.DataAccess;

namespace BuggerOff.ViewModels
{
    public class TicketViewModelShort
    {
        public TicketViewModelShort() 
        {
            buttons = "";
        }

        public TicketViewModelShort(Ticket ticket) : this()
        {
            id = ticket.id; 
            Title = ticket.Title;
            Status = ticket.TicketStatus.Status;
            Created = ticket.Created;
            CreatedBy = ticket.CreatedByUser.UserName;
            isCompleted = ( ticket.Completed != null ) ? "<i class=&quot;fa fa-check&quot;></i>" : "";
            AssignedTo = ( ticket.AssignedToUser != null) ? ticket.AssignedToUser.UserName : "";
            ProjectId = ticket.ProjectId;
            ProjectName = ticket.Project.Name;
            PriorityId = ticket.PriorityId;
            AssignedToUserId = ticket.AssignedTo;
        }



        [DataTables(DisplayName = "Summary", Searchable = true, Sortable=false)]
        public string Title { get; set; }

        [DataTables(DisplayName = "Status", Searchable = false, Sortable = false)]
        public string Status { get; set; }

        [DataTables(DisplayName = "Date Created", Searchable = true, Sortable = true)]
        public DateTimeOffset Created { get; set; }

        [DataTables(DisplayName = "Submitted By", Searchable = true, Sortable = false)]
        public string CreatedBy { get; set; }

        [DataTables(DisplayName = "Assigned To", Searchable = true, Sortable = true)]
        public string AssignedTo { get; set; }

        [DataTables(DisplayName = "Project", Searchable = true, Sortable = true)]
        public string ProjectName { get; set; }
        
        [DataTables(DisplayName = "Closed", Searchable = true, Sortable = true)]
        public string isCompleted { get; set; }

        [DataTables(DisplayName = "Actions", Searchable = false, Sortable = false)]
        public string buttons { get; set; }

        [DataTables(Visible = false)]
        public int id { get; set; }

        [DataTables(Visible = false)]
        public Nullable<int> ProjectId { get; set; }

        [DataTables(Visible = false)]
        public int PriorityId { get; set; }

        [DataTables(Visible = false)]
        public string AssignedToUserId { get; set; }
    }


    public class CommentViewModel {
        public int? id {get; set;}
        public int ticketId {get; set;}
        public string userId {get; set;}
        public string userName { get; set; }
        public string text {get; set;}

        public CommentViewModel() { }

        public CommentViewModel(TicketComment comment) : this() {
            id = comment.id;
            ticketId = comment.ticketId;
            userId = comment.userId;
            userName = comment.userName;
            text = comment.text;
        }
    }

    public class TicketViewModelDetails
    {
        public string Description { get; set; }
        public Nullable<System.DateTimeOffset> Updated { get; set; }
        public Nullable<System.DateTimeOffset> Completed { get; set; }

        //Attachments
        //History
        public List<CommentViewModel> comments { get; set; }

        public TicketViewModelDetails(int id)
        {
            BuggerOffEntities db = new BuggerOffEntities();

            var ticket = db.Tickets.Include(t => t.CreatedByUser)
               .Include(t => t.AssignedToUser)
               .Include(t => t.Project)
               .Include(t => t.TicketPriority)
               .Include(t => t.TicketStatus)
               .Where(m => m.id == id)
               .FirstOrDefault();

            if( ticket == null )
            {
                throw new Exception("No such ticket found");
            }

            Description = ticket.Description;
            Updated = ticket.Updated;
            Completed = ticket.Completed;

            comments = new List<CommentViewModel>();

            var commentList = ticket.TicketComments.ToList();

            foreach(var comment in commentList){
                comments.Add(new CommentViewModel(comment));
            }
        }

    }
}