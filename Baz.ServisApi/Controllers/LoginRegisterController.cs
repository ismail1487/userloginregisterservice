using Baz.Attributes;
using Baz.Model.Entity;
using Baz.ProcessResult;
using Baz.UserLoginServiceApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Baz.Service;
using Baz.Model.Entity.ViewModel;
using Baz.SharedSession;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace Baz.UserLoginServiceApi.Controllers
{
    /// <summary>
    /// Login Register kontrol methodlarının bulunduğu Classdır.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [Route("api/[controller]")]
    [ApiController]
    public class LoginRegisterController : ControllerBase
    {
        private readonly IKisiTemelBilgilerService _kisiTemelBilgilerService;
        private readonly ISistemLoginTarihceService _sistemLoginTarihceService;
        private readonly ISistemLoginSonDurumService _sistemLoginSonDurumService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISharedSession _sharedSession;

        /// <summary>
        /// Login Register kontrol methodlarının bulunduğu Const.
        /// </summary>
        /// <param name="kisiTemelBilgilerService"></param>
        /// <param name="sistemLoginTarihceService"></param>
        /// <param name="sistemLoginSonDurumService"></param>
        /// <param name="serviceProvider"></param>
        public LoginRegisterController(IKisiTemelBilgilerService kisiTemelBilgilerService, ISistemLoginTarihceService sistemLoginTarihceService, ISistemLoginSonDurumService sistemLoginSonDurumService, IServiceProvider serviceProvider, ISharedSession sharedSession)
        {
            _kisiTemelBilgilerService = kisiTemelBilgilerService;
            _sistemLoginTarihceService = sistemLoginTarihceService;
            _sistemLoginSonDurumService = sistemLoginSonDurumService;
            _serviceProvider = serviceProvider;
            _sharedSession = sharedSession;
        }

        /// <summary>
        /// Login işleminin yapılacağı controller methodu.
        /// </summary>
        /// <param name="model">Login işlemi için gerekli model</param>
        /// <returns>Guid SessionID</returns>
        [HttpPost]
        [ProcessName(Name = "Login işleminin gerçekleşmesi")]
        [Route("Login")]
        [AllowAnonymous]
        public Result<string> Login(Baz.Model.Entity.ViewModel.LoginModel model)
        {
            return _kisiTemelBilgilerService.Login(model);
        }

        /// <summary>
        ///  Kişi kullanıcı adı ve mail adresini veritabanında kontrol eden method.
        /// </summary>
        /// <param name="mailOrUsername"> kontrol edilecek mail adresi/ kullanıcı adı</param>
        /// <returns>kontrol edilen değer yoksa true, varsa false döner.</returns>
        [Route("KullaniciAdiKontrolu")]
        [ProcessName(Name = "Kayıt esnasında girilen kullanıcı adı sistemde bulunuyor mu kontrol işlemi")]
        [HttpPost]
        [AllowAnonymous]
        public Result<bool> KullaniciAdiKontrolu([FromBody] string mailOrUsername)
        {
            var result = _kisiTemelBilgilerService.KullaniciAdiKontrolu(mailOrUsername);
            return result;
        }

        /// <summary>
        /// Son giriş tarihini getiren metod
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProcessName(Name = "SonGirisTarihiGetir")]
        [Route("SonGirisTarihiGetir/{id}")]
        [HttpGet]
        public Result<string> SonGirisTarihiGetir(int id)
        {
            var date = _sistemLoginTarihceService.SonKaydiGetir(id).Value.LoginZamani;
            var str = date.ToString("dd.MM.yyyy HH:mm");

            return str.ToResult();
        }

        /// <summary>
        /// Unit testlerde yaratılan kayıtları silen metod
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        [ProcessName(Name = "Unit testlerde yaratılan kayıtların silinmesi")]
        [Route("DeleteUnitTestDumpDatas")]
        [HttpPost]
        public Result<bool> DeleteUnitTestDumpDatas(Dictionary<string, string> dict)
        {
            var kurumService = _serviceProvider.GetService<IKurumTemelBilgilerService>();
            var kisiTemelService = _serviceProvider.GetService<IKisiTemelBilgilerService>();
            var kisiHassasService = _serviceProvider.GetService<IKisiHassasBilgilerService>();
            var kurumOrganizasyonService = _serviceProvider.GetService<IKurumOrganizasyonBirimTanimlariService>();
            var sistemloginsifreyenilemehareketler = _serviceProvider.GetService<ISistemLoginSifreYenilemeAktivasyonHareketleriService>();

            var kurumId = Convert.ToInt32(dict.FirstOrDefault(f => f.Key == "kurumId").Value);
            var kisiId = Convert.ToInt32(dict.FirstOrDefault(f => f.Key == "kisiId").Value);
            var kurumOrganizasyonId = Convert.ToInt32(dict.FirstOrDefault(f => f.Key == "kurumOrganizasyonId").Value);
            var aktivasyonVeriId = Convert.ToInt32(dict.FirstOrDefault(f => f.Key == "aktivasyonVeriId").Value);

            var deleteKontrol = kurumService.Delete(kurumId);
            var deleteKontrol2 = kisiTemelService.Delete(kisiId);
            kurumOrganizasyonService.Delete(kurumOrganizasyonId);
            sistemloginsifreyenilemehareketler.Delete(aktivasyonVeriId);
          
            var kisiHassas = kisiHassasService.List(k => k.KisiTemelBilgiId == kisiId);
            foreach (var item in kisiHassas.Value)
            {
                kisiHassasService.Delete(item.TabloID);
            }
            if (deleteKontrol.Value != null && deleteKontrol2.Value != null)
            {
                return true.ToResult();
            }
            return false.ToResult();          
        }

        //Login son durum ve login cari durum kayıtları sitemin diğer yapılarını etkilemediği ve loglama tarzı bir işlevde oldukları için silme işlemi kapatılmıştır.

        //[ProcessName(Name = "Unit testlerde yaratılan kayıtların silinmesi (2)")]
        //[Route("DeleteUnitTestDumpDatas2")]
        //[HttpPost]
        //public Result<bool> DeleteUnitTestDumpDatas2(Dictionary<string, string> dict)
        //
        //    var sistemloginsondurumservice = _serviceProvider.GetService<ISistemLoginSonDurumService>()
        //    var sistemlogincaridurumservice = _serviceProvider.GetService<ISistemLoginCariDurumService>()
        //    var loginsondurumId = Convert.ToInt32(dict.FirstOrDefault(f => f.Key == "loginSondurumId").Value)
        //    var logincaridurumId = Convert.ToInt32(dict.FirstOrDefault(f => f.Key == "loginCaridurumId").Value)

        //    var deleteKontrol = sistemloginsondurumservice.Delete(loginsondurumId)
        //    var deleteKontrol2 = sistemlogincaridurumservice.Delete(logincaridurumId)
        //    if (deleteKontrol.Value != null && deleteKontrol2 != null)
        //    {
        //        return true.ToResult()
        //    }
        //    return false.ToResult()
        //}

        /// <summary>
        /// Başarısız login sıfırlama metodu
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProcessName(Name = "BasarisizLoginSifirla")]
        [Route("BasarisizLoginSifirla/{id}")]
        [HttpGet]
        [AllowAnonymous]
        public Result<SistemLoginSonDurum> BasarisizLoginSifirla(int id)
        {
            var result = _sistemLoginSonDurumService.BasarisizLoginSifirla(id);
            return result;
        }

        /// <summary>
        /// Kurum Org. birim tipleri listesi
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [ProcessName(Name = "ListTip")]
        [Route("ListTip")]
        [HttpPost]
        public Result<List<KurumOrganizasyonBirimView>> ListTip(KurumOrganizasyonBirimRequest request)
        {
            var kurumOrg = _serviceProvider.GetService<IKurumOrganizasyonBirimTanimlariService>();
            var result = kurumOrg.ListTip(request);
            return result;
        }

        /// <summary>
        /// Kişi kurum admin mi kontrolü sağlayan metot.
        /// </summary>
        /// <param name="kisiId">kişi Id</param>
        /// <returns>sonucu true veya false olarak döndürür.</returns>
        [ProcessName(Name = "GiristeKurumAdminKontrol")]
        [Route("GiristeKurumAdminKontrol/{kisiId}")]
        [HttpGet]
        [AllowAnonymous]
        public Result<bool> GiristeKurumAdminKontrol(int kisiId)
        {
            var kurumlarKisilerService = _serviceProvider.GetService<IKurumlarKisilerService>();
            var kontrol = kurumlarKisilerService.KisiKurumAdminMi(kisiId);
            return kontrol;
        }

        /// <summary>
        /// Kişinin session bilgilerini günceller
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        [Route("KimlikGuncelle/{sessionID}")]
        [HttpGet]
        public Result<bool> KimlikGuncelle(string sessionID)
        {
            var result = _kisiTemelBilgilerService.KimlikGuncelle(sessionID);
            return result;
        }
    }
}