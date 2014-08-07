using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BuggerOff.DataAccess;
using BuggerOff.ViewModels;

namespace BuggerOff.Controllers
{
    public class UsersController : Controller
    {
        private BuggerOffEntities db = new BuggerOffEntities();

        // GET: Users
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Index(int? page)
        {
            var pageNumber = page ?? 1;
            if (Request.IsAjaxRequest())
                return PartialView(new UserViewModel().ToPagedList(pageNumber, 10));
            return View(new UserViewModel().ToPagedList(pageNumber, 10));
        }

        // GET: Users/Details/5
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // GET: Users/Create
        [Authorize(Roles = "Administrator")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] AspNetUser aspNetUser)
        {
            if (ModelState.IsValid)
            {
                db.AspNetUsers.Add(aspNetUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(aspNetUser);
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdminEditUSerViewModel ViewModel = new AdminEditUSerViewModel(id);
            if (ViewModel.UserId == null)
            {
                return HttpNotFound();
            }
            return View(new AdminEditUSerViewModel(id));
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Edit()
        {
            return View();
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
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


        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult GetEditViewModel(string id)
        {
            if (id == "")
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            AdminEditUSerViewModel ViewModel = new AdminEditUSerViewModel(id.ToString())
            {
            };

            return Json(ViewModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult SaveEditViewModel(AdminEditUSerViewModel ViewModel)
        {
            var user = db.AspNetUsers.Find(ViewModel.UserId);
            
            //If user is not found, return http error... needs improvement
            if (user == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Dictionary<int, Project> projectList = db.Projects.ToDictionary(key => key.Id);
            Dictionary<string, AspNetRole> roleList = db.AspNetRoles.ToDictionary(key => key.Id);
            

            //Add project to user
            //Add user to projects
            //Loop through projects, and sync user and project from
            //Database with view model state
            foreach (var project in ViewModel.Projects)
            {
                var currentDbProject = projectList[project.ProjectId];
                if (currentDbProject != null)
                {
                    //If the state in the DB has changed, add or remove project/user combo
                    if (project.IsSelected != project.PreviouslySelected)
                    {
                        project.PreviouslySelected = project.IsSelected;
                        if (project.IsSelected)
                        {
                            //Link the project to the user, and the user to the project, if the user exists
                            currentDbProject.AspNetUsers.Add(user);
                            user.Projects.Add(currentDbProject);
                        }
                        else
                        {
                            currentDbProject.AspNetUsers.Remove(user);
                            user.Projects.Remove(currentDbProject);
                        }
                    }
                }
                //Not sure what this is, need to investigate
                if (ModelState.IsValid)
                {
                    //Is there a more efficient way of doing this?
                    db.Entry(currentDbProject).State = EntityState.Modified;
                    db.Entry(user).State = EntityState.Modified;
                }
            }

            //roleList.First().Value.AspNetUsers (add user to roles)
            //Add roles to user

            foreach (var role in ViewModel.Roles)
            {
                var currentDbRole = roleList[role.RoleId];
                if (currentDbRole != null)
                {
                    //If the state of the role in the view model has changed
                    if (role.IsSelected != role.PreviouslySelected)
                    {
                        role.PreviouslySelected = role.IsSelected;
                        if (role.IsSelected)
                        {
                            //Link the project to the user, and the user to the project, if the user exists
                            currentDbRole.AspNetUsers.Add(user);
                            user.AspNetRoles.Add(currentDbRole);
                        }
                        else
                        {
                            currentDbRole.AspNetUsers.Remove(user);
                            user.AspNetRoles.Remove(currentDbRole);
                        }
                    }
                }
                if (ModelState.IsValid)
                {
                    db.Entry(currentDbRole).State = EntityState.Modified;
                    db.Entry(user).State = EntityState.Modified;
                }
            }
            //Not sure why I need this check?
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    
    }

}
