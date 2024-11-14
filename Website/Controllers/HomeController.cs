using Microsoft.AspNetCore.Mvc;
using RSA_UI.Filter;
using RSA_UI.Models.Entity;
using RSA_UI.Models.Response;
using RSA_UI.Services;
using Website.Services;
using Website.Models.Entity;
using Website.Utils;
using RSA_UI.Utils;
using System.Numerics;
using MyRSA;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RSA_UI.Controllers;
[RequiredLogin]
public class HomeController : Controller
{
    private readonly CompanyService _companyService;
    private readonly EncryptService _encryptService = new EncryptService();
    public HomeController(IService<Company> companyService)
        => _companyService = companyService as CompanyService ?? throw new InvalidCastException("The service could not be cast to CompanyService.");
    public IActionResult Index()
    {
        return RedirectToAction(nameof(EncryptionSecure));
    }
    public IActionResult Signature()
    {
        var uuid = HttpContext.Session.GetString("uuidCompany");
        var company = _companyService.GetById(uuid ?? "");
        if(company == null)
        {
            return BadRequest();
        }
        if(!company.HasDownloadPrivateKey)
        {
            var filePem = Path.Combine(Directory.GetCurrentDirectory(), "Keys", company.Id + ".pem");
            var fileExist = Path.Exists(filePem);
            company.HasDownloadPrivateKey = !fileExist;
            _companyService.Update(company);
        }
        string canGetSign = !company.HasDownloadPrivateKey ? "yes" : "";
        return View("Signature", canGetSign);
    }
    public IActionResult EncryptionSecure()
    {
        Response<List<Company>> response = new Response<List<Company>>
        (
            _companyService.GetAll(),
            200,
            ""
        );
        return View(response);
    }
    [HttpPost]
    public IActionResult EncryptionSecure(IFormFile DocumentFile, string Receiver)
    {
        var publicKey = _companyService.GetById(Receiver)?.PublicKey;
        if (publicKey == null)
        {
            return BadRequest();
        }
        string taskId = "";
        try
        {
            taskId = _encryptService.EncryptSecure(DocumentFile, publicKey: publicKey);
        }
        catch
        {
            return View(new Response<List<Company>>
                (
                    _companyService.GetAll(),
                    400,
                    "Không thể mã hóa file này, vui lòng thử lại!"
                ));
        }
        if(!taskId.IsNotNullOrEmpty())
        {
            return BadRequest();
        }
        Website.Models.Entity.FileResult fileResult = new();
        fileResult.FileName = DocumentFile.FileName;
        fileResult.TaskId = taskId;
        fileResult.FileType = FileUtils.GetFileType(DocumentFile.FileName);
        fileResult.Size = FileUtils.GetFileSize(DocumentFile.Length);
        fileResult.SignType = "Mã hóa bảo mật";
        fileResult.Sender = _companyService.GetById(HttpContext.Session.GetString("uuidCompany") ?? "")?.CompanyName ?? "";
        fileResult.Receiver = _companyService.GetById(Receiver)?.CompanyName ?? "";
        fileResult.ActionTask = "GetResultEnc";
        Response <Website.Models.Entity.FileResult> response = new Response<Website.Models.Entity.FileResult>
        (
            fileResult,
            200,
            "Mã hóa bảo mật thành công! Mời tải file ở dưới"
        );
        return View("ResultSign", response);
    }
    public IActionResult EncryptionSignature()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> EncryptionSignature(IFormFile DocumentFile, IFormFile PrivateKey)
    {
        var authorID = HttpContext.Session.GetString("uuidCompany") ?? "";
        string taskId = "";
        try
        {
            taskId = await _encryptService.EncryptSignature(DocumentFile, privateKey: PrivateKey, authorID);
        }
        catch
        {
            return View(new Response<string>
                (
                    "",
                    400,
                    "Không thể ký chứng thực file này, vui lòng thử lại!"
                ));
        }
        if (!taskId.IsNotNullOrEmpty())
        {
            return BadRequest();
        }
        Website.Models.Entity.FileResult fileResult = new();
        fileResult.FileName = DocumentFile.FileName;
        fileResult.TaskId = taskId;
        fileResult.FileType = FileUtils.GetFileType(DocumentFile.FileName);
        fileResult.Size = FileUtils.GetFileSize(DocumentFile.Length);
        fileResult.SignType = "Mã hóa chứng thực";
        fileResult.Sender = _companyService.GetById(HttpContext.Session.GetString("uuidCompany") ?? "")?.CompanyName ?? "";
        fileResult.Receiver = "";
        fileResult.ActionTask = "GetResultEncSig";
        Response<Website.Models.Entity.FileResult> response = new Response<Website.Models.Entity.FileResult>
        (
            fileResult,
            200,
            "Ký chứng thực file thành công! Mời tải file và chữ ký hash ở dưới"
        );
        return View("ResultSign", response);
    }
    public IActionResult DecryptionSecure()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> DecryptionSecure(IFormFile DocumentFile, IFormFile PrivateKey, IFormFile KeyAes)
    {
        var authorID = HttpContext.Session.GetString("uuidCompany") ?? "";
        string taskId = "";
        try
        {
            taskId = await _encryptService.DecryptSecure(DocumentFile, privateKey: PrivateKey, KeyAes);
        }
        catch
        {
            Response<string> error = new Response<string>
            (
                null,
                400,
                "Không thể giải mã file này, vui lòng kiểm tra lại!"
            );
            return View(nameof(DecryptionSecure), error);
        }
        if (!taskId.IsNotNullOrEmpty())
        {
            return BadRequest();
        }
        Website.Models.Entity.FileResult fileResult = new();
        fileResult.FileName = DocumentFile.FileName;
        fileResult.TaskId = taskId;
        fileResult.FileType = FileUtils.GetFileType(DocumentFile.FileName);
        fileResult.Size = FileUtils.GetFileSize(DocumentFile.Length);
        fileResult.SignType = "Giải mã bảo mật";
        fileResult.Receiver = _companyService.GetById(HttpContext.Session.GetString("uuidCompany") ?? "")?.CompanyName ?? "";
        fileResult.Sender = "";
        fileResult.ActionTask = "GetResultDecSec";
        Response<Website.Models.Entity.FileResult> response = new Response<Website.Models.Entity.FileResult>
        (
            fileResult,
            200,
            "File đã được giải mã thành công! Mời tải file ở dưới"
        );
        return View("ResultSign", response);
    }
    public IActionResult DecryptionSignature()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> DecryptionSignature(IFormFile DocumentFile, string IdCompany, string SignatureCompany)
    {
        var company = _companyService.GetById(IdCompany ?? "");
        var companyPublicKey = company?.PublicKey ?? "";
        bool isValid = false;
        try
        {
            isValid = await _encryptService.DecryptSignature(DocumentFile, companyPublicKey, SignatureCompany);
        }
        catch
        {
            return View(new Response<string>
                (
                    "",
                    400,
                    "Không thể xác thực file này, vui lòng kiểm tra lại!"
                ));
        }
        string message = company != null && company.HasDownloadPrivateKey && isValid ? "Chữ ký số hợp lệ!" : "Chữ ký số không hợp lệ!";
        int code = isValid ? 200 : 400;
        Response<string> response = new Response<string>
        (
            code == 200 ? IdCompany : "",
            code,
            message
        );
        return View(nameof(DecryptionSignature), response);
    }

    public IActionResult DecryptionSignaturePDF()
    {
        return View();
    }
    [HttpPost]
    public IActionResult DecryptionSignaturePDF(IFormFile DocumentFile)
    {
        try
        {
            var serialNumber = PdfExtractor.GetSerialNumber(DocumentFile.OpenReadStream());
            if (serialNumber == null)
            {
                return View(nameof(DecryptionSignaturePDF), new Response<string>
                (
                    "",
                    400,
                    "File PDF không có chữ ký số !"
                ));
            }
            Company? company = _companyService.First(x => x.SerialNumberCertificate == serialNumber.ToString());
            bool isValid = company != null;
            string message = isValid ? "Chữ ký số hợp lệ!" : "Chữ ký số không tồn tại hoặc không hợp lệ !";
            int code = isValid ? 200 : 400;
            Response<string> response = new Response<string>
            (
                code == 200 ? company?.Id : "",
                code,
                message
            );
            return View(nameof(DecryptionSignaturePDF), response);
        }
        catch
        {
            Response<string> response = new Response<string>
            (
                "",
                400,
                "Không thể xác thực file này, vui lòng kiểm tra lại!"
            );
            return View(nameof(DecryptionSignaturePDF), response);
        }
    }


    public IActionResult Profile()
    {
        var uuid = HttpContext.Session.GetString("uuidCompany") ?? "";
        var company = _companyService.GetById(uuid);
        if (company == null)
        {
            return BadRequest();
        }
        return View(nameof(Account), company);
    }
    public IActionResult Account(string uuid)
    {
        var company = _companyService.GetById(uuid);
        if (company == null)
        {
            return NotFound();
        }
        return View(company);
    }

    public IActionResult Member()
    {
        return View();
    }
    public IActionResult Logout()
    {
        HttpContext.Session.SetString("uuidCompany", "");
        return RedirectToAction(nameof(Index));
    }
}