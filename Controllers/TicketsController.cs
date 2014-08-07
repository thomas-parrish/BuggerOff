using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using PagedList;
using PagedList.Mvc;
using System.Web.Mvc;
using BuggerOff.DataAccess;
using Microsoft.AspNet.Identity;
using System.Linq.Expressions;
using BuggerOff.ViewModels;
using Mvc.JQuery.Datatables;
//using System.Linq.Dynamic;

namespace BuggerOff.Controllers
{
    public class TicketsController : Controller
    {
        private BuggerOffEntities db = new BuggerOffEntities();

        private IQueryable<Ticket> searchQuery(IQueryable<Ticket> query, string searchString, string searchType)
        {
            switch(searchType)
            {
                //Generate a Where clause for each acceptable type of search
                case "CreatedBy":
                    return query.Where(x => x.CreatedByUser.UserName.Contains(searchString));
                case "AssignedTo":
                    return query.Where(x => x.AssignedToUser.UserName.Contains(searchString));
                case "Project":
                    return query.Where(x => x.Project.Name.Contains(searchString));
                default:
                    return query;
            }
        }

        [HttpPost]
        public JsonResult getTicketDetails(int id)
        {
            TicketViewModelDetails details;
            TicketViewModelShort ticketInfo;

            var tickets = db.Tickets.Include(t => t.CreatedByUser).Include(t => t.AssignedToUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus);
            
            try
            {
                details = new TicketViewModelDetails(id);

                ticketInfo = tickets.Where(t=>t.id==id).Select(ticket => new TicketViewModelShort()
                {
                    id = ticket.id,
                    Title = ticket.Title,
                    Status = ticket.TicketStatus.Status,
                    Created = ticket.Created,
                    CreatedBy = ticket.CreatedByUser.UserName,
                    isCompleted = (ticket.Completed != null) ? "<i class=&quot;fa fa-check&quot;></i>" : "",
                    AssignedTo = ticket.AssignedToUser.UserName,
                    ProjectId = ticket.ProjectId,
                    ProjectName = ticket.Project.Name,
                    PriorityId = ticket.PriorityId,
                    AssignedToUserId = ticket.AssignedTo,
                }).FirstOrDefault();
            }
            catch(Exception e){
                return Json(new { success = false, data = e.Message});
            }

            return Json(new { success = true, ticket = ticketInfo, details = details });
            
        }


        [HttpPost]
        public JsonResult addComment(CommentViewModel newComment)
        {
            if (ModelState.IsValid)
            {
                var currentUser = db.AspNetUsers.Find(User.Identity.GetUserId());
                var ticketAttachedTo = db.Tickets.Find(newComment.ticketId);
                TicketComment comment = new TicketComment()
                {
                    AspNetUser = currentUser,
                    userId = currentUser.Id,
                    userName = currentUser.UserName,
                    Ticket = ticketAttachedTo,
                    ticketId = ticketAttachedTo.id,
                    text = newComment.text,
                };


                db.TicketComments.Add(comment);
                db.SaveChanges();
            }
            return Json(new { success = true });
        }



        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public DataTablesResult<TicketViewModelShort> getTickets(DataTablesParam dataTableParam)
        {
            var tickets = db.Tickets.Include(t => t.CreatedByUser).Include(t => t.AssignedToUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus);

            var currentUserId = User.Identity.GetUserId();
            if (!User.IsInRole("Administrator"))
            {
                tickets = tickets.Where(m => m.Project.AspNetUsers.Any(u => u.Id == currentUserId));
                if (!User.IsInRole("Project Manager"))
                {
                    tickets = tickets.Where(m => (m.AssignedToUser.Id == currentUserId) || (m.AssignedToUser == null));
                }
            }
            var result = DataTablesResult.Create(tickets.Select(ticket => new TicketViewModelShort()
            {
                id = ticket.id, 
                Title = ticket.Title,
                Status = ticket.TicketStatus.Status,
                Created = ticket.Created,
                CreatedBy = ticket.CreatedByUser.UserName,
                isCompleted = ( ticket.Completed != null ) ? "<i class=&quot;fa fa-check&quot;></i>" : "",
                AssignedTo = ticket.AssignedToUser.UserName ?? "",
                ProjectId = ticket.ProjectId,
                ProjectName = ticket.Project.Name ?? "",
                PriorityId = ticket.PriorityId,
                AssignedToUserId = ticket.AssignedTo ?? "",
                }),
                dataTableParam,
                formatter => new
                {
                    buttons = "<a href=" + @Url.Action("Edit", new { id = formatter.id }) + " class=\"btn btn-sm btn-success\">" +
                                    "<i class=&quot;glyphicon glyphicon-edit&quot;></i> Edit" +
                                "</a>" +
                                "<a href=\"#\" class=\"btn btn-sm btn-success details\" data-ticketId=\"" + formatter.id + "\"" +
                                    "data-toggle=\"modal\" data-target=\"#detailsPopup\">" +
                                    "<i class=\"glyphicon glyphicon-plus-sign\"></i> Details" +
                                "</a>" +
                                ((User.IsInRole("Administrator")) ?
                                "<a href=" + @Url.Action("Delete", new { id = formatter.id }) + " class=\"btn btn-sm btn-danger\">" +
                                    "<i class=\"icon-flash-off\"></i> Delete" +
                                "</a>" : "")
                }
            );
            return result;
        }

        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult Index()
        {
            if (Request.IsAjaxRequest())
                return PartialView();
            return View();
        }


        // GET: Tickets
        //[Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        //public ActionResult Index(int? page, int? numPerPage, string sortBy, bool showCompleted = false, string search = "", string searchBy = "", bool ascending = true )
        //{
        //    var pageNumber = page ?? 1;
        //    sortBy = sortBy ?? "PriorityId";
        //    searchBy = searchBy ?? "TicketName";

        //    //set ViewBag items to remember last view options
        //    ViewBag.sortBy = sortBy;
        //    ViewBag.Search = search;
        //    ViewBag.ascending = ascending;
        //    ViewBag.searchBy = searchBy;
        //    ViewBag.showCompleted = showCompleted;

        //    var currentPage = page ?? 1;
        //    var tickets = db.Tickets.Include(t => t.CreatedByUser).Include(t => t.AssignedToUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus);

        //    //if the search string is not empty, add the corresponding where clause for search type
        //    if (search != "")
        //        tickets = searchQuery(tickets, search, searchBy);

        //    //If show completed is not checked, select only unresolved bugs
        //    if (!showCompleted)
        //        tickets = tickets.Where(m => m.Completed == null);
        //    var currentUserId = User.Identity.GetUserId();
        //    if (!User.IsInRole("Administrator"))
        //    {
        //        tickets = tickets.Where(m => m.Project.AspNetUsers.Any(u => u.Id == currentUserId));
        //        if (!User.IsInRole("Project Manager"))
        //        {
        //            tickets = tickets.Where(m => (m.AssignedToUser.Id == currentUserId) || (m.AssignedToUser == null));
        //        }
        //    }


        //    if (ascending)
        //    {
        //        //Sort ascending by sortBy
        //        switch (sortBy.ToString())
        //        {
        //            case "Created":
        //                return View(tickets.OrderBy(m => m.Created).ToPagedList(pageNumber, numPerPage ?? 10));
        //            case "Updated":
        //                return View(tickets.OrderBy(m => m.Updated).ToPagedList(pageNumber, numPerPage ?? 10));
        //            case "Completed":
        //                return View(tickets.OrderBy(m => m.Completed).ToPagedList(pageNumber, numPerPage ?? 10));
        //            case "CreatedBy":
        //                return View(tickets.OrderBy(m => m.CreatedByUser.UserName).ToPagedList(pageNumber, numPerPage ?? 10));
        //            case "AssignedTo":
        //                return View(tickets.OrderBy(m => m.AssignedToUser.UserName).ToPagedList(pageNumber, numPerPage ?? 10));
        //            case "Project.Name":
        //                return View(tickets.OrderBy(m => m.Project.Name).ToPagedList(pageNumber, numPerPage ?? 10));
        //            case "PriorityId":
        //                 default:
        //                 return View(tickets.OrderBy(m => m.PriorityId).ToPagedList(pageNumber, numPerPage ?? 10));
        //        }
        //    }
        //    else
        //    {
        //        //Sort descending by sortBy
        //        switch (sortBy.ToString())
        //        {
        //            case "Created":
        //                return View(tickets.OrderByDescending(m => m.Created).ToPagedList(pageNumber, numPerPage ?? 10));
        //            case "Updated":
        //                return View(tickets.OrderByDescending(m => m.Updated).ToPagedList(pageNumber, numPerPage ?? 10));
        //            case "Completed":
        //                return View(tickets.OrderByDescending(m => m.Completed).ToPagedList(pageNumber, numPerPage ?? 10));
        //            case "CreatedBy":
        //                return View(tickets.OrderByDescending(m => m.CreatedByUser.UserName).ToPagedList(pageNumber, numPerPage ?? 10));
        //            case "AssignedTo":
        //                return View(tickets.OrderByDescending(m => m.AssignedToUser.UserName).ToPagedList(pageNumber, numPerPage ?? 10));
        //            case "Project.Name":
        //                return View(tickets.OrderByDescending(m => m.Project.Name).ToPagedList(pageNumber, numPerPage ?? 10));
        //            case "PriorityId":
        //                 default:
        //                 return View(tickets.OrderByDescending(m => m.PriorityId).ToPagedList(pageNumber, numPerPage ?? 10));
        //        }
        //    }
        //}

        // GET: Tickets/Details/5
        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult Create()
        {
            ViewBag.AssignedTo = new SelectList(db.AspNetUsers, "Id", "UserName");
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            ViewBag.PriorityId = new SelectList(db.TicketPriorities, "Id", "Priority");
            ViewBag.StatusId = new SelectList(db.TicketStatuses, "Id", "Status");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize( Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult Create([Bind(Include = "Title,Description,StatusId,AssignedTo,ProjectId,PriorityId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Created = System.DateTimeOffset.Now;
                ticket.CreatedBy = User.Identity.GetUserId();
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AssignedTo = ticket.AssignedTo == null ? null : new SelectList(db.AspNetUsers, "Id", "UserName", ticket.AssignedTo);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.PriorityId = new SelectList(db.TicketPriorities, "Id", "Priority", ticket.PriorityId);
            ViewBag.StatusId = new SelectList(db.TicketStatuses, "Id", "Status", ticket.StatusId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.AssignedTo = new SelectList(db.AspNetUsers, "Id", "Email", ticket.AssignedTo);
            ViewBag.CreatedBy = new SelectList(db.AspNetUsers, "Id", "Email", ticket.CreatedBy);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.PriorityId = new SelectList(db.TicketPriorities, "Id", "Priority", ticket.PriorityId);
            ViewBag.StatusId = new SelectList(db.TicketStatuses, "Id", "Status", ticket.StatusId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult Edit([Bind(Include = "id,Title,Description,StatusId,Created,Updated,CreatedBy,Completed,AssignedTo,ProjectId,PriorityId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.AssignedToUser = db.AspNetUsers.Find(ticket.AssignedTo);
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AssignedTo = new SelectList(db.AspNetUsers, "Id", "Email", ticket.AssignedTo);
            ViewBag.CreatedBy = new SelectList(db.AspNetUsers, "Id", "Email", ticket.CreatedBy);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.PriorityId = new SelectList(db.TicketPriorities, "Id", "Priority", ticket.PriorityId);
            ViewBag.StatusId = new SelectList(db.TicketStatuses, "Id", "Status", ticket.StatusId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
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
