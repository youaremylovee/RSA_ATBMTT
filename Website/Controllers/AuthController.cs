using Microsoft.AspNetCore.Mvc;
using RSA_UI.Filter;
using RSA_UI.Models.Entity;
using RSA_UI.Models.Response;
using RSA_UI.Services;

namespace RSA_UI.Controllers
{
    [RequiredLogin]
    public class AuthController : Controller
    {
        private readonly CompanyService _companyService;
        public AuthController(IService<Company> companyService) 
            => _companyService = companyService as CompanyService ?? throw new InvalidCastException("The service could not be cast to CompanyService.");
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Login));
        }
        public IActionResult Login()
        {
            return View(nameof(Login));
        }
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            Company? company = _companyService.Login(email, password);
            if (company != null) 
            {
                HttpContext.Session.SetString("uuidCompany", company.Id);
                return RedirectToAction(nameof(HomeController.Index), controllerName: nameof(HomeController).Replace("Controller", ""));
            }
            Response<object> response = new Response<object>(null, 400, "Thông tin tài khoản hoặc mật khẩu không chính xác !");
            return View(nameof(Login), response);
        }
        public IActionResult Register()
        {
            return View(nameof(Register));
        }
        [HttpPost]
        public IActionResult Register(Company company, string RePassword, IFormFile CompanyLogoFile)
        {
            Response<object> response = new Response<object>(null, 200, "Đăng ký thành công !");
            if(company.Password != RePassword)
            {
                response.StatusCode = 400;
                response.Message = "Mật khẩu không khớp !";
            }
            else
            {
                RegisterStatus status = _companyService.Register(company, CompanyLogoFile);
                switch (status)
                {
                    case RegisterStatus.EmailExist:
                        response.StatusCode = 400;
                        response.Message = "Email đã tồn tại !";
                        break;
                    case RegisterStatus.Error:
                        response.StatusCode = 400;
                        response.Message = "Đã có lỗi xảy ra !";
                        break;
                }
            }
            return View(nameof(Register), response);
        }
    }
}
