using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BuggerOff;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BuggerOff.ViewModels
{
    public class RolesViewModel
    {
        [Display(Name = "Role Name")]
        public string Name { get; set; }
        public string Id { get; set; }

        public RolesViewModel() { }

        public RolesViewModel(IdentityRole role) : this()
        {
            this.Name = role.Name;
            this.Id = role.Id;
        }
    }
}