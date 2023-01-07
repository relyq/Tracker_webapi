using AutoMapper;
using Tracker.Models;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;
using System.Linq.Expressions;


namespace Tracker
{

    public static class ExtensionMapping
    {
        public static IMappingExpression<T, L> Ignore<T, L>(this IMappingExpression<T, L> createmap, params Expression<Func<L, object?>>[] destinationMember)
        {
            return destinationMember.Aggregate(createmap, (current, includeMember) => current.ForMember(includeMember, opt => opt.Ignore()));
        }
    }
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {

            CreateMap<Ticket, TicketDto>()
                .Ignore(t => t.Activity)
                .AfterMap<TicketActivityAction>();

            CreateMap<TicketDto, Ticket>()
                .Ignore(s => s.Status, s => s.Type, s => s.TicketStatusId, s => s.TicketTypeId, s => s.Comments, s => s.Project, s => s.Submitter, s => s.Assignee)
                .AfterMap<TicketTypeStatusAction>();

            CreateMap<Project, ProjectDto>();
            CreateMap<ProjectDto, Project>()
                .Ignore(p => p.Author, p => p.Organization, p => p.OrganizationId, p => p.Tickets);

            CreateMap<Comment, CommentDto>();
            CreateMap<CommentDto, Comment>()
                .Ignore(c => c.Ticket, c => c.Author, c => c.Parent, c => c.Replies);

            CreateMap<ApplicationUser, UserDto>()
                .Ignore(u => u.OrganizationsId, u => u.Roles)
                .AfterMap<UserOrganizationsAction>()
                .AfterMap<UserRolesAction>();
            CreateMap<UserDto, ApplicationUser>()
                .Ignore(u => u.Organizations, u => u.Roles, u => u.Comments, u => u.Updated,
                        u => u.NormalizedUserName, u => u.NormalizedEmail, u => u.EmailConfirmed, u => u.PasswordHash,
                        u => u.SecurityStamp, u => u.ConcurrencyStamp, u => u.PhoneNumber, u => u.PhoneNumberConfirmed,
                        u => u.TwoFactorEnabled, u => u.LockoutEnd, u => u.LockoutEnabled, u => u.AccessFailedCount);

            CreateMap<Organization, OrganizationDto>();
            CreateMap<OrganizationDto, Organization>()
                .Ignore(o => o.Users, o => o.Projects, o => o.Roles, o => o.TicketTypes, o => o.TicketStatuses);
        }
        public class TicketTypeStatusAction : IMappingAction<TicketDto, Ticket>
        {
            private readonly ApplicationDbContext _context;
            public TicketTypeStatusAction(ApplicationDbContext context)
            {
                _context = context;
            }
            public void Process(TicketDto ticketDto, Ticket ticket, ResolutionContext context)
            {
                var status = _context.TicketStatus.FirstOrDefault(s => s.Status == ticketDto.Status);
                var type = _context.TicketType.FirstOrDefault(t => t.Type == ticketDto.Type);
                ticket.TicketStatusId = status.Id;
                ticket.TicketTypeId = type.Id;
            }
        }
        public class TicketActivityAction : IMappingAction<Ticket, TicketDto>
        {
            private readonly ApplicationDbContext _context;
            public TicketActivityAction(ApplicationDbContext context)
            {
                _context = context;
            }
            public void Process(Ticket ticket, TicketDto ticketDto, ResolutionContext context)
            {
                DateTime? activity = _context.Comment.Where(c => c.TicketId == ticket.Id).OrderBy(c => c.Created).LastOrDefault()?.Created;

                ticketDto.Activity = activity;
            }
        }

        public class UserOrganizationsAction : IMappingAction<ApplicationUser, UserDto>
        {
            public void Process(ApplicationUser applicationUser, UserDto userDto, ResolutionContext context)
            {
                if (applicationUser.Organizations != null)
                {
                    userDto.OrganizationsId = new List<Guid>();

                    applicationUser.Organizations.ToList().ForEach(o =>
                    {
                        userDto.OrganizationsId.Add(o.Id);
                    });
                }
            }
        }

        public class UserRolesAction : IMappingAction<ApplicationUser, UserDto>
        {
            public void Process(ApplicationUser applicationUser, UserDto userDto, ResolutionContext context)
            {
                if (applicationUser.Roles != null)
                {
                    userDto.Roles = new List<OrganizationRole>();

                    ((List<UserRole>)applicationUser.Roles).ForEach(o =>
                    {
                        userDto.Roles.Add(new OrganizationRole(o.OrganizationId, o.RoleId));
                    });
                }
            }
        }
    }

}
