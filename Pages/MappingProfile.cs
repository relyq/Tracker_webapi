using AutoMapper;
using Tracker.Models;

namespace Tracker.Pages
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Ticket, TicketDto>();
            CreateMap<Comment, CommentDto>();
            CreateMap<Project, ProjectDto>();
        }
    }
}
