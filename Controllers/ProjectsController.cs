using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BuggerOff.DataAccess;
using Microsoft.AspNet.Identity;
using BuggerOff.ViewModels;
using PagedList;
using PagedList.Mvc;
using BuggerOff.ViewModels;
using Mvc.JQuery.Datatables;

namespace BuggerOff.Controllers
{
    public class ProjectsController : Controller
    {
        private BuggerOffEntities db = new BuggerOffEntities();

        // GET: Projects
        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult Index()
        {
            if (Request.IsAjaxRequest())
                return PartialView();
            return View();
        }

        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public DataTablesResult<ProjectViewModel> getProjects(DataTablesParam dataTableParam)
        {
            var projects = db.Projects.AsQueryable();

            var currentUserId = User.Identity.GetUserId();
            if (!User.IsInRole("Administrator"))
            {
                projects = projects.Where(m => m.AspNetUsers.Any(u => u.Id == currentUserId));
            }
            var result = DataTablesResult.Create(projects.Select(project => new ProjectViewModel()
            {
                projectName = project.Name,
                numTickets = project.Tickets.Count,
                buttons = "",
                projectId = project.Id
            }),
                dataTableParam,
                formatter => new
                {
                    buttons = "<a href=" + @Url.Action("Edit", new { id = formatter.projectId }) + " class=\"btn btn-sm btn-success\">" +
                                    "<i class=&quot;glyphicon glyphicon-edit&quot;></i> Edit" +
                                "</a>" +
                                "<a href=\"#\" class=\"btn btn-sm btn-success details\" data-ticketId=\"" + formatter.projectId + "\"" +
                                    "data-toggle=\"modal\" data-target=\"#detailsPopup\">" +
                                    "<i class=\"glyphicon glyphicon-plus-sign\"></i> Details" +
                                "</a>" +
                                ((User.IsInRole("Administrator")) ?
                                "<a href=" + @Url.Action("Delete", new { id = formatter.projectId }) + " class=\"btn btn-sm btn-danger\">" +
                                    "<i class=\"icon-flash-off\"></i> Delete" +
                                "</a>" : "")
                }
            );
            return result;
        }



        // GET: Projects/Details/5
        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Create()
        {
            if (Request.IsAjaxRequest())
                return PartialView();
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Create([Bind(Include = "Name")] Project project)
        {
            project.CreatedBy = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var Project = db.Projects.Find(id);
            if (Project == null)
                return HttpNotFound();
            EditProjectViewModel ViewModel = new EditProjectViewModel()
            {
                ProjectId = Project.Id,
                ProjectName = Project.Name,
                UserList = new List<selectUserHelper>()
            };

            var userList = db.AspNetUsers.ToList();

            foreach( var user in userList )
                ViewModel.UserList.Add(new selectUserHelper() 
                { 
                    UserId = user.Id, 
                    UserName = user.UserName, 
                    IsSelected = user.Projects.Any(x=>x.Id == id),
                    PreviouslySelected = user.Projects.Any(x=>x.Id == id)
                });

            if (Request.IsAjaxRequest())
                return PartialView(ViewModel);
            return View(ViewModel);
        }
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult GetViewModel(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var Project = db.Projects.Find(id);
            if (Project == null)
                return HttpNotFound();
            EditProjectViewModel ViewModel = new EditProjectViewModel() 
                                                 { 
                                                     ProjectId = Project.Id, 
                                                     ProjectName = Project.Name, 
                                                     UserList = new List<selectUserHelper>() 
                                                 };

            var userList = db.AspNetUsers.ToList();

            foreach (var user in userList)
                ViewModel.UserList.Add(new selectUserHelper()
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    IsSelected = user.Projects.Any(x => x.Id == id),
                    PreviouslySelected = user.Projects.Any(x => x.Id == id)
                });

            return Json(ViewModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult SaveViewModel(EditProjectViewModel ViewModel)
        {
            var project = db.Projects.Where(m => m.Id == ViewModel.ProjectId).ToList().FirstOrDefault();
            if(project.Name != ViewModel.ProjectName)
            {
                project.Name = ViewModel.ProjectName;
                db.Entry(project).State = EntityState.Modified;
            }
            
            
            Dictionary<string, AspNetUser> userList = db.AspNetUsers.ToDictionary(key=>key.Id);
            foreach(var user in ViewModel.UserList)
            {
                var currentUser = userList[user.UserId];
                if (currentUser != null)
                {
                    if (user.IsSelected != user.PreviouslySelected)
                    {
                        user.PreviouslySelected = user.IsSelected;
                        if (user.IsSelected)
                        {
                            //Link the project to the user, and the user to the project, if the user exists
                            project.AspNetUsers.Add(currentUser);
                            currentUser.Projects.Add(project);
                        }
                        else
                        {
                            project.AspNetUsers.Remove(currentUser);
                            currentUser.Projects.Remove(project);
                        }
                    }
                }
                if (ModelState.IsValid)
                {
                    db.Entry(project).State = EntityState.Modified;
                    db.Entry(currentUser).State = EntityState.Modified;
                }
            }

            if (ModelState.IsValid)
            {
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Edit([Bind(Include = "Id,Name,CreatedBy")] Project project, string newUserName="")
        {
            AspNetUser newUser = db.AspNetUsers.Where(m => m.UserName == newUserName).ToList().FirstOrDefault();
            if (newUser != null)
            {
                //Link the project to the user, and the user to the project, if the user exists
                project.AspNetUsers.Add(newUser);
                newUser.Projects.Add(project);
            }
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.Entry(newUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
