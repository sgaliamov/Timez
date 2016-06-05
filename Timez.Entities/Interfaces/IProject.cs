using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Timez.Entities
{
    public interface IProject : IId
    {
        int BoardId { get; set; }

        [DisplayName("Название проекта")]
        [Required(ErrorMessage = "Название обязательно")]
        string Name { get; set; }
    }

    public class TimezProject : IProject
    {
        public TimezProject(IProject project)
        {
            _Id = project.Id;
            Name = project.Name;
            BoardId = project.BoardId;
        }

        private readonly int _Id;
        public int Id { get { return _Id; } }
        public int BoardId { get; set; }
        public string Name { get; set; }
    }
}
