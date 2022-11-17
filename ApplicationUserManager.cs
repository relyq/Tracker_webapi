using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Threading;
using Tracker.Data;
using Tracker.Models;

namespace Tracker
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        private readonly UserStore<ApplicationUser, IdentityRole, ApplicationDbContext, string, IdentityUserClaim<string>,
        UserRole, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>
        _store;

        public ApplicationUserManager(
            IUserStore<ApplicationUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
            IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _store = (UserStore<ApplicationUser, IdentityRole, ApplicationDbContext, string, IdentityUserClaim<string>,
            UserRole, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>)store;
        }

        public virtual async Task<ApplicationUser> FindByEmailAsync(string email)
        {
            ThrowIfDisposed();

            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }

            var user = await Users.Include(u => u.Organizations).FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());

            if (user != null)
            {
                user.Roles = await GetRolesAsync(user);
            }

            return user;
        }

        public virtual async Task<ApplicationUser> FindByIdAsync(string id)
        {
            ThrowIfDisposed();

            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var user = await Users.Include(u => u.Organizations).FirstOrDefaultAsync(u => u.Id == id);

            if (user != null)
            {
                user.Roles = await GetRolesAsync(user);
            }

            return user;
        }

        public virtual async Task<IdentityResult> AddToRoleByIdAsync(ApplicationUser user, string roleId, Organization organization)
        {
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentNullException(nameof(roleId));

            if (await IsInRoleByIdAsync(user, roleId, organization, CancellationToken))
                return IdentityResult.Failed(ErrorDescriber.UserAlreadyInRole(roleId));

            _store.Context.Set<UserRole>().Add(new UserRole { RoleId = roleId, UserId = user.Id, OrganizationId = organization.Id });

            return await UpdateUserAsync(user);
        }

        public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName, Organization organization)
        {
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException(nameof(roleName));

            if (await IsInRoleAsync(user, roleName, organization))
                return IdentityResult.Failed(ErrorDescriber.UserAlreadyInRole(roleName));

            var role = await _store.Context.Set<IdentityRole>().SingleOrDefaultAsync(r => r.Name.ToUpper() == roleName.ToUpper());

            _store.Context.Set<UserRole>().Add(new UserRole { RoleId = role.Id, UserId = user.Id, OrganizationId = organization.Id });

            return await UpdateUserAsync(user);
        }

        public async Task<IList<UserRole>> GetRolesAsync(ApplicationUser user)
        {
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var roles = await _store.Context.Set<UserRole>().Where(ur => ur.UserId == user.Id).ToListAsync();

            return roles;
        }

        public async Task<IList<string>> GetRolesInOrganizationAsync(ApplicationUser user, Organization organization)
        {
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var roles = await _store.Context.Set<UserRole>().Where(ur => ur.UserId == user.Id && ur.OrganizationId == organization.Id).ToListAsync();

            IList<string> roleNames = new List<string>();

            roles.ForEach(r => roleNames.Add(_store.Context.Set<IdentityRole>().Find(r.RoleId).Name));

            return roleNames;
        }

        public async Task<bool> IsInRoleByIdAsync(ApplicationUser user, string roleId, Organization organization, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentNullException(nameof(roleId));

            if (organization == null)
                throw new ArgumentNullException(nameof(organization));

            var role = await _store.Context.Set<IdentityRole>().FindAsync(roleId);

            if (role == null)
                return false;

            var userRole = await _store.Context.Set<UserRole>().FindAsync(new object[] { user.Id, roleId, organization.Id }, cancellationToken);
            return userRole != null;
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, Organization organization, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException(nameof(roleName));

            if (organization == null)
                throw new ArgumentNullException(nameof(organization));

            var role = await _store.Context.Set<IdentityRole>().SingleOrDefaultAsync(r => r.Name.ToUpper() == roleName.ToUpper());

            if (role == null)
                return false;

            var userRole = await _store.Context.Set<UserRole>().FindAsync(new object[] { user.Id, role.Id, organization.Id }, cancellationToken);
            return userRole != null;
        }


        public async Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string role)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            throw new NotImplementedException();
        }


    }
}
