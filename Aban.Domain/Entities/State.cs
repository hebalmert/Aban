using System.ComponentModel.DataAnnotations;
using Aban.Domain.Resources;

namespace Aban.Domain.Entities;

public class State
{
    [Key]
    public int StateId { get; set; }

    public int CountryId { get; set; }

    [MaxLength(100, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.State), ResourceType = typeof(Resource))]
    public string Name { get; set; } = null!;

    [Display(Name = nameof(Resource.Cities), ResourceType = typeof(Resource))]
    public int CitiesNumber => Cities == null ? 0 : Cities.Count;

    //Relaciones
    public Country? Country { get; set; }

    public ICollection<City>? Cities { get; set; }
}