using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BuggerOff.DataAccess;
using BuggerOff.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;

namespace BuggerOff.Controllers
{
    public class AdminController : Controller
    {
        private BuggerOffEntities db = new BuggerOffEntities();

        // GET: Admin
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Index()
        {
            return View(db.AspNetRoles.ToList());
        }

        // GET: Admin/Details/5
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetRole aspNetRole = db.AspNetRoles.Find(id);
            if (aspNetRole == null)
            {
                return HttpNotFound();
            }
            return View(aspNetRole);
        }

        // GET: Admin/Create
        [Authorize(Roles = "Administrator")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Create([Bind(Include = "Name")] RolesViewModel newRole)
        {
            var roleStore = new ApplicationRoleStore( db );

            var roleManager = new RoleManager<AspNetRole, string>(roleStore);

            // Check to see if Role Exists, if not create it
            if (!roleManager.RoleExists(newRole.Name))
            {
                roleManager.Create(new AspNetRole(newRole.Name));
            }
            return View();
        }

        // GET: Admin/Edit/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetRole aspNetRole = db.AspNetRoles.Find(id);
            if (aspNetRole == null)
            {
                return HttpNotFound();
            }
            return View(aspNetRole);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Edit([Bind(Include = "Id,Name")] AspNetRole aspNetRole)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetRole).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(aspNetRole);
        }

        // GET: Admin/Delete/5
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetRole aspNetRole = db.AspNetRoles.Find(id);
            if (aspNetRole == null)
            {
                return HttpNotFound();
            }
            return View(aspNetRole);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult DeleteConfirmed(string id)
        {
            AspNetRole aspNetRole = db.AspNetRoles.Find(id);
            db.AspNetRoles.Remove(aspNetRole);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            //new ApplicationRoleStore().DoSomething();


            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}


//static class Extensions 
//{
//    public static void DoSomething<T>(this IRoleStore<T> role)
//    {
//        Console.WriteLine(role);
//    }
//}

