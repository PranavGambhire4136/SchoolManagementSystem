using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models.ViewModel
{
    public class AddSubjectViewModel
    {
        public string Name { get; set; } = string.Empty;

        public List<int> ClassIds { get; set; } = new();

        public List<Class> Classes { get; set; } = new();
    }
}