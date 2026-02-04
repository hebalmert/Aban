using System.ComponentModel.DataAnnotations;
using Aban.Domain.Resources;

namespace Aban.Domain.Entities;

public class City
{
    [Key]
    public int CityId { get; set; }

    [Display(Name = nameof(Resource.State), ResourceType = typeof(Resource))]
    public int StateId { get; set; }

    [MaxLength(100, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.City), ResourceType = typeof(Resource))]
    public string Name { get; set; } = null!;

    //Relaciones
    public State? State { get; set; }
}