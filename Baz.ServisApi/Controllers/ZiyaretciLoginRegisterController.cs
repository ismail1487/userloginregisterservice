using Baz.Attributes;
using Baz.Model.Entity.ViewModel;
using Baz.ProcessResult;
using Baz.Service;
using Baz.SharedSession;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Baz.UserLoginServiceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZiyaretciLoginRegisterController : Controller
    {
        private readonly IKurumTemelBilgilerService _kurumTemelBilgilerService;
        private readonly IKisiTemelBilgilerService _kisiTemelBilgilerService;

        public ZiyaretciLoginRegisterController(IKurumTemelBilgilerService kurumTemelBilgilerService,IKisiTemelBilgilerService kisiTemelBilgilerService)
        {
            _kurumTemelBilgilerService = kurumTemelBilgilerService;
            _kisiTemelBilgilerService = kisiTemelBilgilerService;
        }
        [HttpPost]
        [ProcessName(Name = "Ziyaretci Register işleminin gerçekleşmesi")]
        [Route("ZiyaretciRegister")]
        [AllowAnonymous]
        public Result<BasicKisiModel> ZiyaretciRegister(BasicKisiModel model)
        {
            return _kurumTemelBilgilerService.ZiyaretciRegister(model);
        }
        [HttpPost]
        [ProcessName(Name = "Ziyaretci Register işleminin gerçekleşmesi")]
        [Route("ZiyaretciLogin")]
        [AllowAnonymous]
        public Result<string> ZiyaretciLogin(LoginModel model)
        {
            return _kisiTemelBilgilerService.ZiyaretciLogin(model);
        }
    }
}
