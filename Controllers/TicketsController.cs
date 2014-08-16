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
using System.Security.Cryptography;
using System.IO;

namespace BuggerOff.Controllers
{
    public class TicketsController : Controller
    {
        private BuggerOffEntities db = new BuggerOffEntities();

        [HttpPost]
        public JsonResult getTicketDetails(int id)
        {
            TicketViewModelDetails details;
            TicketViewModelShort ticketInfo;
            var users = new List<Object>();
            var tickets = db.Tickets.Include(t => t.CreatedByUser).Include(t => t.AssignedToUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus);
            
            try
            {
                details = new TicketViewModelDetails(id);
                var ticket =  tickets.Where(t=>t.id==id);

                ticketInfo = ticket.Select(currentTicket => new TicketViewModelShort()
                {
                    id = currentTicket.id,
                    Title = currentTicket.Title,
                    Status = currentTicket.TicketStatus.Status,
                    Created = currentTicket.Created,
                    CreatedBy = currentTicket.CreatedByUser.UserName,
                    isCompleted = (currentTicket.Completed != null) ? "<i class=&quot;fa fa-check&quot;></i>" : "",
                    AssignedTo = currentTicket.AssignedToUser.UserName,
                    ProjectId = currentTicket.ProjectId,
                    ProjectName = currentTicket.Project.Name,
                    PriorityId = currentTicket.PriorityId,
                    AssignedToUserId = currentTicket.AssignedTo,
                }).FirstOrDefault();

                var userList = ticket.FirstOrDefault().Project.AspNetUsers.ToList();
                
                foreach (var user in userList)
                {
                    users.Add(new { id = user.Id, username = user.UserName });
                }
                
            }
            catch(Exception e){
                return Json(new { success = false, data = e.Message});
            }

            return Json(new { success = true, ticket = ticketInfo, details = details, users = users });
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

        private string createAttachment(int ticketId, string uploadDescription, HttpPostedFileBase attachment)
        {
            try
            {
                var filenameMd5 = string.Concat(MD5CryptoServiceProvider.Create().ComputeHash(attachment.InputStream).Select(x => x.ToString("X2")));

                var ticketAttachment = new TicketAttachment()
                {
                    Description = uploadDescription,
                    AddedBy = User.Identity.GetUserId(),
                    FilenameMd5 = filenameMd5,
                    Filename = attachment.FileName,
                    Created = DateTimeOffset.UtcNow,
                    TicketID = ticketId,
                    Ticket = db.Tickets.Find(ticketId)
                };

                var uploadPath = Server.MapPath("~/Uploads/" + ticketId.ToString() + '/');

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                attachment.SaveAs(uploadPath + filenameMd5 + attachment.FileName);
                db.TicketAttachments.Add(ticketAttachment);
                db.SaveChanges();
            }
            catch (Exception e) { return e.Message; }

            return "success";
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult uploadAttachment()
        {
            if( String.IsNullOrEmpty(Request["ticketId"]) )
                return Json(new {result = "Invalid ticket Id"});

            int id = Int32.Parse(Request["ticketId"]);
            var description = Request["uploadDescription"];
            var attachment = Request.Files["attachment"];

            return Json(new { result = createAttachment(id, description, attachment) });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult editTicket(TicketViewModel postedTicket)
        {
            /*
             * Editable fields:
             * AssignedTo (User)
             * Description
             * Title
             * Status (Id)
             * 
             * Priority /NYI
             * Title /NYI
             */
            string attachmentResult = "no attachment";
            string result = "success";

            if (Request.Files["attachment"].ContentLength != 0)
                attachmentResult = createAttachment(postedTicket.id, postedTicket.description, Request.Files["attachment"]);
            try
            {
                Ticket ticket = db.Tickets.Single(t => t.id == postedTicket.id);

                ticket.AssignedToUser = db.AspNetUsers.Single(u => u.Id == postedTicket.AssignedToUser);
                ticket.Description = postedTicket.description;
                ticket.TicketStatus = db.TicketStatuses.Single(s => s.Id == postedTicket.statusId);
                ticket.Updated = System.DateTimeOffset.UtcNow;

                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                result = e.Message;
            }
            return Json(new { result = result, attachmentResult = attachmentResult });
        }

        [HttpPost]
        public ActionResult createTicket(TicketViewModel postedTicket)
        {
            var ticket = new Ticket() { 
                Title = postedTicket.Title,
                Description = postedTicket.description,
                ProjectId = postedTicket.ProjectId,
                StatusId = postedTicket.statusId,
                CreatedBy = User.Identity.GetUserId(),
                AssignedTo = postedTicket.AssignedTo,
                Created = System.DateTimeOffset.UtcNow,
                PriorityId = 1
            };
            db.Tickets.Add(ticket);
            db.SaveChanges();

            //return the id of the newly created ticket
            return Json(new { id = ticket.id });
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
                    buttons =   "<a href=\"#\" class=\"btn btn-sm btn-success details\" data-ticketId=\"" + formatter.id + "\"" +
                                    "data-toggle=\"modal\" data-target=\"#detailsPopup\">" +
                                    "<i class=\"glyphicon glyphicon-plus-sign\"></i> Details" +
                                "</a>"
                }
            );
            return result;
        }

        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult Index()
        {
            ViewBag.StatusId = new SelectList(db.TicketStatuses, "Id", "Status");
            ViewBag.AssignedTo = new SelectList(db.AspNetUsers, "Id", "UserName");
            var userId = User.Identity.GetUserId();
            var user = db.AspNetUsers.Single(u=> u.Id == userId);
            
            //Get all of the projects the current user is associated with

            var projectQuery = db.Projects.Where(p => p.AspNetUsers.Contains(user));
            if (User.IsInRole("Administrator"))
                projectQuery = db.Projects;
            
            ViewBag.ProjectId = new SelectList(projectQuery, "Id", "Name");
            if (Request.IsAjaxRequest())
                return PartialView(new Ticket());
            return View(new Ticket());
        }

        [Authorize(Roles = "Administrator, Project Manager, Senior Developer, Developer")]
        public ActionResult getUsersForProjects()
        {
            var projectUserDict = new Dictionary<string, Dictionary<string,string>>();
           
            foreach (var project in db.Projects.ToList()) {
                var userDict = new Dictionary<string, string>();
                foreach (var user in project.AspNetUsers.ToList())
                {
                    userDict.Add(user.Id,user.UserName);
                }
                projectUserDict.Add(project.Id.ToString(), userDict);
            }

            return Json(projectUserDict, JsonRequestBehavior.AllowGet);
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
