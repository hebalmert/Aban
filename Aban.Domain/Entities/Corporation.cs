using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Aban.Domain.EntitesSoftSec;
using Aban.Domain.Resources;

namespace Aban.Domain.Entities;

public class Corporation
{
    [Key]
    public int CorporationId { get; set; }

    [MaxLength(100, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Corporation), ResourceType = typeof(Resource))]
    public string? Name { get; set; }

    [MaxLength(15, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Document), ResourceType = typeof(Resource))]
    public string? NroDocument { get; set; }

    [MaxLength(50, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [DataType(DataType.PhoneNumber)]
    [Display(Name = "Phone", ResourceType = typeof(Resource))]
    public string? Phone { get; set; }

    [MaxLength(50, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [DataType(DataType.PhoneNumber)]
    [Display(Name = "Phone", ResourceType = typeof(Resource))]
    public string? Phone2 { get; set; }

    [MaxLength(50, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [DataType(DataType.PhoneNumber)]
    [Display(Name = "Fax Number")]
    public string? FaxNumber { get; set; }

    [MaxLength(50, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [DataType(DataType.PhoneNumber)]
    [Display(Name = "Fax Number")]
    public string? FaxNumber2 { get; set; }

    [MaxLength(256, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Address), ResourceType = typeof(Resource))]
    public string? Address { get; set; }

    [Required]
    [Display(Name = nameof(Resource.Country), ResourceType = typeof(Resource))]
    public int CountryId { get; set; }

    //Tiempo Activo de la cuenta
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.DateStart), ResourceType = typeof(Resource))]
    public DateTime DateStart { get; set; }

    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.DateEnd), ResourceType = typeof(Resource))]
    public DateTime DateEnd { get; set; }

    [Display(Name = nameof(Resource.Logo), ResourceType = typeof(Resource))]
    public string? Imagen { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    [NotMapped]
    public string? ImageFullPath { get; set; }

    [NotMapped]
    public string? ImgBase64 { get; set; }

    //Relaciones
    public Country? Country { get; set; }

    public ICollection<Manager>? Managers { get; set; }

    public ICollection<Usuario>? Usuarios { get; set; }

    public ICollection<UsuarioRole>? UsuarioRoles { get; set; }
}