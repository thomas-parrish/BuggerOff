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

    public class projectCheckboxItem
    {
        public int id { get; set; }
        public bool selected { get; set; }
    }

    public class AdminEditUSerViewModel
    {
        public IList<selectProjectHelper> Projects { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string name { get; set; }
        public string roleId { get; set; }
        public string roleName { get; set; }

        public AdminEditUSerViewModel()
        {
            Projects = new List<selectProjectHelper>();
        }

        //This needs error checking!
        public AdminEditUSerViewModel(string UserId) : this()
        {
            BuggerOffEntities db = new BuggerOffEntities();

            var ProjectList = db.Projects.ToList();

            var User = db.AspNetUsers.Find(UserId);

            this.UserId = UserId;
            this.Username = User.UserName;
            this.Email = User.Email;

            roleId = User.AspNetRoles.First().Id;
            roleName = User.AspNetRoles.First().Name;

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

        }
        
    }
}