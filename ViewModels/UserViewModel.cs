using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BuggerOff.DataAccess;
using PagedList;
using PagedList.Mvc;

namespace BuggerOff.ViewModels
{
    public class UserViewModelItem
    {
        public int numTickets { get; set; }
        public string userId { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
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

        public IPagedList ToPagedList(int page, int numPerPage)
        {
            return userItems.ToPagedList(page, numPerPage);
        }

    }
}