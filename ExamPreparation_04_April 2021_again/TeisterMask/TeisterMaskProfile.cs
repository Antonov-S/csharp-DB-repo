namespace TeisterMask
{
    using AutoMapper;
    using System.Linq;
    using System.Threading.Tasks;
    using TeisterMask.Data.Models;
    using TeisterMask.DataProcessor.ExportDto;

    public class TeisterMaskProfile : Profile
    {
        // Configure your AutoMapper here if you wish to use it. If not, DO NOT DELETE OR RENAME THIS CLASS
        public TeisterMaskProfile()
        {
            this.CreateMap<Data.Models.Task, ExportProjectTaskDto>()
                .ForMember(dto => dto.Name, m => m.MapFrom(t => t.Name))
                .ForMember(dto => dto.Label, m => m.MapFrom(t => t.LabelType.ToString()));
            this.CreateMap<Project, ExportProjectDto>()
                .ForMember(dto => dto.ProjectName, m => m.MapFrom(p => p.Name))
                .ForMember(dto => dto.HasEndDate, m => m.MapFrom(p => p.DueDate.HasValue ? "Yes" : "No"))
                .ForMember(dto => dto.TasksCount, m => m.MapFrom(p => p.Tasks.Count))
                .ForMember(dto => dto.Tasks, m => m.MapFrom(p => p.Tasks.ToArray().OrderBy(t => t.Name).ToArray()));
        }
    }
}
