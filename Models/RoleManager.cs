using Microsoft.AspNet.Identity;
using BuggerOff.DataAccess;
using System;
using System.Threading.Tasks;
using System.Linq;
namespace BuggerOff.DataAccess
{
    public partial class AspNetRole : IRole
    {
        //Must use name constructor when 
        //creating a new AspNetRole to add to database
        public AspNetRole(string roleName)
            : this()
        {
            Id = Guid.NewGuid().ToString();
            Name = roleName;
        }
        //public string Id { get; set; }
        //public string Name { get; set; }

        
    }

    public class ApplicationRoleStore : IRoleStore<AspNetRole>
    {
        private BuggerOffEntities _context;
        public ApplicationRoleStore() { }

        public ApplicationRoleStore(BuggerOffEntities database)
        {
            _context = database;
        }

        public async Task CreateAsync(AspNetRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("RoleIsRequired");
            }
            _context.AspNetRoles.Add(role);
            await _context.SaveChangesAsync();
        }

        public Task DeleteAsync(AspNetRole role)
        {
            var roleEntity = _context.AspNetRoles.FirstOrDefault(x => x.Id == role.Id);
            if (roleEntity == null) throw new InvalidOperationException("No such role exists!");
            _context.AspNetRoles.Remove(roleEntity);
            return _context.SaveChangesAsync();
        }

        public Task<AspNetRole> FindByIdAsync(string roleId)
        {
            var role = _context.AspNetRoles.FirstOrDefault(x => x.Id == roleId);

            return Task.FromResult(role);
        }

        public async Task<AspNetRole> FindByNameAsync(string roleName)
        {

            var role = _context.AspNetRoles.FirstOrDefault(x => x.Name == roleName);

            return await Task.FromResult(role);
        }

        public Task UpdateAsync(AspNetRole role)
        {

            return _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}