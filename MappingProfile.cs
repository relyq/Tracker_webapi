﻿using AutoMapper;
using Tracker.Models;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;

namespace Tracker
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<Ticket, TicketDto>()
                .ForMember(t => t.Activity, opt => opt.Ignore())
                .AfterMap<TicketActivityAction>();
            CreateMap<TicketDto, Ticket>()
                .ForMember(t => t.Status, opt => opt.Ignore())
                .ForMember(t => t.Type, opt => opt.Ignore())
                .ForMember(t => t.TicketStatusId, opt => opt.Ignore())
                .ForMember(t => t.TicketTypeId, opt => opt.Ignore())
                .ForMember(t => t.Comments, opt => opt.Ignore())
                .ForMember(t => t.Project, opt => opt.Ignore())
                .ForMember(t => t.Submitter, opt => opt.Ignore())
                .ForMember(t => t.Assignee, opt => opt.Ignore())
                .AfterMap<TicketTypeStatusAction>();
            CreateMap<Project, ProjectDto>();
            CreateMap<ProjectDto, Project>()
                .ForMember(p => p.Author, opt => opt.Ignore())
                .ForMember(p => p.Organization, opt => opt.Ignore())
                .ForMember(p => p.OrganizationId, opt => opt.Ignore())
                .ForMember(p => p.Tickets, opt => opt.Ignore());
            CreateMap<Comment, CommentDto>();
            CreateMap<CommentDto, Comment>()
                .ForMember(c => c.Ticket, opt => opt.Ignore())
                .ForMember(c => c.Author, opt => opt.Ignore())
                .ForMember(c => c.Parent, opt => opt.Ignore())
                .ForMember(c => c.Replies, opt => opt.Ignore());
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(u => u.OrganizationsId, opt => opt.Ignore())
                .ForMember(u => u.Roles, opt => opt.Ignore())
                .AfterMap<UserOrganizationsAction>()
                .AfterMap<UserRolesAction>();
            CreateMap<UserDto, ApplicationUser>()
                .ForMember(u => u.Organizations, opt => opt.Ignore())
                .ForMember(u => u.Roles, opt => opt.Ignore())
                .ForMember(u => u.Comments, opt => opt.Ignore())
                .ForMember(u => u.Updated, opt => opt.Ignore())
                .ForMember(u => u.NormalizedUserName, opt => opt.Ignore())
                .ForMember(u => u.NormalizedEmail, opt => opt.Ignore())
                .ForMember(u => u.EmailConfirmed, opt => opt.Ignore())
                .ForMember(u => u.PasswordHash, opt => opt.Ignore())
                .ForMember(u => u.SecurityStamp, opt => opt.Ignore())
                .ForMember(u => u.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(u => u.PhoneNumber, opt => opt.Ignore())
                .ForMember(u => u.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(u => u.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(u => u.LockoutEnd, opt => opt.Ignore())
                .ForMember(u => u.LockoutEnabled, opt => opt.Ignore())
                .ForMember(u => u.AccessFailedCount, opt => opt.Ignore());
            CreateMap<Organization, OrganizationDto>();
            CreateMap<OrganizationDto, Organization>()
                .ForMember(o => o.Users, opt => opt.Ignore())
                .ForMember(o => o.Projects, opt => opt.Ignore())
                .ForMember(o => o.Roles, opt => opt.Ignore())
                .ForMember(o => o.TicketTypes, opt => opt.Ignore())
                .ForMember(o => o.TicketStatuses, opt => opt.Ignore());
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
