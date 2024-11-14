using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSA_UI.Models.Entity;

public class Company
{
    [Key] 
    [Required(ErrorMessage = "Company ID is required")]
    [Column("ID")]
    public string Id { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LogoCompany  { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public bool HasDownloadPrivateKey { get; set; }
    public string SerialNumberCertificate { get; set; } = string.Empty;
}