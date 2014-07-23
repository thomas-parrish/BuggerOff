using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BuggerOff.DataAccess;
using System.Web.Mvc;

namespace BuggerOff.ViewModels
{
    public class selectProjectHelper
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public bool IsSelected { get; set; }
        public bool PreviouslySelected { get; set; }
    }

    public class selectRoleHelper
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
        public bool PreviouslySelected { get; set; }
    }
    
    public class AdminEditUSerViewModel
    {
        public IList<selectProjectHelper> Projects { get; set; }
        public IList<selectRoleHelper> Roles { get; set; }
        public string UserId { get; set; }

        public AdminEditUSerViewModel()
        {
            Projects = new List<selectProjectHelper>();
            Roles = new List<selectRoleHelper>();
        }

        //This needs error checking!
        public AdminEditUSerViewModel(string UserId) : this()
        {
            BuggerOffEntities db = new BuggerOffEntities();

            var ProjectList = db.Projects.ToList();
            var RoleList = db.AspNetRoles.ToList();

            var User = db.AspNetUsers.Find(UserId);

            this.UserId = UserId;
 

            foreach(var item in ProjectList)
            {


                Projects.Add(new selectProjectHelper()
                {
                    ProjectId = item.Id,
                    ProjectName = item.Name,
                    IsSelected = User.Projects.Contains(item),
                    PreviouslySelected = User.Projects.Contains(item)
                });
            }
            foreach(var item in RoleList)
            {
                Roles.Add(new selectRoleHelper()
                {
                    RoleId = item.Id,
                    RoleName = item.Name,
                    IsSelected = User.AspNetRoles.Contains(item),
                    PreviouslySelected = User.AspNetRoles.Contains(item)
                });
            }

        }
        
    }
}