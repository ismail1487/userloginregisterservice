using Baz.Attributes;
using Baz.Model.Entity;
using Baz.ProcessResult;
using Microsoft.AspNetCore.Mvc;
using System;
using Baz.Service;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;

namespace Baz.UserLoginServiceApi.Controllers
{
    /// <summary>
    /// Login ve logout işlemlerinde veritabanına ilgili işlemin kaydını atan methodların bulunduğu controller class'ı.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [Route("api/[controller]")]
    [ApiController]
    public class LoginDurumlariController : ControllerBase
    {
        private readonly ISistemLoginCariDurumService _sistemLoginCariDurumService;
        private readonly ISistemLoginSonDurumService _sistemLoginSonDurumService;
        private readonly ISistemLoginTarihceService _sistemLoginTarihceService;

        /// <summary>
        /// Login ve logout işlemlerinde veritabanına ilgili işlemin kaydını atan methodların bulunduğu const.
        /// </summary>
        /// <param name="sistemLoginCariDurumService"></param>
        /// <param name="sistemLoginSonDurumService"></param>
        /// <param name="sistemLoginTarihceService"></param>
        public LoginDurumlariController(ISistemLoginCariDurumService sistemLoginCariDurumService, ISistemLoginSonDurumService sistemLoginSonDurumService, ISistemLoginTarihceService sistemLoginTarihceService)
        {
            _sistemLoginCariDurumService = sistemLoginCariDurumService;
            _sistemLoginSonDurumService = sistemLoginSonDurumService;
            _sistemLoginTarihceService = sistemLoginTarihceService;
        }

        /// <summary>
        /// Login işlemi sonrası ilgili kullanıcıya dair login işleminin ayrıntılarını veritabanına kaydeden method.
        /// </summary>
        /// <param name="model"> kaydedilecek login işlemi detaylarını içeren model. </param>
        /// <returns>kaydedilen modeli döndürür.</returns>
        [HttpPost]
        [ProcessName(Name = "Login kaydının gerçekleşmesi")]
        [Route("LoginKaydiOlustur")]
        [AllowAnonymous]
        public Result<SistemLoginCariDurum> LoginKaydiOlustur(SistemLoginCariDurum model)
        {
            var LoginTarihceModel = new SistemLoginTarihce()
            {
                LoginOlanKisiId = model.LoginOlanKisiId,
                LoginZamani = model.LoginZamani,
                LoginSistemToken = model.LoginSistemToken,
                LoginBasariliMi = true,
                GuncellenmeTarihi = DateTime.Now,
                KayitTarihi = DateTime.Now,
                AktifMi = 1
            };
            //Tarihce ve cari kayıtların atılması
            var cariDurumReturn = _sistemLoginCariDurumService.Add(model);
            var tarihceReturn = _sistemLoginTarihceService.Add(LoginTarihceModel);

            var sonDurumModel = _sistemLoginSonDurumService.LoginOlanKisiIDyeGoreGetir(cariDurumReturn.Value.LoginOlanKisiId);
            // Daha önce login olmuşsa login durumu güncellenir olmamışsa yeni bir durum oluşturulur.
            if (sonDurumModel.Value != null)
            {
                sonDurumModel.Value.SonLoginDenemeZamani = tarihceReturn.Value.LoginZamani;
                sonDurumModel.Value.SonLogoutZamani = tarihceReturn.Value.LogoutZamani;
                sonDurumModel.Value.SonLoginSistemToken = tarihceReturn.Value.LoginSistemToken;
                sonDurumModel.Value.KacinciBasarisizLogin = 0;
                sonDurumModel.Value.KisiID = model.KisiID;
                sonDurumModel.Value.KurumID = model.KurumID;
                var result = _sistemLoginSonDurumService.Update(sonDurumModel.Value);
            }
            else
            {
                var newDurumModel = new SistemLoginSonDurum()
                {
                    KisiID = model.KisiID,
                    KurumID = model.KurumID,
                    LoginOlanKisiId = cariDurumReturn.Value.LoginOlanKisiId,
                    SonLoginDenemeZamani = tarihceReturn.Value.LoginZamani,
                    SonLogoutZamani = tarihceReturn.Value.LogoutZamani,
                    SonLoginSistemToken = tarihceReturn.Value.LoginSistemToken,
                    KacinciBasarisizLogin = 0,
                    GuncellenmeTarihi = DateTime.Now,
                    KayitTarihi = DateTime.Now,
                    AktifMi = 1
                };
                var result = _sistemLoginSonDurumService.Add(newDurumModel);
            }
            return cariDurumReturn;
        }

        /// <summary>
        /// Logout işlemi sonrası ilgili kullanıcıya dair logout işleminin ayrıntılarını veritabanına kaydeden method.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>kaydedilen modeli döndürür.</returns>
        [HttpPost]
        [ProcessName(Name = "Logout işleminin gerçekleşmesi")]
        [Route("LogoutKayitAt")]
        [ExcludeFromCodeCoverage]
        [AllowAnonymous]

        public Result<SistemLoginSonDurum> LogoutKayitAt([FromBody] string token)
        {
            var cariDurumModel = _sistemLoginCariDurumService.TokenIleKaydiSil(token).Value;
            var tarihceModel = _sistemLoginTarihceService.LogoutZamaniAta(token).Value;
            if (cariDurumModel == null || tarihceModel == null)
            {
                return Results.Fail("logout kaydı oluşturma başarısız", ResultStatusCode.UpdateError);
            }
            // Daha önce login olmuşsa login durumu güncellenir olmamışsa yeni bir durum oluşturulur.
            var sonDurumModel = _sistemLoginSonDurumService.LoginOlanKisiIDyeGoreGetir(cariDurumModel.LoginOlanKisiId);
            if (sonDurumModel.Value != null)
            {
                sonDurumModel.Value.SonLoginDenemeZamani = tarihceModel.LoginZamani;
                sonDurumModel.Value.SonLogoutZamani = tarihceModel.LogoutZamani;
                sonDurumModel.Value.SonLoginSistemToken = tarihceModel.LoginSistemToken;
                var result = _sistemLoginSonDurumService.Update(sonDurumModel.Value);
                return result;
            }
            else
            {
                var newDurumModel = new SistemLoginSonDurum()
                {
                    LoginOlanKisiId = cariDurumModel.LoginOlanKisiId,
                    SonLoginDenemeZamani = tarihceModel.LoginZamani,
                    SonLogoutZamani = tarihceModel.LogoutZamani,
                    SonLoginSistemToken = tarihceModel.LoginSistemToken,
                    //KacinciBasarisizLogin = 0,
                    GuncellenmeTarihi = DateTime.Now,
                    KayitTarihi = DateTime.Now,
                    AktifMi = 1
                };
                var result = _sistemLoginSonDurumService.Add(newDurumModel);
                return result;
            }
        }

        /// <summary>
        /// İlgili kullanıcının başarısız login sayısı değerini getiren method.
        /// </summary>
        /// <param name="kisiId"> ilgili kisinin Id değeri. </param>
        /// <returns>istenilen döndürür.</returns>
        [HttpGet]
        [ProcessName(Name = "Kişinin başarısız login sayısının getirilmesi.")]
        [Route("BasarisizLoginSayisiGetir/{kisiId}")]
        [AllowAnonymous]
        public Result<SistemLoginSonDurum> BasarisizLoginSayisiGetir(int kisiId)
        {
            var result = _sistemLoginSonDurumService.LoginOlanKisiIDyeGoreGetir(kisiId);
            return result;
        }

        /// <summary>
        /// İlgili kullanıcının başarısız login sayısı değerini güncelleyen method.
        /// </summary>
        /// <param name="model"> ilgili kisinin berilerini barındıran model.</param>
        /// <returns> güncellenen döndürür.</returns>
        [HttpPost]
        [ProcessName(Name = "Başarısız login işlemi sonrasında ilgili kaydın güncellenmesi")]
        [Route("BasarisizLoginSayisiGuncelle")]
        [AllowAnonymous]
        public Result<SistemLoginSonDurum> BasarisizLoginSayisiGuncelle([FromBody] SistemLoginSonDurum model)
        {
            var kisiDetay = _sistemLoginSonDurumService.LoginOlanKisiIDyeGoreGetir(model.LoginOlanKisiId).Value;
            if (kisiDetay == null)
                return Results.Fail("Başarısız login sayısı güncelleme işlemi başarısız", ResultStatusCode.UpdateError);
            kisiDetay.KacinciBasarisizLogin = model.KacinciBasarisizLogin;
            kisiDetay.GuncellenmeTarihi = DateTime.Now;
            var updateResult = _sistemLoginSonDurumService.Update(kisiDetay);

            return updateResult;
        }
    }
}