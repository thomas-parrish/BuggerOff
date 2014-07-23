using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BuggerOff.ViewModels
{

    public class selectUserHelper
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsSelected { get; set; }
        public bool PreviouslySelected { get; set; }
    }

    public class EditProjectViewModel
    {
        public IList<selectUserHelper> UserList { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        
    }
}