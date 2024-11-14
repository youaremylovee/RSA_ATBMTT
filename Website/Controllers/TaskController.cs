using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using RSA_UI.Filter;
using RSA_UI.Services;
using RSA_UI.Models.Entity;

namespace RSA_UI.Controllers
{
    [RequiredLogin]
    public class TaskController : Controller
    {
        private readonly CompanyService _companyService;
        public TaskController(IService<Company> service)
        {
            _companyService = service as CompanyService ?? throw new InvalidCastException("The service could not be cast to CompanyService.");
        }
        // Phương thức để tải xuống 3 file trong 1 file ZIP
        public IActionResult Certificates()
        {
            var uuid = HttpContext.Session.GetString("uuidCompany");
            var company = _companyService.GetById(uuid ?? "");
            if (uuid == null || company == null)
            {
                return NotFound("Không tìm thấy thông tin công ty.");
            }
            var pfxFile = Path.Combine(Directory.GetCurrentDirectory(), "Keys", $"{uuid}.pfx");
            var pemPrivateFile = Path.Combine(Directory.GetCurrentDirectory(), "Keys", $"{uuid}.pem");
            var pemPublicFile = Path.Combine(Directory.GetCurrentDirectory(), "Keys", $"{uuid}_public.pem");
            // Kiểm tra xem các file có tồn tại không
            if (!System.IO.File.Exists(pfxFile) || !System.IO.File.Exists(pemPrivateFile))
            {
                return NotFound("Một hoặc nhiều file không tồn tại.");
            }
            var zipMemoryStream = new MemoryStream();
            using (var archive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create, true))
            {
                // Thêm file PFX vào ZIP
                var pfxEntry = archive.CreateEntry("certificate.pfx");
                using (var entryStream = pfxEntry.Open())
                using (var fileStream = System.IO.File.OpenRead(pfxFile))
                {
                    fileStream.CopyTo(entryStream);
                }

                var privateKeyEntry = archive.CreateEntry("private.pem");
                using (var entryStream = privateKeyEntry.Open())
                using (var fileStream = System.IO.File.OpenRead(pemPrivateFile))
                {
                    fileStream.CopyTo(entryStream);
                }

                var publicKeyEntry = archive.CreateEntry("public.pem");
                using (var entryStream = publicKeyEntry.Open())
                using (var fileStream = System.IO.File.OpenRead(pemPublicFile))
                {
                    fileStream.CopyTo(entryStream);
                }
            }

            zipMemoryStream.Seek(0, SeekOrigin.Begin);

            // Xóa file PFX và PEM
            System.IO.File.Delete(pfxFile);
            System.IO.File.Delete(pemPrivateFile);
            System.IO.File.Delete(pemPublicFile);
            
            company.HasDownloadPrivateKey = true;

            return File(zipMemoryStream.ToArray(), "application/zip",  company.CompanyName.Replace(" ","") + ".zip");
        }
        public IActionResult GetResultEnc(string filename, string taskId)
        {
            var taskFile = Path.Combine(Directory.GetCurrentDirectory(), "Tasks", taskId + filename);
            var taskKeyFile = Path.Combine(Directory.GetCurrentDirectory(), "Tasks", taskId + ".aes");

            if (!System.IO.File.Exists(taskFile) || !System.IO.File.Exists(taskKeyFile))
            {
                return NotFound("File không tồn tại.");
            }

            // Tạo tên file zip
            var zipFileName = "EncSecure_" + taskId + ".zip";
            var zipFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Temp", zipFileName);

            // Tạo thư mục tạm thời nếu chưa có
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Temp")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Temp"));
            }

            // Tạo file zip và thêm file vào
            using (var zipFileStream = new FileStream(zipFilePath, FileMode.Create))
            using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
            {
                var zipEntry = archive.CreateEntry("encrypted_" + filename);
                using (var fileStream = new FileStream(taskFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var entryStream = zipEntry.Open())
                {
                    fileStream.CopyTo(entryStream);
                }
                var keyEntry = archive.CreateEntry("key.aes");
                using (var fileStream = new FileStream(taskKeyFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var entryStream = keyEntry.Open())
                {
                    fileStream.CopyTo(entryStream);
                }
            }

            // Mở file zip cho việc trả về
            var zipFileStreamResult = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileResult = new FileStreamResult(zipFileStreamResult, "application/zip")
            {
                FileDownloadName = zipFileName
            };

            // Đặt Task để xóa file zip và tạm sau khi response hoàn thành
            Response.OnCompleted(() =>
            {
                zipFileStreamResult.Dispose();
                if (System.IO.File.Exists(zipFilePath))
                {
                    System.IO.File.Delete(zipFilePath);
                }
                if (System.IO.File.Exists(taskFile))
                {
                    System.IO.File.Delete(taskFile);
                }
                if (System.IO.File.Exists(taskKeyFile))
                {
                    System.IO.File.Delete(taskKeyFile);
                }
                return Task.CompletedTask;
            });

            return fileResult;
        }
        public IActionResult GetResultEncSig(string filename, string taskId)
        {
            var taskFile = Path.Combine(Directory.GetCurrentDirectory(), "Tasks", taskId + filename);
            var taskKeyFile = Path.Combine(Directory.GetCurrentDirectory(), "Tasks", taskId + ".hash");

            if (!System.IO.File.Exists(taskFile) || !System.IO.File.Exists(taskKeyFile))
            {
                return NotFound("File không tồn tại.");
            }

            // Tạo tên file zip
            var zipFileName = "EncSignature_" + taskId + ".zip";
            var zipFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Temp", zipFileName);

            // Tạo thư mục tạm thời nếu chưa có
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Temp")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Temp"));
            }

            // Tạo file zip và thêm file vào
            using (var zipFileStream = new FileStream(zipFilePath, FileMode.Create))
            using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
            {
                var zipEntry = archive.CreateEntry(filename);
                using (var fileStream = new FileStream(taskFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var entryStream = zipEntry.Open())
                {
                    fileStream.CopyTo(entryStream);
                }
                var keyEntry = archive.CreateEntry("signature.hash");
                using (var fileStream = new FileStream(taskKeyFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var entryStream = keyEntry.Open())
                {
                    fileStream.CopyTo(entryStream);
                }
            }

            // Mở file zip cho việc trả về
            var zipFileStreamResult = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileResult = new FileStreamResult(zipFileStreamResult, "application/zip")
            {
                FileDownloadName = zipFileName
            };

            // Đặt Task để xóa file zip và tạm sau khi response hoàn thành
            Response.OnCompleted(() =>
            {
                zipFileStreamResult.Dispose();
                if (System.IO.File.Exists(zipFilePath))
                {
                    System.IO.File.Delete(zipFilePath);
                }
                if (System.IO.File.Exists(taskFile))
                {
                    System.IO.File.Delete(taskFile);
                }
                if (System.IO.File.Exists(taskKeyFile))
                {
                    System.IO.File.Delete(taskKeyFile);
                }
                return Task.CompletedTask;
            });

            return fileResult;
        }
        public IActionResult GetResultDecSec(string filename, string taskId)
        {
            var taskFile = Path.Combine(Directory.GetCurrentDirectory(), "Tasks", taskId + filename);

            if (!System.IO.File.Exists(taskFile))
            {
                return NotFound("File không tồn tại.");
            }
            string fileDownloadName = filename.Replace("encrypted_", "decrypted_");
            // Mở file trực tiếp cho việc trả về
            var fileStream = new FileStream(taskFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileResult = new FileStreamResult(fileStream, "application/octet-stream")
            {
                FileDownloadName = fileDownloadName
            };

            // Đặt Task để xóa file sau khi response hoàn thành
            Response.OnCompleted(() =>
            {
                fileStream.Dispose();
                if (System.IO.File.Exists(taskFile))
                {
                    System.IO.File.Delete(taskFile);
                }
                return Task.CompletedTask;
            });

            return fileResult;
        }
    }
}
