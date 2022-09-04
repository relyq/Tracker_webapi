using AutoMapper;
using Tracker.Models;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;

namespace Tracker.Pages
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<Ticket, TicketDto>();
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
                .ForMember(p => p.Tickets, opt => opt.Ignore());
            CreateMap<Comment, CommentDto>();
            CreateMap<CommentDto, Comment>()
                .ForMember(c => c.Ticket, opt => opt.Ignore())
                .ForMember(c => c.Author, opt => opt.Ignore())
                .ForMember(c => c.Parent, opt => opt.Ignore())
                .ForMember(c => c.Replies, opt => opt.Ignore());
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
    }
}
