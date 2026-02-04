using System.ComponentModel.DataAnnotations;
using Aban.Domain.Resources;

namespace Aban.Domain.Entities;

public class Country
{
    [Key]
    public int CountryId { get; set; }

    [MaxLength(100, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Country), ResourceType = typeof(Resource))]
    public string Name { get; set; } = null!;

    [Display(Name = nameof(Resource.States), ResourceType = typeof(Resource))]
    public int StatesNumber => States == null ? 0 : States.Count;

    //relaciones
    public ICollection<State>? States { get; set; }

    public ICollection<Corporation>? Corporations { get; set; }
}