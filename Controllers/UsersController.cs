using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity;
using System.Web;
using System.Web.Mvc;
using BuggerOff.DataAccess;
using BuggerOff.ViewModels;
using Mvc.JQuery.Datatables;

namespace BuggerOff.Controllers
{
    public class UsersController : Controller
    {
        private BuggerOffEntities db = new BuggerOffEntities();

        // GET: Users
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Index()
        {
            if (Request.IsAjaxRequest())
                return PartialView();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult editUser(string id, string role, IEnumerable<int> Projects)
        {
            try
            {
                var user = db.AspNetUsers.Find(id);
                Dictionary<int, Project> projectList = db.Projects.ToDictionary(key => key.Id);
                Dictionary<string, AspNetRole> roleList = db.AspNetRoles.ToDictionary(key => key.Id);

                user.AspNetRoles.Clear();
                user.AspNetRoles.Add(roleList[role]);

                //This needs to be obtimized, so that we only perform necessary operations!
                //I.e. don't clear if we aren't making any changes!
                user.Projects.Clear();

                foreach (var projectId in Projects)
                    user.Projects.Add(projectList[projectId]);

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e) { }
            return Json("");
        }


        [Authorize(Roles = "Administrator, Project Manager")]
        public DataTablesResult<UserViewModelItem> getUsers(DataTablesParam dataTableParam)
        {
            var users = db.AspNetUsers.AsQueryable();

            var currentUserId = User.Identity.GetUserId();


            var result = DataTablesResult.Create(users.Select(user => new UserViewModelItem() {
                numTickets = user.Tickets.Count,
                userId = user.Id,
                userName = user.UserName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                highestRole = user.AspNetRoles.FirstOrDefault().Name
                }),
                dataTableParam,
                formatter => new
                {
                    buttons = "<a href=\"#\" class=\"btn btn-sm btn-success userDetails\" data-userId=\"" + formatter.userId + "\"" +
                                    "data-toggle=\"modal\" data-target=\"#userDetailsPopup\">" +
                                    "<i class=\"glyphicon glyphicon-plus-sign\"></i> Details" +
                                "</a>"
                }
            );
            return result;
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteConfirmed(string id)
        {
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            db.AspNetUsers.Remove(aspNetUser);
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

        [Authorize(Roles = "Administrator")]
        public ActionResult getRoles()
        {
            var rolesDict = new Dictionary<string, string>();

            foreach (var role in db.AspNetRoles.ToList())
            {
                rolesDict.Add(role.Id, role.Name);
            }

            return Json(rolesDict, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult getProjects()
        {
            var projectsDict = new Dictionary<string, string>();

            foreach (var project in db.Projects.ToList())
            {
                projectsDict.Add(project.Id.ToString(), project.Name);
            }

            return Json(projectsDict, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult getEditViewModel(string id)
        {
            if (id == "")
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            AdminEditUSerViewModel ViewModel = new AdminEditUSerViewModel(id.ToString());

            return Json(ViewModel, JsonRequestBehavior.AllowGet);
        }
  
    }

}
