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

namespace BuggerOff.Controllers
{
    public class ProjectsController : Controller
    {
        private BuggerOffEntities db = new BuggerOffEntities();

        // GET: Projects
        public ActionResult Index(int? page)
        {
            var pageNumber = page ?? 1;
            return View(new ProjectViewModel().ToPagedList(pageNumber, 10));
        }

        // GET: Projects/Details/5
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
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
            
            return View(ViewModel);
        }

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
