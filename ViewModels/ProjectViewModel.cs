using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BuggerOff.DataAccess;
using PagedList;
using PagedList.Mvc;
using Mvc.JQuery.Datatables;

namespace BuggerOff.ViewModels
{
    public class ProjectViewModel
    {
        [DataTables(DisplayName = "Project Name", Searchable = true, Sortable = true)]
        public string projectName { get; set; }

        [DataTables(DisplayName = "Number of Tickets", Searchable = false, Sortable = false)]
        public int numTickets { get; set; }

        [DataTables(DisplayName = "Actions", Searchable = false, Sortable = false)]
        public string buttons { get; set; }

        [DataTables(Visible = false)]
        public int projectId { get; set; }
    }

}