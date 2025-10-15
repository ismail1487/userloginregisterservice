using Baz.Model.Entity;
using Baz.Model.Entity.ViewModel;
using Baz.ProcessResult;
using Baz.RequestManager.Abstracts;
using LoginRegister.UnitTest.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using Baz.RequestManager;

namespace UserLoginServis.UnitTest
{
    /// <summary>
    /// Kurum kişi ve organizasyon kayıtlarının testlerinin yapıldığı sınıftır.
    /// </summary>
    [TestClass()]
    public class KurumKisiRegisterTests
    {
        private readonly IRequestHelper _helper;
        private readonly IRequestHelper _noHeader;

        /// <summary>
        /// Kurum kişi ve organizasyon kayıtlarının testlerinin yapıldığı sınıfın yapıcı metodu
        /// </summary>
        public KurumKisiRegisterTests()
        {
            _noHeader = TestServerRequestHelperNoHeader.CreateHelper();
            _helper = TestServerRequestHelper.CreateHelper();
           
        }

        /// <summary>
        /// Register esnasında gerçekleşen kurum, kişi, kurum organizasyon birimi kaydetme işlemleri ile birlikte oluşturulan hesabın aktivasyon işlemlerine dair CRUD metotlarını kapsayan unit test metodu.
        /// </summary>
        [TestMethod()]
        public void KurumKisiKaydetCRUDTest()
        {
            var posta = "Test" + Guid.NewGuid().ToString().Substring(0, 5) + "@mail.com";
            var posta1 = "Test1" + Guid.NewGuid().ToString().Substring(0, 5) + "@mail.com";
            var basicKisiModel = new BasicKisiModel()
            {
                
                KisiSifre = "12345Aa!",
                UyelikSartiKabulEttiMi = true,
                KisiAdi = posta,
                KisiSoyadi = "TestLastName",
                MeslekiUnvan = "TestUnvan",
                KisiEposta = posta,
                KisiKullaniciAdi = posta,
                KisiTelefon = "05555555555",
                KisiMail = posta,
                KurumsalMi = false,
            };
            var basicKurumModel = new BasicKurumModel()
            {
                KurumKisaUnvan = "TestCompany" + Guid.NewGuid().ToString().Substring(0, 10),
                KurumTicariUnvani = "TestUnvan" + Guid.NewGuid().ToString().Substring(0, 10),
                KurumTipiId = 1,
                UlkeID = 1,
                KurumVergiNo = "123456TestKurumVergi" + Guid.NewGuid().ToString().Substring(0, 10),
                //LisansKisiSayisi = 100,
                //LisansId = 5,
                //LisansZamanId = 1
            };
            var postModel = new KurumKisiPostModel { KisiModel = basicKisiModel, KurumModel = basicKurumModel };

            //Assert-1 Kullanıcı adı kontrolü
            var kullaniciAdikontrolü = _helper.Post<Result<bool>>($"/api/LoginRegister/KullaniciAdiKontrolu", postModel.KisiModel.KisiEposta);
            Assert.AreEqual(kullaniciAdikontrolü.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(kullaniciAdikontrolü.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(kullaniciAdikontrolü.Result.Value);

            //Assert-1.1 Kullanıcı adı kontrolü Olumsuz
            var kullaniciAdikontroluNegative = _helper.Post<Result<bool>>($"/api/LoginRegister/KullaniciAdiKontrolu", "dsd");
            Assert.IsFalse(kullaniciAdikontroluNegative.Result.Value);

            // Assert-2 Ekleme
            var kurumKisiKaydet = _noHeader.Post<Result<KurumKisiPostModel>>($"/api/KurumKisiKayit/KurumKisiKaydet", postModel);
            Assert.AreEqual(kurumKisiKaydet.StatusCode, HttpStatusCode.OK);
            Assert.IsNotNull(kurumKisiKaydet.Result.Value);

            var basicKisiModel1 = new BasicKisiModel()
            {

                KisiSifre = "12345Aa!",
                UyelikSartiKabulEttiMi = true,
                KisiAdi = posta1,
                KisiSoyadi = "TestLastName",
                MeslekiUnvan = "TestUnvan",
                KisiEposta = posta1,
                KisiKullaniciAdi = posta1,
                KisiTelefon = "05555555555",
                KisiMail = posta1,
                KurumsalMi = true,
            };
            var basicKurumModel1 = new BasicKurumModel()
            {
                KurumKisaUnvan = "TestCompany11" + Guid.NewGuid().ToString().Substring(0, 10),
                KurumTicariUnvani = "TestUnvan2" + Guid.NewGuid().ToString().Substring(0, 10),
                KurumTipiId = 1,
                UlkeID = 1,
                KurumVergiNo = "12342TestKurumVergi" + Guid.NewGuid().ToString().Substring(0, 10),
                //LisansKisiSayisi = 10,
                //LisansId = 6,
                //LisansZamanId = 1
            };
            var postModel1 = new KurumKisiPostModel { KisiModel = basicKisiModel1, KurumModel = basicKurumModel1 };


            // Assert-2 Ekleme
            var kurumKisiKaydet1 = _noHeader.Post<Result<KurumKisiPostModel>>($"/api/KurumKisiKayit/KurumKisiKaydet", postModel1);
            Assert.AreEqual(kurumKisiKaydet1.StatusCode, HttpStatusCode.OK);
            Assert.IsNotNull(kurumKisiKaydet1.Result.Value);

            var posta2 = "Test" + Guid.NewGuid().ToString().Substring(0, 5) + "@mail.com";
            var basicKisiModel2 = new BasicKisiModel()
            {
               
                KisiSifre = "12345Aa!",
                UyelikSartiKabulEttiMi = true,
                KisiAdi = posta2,
                KisiSoyadi = "TestLastName",
                MeslekiUnvan = "TestUnvan",
                KisiEposta = posta2,
                KisiKullaniciAdi = posta2,
                KisiTelefon = "05555555555",
                KisiMail = posta2,
                KurumsalMi = false,
            };
            var basicKurumModel2 = new BasicKurumModel()
            {
                KurumKisaUnvan = "TestCompany" + Guid.NewGuid().ToString().Substring(0, 10),
                KurumTicariUnvani = "TestUnvan" + Guid.NewGuid().ToString().Substring(0, 10),
                KurumTipiId = 1,
                UlkeID = 1,
                KurumVergiNo = "123456TestKurumVergi" + Guid.NewGuid().ToString().Substring(0, 10),
                //LisansKisiSayisi = 100,
                //LisansId = 1,
                //LisansZamanId = 1
            };
            var postModel3 = new KurumKisiPostModel { KisiModel = basicKisiModel2, KurumModel = basicKurumModel2 };
            var kurumKisiKaydet2 = _noHeader.Post<Result<KurumKisiPostModel>>($"/api/KurumKisiKayit/KurumKisiKaydet", postModel3);
            Assert.IsNull(kurumKisiKaydet2.Result.Value);

            //Assert-2.1 kaydet Olumsuz
            var postModel2 = new KurumKisiPostModel { KisiModel = basicKisiModel };

            var kurumKisiKaydetOlumsuz = _noHeader.Post<Result<KurumKisiPostModel>>($"/api/KurumKisiKayit/KurumKisiKaydet", postModel2);
            Assert.IsNull(kurumKisiKaydetOlumsuz.Result.Value);

            //Assert-2.2 kaydet Olumsuz Aynı vergi No
            var kurumKisiKaydetOlumsuzAynıVergi = _noHeader.Post<Result<KurumKisiPostModel>>($"/api/KurumKisiKayit/KurumKisiKaydet", postModel);
            Assert.IsNull(kurumKisiKaydetOlumsuzAynıVergi.Result.Value);

            //Assert-3 Listeleme/ İsme göre kurum getirme
            var kr = _helper.Get<Result<KurumTemelBilgiler>>("/api/KurumKisiKayit/IsmeGoreGetir/" + kurumKisiKaydet.Result.Value.KurumModel.KurumTicariUnvani);
            Assert.AreEqual(kr.StatusCode, HttpStatusCode.OK);
            Assert.IsNotNull(kr.Result.Value);

            //Assert-4 Aktivasyon maili oluşturma
            var aktivasyonMaili = _noHeader.Post<Result<SistemLoginSifreYenilemeAktivasyonHareketleri>>("/api/KurumKisiKayit/HesapAktivasyonMailiOlustur/", kurumKisiKaydet.Result.Value.KisiModel.TabloID);
            Assert.AreEqual(aktivasyonMaili.StatusCode, HttpStatusCode.OK);
            Assert.IsNotNull(aktivasyonMaili.Result.Value);

             //Assert-4 Aktivasyon maili oluşturma
            var aktivasyonMaili1 = _noHeader.Post<Result<SistemLoginSifreYenilemeAktivasyonHareketleri>>("/api/KurumKisiKayit/HesapAktivasyonMailiOlustur/", kurumKisiKaydet1.Result.Value.KisiModel.TabloID);
            Assert.AreEqual(aktivasyonMaili1.StatusCode, HttpStatusCode.OK);
            Assert.IsNotNull(aktivasyonMaili1.Result.Value);

            //Assert-4.1 Aktivasyon maili oluşturma olumsuz
            var aktivasyonMaili2 = _helper.Post<Result<SistemLoginSifreYenilemeAktivasyonHareketleri>>("/api/KurumKisiKayit/HesapAktivasyonMailiOlustur/", 0);
            Assert.IsNull(aktivasyonMaili2.Result.Value);

            //Assert-5 Hesap Aktivasyonu
            var url = aktivasyonMaili.Result.Value.HesapAktivasyonSayfasiGeciciUrl;
            var hesapAktivasyon = _noHeader.Post<Result<KisiTemelBilgiler>>("/api/KurumKisiKayit/HesapAktivasyonIslemi", url[^36..]);
            Assert.AreEqual(hesapAktivasyon.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(hesapAktivasyon.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(hesapAktivasyon.Result.Value);
            
            
            //Assert-5 Hesap Aktivasyonu
            var url1 = aktivasyonMaili1.Result.Value.HesapAktivasyonSayfasiGeciciUrl;
            var hesapAktivasyon1 = _noHeader.Post<Result<KisiTemelBilgiler>>("/api/KurumKisiKayit/HesapAktivasyonIslemi", url[^36..]);
            Assert.AreEqual(hesapAktivasyon1.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(hesapAktivasyon1.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(hesapAktivasyon1.Result.Value);

            //Assert-5.1 Hesap Aktivasyonu olumsuz
            var guid = Guid.NewGuid().ToString();
            var hesapAktivasyon2 = _noHeader.Post<Result<KisiTemelBilgiler>>("/api/KurumKisiKayit/HesapAktivasyonIslemi/", guid);
            Assert.IsNull(hesapAktivasyon2.Result.Value);

            //Assert-6 Hesap aktivasyon maili geçerli mi
            var hesapAktivasyonGecerliMi = _noHeader.Post<Result<bool>>("/api/KurumKisiKayit/HesapAktivasyonLinkiGecerliMi", url[^36..]);
            Assert.AreEqual(hesapAktivasyonGecerliMi.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(hesapAktivasyonGecerliMi.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(hesapAktivasyonGecerliMi.Result.Value);
              
            
            //Assert-6 Hesap aktivasyon maili geçerli mi
            var hesapAktivasyonGecerliMi1 = _noHeader.Post<Result<bool>>("/api/KurumKisiKayit/HesapAktivasyonLinkiGecerliMi", url1[^36..]);
            Assert.AreEqual(hesapAktivasyonGecerliMi1.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(hesapAktivasyonGecerliMi1.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(hesapAktivasyonGecerliMi1.Result.Value);

            //Assert-6.1 Hesap aktivasyon maili geçerli mi olumsuz
            var hesapAktivasyonGecerliMi2 = _helper.Post<Result<bool>>("/api/KurumKisiKayit/HesapAktivasyonLinkiGecerliMi", guid);
            Assert.IsNotNull(hesapAktivasyonGecerliMi2.Result.Value);
            Assert.IsFalse(hesapAktivasyonGecerliMi2.Result.Value);

            //Assert-7 Kurum Organizasyon birimi oluşturma
            var kurumOrganizsyonBirimTanim = new KurumOrganizasyonBirimTanimlari
            {
                IlgiliKurumId = kurumKisiKaydet.Result.Value.KurumModel.TabloID,
                UstId = 1,
                BirimTanim = "Yeni11",
                OrganizasyonBirimTipiId = 1,
                BirimKisaTanim = "Yenii",
                GuncellenmeTarihi = DateTime.Now,
                KayitTarihi = DateTime.Now
            };

            var kurumOrganizasyon = _noHeader.Post<Result<KurumOrganizasyonBirimTanimlari>>("/api/KurumKisiKayit/KurumOrganizasyonKaydet", kurumOrganizsyonBirimTanim);
            Assert.AreEqual(kurumOrganizasyon.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(kurumOrganizasyon.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(kurumOrganizasyon.Result.Value);

             //Assert-7 Kurum Organizasyon birimi oluşturma
            var kurumOrganizsyonBirimTanim1 = new KurumOrganizasyonBirimTanimlari
            {
                IlgiliKurumId = kurumKisiKaydet1.Result.Value.KurumModel.TabloID,
                UstId = 1,
                BirimTanim = "Yeni11",
                OrganizasyonBirimTipiId = 1,
                BirimKisaTanim = "Yenii",
                GuncellenmeTarihi = DateTime.Now,
                KayitTarihi = DateTime.Now
            };

            var kurumOrganizasyon1 = _noHeader.Post<Result<KurumOrganizasyonBirimTanimlari>>("/api/KurumKisiKayit/KurumOrganizasyonKaydet", kurumOrganizsyonBirimTanim1);
            Assert.AreEqual(kurumOrganizasyon1.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(kurumOrganizasyon1.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(kurumOrganizasyon1.Result.Value);

            postModel.KurumModel.KurumVergiNo = "";
            //kurumKisiKaydetvergiNosuz
            var kurumKisiKaydetvergiNosuz = _noHeader.Post<Result<KurumKisiPostModel>>($"/api/KurumKisiKayit/KurumKisiKaydet", postModel);
            Assert.IsNull(kurumKisiKaydetvergiNosuz.Result.Value);

          

            //Login - Logut testleri buarada ayrıca yapılmıştır Kişi kayıt olması gerekmektedir.
            //Assert - 1 Başarılı login işlemi
            var login = new LoginModel
            {
                EmailOrUserName = posta,
                Password = "12345Aa!",
                IpAdress = "",
                UserAgent = ""
            };

            var loginSuccess = _noHeader.Post<Result<string>>($"/api/LoginRegister/Login", login);
            Assert.AreEqual(loginSuccess.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(loginSuccess.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(loginSuccess.Result.Value);
             
            //Assert - 1 Başarılı login işlemi
            var login1 = new LoginModel
            {
                EmailOrUserName = posta2,
                Password = "12345Aa!",
                IpAdress = "",
                UserAgent = ""
            };

            //var loginSuccess1 = _noHeader.Post<Result<string>>($"/api/LoginRegister/Login", login1);
            //Assert.AreEqual(loginSuccess.StatusCode, HttpStatusCode.OK);
            //Assert.AreEqual(loginSuccess.Result.StatusCode, (int)ResultStatusCode.Success);
            //Assert.IsNotNull(loginSuccess.Result.Value);

            //LoginCariKaydı
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
                LoginOlanKisiId = kurumKisiKaydet.Result.Value.KisiModel.TabloID,
                LoginZamani = DateTime.Now,
                LoginSistemToken = loginSuccess.Result.Value,
                LoginDisSistemTokenTipId = 0,
                LoginDisSistemToken = "string"
            });
            Assert.AreEqual(loginKaydiOlustur.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(loginKaydiOlustur.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(loginKaydiOlustur.Result.Value);

            //Kişi ilk kez login olduktan sonra ki logout olma durumunda atılan son durum kaydı için yazılan metod
            var logoutKayitAtOlumsuz = _noHeader.Post<Result<SistemLoginSonDurum>>($"/api/LoginDurumlari/LogoutKayitAt", loginSuccess.Result.Value);
            Assert.AreEqual(logoutKayitAtOlumsuz.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(logoutKayitAtOlumsuz.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(logoutKayitAtOlumsuz.Result.Value); 
            

              //Kişi ilk kez login olduktan sonra ki logout olma durumunda atılan son durum kaydı için yazılan metod
            var logoutKayitAtOlumsuz11 = _noHeader.Post<Result<SistemLoginSonDurum>>($"/api/LoginDurumlari/LogoutKayitAt", loginKaydiOlustur.Result.Value.LoginDisSistemToken);
            Assert.IsFalse(logoutKayitAtOlumsuz11.Result.IsSuccess);
            Assert.IsNull(logoutKayitAtOlumsuz11.Result.Value);
           

            //Assert- Test süresinde eklene verileri temizleme
            var dict = new Dictionary<string, string>
            {
                {"kurumId", kurumKisiKaydet.Result.Value.KurumModel.TabloID.ToString()},
                {"kisiId", kurumKisiKaydet.Result.Value.KisiModel.TabloID.ToString()},
                {"kurumOrganizasyonId", kurumOrganizasyon.Result.Value.TabloID.ToString()},
                {"aktivasyonVeriId", aktivasyonMaili.Result.Value.TabloID.ToString()}
            };

            var delete = _noHeader.Post<Result<bool>>("/api/LoginRegister/DeleteUnitTestDumpDatas", dict);
            Assert.AreEqual(delete.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(delete.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(delete.Result.Value);
            
            //Assert- Test süresinde eklene verileri temizleme
            var dict1 = new Dictionary<string, string>
            {
                {"kurumId", kurumKisiKaydet1.Result.Value.KurumModel.TabloID.ToString()},
                {"kisiId", kurumKisiKaydet1.Result.Value.KisiModel.TabloID.ToString()},
                {"kurumOrganizasyonId", kurumOrganizasyon1.Result.Value.TabloID.ToString()},
                {"aktivasyonVeriId", aktivasyonMaili1.Result.Value.TabloID.ToString()}
            };

            var delete1 = _noHeader.Post<Result<bool>>("/api/LoginRegister/DeleteUnitTestDumpDatas", dict1);
            Assert.AreEqual(delete1.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(delete1.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(delete1.Result.Value);
        }

        /// <summary>
        /// Register esnasında gerçekleşen kurum, kişi, kurum organizasyon birimi kaydetme işlemleri ile birlikte oluşturulan hesabın aktivasyon işlemlerine dair CRUD metotlarını kapsayan unit test metodu.
        /// </summary>
        [TestMethod()]
        public void KisiKurumCrud2()
        {
            var posta = "Test" + Guid.NewGuid().ToString().Substring(0, 5) + "@mail.com";
            var vergiNo = "KurumVergiBilmemkaç123" + Guid.NewGuid().ToString().Substring(0, 10);
            var basicKisiModel = new BasicKisiModel()
            {
                
                KisiSifre = "12345Aa!",
                UyelikSartiKabulEttiMi = true,
                KisiAdi = posta,
                KisiSoyadi = "TestLastName",
                MeslekiUnvan = "TestUnvan",
                KisiEposta = posta,
                KisiKullaniciAdi = posta,
                KisiTelefon = "05555555555",
                KisiMail = posta,
                KurumsalMi = true,
            };
            var basicKurumModel = new BasicKurumModel()
            {
                KurumKisaUnvan = "TestCompany" + Guid.NewGuid().ToString().Substring(0, 10),
                KurumTicariUnvani = "TestUnvan" + Guid.NewGuid().ToString().Substring(0, 10),
                KurumTipiId = 1,
                UlkeID = 1,
                //LisansKisiSayisi = 100,
                //LisansId = 5,
                //LisansZamanId = 1,
                KurumVergiNo = vergiNo,
            };
            var postModel = new KurumKisiPostModel { KisiModel = basicKisiModel, KurumModel = basicKurumModel };

            //kurumKisiKaydetvergiNosuz
            var kurumKisiKaydet = _noHeader.Post<Result<KurumKisiPostModel>>("/api/KurumKisiKayit/KurumKisiKaydet", postModel);
            Assert.AreEqual(kurumKisiKaydet.StatusCode, HttpStatusCode.OK);
            Assert.IsNotNull(kurumKisiKaydet.Result.Value);

            //Assert- Test süresinde eklene verileri temizleme
            var dict = new Dictionary<string, string>
            {
                {"kurumId", kurumKisiKaydet.Result.Value.KurumModel.TabloID.ToString()},
                {"kisiId", kurumKisiKaydet.Result.Value.KisiModel.TabloID.ToString()}
            };

            var delete = _noHeader.Post<Result<bool>>("/api/LoginRegister/DeleteUnitTestDumpDatas", dict);
            Assert.AreEqual(delete.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(delete.Result.StatusCode, (int)ResultStatusCode.Success);
            Assert.IsNotNull(delete.Result.Value);
        }
    }
}