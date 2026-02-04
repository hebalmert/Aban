namespace Aban.Domain.ResponcesSec;

public class ClaimsDTOs
{
    public string UserName { get; set; } = default!;
    public string Id { get; set; } = default!;
    public string Role { get; set; } = default!;
    public int CorporationId { get; set; }
}