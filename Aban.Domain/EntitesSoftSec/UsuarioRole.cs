using System.ComponentModel.DataAnnotations;
using Aban.Domain.EntitesSoftSec;
using Aban.Domain.Entities;
using Aban.Domain.Enum;
using Aban.Domain.Resources;

namespace Aban.Domain.EntitesSoftSec;

public class UsuarioRole
{
    [Key]
    public int UsuarioRoleId { get; set; }

    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.User), ResourceType = typeof(Resource))]
    public int UsuarioId { get; set; }

    [Display(Name = nameof(Resource.RoleUser), ResourceType = typeof(Resource))]
    public UserType UserType { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public Usuario? Usuario { get; set; }
}