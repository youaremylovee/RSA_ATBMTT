using MyRSA;
using RSA_UI.Models.Entity;
using RSA_UI.Repositories;
using RSA_UI.Repositories.RSA;
using RSA_UI.Utils;
using System.IO;

namespace RSA_UI.Services;
public enum RegisterStatus
{
    Success,
    EmailExist,
    Error
}
public class CompanyService : IService<Company>
{
    private readonly CompanyRepository _repository;
    
    // Constructor
    public CompanyService(IRepository<Company> repository) => _repository = repository as CompanyRepository ?? throw new NullReferenceException();

    public Company? Login(string email, string password)
    {
        return _repository.First(c => c.Email == email && password.ToMd5() == c.Password);
    }

    public RegisterStatus Register(Company company, IFormFile logo)
    {
        var email = company.Email.ToLower();
        var existEmail = _repository.First(m => m.Email.ToLower().Equals(email));
        if(existEmail != null)
        {
            return RegisterStatus.EmailExist;
        }
        var guid = Guid.NewGuid();
        company.Id = guid.ToString();
        company.Password = company.Password.ToMd5();
        //Create RSA key
        var p = MyRSA.Utils.GenerateLargePrime(305, 10);
        var q = MyRSA.Utils.GenerateLargePrime(305, 10);
        var customRsa = Rsa.Create(p, q, 10).Build();
        company.PublicKey = customRsa.RsaChain.GetPublicKey();
        var filePrivateKey = $"{company.Id}.pem";
        var filePublicKey = $"{company.Id}_public.pem";
        var pathPrivateKey = Path.Combine(Directory.GetCurrentDirectory(), "Keys", filePrivateKey);
        var pathPublicKey = Path.Combine(Directory.GetCurrentDirectory(), "Keys", filePublicKey);
        var filePfx = $"{company.Id}.pfx";
        var pathPfx = Path.Combine(Directory.GetCurrentDirectory(), "Keys", filePfx);
        Org.BouncyCastle.Math.BigInteger serialNumber = customRsa.RsaChain.ToCertificationFile(company.CompanyName, guid, pathPfx);
        customRsa.RsaChain.ToPemFile(pathPrivateKey, isPrivateKey: true);
        customRsa.RsaChain.ToPemFile(pathPublicKey, isPrivateKey: false);
        company.SerialNumberCertificate = serialNumber.Abs().ToString();
        //Upload logo file
        if (logo != null)
        {
            var fileName = $"{company.Id}{Path.GetExtension(logo.FileName)}";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logos", fileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                logo.CopyTo(stream);
            }
            company.LogoCompany = fileName;
        }
        else
        {
            company.LogoCompany = "default.webp";
        }
        var t = _repository.Add(company);
        if(t == null)
        {
            return RegisterStatus.Error;
        }
        return RegisterStatus.Success;
    }

    // Lấy Company theo ID
    public Company? GetById(string id)
    {
        return _repository.GetById(id);
    }

    // Cập nhật thông tin Company
    public bool Update(Company entity)
    {
        return _repository.Update(entity);
    }

    // Thêm Company mới
    public Company? Add(Company entity)
    {
        return _repository.Add(entity);
    }

    // Xóa Company
    public bool Delete(Company entity)
    {
        return _repository.Delete(entity);
    }
    public List<Company> GetAll()
    {
        return _repository.GetAll().ToList();
    }

    public Company? First(Func<Company, bool> predicate)
    {
        return _repository.First(predicate);
    }
}