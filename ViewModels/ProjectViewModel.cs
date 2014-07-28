using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BuggerOff.DataAccess;
using PagedList;
using PagedList.Mvc;

namespace BuggerOff.ViewModels
{
    public class ProjectViewModelItem
    {
        public int numTickets { get; set; }
        public int projectId { get; set; }
        public string projectName { get; set; }

        public ProjectViewModelItem()
        {
        }
    }


    public class ProjectViewModel
    {

        List<ProjectViewModelItem> projectItems { get; set; }

        public ProjectViewModel()
        {
            BuggerOffEntities db = new BuggerOffEntities();
            projectItems = new List<ProjectViewModelItem>();

            var projectList = db.Projects.ToList();

            foreach(var item in projectList)
            {
                projectItems.Add(new ProjectViewModelItem() { projectId = item.Id, projectName = item.Name, numTickets = item.Tickets.Count });
            }
        }

        public IPagedList ToPagedList(int page, int numPerPage)
        {
            return projectItems.ToPagedList(page, numPerPage);
        }
    }
}