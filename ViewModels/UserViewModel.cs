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
    }


    public class UserViewModel
    {

        List<UserViewModelItem> userItems { get; set; }

        public UserViewModel()
        {
            BuggerOffEntities db = new BuggerOffEntities();
            userItems = new List<UserViewModelItem>();

            foreach(var item in db.AspNetUsers)
            {
                userItems.Add(new UserViewModelItem() { userId = item.Id, userName = item.UserName, email = item.Email, phoneNumber = item.PhoneNumber, numTickets = item.Tickets.Count });
            }
        }

        public IPagedList ToPagedList(int page, int numPerPage)
        {
            return userItems.ToPagedList(page, numPerPage);
        }

    }
}