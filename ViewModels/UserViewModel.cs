using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BuggerOff.DataAccess;
using Mvc.JQuery.Datatables;

namespace BuggerOff.ViewModels
{
    public class UserViewModelItem
    {
        [DataTables(DisplayName = "Username", Searchable = true, Sortable = true)]
        public string userName { get; set; }
        [DataTables(DisplayName = "E-mail", Searchable = true, Sortable = true)]
        public string email { get; set; }
        [DataTables(DisplayName = "Tickets", Searchable = true, Sortable = true)]
        public int numTickets { get; set; }
        [DataTables(DisplayName = "Role", Searchable = true, Sortable = true)]
        public string highestRole { get; set; }
        [DataTables(DisplayName = "Actions", Searchable = true, Sortable = true)]
        public string buttons { get; set; }

        [DataTables(Visible = false)]
        public string phoneNumber { get; set; }
        [DataTables(Visible = false)]
        public string userId { get; set; }
        [DataTables(Visible = false)]
        public List<String> roles { get; set; }
    }


    public class UserViewModel
    {

        List<UserViewModelItem> userItems { get; set; }

        public UserViewModel()
        {
            BuggerOffEntities db = new BuggerOffEntities();
            userItems = new List<UserViewModelItem>();

            var userList = db.AspNetUsers.ToList();
            foreach(var user in userList)
            {
                var roleNames = new List<String>();
                var roleList = user.AspNetRoles.ToList();
                foreach (var role in roleList)
                {
                    roleNames.Add(role.Name);
                }
                userItems.Add(new UserViewModelItem() { userId = user.Id, userName = user.UserName, email = user.Email, phoneNumber = user.PhoneNumber, numTickets = user.Tickets.Count, roles = roleNames });
            }
        }
    }
}