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
                    buttons = "<a href=\"#\" class=\"btn btn-sm btn-success projectDetails\" data-projectId=\"" + formatter.projectId + "\"" +
                                    "data-toggle=\"modal\" data-target=\"#projectDetailsPopup\">" +
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

        [HttpPost]
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult editProject(int id, IEnumerable<string> Users)
        {
            try
            {
                var project = db.Projects.Find(id);

                Dictionary<string, AspNetUser> userList = db.AspNetUsers.ToDictionary(key => key.Id);

                //This needs to be obtimized, so that we only perform necessary operations!
                //I.e. don't clear if we aren't making any changes!
                project.AspNetUsers.Clear();

                foreach (var userId in Users)
                    project.AspNetUsers.Add(userList[userId]);

                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e) { }
            return Json("");
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
        
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult getProjectViewModel(int id)
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

        [Authorize(Roles = "Administrator")]
        public ActionResult getUsers()
        {
            var usersDict = new Dictionary<string, string>();

            foreach (var user in db.AspNetUsers.ToList())
            {
                usersDict.Add(user.Id, user.UserName);
            }

            return Json(usersDict, JsonRequestBehavior.AllowGet);
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
