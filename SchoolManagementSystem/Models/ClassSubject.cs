using SchoolManagementSystem.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class ClassSubject
{
    [ForeignKey("Class")]
    public int ClassId { get; set; }

    public Class Class { get; set; } = null!;

    [ForeignKey("Subject")]
    public int SubjectId { get; set; }

    public Subject Subject { get; set; } = null!;
}