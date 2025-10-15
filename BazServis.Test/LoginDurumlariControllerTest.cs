using Baz.Model.Entity;
using Baz.ProcessResult;
using Baz.RequestManager.Abstracts;
using LoginRegister.UnitTest.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;

namespace UserLoginServis.UnitTest
{
    /// <summary>
    /// Login Durumlarının kontrol test methodlarının yer aldığı sınıftır.
    /// </summary>
    [TestClass()]
    public class LoginDurumlariControllerTest
    {
        private readonly IRequestHelper _helper;
        private readonly IRequestHelper _noHeader;

        /// <summary>
        /// Login Durumlarının kontrol test methodlarının yer aldığı sınıfın yapıcı metodu
        /// </summary>
        public LoginDurumlariControllerTest()
        {
            _noHeader = TestServerRequestHelperNoHeader.CreateHelper();
            _helper = TestServerRequestHelper.CreateHelper();
        }

        /// <summary>
        /// LoginDurumlariController metotlarının unit testlerini içeren test metotudur.
        /// </summary>
        [TestMethod()]
        public void LoginKaydiOlusturCRUDTest()
        {
            //Assert-1 login kaydı oluşturma
            var loginKaydiOlustur = _noHeader.Post<Result<SistemLoginCariDurum>>($"/api/LoginDurumlari/LoginKaydiOlustur", new
            {
                AktifMi = 1,
                SilindiMi = 0,
                KayitTarihi = DateTime.Now,
                KayitEdenID = 0,
                GuncellenmeTarihi = DateTime.Now,
                GuncelleyenKisiID = 0,
                AktiflikTarihi = DateTime.Now,
                AktifEdenKisiID = 0,
                PasifEdenKisiID = 0,
                PasiflikTarihi = DateTime.Now,
                SilenKisiID = 0,
                SilinmeTarihi = DateTime.Now,
                LoginOlanKisiId = 74,
                LoginZamani = DateTime.Now,
                LoginSistemToken = "a40c9de0-f83d-40b4-834e-b5ac523cf85d",
                LoginDisSistemTokenTipId = 0,
                LoginDisSistemToken = "string"
            });
            Assert.AreEqual(loginKaydiOlustur.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(loginKaydiOlustur.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(loginKaydiOlustur.Result.Value);

            ////Assert-1.1 login kaydı oluşturma olumsuz
            //var loginKaydiOlusturOlumsuz = _helper.Post<Result<SistemLoginCariDurum>>($"/api/LoginDurumlari/LoginKaydiOlustur", new
            //{
            //    LoginSistemToken = "a40c9de0-f83d-40b4-834e-b5ac523cf85d",
            //    LoginZamani = DateTime.Now,
            //    GuncellenmeTarihi = DateTime.Now,
            //});
            //Assert.AreEqual(loginKaydiOlusturOlumsuz.StatusCode, HttpStatusCode.OK);
            //Assert.IsNull(loginKaydiOlusturOlumsuz.Result.Value);

            //Assert-2 logout kaydı atma
            var logoutKayitAt = _noHeader.Post<Result<SistemLoginSonDurum>>($"/api/LoginDurumlari/LogoutKayitAt/", "a40c9de0-f83d-40b4-834e-b5ac523cf85d");
            Assert.AreEqual(logoutKayitAt.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(logoutKayitAt.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(logoutKayitAt.Result.Value);

            //Assert-2.1 logout kaydı atma olumsuz
            var guid = Guid.NewGuid().ToString();
            var logoutKayitAtOlumsuz = _noHeader.Post<Result<SistemLoginSonDurum>>($"/api/LoginDurumlari/LogoutKayitAt/", guid);
            Assert.AreEqual(logoutKayitAtOlumsuz.StatusCode, HttpStatusCode.OK);
            Assert.IsNull(logoutKayitAtOlumsuz.Result.Value);

            //Assert-3 başarısız login sayısı getirme
            var basarisizloginsayisigetir = _noHeader.Get<Result<SistemLoginSonDurum>>($"/api/LoginDurumlari/BasarisizLoginSayisiGetir/{loginKaydiOlustur.Result.Value.LoginOlanKisiId}");
            Assert.AreEqual(basarisizloginsayisigetir.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(basarisizloginsayisigetir.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(basarisizloginsayisigetir.Result.Value);

            //Assert-3.1 başarısız login sayısı getirme olumsuz
            var basarisizloginsayisigetirOlumsuz = _helper.Get<Result<SistemLoginSonDurum>>($"/api/LoginDurumlari/BasarisizLoginSayisiGetir/{0}");
            Assert.IsNull(basarisizloginsayisigetirOlumsuz.Result.Value);

            //Assert-4 Başarısız login sayısı güncelleme
            var basarisizloginsayisiguncelle = _noHeader.Post<Result<SistemLoginSonDurum>>($"/api/LoginDurumlari/BasarisizLoginSayisiGuncelle", new SistemLoginSonDurum
            {
                LoginOlanKisiId = loginKaydiOlustur.Result.Value.LoginOlanKisiId,
                SonLoginDenemeZamani = DateTime.Now.AddHours(-3),
                SonLoginSistemToken = "60daa083-6502-41fb-9187-e23b52bdecb4",
                KacinciBasarisizLogin = 7,
                SonLogoutZamani = DateTime.Now
            });
            Assert.AreEqual(basarisizloginsayisiguncelle.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(basarisizloginsayisiguncelle.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(basarisizloginsayisiguncelle.Result.Value);

            //Assert-4.1 Başarısız login sayısı güncelleme olumsuz
            var basarisizloginsayisiguncelleOlumsuz = _helper.Post<Result<SistemLoginSonDurum>>($"/api/LoginDurumlari/BasarisizLoginSayisiGuncelle", new SistemLoginSonDurum
            {
                LoginOlanKisiId = 0,
                SonLoginDenemeZamani = DateTime.Now.AddHours(-3),
                SonLoginSistemToken = "a40c9de0-f83d-40b4-834e-b5ac523cf85d",
                KacinciBasarisizLogin = 7,
                SonLogoutZamani = DateTime.Now
            });
            Assert.IsNull(basarisizloginsayisiguncelleOlumsuz.Result.Value);
        }
    }
}