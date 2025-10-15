using Baz.Model.Entity;
using Baz.Model.Entity.ViewModel;
using Baz.Model.Pattern;
using Baz.ProcessResult;
using Baz.RequestManager.Abstracts;
using LoginRegister.UnitTest.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace UserLoginServis.UnitTest
{
    /// <summary>
    /// Login unit testlerinin yapıldığı class
    /// </summary>
    [TestClass]
    public class LoginTest
    {
        private readonly IRequestHelper _helper;

        /// <summary>
        /// Login unit testlerinin yapıldığı classın yapıcı metodu
        /// </summary>
        public LoginTest()
        {
            _helper = TestServerRequestHelper.CreateHelper();
        }

        /// <summary>
        /// Login testlerini yapan metod
        /// </summary>
        [TestMethod()]
        public void LoginTest1()
        {
            // Assert-1 Başarılı login işlemi
            var login = new LoginModel
            {
                EmailOrUserName = "b@mail.com",
                Password = "12345Aa!",
                IpAdress = "",
                UserAgent = ""
            };

            var loginSuccess = _helper.Post<Result<string>>($"/api/LoginRegister/Login", login);
            Assert.AreEqual(loginSuccess.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(loginSuccess.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(loginSuccess.Result.Value);

            // Assert-2 Kullanıcı adı doğru parola yanlış login işlemi
            var login1 = new LoginModel
            {
                EmailOrUserName = "b@mail.com",
                Password = "digi321Aa!!!",
                IpAdress = "",
                UserAgent = ""
            };

            var loginFail = _helper.Post<Result<string>>($"/api/LoginRegister/Login", login1);
            Assert.IsNull(loginFail.Result.Value);

            // Assert-3 Kullanıcı adı ve parola hatalı login işlemi
            var login2 = new LoginModel
            {
                EmailOrUserName = "test@test.com",
                Password = "digi321Aa!!!",
                IpAdress = "",
                UserAgent = ""
            };
            var loginFail2 = _helper.Post<Result<string>>($"/api/LoginRegister/Login", login2);
            Assert.IsNull(loginFail2.Result.Value);

            // Assert-4 SonGirisTarihiGetir
            var sonGirisTarihiGetir = _helper.Get<Result<string>>("/api/LoginRegister/SonGirisTarihiGetir/" + 129);
            Assert.AreEqual(sonGirisTarihiGetir.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(sonGirisTarihiGetir.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(sonGirisTarihiGetir.Result.Value);

            // Assert-5 BasarisizLoginSifirla
            var basarisizLoginSifirla = _helper.Get<Result<SistemLoginSonDurum>>("/api/LoginRegister/BasarisizLoginSifirla/" + 129);
            Assert.AreEqual(basarisizLoginSifirla.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(basarisizLoginSifirla.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(basarisizLoginSifirla.Result.Value);


            // Assert-4 SonGirisTarihiGetir
            var giristeKurumAdminKontrol = _helper.Get<Result<bool>>("/api/LoginRegister/GiristeKurumAdminKontrol/" + 129);
            Assert.AreEqual(giristeKurumAdminKontrol.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(giristeKurumAdminKontrol.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsTrue(giristeKurumAdminKontrol.Result.Value);

            // Assert-4 KimlikGuncelle
            var kimlikGuncelle = _helper.Get<Result<bool>>("/api/LoginRegister/KimlikGuncelle/" + loginSuccess.Result.Value);
            Assert.AreEqual(kimlikGuncelle.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(kimlikGuncelle.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsTrue(kimlikGuncelle.Result.Value);
        }
    }
}