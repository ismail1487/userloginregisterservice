using Baz.AletKutusu;
using Baz.AOP.Logger.ExceptionLog;
using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.Model.Entity.Constants;
using Baz.Model.Entity.ViewModel;
using Baz.ProcessResult;
using Baz.Repository.Pattern;
using Baz.RequestManager.Abstracts;

using Baz.SharedSession;
using BazWebApp.Services;
using Decor;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Baz.Service
{
    /// <summary>
    /// Kişi temel bilgilerin Interface'dir.
    /// </summary>
    public interface IKisiTemelBilgilerService : Baz.Service.Base.IService<KisiTemelBilgiler>
    {
        /// <summary>
        /// Sisteme login işleminin yapılabileceği method dur.
        /// Login modeli kullanıcı adı, şifre, ipAdres ve userAgent alır
        /// Kullanıcı adı KisiTemelBilgilerde şifre hashli bir şekilde KullaniciHassasBilgilerde tutulur.
        /// Eğer kullanıcı geçerli ise session serverda Guid bir sessionID ile session olusturulur.
        /// Kullanıcıya bu sessionID dönülür.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns>Guid sessionID</returns>
        Result<string> Login(Baz.Model.Entity.ViewModel.LoginModel login);

        /// <summary>
        /// kişi bilgilerini KisiTemelBilgiler ve KisiHassasBilgiler tablolarına kaydeden method.
        /// </summary>
        /// <param name="model">Kaydedilecek kişi bilgilerini içeren model parametresi.</param>
        /// <returns>kaydedilen kişi bilgilerini içeren modeli JSON formatında döndürür.</returns>
        Result<BasicKisiModel> KisiKaydet(BasicKisiModel model, bool AktifDurum = false);

        /// <summary>
        /// KişiID değeriyle aktivasyon talebi oluşturan method.
        /// </summary>
        /// <param name="KisiID"> ilgili kişinin Id değeri</param>
        /// <returns>sonucu döndürür.</returns>
        Result<SistemLoginSifreYenilemeAktivasyonHareketleri> HesapAktivasyonMailiOlustur(int KisiID);

        /// <summary>
        /// Kisi id ile KisiTemelBilgiler ve KisiHassasBilgiler tablosundaki kayıtları bulan ve AktifMi değerini aktif eden method.
        /// </summary>
        /// <param name="guid">Kişinin TemelBilgiler tablosunda kayıtlı Id değeri</param>
        /// <returns>Aktifleştirme işlemi sonucuna göre kişi verilerini içeren modeli döndürür.</returns>
        Result<KisiTemelBilgiler> KisiAktiveEtme(string guid);

        /// <summary>
        /// İlgili şifre yenileme maili geçerli mi kontrolü sağlayan method.
        /// </summary>
        /// <param name="guid">şifre yenileme için gereke GUID değerini içeren string parametre.</param>
        /// <returns>Geçerliyse true, değilse false döndürür.</returns>
        Result<bool> HesapAktivasyonLinkiGecerliMi(string guid);

        /// <summary>
        ///  Kişi kullanıcı adı ve mail adresinin bulunup bulunmadığını kontrol eden method.
        /// </summary>
        /// <param name="mailOrUsername"> kontrol edilecek mail adresi/ kullanıcı adı</param>
        /// <returns>kontrol edilen değer yoksa true, varsa false döner.</returns>
        Result<bool> KullaniciAdiKontrolu(string mailOrUsername);
        /// <summary>
        ///  Kişi mail adresinin ya da  telefon numarasınının bulunup bulunmadığını kontrol eden method.
        /// </summary>
        /// <param name="mailOrUsername"> kontrol edilecek mail adresi/ kullanıcı adı</param>
        /// <returns>kontrol edilen değer yoksa true, varsa false döner.</returns>
        bool MailveTelefonKontrolu(string mailOrUsername, string TelefonNo);

        /// <summary>
        /// KişiID değeri ile ilgili kişi amirse kendisine bağlı müşteri temsilcilerini geiren metot.
        /// </summary>
        /// <param name="kisiID">ilgili kişiID değeri</param>
        /// <returns>müşteri temsilcisi Idleri listesini döndürür.</returns>
        Result<List<int>> AmireBagliMusteriTemsilcileriList(int kisiID);

        /// <summary>
        /// Kişinin session bilgilerini günceller
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public Result<bool> KimlikGuncelle(string sessionID);
        /// <summary>
        /// Sisteme Web sitesi Üzerinden ziyaretci login işleminin yapılabileceği method dur.
        /// Login modeli kullanıcı adı, şifre, ipAdres ve userAgent alır
        /// Kullanıcı adı KisiTemelBilgilerde şifre hashli bir şekilde KullaniciHassasBilgilerde tutulur.
        /// Eğer kullanıcı geçerli ise session serverda Guid bir sessionID ile session olusturulur.
        /// Kullanıcıya bu sessionID dönülür.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns>Guid sessionID</returns>
        Result<string> ZiyaretciLogin(Baz.Model.Entity.ViewModel.LoginModel login);
    }

    /// <summary>
    /// KişiTemelBilgiler Tablosu ile ilgili servislerin yazılacağı class
    /// </summary>
    public class KisiTemelBilgilerService : Baz.Service.Base.Service<KisiTemelBilgiler>, IKisiTemelBilgilerService
    {
        private readonly ISistemLoginSifreYenilemeAktivasyonHareketleriService _sistemLoginSifreYenilemeAktivasyonHareketleriService;
        private readonly IKisiHassasBilgilerService _kisiHassasBilgilerService;
        private readonly ISharedSession _sharedSession;
        private readonly IKureselParametrelerService _kureselParametrelerService;
        private readonly IRequestHelper _requestHelper;
        private readonly IKurumlarKisilerService _kurumlarKisilerService;
        private readonly IYetkiMerkeziService _yetkiMerkezi;
        private readonly ISistemLoginSonDurumService _loginSonDurumService;
        private readonly IPostaciBekleyenIslemlerGenelService _postaciBekleyenIslemlerGenelService;
        private readonly IPostaciBekleyenIslemlerAyrintilarService _postaciBekleyenIslemlerAyrintilarService;
        /// <summary>
        /// KişiTemelBilgiler Tablosu ile ilgili servislerin yazılacağı class yapıcı metodu
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="kisiHassasBilgilerService"></param>
        /// <param name="sistemLoginSifreYenilemeAktivasyonHareketleriService"></param>
        /// <param name="kureselParametrelerService"></param>
        /// <param name="sharedSession"></param>
        /// <param name="requestHelper"></param>
        /// <param name="dataMapper"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="kurumlarKisilerService"></param>
        public KisiTemelBilgilerService(IRepository<KisiTemelBilgiler> repository, IKisiHassasBilgilerService kisiHassasBilgilerService, ISistemLoginSifreYenilemeAktivasyonHareketleriService sistemLoginSifreYenilemeAktivasyonHareketleriService, IKureselParametrelerService kureselParametrelerService, ISharedSession sharedSession, IRequestHelper requestHelper, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<KisiTemelBilgilerService> logger, IKurumlarKisilerService kurumlarKisilerService, IYetkiMerkeziService yetkiMerkeziService, ISistemLoginSonDurumService loginSonDurumService, IPostaciBekleyenIslemlerGenelService postaciBekleyenIslemlerGenelService, IPostaciBekleyenIslemlerAyrintilarService postaciBekleyenIslemlerAyrintilarService) : base(repository, dataMapper, serviceProvider, logger)
        {
            _kisiHassasBilgilerService = kisiHassasBilgilerService;
            _sharedSession = sharedSession;
            _sistemLoginSifreYenilemeAktivasyonHareketleriService = sistemLoginSifreYenilemeAktivasyonHareketleriService;
            _kureselParametrelerService = kureselParametrelerService;
            _requestHelper = requestHelper;
            _kurumlarKisilerService = kurumlarKisilerService;
            _yetkiMerkezi = yetkiMerkeziService;
            _loginSonDurumService = loginSonDurumService;
            _postaciBekleyenIslemlerGenelService = postaciBekleyenIslemlerGenelService;
            _postaciBekleyenIslemlerAyrintilarService = postaciBekleyenIslemlerAyrintilarService;
        }

        /// <summary>
        /// Sisteme login işleminin yapılabileceği method dur.
        /// Login modeli kullanıcı adı, şifre, ipAdres ve userAgent alır
        /// Kullanıcı adı KisiTemelBilgilerde şifre hashli bir şekilde KullaniciHassasBilgilerde tutulur.
        /// Eğer kullanıcı geçerli ise session serverda Guid bir sessionID ile session olusturulur.
        /// Kullanıcıya bu sessionID dönülür.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns>Guid sessionID</returns>

        public Result<string> Login(Baz.Model.Entity.ViewModel.LoginModel login)
        {
            //Kişinin mailine göre hassas ve temel bilgileri çeken sorgu
            var kisi = (from temelBilgiler in List().Value
                        join hassasBilgiler in _kisiHassasBilgilerService.ListForQuery() on temelBilgiler.TabloID equals hassasBilgiler.KisiTemelBilgiId
                        where temelBilgiler.AktifMi == 1 &&
                        (temelBilgiler.KisiEposta.ToLower() == login.EmailOrUserName.ToLower().Trim() || temelBilgiler.KisiKullaniciAdi.ToLower() == login.EmailOrUserName.ToLower().Trim())
                        select new { hassasBilgiler, temelBilgiler }
                        ).FirstOrDefault();
            if (kisi == null)
            {
                _logger.LogWarning("Geçersiz Kullanıcı Adı: {0}, İPAdress: {1}, UserAgent: {2}", login.EmailOrUserName, login.IpAdress, login.UserAgent);

                return Results.Fail("Geçersiz kullanıcı adı!", ResultStatusCode.ReadError);
            }
            if (kisi.hassasBilgiler.HesabiAktifMi == false)
            {
                return Results.Fail("Hesabınız pasif durumdadır. Yöneticinizle iletişime geçiniz.", ResultStatusCode.ReadError);
            }
            //Kişinin şifre doğrulaması
            var r = HashSalt.VerifyPassword(login.Password, kisi.hassasBilgiler.HashValue, kisi.hassasBilgiler.SaltValue) || (login.Google || login.Facebook) && !string.IsNullOrEmpty(login.ExternalId);

            if (!r)
            {
                _logger.LogWarning("Hatalı Giriş: {0}, İPAdress: {1}, UserAgent: {2}", login.EmailOrUserName, login.IpAdress, login.UserAgent);

                return Results.Fail("Geçersiz kullanıcı adı şifre!", ResultStatusCode.ReadError);
            }
            // kişinin yetkilendirildiği sayfalar
            var yetkiList = this.KisiYetkilerListGetir(kisi.temelBilgiler.TabloID);




            var token = Guid.NewGuid().ToString();
            var _kurumService = _serviceProvider.GetService<IKurumTemelBilgilerService>();
            var kurum = _kurumService.SingleOrDefault(kisi.temelBilgiler.KurumID);

            var kurumId = kisi != null && kisi.temelBilgiler.KisiBagliOlduguKurumId.HasValue
                ? kisi.temelBilgiler.KisiBagliOlduguKurumId.Value
                : 0;
            // Kişinin organizasyon ağaç yapısına bağlı olarak yetkili olduğu kişileri getirir
            var yetkiliKisiler = AmirTemsilciyeGoreKisiListesi(kisi.temelBilgiler.TabloID, kurumId);
            //Kişinin yetkili  olduğu kurumları getirir
            var yetkiliKurumlar = _kurumService.AmirTemsilciyeGoreKurumListesi(kisi.temelBilgiler.TabloID, kurumId);
            var yetkiliKurumlarIds = yetkiliKurumlar.Value.Select(x => x.TabloID).ToList();
            var yetkiliKisilerIds = yetkiliKisiler.Value.Select(x => x.TabloID).ToList();
            yetkiliKisilerIds.Add(kisi.temelBilgiler.TabloID);
            yetkiliKurumlarIds.Add(kurumId);

            //Lisans ve session bilgileri
            KullaniciSession kullaniciSession = new()
            {
                AktifMi = kisi.temelBilgiler.AktifMi,
                DilID = kisi.temelBilgiler.DilID,
                KisiID = kisi.temelBilgiler.TabloID,
                KurumID = kurumId,
                YetkiliKisiIdleri = yetkiliKisilerIds,
                YetkiliKurumIdleri = yetkiliKurumlarIds
            };




            kullaniciSession.KurumID = kisi.temelBilgiler.KurumID;
            kullaniciSession.KurumAdi = kurum.Value.KurumKisaUnvan;
            kullaniciSession.IpAdress = login.IpAdress;
            kullaniciSession.KullaniciYetkiListesi = yetkiList.Value;
            kullaniciSession.UserAgent = login.UserAgent;
            kullaniciSession.Ad = kisi.temelBilgiler.KisiAdi;
            kullaniciSession.SoyAd = kisi.temelBilgiler.KisiSoyadi;
            kullaniciSession.KurumAdminMi = _kurumlarKisilerService.KisiKurumAdminMi(kisi.temelBilgiler.TabloID).Value;

            //Giriş yapan kişinin Kurum İlişkilerini çeker
            var kullaniciYetkiKimligiListesi = _kurumlarKisilerService.KisiIDileListGetir(kisi.temelBilgiler.TabloID);

            //Kişinin organizasyon birim ıdlerini çeker ve sessiona ekler
            if (kullaniciYetkiKimligiListesi != null && kullaniciYetkiKimligiListesi.Value != null)
                kullaniciSession.KullaniciYetkiKimligiListesi = kullaniciYetkiKimligiListesi.Value.Select(p => p.KurumOrganizasyonBirimTanimId).ToList();

            // Kişiye bağlı müşteri temsilcileri varsa sessiona ekler
            var musteriTemsilcileri = AmireBagliMusteriTemsilcileriList(kisi.temelBilgiler.TabloID);
            if (musteriTemsilcileri.Value != null)
                kullaniciSession.MusteriTemsilcisiIdListesi = musteriTemsilcileri.Value;
            else
                kullaniciSession.MusteriTemsilcisiIdListesi = new List<int>();

            DistributedCacheEntryOptions options = new();

            var kureselParamModel = new KureselParametreModel()
            {
                ParamTanim = "Session Timeout",
                KurumID = kisi.temelBilgiler.KisiBagliOlduguKurumId.Value,
            };

            //var sessionTimeoutParamModel = _kureselParametrelerService.SessionTimeoutParamGetir("Session Timeout").Value;

            var sessionTimeoutParamModel = new KureselParametreModel();

            sessionTimeoutParamModel.ParametreBaslangicDegeri = 5000;
            sessionTimeoutParamModel.ParametreBitisDegeri = 8500;
            sessionTimeoutParamModel.ParametreMetinDegeri = "";
            sessionTimeoutParamModel.ParametreTarihBaslangicDegeri = null;
            sessionTimeoutParamModel.ParametreTarihBitisDegeri = null;
            //sessionTimeoutParamModel.ParametreKurumsalMiSistemMi = 0;


            options = options.SetAbsoluteExpiration(TimeSpan.FromSeconds(Convert.ToInt32(sessionTimeoutParamModel.ParametreBaslangicDegeri)));

            //Session ataması ve log kaydı
            _sharedSession.SetString(token, JsonConvert.SerializeObject(kullaniciSession), options);
            _logger.LogInformation("Başarılı Giriş: {0}, İPAdress: {1}, UserAgent: {2}", login.EmailOrUserName, login.IpAdress, login.UserAgent);

            return token.ToResult();
        }

        /// <summary>
        /// kişi bilgilerini KisiTemelBilgiler ve KisiHassasBilgiler tablolarına kaydeden method.
        /// </summary>
        /// <param name="model">Kaydedilecek kişi bilgilerini içeren model parametresi.</param>
        /// <returns>kaydedilen kişi bilgilerini içeren modeli JSON formatında döndürür.</returns>

        public Result<BasicKisiModel> KisiKaydet(BasicKisiModel model, bool AktifDurum = false)
        {
            _repository.DataContextConfiguration().BeginNewTransactionIsNotFound();
            var hashSalt = HashSalt.GenerateSaltedHash(64, model.KisiSifre);
            KisiTemelBilgiler kisiTemelReturn = new();
            KisiHassasBilgiler kisiHassasReturn = new();
            var kisiTemelBilgilerModel = new KisiTemelBilgiler
            {
                KisiTelefon1 = model.KisiTelefon,
                KisiAdi = model.KisiAdi,
                KisiSoyadi = model.KisiSoyadi,
                KurumsalMi = model.KurumsalMi,
                KisiKullaniciAdi = model.KisiKullaniciAdi,
                KurumID = Convert.ToInt32(model.KisiBagliOlduguKurumId),
                KisiBagliOlduguKurumId = model.KisiBagliOlduguKurumId,
                KisiEposta = model.KisiMail,
                AktifMi = Convert.ToInt32(false),// varsayılan 0 olunca onay maili sürec,nden geçirilir.
                SilindiMi = Convert.ToInt32(!true),
                KisiEkranAdi = model.KisiKullaniciAdi,
                KayitTarihi = DateTime.Now,
                GuncellenmeTarihi = DateTime.Now,
                DilID = model.KisiDilID,
                MeslekiUnvan = model.MeslekiUnvan,
                UyelikSartiKabulEttiMi = model.UyelikSartiKabulEttiMi,
                PazarlamaBilgisiOnayladiMi = model.PazarlamaBilgisiOnayladiMi
            };
            if (model.MeslekiUnvan == "Ziyaretci")
            {
                kisiTemelBilgilerModel.AktifMi = 1;
                kisiTemelBilgilerModel.SilindiMi = 0;
            }
            kisiTemelReturn = Add(kisiTemelBilgilerModel).Value;

            Update(kisiTemelReturn);


            model.TabloID = kisiTemelReturn.TabloID;
            var kisiHassasBilgilerModel = new KisiHassasBilgiler
            {
                KisiTemelBilgiId = kisiTemelReturn.TabloID,
                AktifMi = Convert.ToInt32(false),
                KisiKimlikNo = "",
                KisiPasaportNo = "",
                KayitTarihi = DateTime.Now,
                GuncellenmeTarihi = DateTime.Now,
                HashValue = hashSalt.Hash,
                SaltValue = hashSalt.Salt,

                KisiID = kisiTemelReturn.TabloID,
                SifreSonYenilemeTarihi = DateTime.Now,
                HesabiAktifMi = false,//
                KurumID = model.KisiBagliOlduguKurumId ?? 0
            };
            if (model.MeslekiUnvan == "Ziyaretci")
            {
                kisiHassasBilgilerModel.HesabiAktifMi = true;
            }
            _kisiHassasBilgilerService.Add(kisiHassasBilgilerModel);
            _repository.DataContextConfiguration().Commit();
            return model.ToResult();
        }





        /// <summary>
        /// KişiID değeriyle aktivasyon talebi oluşturan method.
        /// </summary>
        /// <param name="KisiID"> ilgili kişinin Id değeri</param>
        /// <returns>sonucu döndürür.</returns>

        public Result<SistemLoginSifreYenilemeAktivasyonHareketleri> HesapAktivasyonMailiOlustur(int KisiID)
        {
            DataContextConfiguration().BeginNewTransactionIsNotFound();
            try
            {
                var guid = Guid.NewGuid().ToString();
                var url = "/LoginRegister/Activation?g=" + guid;

                var kisiModel = SingleOrDefault(KisiID).Value;
                if (kisiModel == null)
                {
                    return Results.Fail("Kişi verileri bulunamadı.", ResultStatusCode.CreateError);
                }

                var PostaciIslemGenel = new PostaciBekleyenIslemlerGenel()
                {
                    AktifMi = 1,
                    SilindiMi = 0,
                    KayitTarihi = DateTime.Now,
                    KayitEdenID = kisiModel.TabloID,
                    GuncellenmeTarihi = DateTime.Now,
                    GuncelleyenKisiID = kisiModel.TabloID,
                    KisiID = kisiModel.TabloID,
                    KurumID = kisiModel.KurumID,
                    PostaciIslemDurumTipiId = 1,
                    PostaciIslemReferansNo = Guid.NewGuid().ToString(),
                    TetiklemeEpostaMi = true,
                    TetiklemeIlgiliKisiId = kisiModel.TabloID,
                    TetiklemeIlgiliKurumId = kisiModel.KisiBagliOlduguKurumId,
                    IcerikSablonTanimID = (int)BildirimSablonTipleri.HesapAktivasyon,
                };
                _postaciBekleyenIslemlerGenelService.Add(PostaciIslemGenel);
                var PostaciIslemAyrinti = new PostaciBekleyenIslemlerAyrintilar()
                {
                    AktifMi = 1,
                    SilindiMi = 0,
                    KayitTarihi = DateTime.Now,
                    KayitEdenID = kisiModel.TabloID,
                    GuncellenmeTarihi = DateTime.Now,
                    GuncelleyenKisiID = kisiModel.TabloID,
                    KisiID = kisiModel.TabloID,
                    KurumID = kisiModel.KurumID,
                    PostaciBekleyenIslemlerGenelId = PostaciIslemGenel.TabloID,
                    PlanlananEpostaGonderimZamani = DateTime.Now.AddMinutes(1),
                    GonderimHedefKisiId = kisiModel.TabloID,
                };
                _postaciBekleyenIslemlerAyrintilarService.Add(PostaciIslemAyrinti);


                var sifreyenilemodel = new SistemLoginSifreYenilemeAktivasyonHareketleri()
                {
                    SifreYenilemeAktivasyonLinkiYollananKisiId = kisiModel.TabloID,
                    SifreYenilemeAktivasyonPostaciBekleyenIslemlerAyrintiId = PostaciIslemAyrinti.TabloID,
                    HesapAktivasyonSayfasiGeciciUrl = url,
                    //GeciciUrlsonGecerlilikZamani = DateTime.Now.AddHours(5),
                    GeciciUrlsonGecerlilikZamani = DateTime.Now.AddMinutes(5),
                    GuncellenmeTarihi = DateTime.Now,
                    KayitTarihi = DateTime.Now,
                    KisiID = kisiModel.TabloID,
                    KurumID = kisiModel.KurumID,
                    AktifMi = 1,
                };

                var result = _sistemLoginSifreYenilemeAktivasyonHareketleriService.Add(sifreyenilemodel);




                DataContextConfiguration().Commit();

                return result;

            }
            catch (Exception ex)
            {
                DataContextConfiguration().RollBack();
                return new SistemLoginSifreYenilemeAktivasyonHareketleri().ToResult();
            }

        }

        /// <summary>
        /// Kisi id ile KisiTemelBilgiler ve KisiHassasBilgiler tablosundaki kayıtları bulan ve AktifMi değerini aktif eden method.
        /// </summary>
        /// <param name="guid">Kişinin TemelBilgiler tablosunda kayıtlı Id değeri</param>
        /// <returns>Aktifleştirme işlemi sonucuna göre kişi verilerini içeren modeli döndürür.</returns>

        public Result<KisiTemelBilgiler> KisiAktiveEtme(string guid)
        {
            DataContextConfiguration().BeginNewTransactionIsNotFound();
            var _kurumTemelBilgilerService = _serviceProvider.GetService<IKurumTemelBilgilerService>();

            var gecerlilik = HesapAktivasyonLinkiGecerliMi(guid).Value;
            if (!gecerlilik)
                return null;

            var sifreyenilemeModel = _sistemLoginSifreYenilemeAktivasyonHareketleriService.List(x => x.HesapAktivasyonSayfasiGeciciUrl.EndsWith("g=" + guid)).Value.FirstOrDefault();
            var id = sifreyenilemeModel.SifreYenilemeAktivasyonLinkiYollananKisiId;
            var temelBilgilerData = List(a => a.TabloID == id).Value.SingleOrDefault();
            var hassasBilgilerData = _kisiHassasBilgilerService.List(a => a.KisiTemelBilgiId == id).Value.SingleOrDefault();
            temelBilgilerData.AktifMi = 1;
            temelBilgilerData.AktiflikTarihi = DateTime.Now;
            Update(temelBilgilerData);
            hassasBilgilerData.AktifMi = 1;
            hassasBilgilerData.AktiflikTarihi = DateTime.Now;
            hassasBilgilerData.HesabiAktifMi = true;
            _kisiHassasBilgilerService.Update(hassasBilgilerData);
            var sonDurum = _loginSonDurumService.LoginOlanKisiIDyeGoreGetir(temelBilgilerData.TabloID);
            if (sonDurum.Value != null)
            {
                sonDurum.Value.KacinciBasarisizLogin = 0;
                _loginSonDurumService.Update(sonDurum.Value);
            }
            if (temelBilgilerData.KisiBagliOlduguKurumId != null)
            {
                var kisiKurumData = _kurumTemelBilgilerService.SingleOrDefault(temelBilgilerData.KisiBagliOlduguKurumId.Value);

                if (kisiKurumData.Value.KurumHesabiAktifMi == false)
                {
                    kisiKurumData.Value.KurumHesabiAktifMi = true;
                    _kurumTemelBilgilerService.Update(kisiKurumData.Value);
                }
            }
            DataContextConfiguration().Commit();
            return temelBilgilerData.ToResult();
        }

        /// <summary>
        /// İlgili şifre yenileme maili geçerli mi kontrolü sağlayan method.
        /// </summary>
        /// <param name="guid">şifre yenileme için gereke GUID değerini içeren string parametre.</param>
        /// <returns>Geçerliyse true, değilse false döndürür.</returns>

        public Result<bool> HesapAktivasyonLinkiGecerliMi(string guid)
        {
            var sifreyenilemeModel = _sistemLoginSifreYenilemeAktivasyonHareketleriService.List(x => x.HesapAktivasyonSayfasiGeciciUrl.EndsWith("g=" + guid)).Value.FirstOrDefault();
            if (sifreyenilemeModel == null)
            {
                return Results.Fail("Hesap aktivasyon linkiniz geçersizdir.", ResultStatusCode.ReadError);
            }
            if (sifreyenilemeModel.GeciciUrlsonGecerlilikZamani > DateTime.Now)
            {
                return true.ToResult();
            }
            return false.ToResult().WithError("Hesap aktivasyon linkinizin kullanım zamanı geçmiş.");
        }

        /// <summary>
        ///  Kişi kullanıcı adı ve mail adresinin bulunup bulunmadığını kontrol eden method.
        /// </summary>
        /// <param name="mailOrUsername"> kontrol edilecek mail adresi/ kullanıcı adı</param>
        /// <returns>kontrol edilen değer yoksa true, varsa false döner.</returns>

        public Result<bool> KullaniciAdiKontrolu(string mailOrUsername)
        {
            var result = this.List(x => x.AktifMi == 1 && (x.KisiEposta == mailOrUsername)).Value.SingleOrDefault();
            if (result == null)
            {
                return false.ToResult();
            }

            return true.ToResult();
        }
        /// <summary>
        ///  Kişinin mail adresi ya da telefon numarası daha önce kayıt edilmiş mi.
        /// </summary>
        /// <param name="mailOrUsername"> kontrol edilecek mail adresi/ kullanıcı adı</param>
        /// <returns>kontrol edilen değer yoksa true, varsa false döner.</returns>
        public bool MailveTelefonKontrolu(string mailOrUsername, string TelefonNo)
        {
            try
            {

                var result = this.List(x => x.AktifMi == 1 && (x.KisiEposta == mailOrUsername || x.KisiTelefon1 == TelefonNo)).Value.SingleOrDefault();
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MailveTelefonKontrolu error for {Mail} or {Phone}", mailOrUsername, TelefonNo);
                return false;
            }
        }

        /// <summary>
        /// KişiID değeri ile ilgili kişinin yetkilendirildiği sayfa tanımlarını getiren method.
        /// </summary>
        /// <param name="kisiID">ilgili kişiID değeri</param>
        /// <returns>yetkilendirilen sayfa tanımları listesiin döndürür.</returns>

        public Result<List<string>> KisiYetkilerListGetir(int kisiID)
        {
            var result = _yetkiMerkezi.KisiYetkilerListGetir(kisiID);
            return result;
        }

        /// <summary>
        /// KişiID değeri ile ilgili kişi amirse kendisine bağlı müşteri temsilcilerini geiren metot.
        /// </summary>
        /// <param name="kisiID">ilgili kişiID değeri</param>
        /// <returns>müşteri temsilcisi Idleri listesini döndürür.</returns>

        public Result<List<int>> AmireBagliMusteriTemsilcileriList(int kisiID)
        {
            var kisiService = _serviceProvider.GetService<IKisiService>();
            var result = kisiService.AmireBagliMusteriTemsilcileriList(kisiID);
            return result;
        }

        /// <summary>
        /// Amir temsilciye göre kişi listesini getiren metod
        /// </summary>
        /// <returns></returns>
        public Result<List<KisiListeModel>> AmirTemsilciyeGoreKisiListesi(int kisiId, int kurumId)
        {
            //Kişi ıdye bağlı müşteri temsilcilerini getirir yoksa müşteri temsilcisi ise müşteri temsilcisine bağşlı kişileri oda yoksa hiyerarşi kişileri getirir
            var MusteriTemsilcisiIdList = this.AmireBagliMusteriTemsilcileriList(kisiId);
            if (MusteriTemsilcisiIdList.Value != null && MusteriTemsilcisiIdList.Value.Count > 0)
            {
                var result = AmirlereAstMusteriTemsilcisiKisileriniGetir(kisiId);
                return result;
            }
            else
            {
                var kontrol = _kurumlarKisilerService.KisiMusteriTemsilcisiMi(kisiId).Value;
                if (kontrol)
                {
                    var result = MusteriTemsilcisiBagliKisilerList(kisiId);
                    return result;
                }
                else
                {
                    var result = HiyerarsiDisiKisilerKisiListesi(kisiId, kurumId);
                    return result;
                }
            }
        }

        /// <summary>
        /// Müşteri temsilcisine bağlı kişileri getirme
        /// </summary>
        /// <param name="musteriTemsilcisiId"></param>
        /// <returns></returns>
        public Result<List<KisiListeModel>> MusteriTemsilcisiBagliKisilerList(int musteriTemsilcisiId)
        {
            var _iliskiler = _serviceProvider.GetService<IKisiIliskiService>();
            var kurumlarList = _iliskiler.MusteriTemsilcisiBagliKurumGetir(musteriTemsilcisiId);
            List<KisiListeModel> kisiler = new();
            foreach (var kurumId in kurumlarList.Value)
            {
                var tempKisiler = KisiListesiGetir(kurumId);
                kisiler.AddRange(tempKisiler.Value);
            }
            if (kisiler.Count == 0)
            {
                var kisi = SingleOrDefaultListeModel(musteriTemsilcisiId).Value;
                if (kisi != null)
                    kisiler.Add(kisi);
            }
            return kisiler.Distinct().ToList().ToResult();
        }

        /// <summary>
        /// Pozisyona bağlı hiyerarşik ağaçta ast-üst ilişkisi bulunmayan ancak ilgili kişilere irişmesi gereken kullanıcılar için kullanılacak kişi listesi metodu.
        /// </summary>
        /// <returns>KisiListeModel listesi döndürür. <see cref="KisiListeModel"></see></returns>
        public Result<List<KisiListeModel>> HiyerarsiDisiKisilerKisiListesi(int kisiId, int kurumId)
        {
            var _kurumService = _serviceProvider.GetService<IKurumTemelBilgilerService>();
            var kurumIdList = _kurumService.KurumaBagliKurumIdleriList(kurumId).Value;
            var temelbilgiList = this.List(p => p.AktifMi == 1 && p.SilindiMi == 0 && kurumIdList.Contains(p.KurumID)).Value;
            var kisiListModel = new List<KisiListeModel>();
            var amirler = KisiAmirlerListGetir(kisiId);
            var amirVarmi = amirler.Value.Any();
            foreach (var kisi in temelbilgiList)
            {
                var kisiHassasBilgi = _kisiHassasBilgilerService.List(x => x.KisiID == kisi.TabloID).Value.FirstOrDefault();
                var kurumModel = _kurumService.SingleOrDefault(kisi.KisiBagliOlduguKurumId.Value).Value;
                if (kurumModel != null)
                {
                    var kisiListItem = new KisiListeModel()
                    {
                        TabloID = kisi.TabloID,
                        KisiAdi = kisi.KisiAdi,
                        KisiSoyadi = kisi.KisiSoyadi,
                        KisiKullaniciAdi = kisi.KisiKullaniciAdi,
                        KisiEkranAdi = kisi.KisiEkranAdi,
                        KisiEposta = kisi.KisiEposta,
                        KurumAdi = kurumModel.KurumTicariUnvani,
                        KisiResimUrl = kisi.KisiResimUrl,
                        sifreVarMi = amirVarmi,
                        KurumID = kisi.KurumID,
                        KisiBagliOlduguKurumId = kisi.KisiBagliOlduguKurumId
                    };
                    kisiListModel.Add(kisiListItem);
                }
            }
            return kisiListModel.ToResult();
        }

        /// <summary>
        /// Amir Id ile müsteri temsilcilerini getirme
        /// </summary>
        /// <param name="amirId"></param>
        /// <returns></returns>

        public Result<List<KisiListeModel>> AmirlereAstMusteriTemsilcisiKisileriniGetir(int amirId)
        {
            var kisiService = _serviceProvider.GetService<IKisiService>();
            Result<List<KisiOrganizasyonBirimView>> astlarList = kisiService.KisiAstlarListGetir(amirId);
            List<KisiListeModel> result = new();
            var astlar = IdlereGoreKisiListeModelGetir(astlarList.Value.Select(a => a.KisiId).ToList());
            result.AddRange(astlar.Value);
            //Amir müşteri temsilci ise müşteri temsilcisine bağlı kişileri getirir
            if (_kurumlarKisilerService.KisiMusteriTemsilcisiMi(amirId).Value)
            {
                var temp = MusteriTemsilcisiBagliKisilerList(amirId);
                result.AddRange(temp.Value);
            }
            //Kişinin tüm astlarını getiren döngü.
            foreach (var ast in astlarList.Value)
            {
                var kontrol = _kurumlarKisilerService.KisiMusteriTemsilcisiMi(ast.KisiId);
                if (!kontrol.Value) continue;
                var temp1 = MusteriTemsilcisiBagliKisilerList(ast.KisiId);
                if (ast.KisiAstlar.Count > 0)
                {
                    var temp2 = AmirlereAstMusteriTemsilcisiKisileriniGetir(ast.KisiId);
                    result.AddRange(temp2.Value);
                }
                result.AddRange(temp1.Value);
            }
            int i = 1;
            //Tablo ıdye göre distinct yapan döngü
            while (i < result.Count)
            {
                int j = 0;
                bool remove = false;
                while (j < i && !remove)
                {
                    if (result[i].TabloID.Equals(result[j].TabloID))
                    {
                        remove = true;
                    }
                    j++;
                }
                if (remove)
                {
                    result.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            return result.ToResult();
        }

        /// <summary>
        /// Id Lere göre kişileri KisiListeModel türünde getirir.
        /// </summary>
        /// <param name="model"> ilgili kisi Idleri</param>
        /// <returns>listelenen kişileri döndürür.</returns>

        public Result<List<KisiListeModel>> IdlereGoreKisiListeModelGetir(List<int> model)
        {
            var _kurumService = _serviceProvider.GetService<IKurumTemelBilgilerService>();
            var list = List(x => model.Contains(x.TabloID) && x.AktifMi == 1).Value.ToList();
            var kisiListModel = new List<KisiListeModel>();
            foreach (var kisi in list)
            {
                var kurumModel = _kurumService.SingleOrDefault(kisi.KisiBagliOlduguKurumId.Value).Value;
                if (kurumModel != null)
                {
                    var kisiListItem = new KisiListeModel()
                    {
                        TabloID = kisi.TabloID,
                        KisiAdi = kisi.KisiAdi,
                        KisiSoyadi = kisi.KisiSoyadi,
                        KisiKullaniciAdi = kisi.KisiKullaniciAdi,
                        KisiEkranAdi = kisi.KisiEkranAdi,
                        KisiEposta = kisi.KisiEposta,
                        KurumID = kisi.KurumID,
                        KurumAdi = kurumModel.KurumTicariUnvani,
                        KisiResimUrl = kisi.KisiResimUrl,
                        KisiBagliOlduguKurumId = kisi.KisiBagliOlduguKurumId
                    };
                    kisiListModel.Add(kisiListItem);
                }
            }
            return kisiListModel.ToResult();
        }

        /// <summary>
        /// Kurum organizasyon birimlerine göre kişinin amirlerini getiren method
        /// </summary>
        /// <param name="kisiID"> amirleri getirilecek kişiId</param>
        /// <returns>kişinin amirlerinin listesi</returns>

        public Result<List<KisiOrganizasyonBirimView>> KisiAmirlerListGetir(int kisiID)
        {
            List<KisiOrganizasyonBirimView> ustBirimler = new();
            Amirler(kisiID, ustBirimler);
            return ustBirimler.ToResult();
        }

        /// <summary>
        /// Kurum organizasyon birimlerine göre kişinin amirlerinin amirlerini getiren method
        /// </summary>
        /// <param name="kisiID"> amirleri getirilecek kişiId</param>
        /// <param name="ustBirimler"></param>
        /// <returns>kişinin amirlerinin listesi</returns>
        private void Amirler(int kisiID, List<KisiOrganizasyonBirimView> ustBirimler)
        {
            var _kurumOrganizasyon = _serviceProvider.GetService<IKurumOrganizasyonBirimTanimlariService>();
            var kisiBilgileri = this.SingleOrDefault(kisiID).Value;
            if (kisiBilgileri == null)
            {
                return;
            }
            //Kişinin kurumdaki organizasyonIdleri sorgusu
            var kisiOrgBirimleri = _kurumlarKisilerService.List(s => s.IlgiliKisiId == kisiID && s.KurumID == kisiBilgileri.KurumID && s.AktifMi == 1).Value;
            if (kisiOrgBirimleri == null)
            {
                return;
            }
            var kurumPozisyonlari = _kurumlarKisilerService.PozisyonlarList(kisiBilgileri.KurumID).Value;
            if (kurumPozisyonlari == null)
            {
                return;
            }
            //Kurumdaki pozisyonlar içinde kişinin organziasyon ıdlerini filtreleme
            var x = kurumPozisyonlari.Where(a => kisiOrgBirimleri.Select(a => a.KurumOrganizasyonBirimTanimId).Contains(a.TabloId)).ToList();
            //Kişinin amir pozisyonlarına göre amirleri döndüren döngü
            foreach (var item in x)
            {
                var AmirPozisyonlar = _kurumlarKisilerService.List(w => w.KurumOrganizasyonBirimTanimId == item.UstId && item.UstId != 0 && w.AktifMi == 1 && !ustBirimler.Select(a => a.KisiId).Contains(w.IlgiliKisiId)).Value;
                foreach (var AmirPozisyon in AmirPozisyonlar)
                {
                    if (AmirPozisyon != null)
                    {
                        if (AmirPozisyon.IlgiliKisiId == kisiID)
                            break;
                        var kisiTemelBilgi = this.SingleOrDefault(AmirPozisyon.IlgiliKisiId).Value;
                        var amirPozisyonData = _kurumOrganizasyon.List(a => a.TabloID == AmirPozisyon.KurumOrganizasyonBirimTanimId && a.AktifMi == 1).Value;

                        if (kisiTemelBilgi != null && amirPozisyonData != null)
                        {
                            var ustBirim = new KisiOrganizasyonBirimView()
                            {
                                KisiAdi = kisiTemelBilgi.KisiAdi,
                                KisiSoyadi = kisiTemelBilgi.KisiSoyadi,
                                KurumId = kisiTemelBilgi.KurumID,
                                TipId = item.TabloId,
                                Tanim = amirPozisyonData.FirstOrDefault().BirimTanim,
                                UstId = item.UstId,
                                KisiId = kisiTemelBilgi.TabloID,
                                KisiAstlar = new List<KisiOrganizasyonBirimView>(),
                                KisiUstler = new List<KisiOrganizasyonBirimView>()
                            };
                            ustBirimler.Add(ustBirim);
                            Amirler(kisiTemelBilgi.TabloID, ustBirimler);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Kisi Id ile liste model şeklinde kişi getirme
        /// </summary>
        /// <param name="kisiId"></param>
        /// <returns></returns>
        public Result<KisiListeModel> SingleOrDefaultListeModel(int kisiId)
        {
            var kurumservice = _serviceProvider.GetService<IKurumTemelBilgilerService>();
            var kisiListItem = new KisiListeModel();
            var kisi = SingleOrDefault(kisiId).Value;
            var kurumModel = kurumservice.SingleOrDefault(kisi.KurumID).Value;
            if (kurumModel != null)
            {
                kisiListItem.TabloID = kisi.TabloID;
                kisiListItem.KisiAdi = kisi.KisiAdi;
                kisiListItem.KisiSoyadi = kisi.KisiSoyadi;
                kisiListItem.KisiKullaniciAdi = kisi.KisiKullaniciAdi;
                kisiListItem.KisiEkranAdi = kisi.KisiEkranAdi;
                kisiListItem.KisiEposta = kisi.KisiEposta;
                kisiListItem.KurumAdi = kurumModel.KurumTicariUnvani;
            }
            return kisiListItem.ToResult();
        }

        /// <summary>
        /// Kuruma bağlı kişilerin listelendiği method
        /// </summary>
        /// <param name="kurumId"></param>
        /// <returns>listelenen kişileri döndürür.</returns>
        public Result<List<KisiListeModel>> KisiListesiGetir(int kurumId)
        {
            var kurumservice = _serviceProvider.GetService<IKurumTemelBilgilerService>();
            var temelbilgiList = this.List(p => p.AktifMi == 1 && p.SilindiMi == 0 && p.KurumID == kurumId).Value;
            var kisiListModel = new List<KisiListeModel>();
            foreach (var kisi in temelbilgiList)
            {
                var kurumModel = kurumservice.SingleOrDefault(kisi.KisiBagliOlduguKurumId.Value).Value;
                if (kurumModel != null)
                {
                    var kisiListItem = new KisiListeModel()
                    {
                        TabloID = kisi.TabloID,
                        KisiAdi = kisi.KisiAdi,
                        KisiSoyadi = kisi.KisiSoyadi,
                        KisiKullaniciAdi = kisi.KisiKullaniciAdi,
                        KisiEkranAdi = kisi.KisiEkranAdi,
                        KisiEposta = kisi.KisiEposta,
                        KurumAdi = kurumModel.KurumTicariUnvani,
                        KurumID = Convert.ToInt32(kisi.KisiBagliOlduguKurumId),
                        KisiResimUrl = kisi.KisiResimUrl,
                        sifreVarMi = kurumId != kisi.KurumID,
                        KisiBagliOlduguKurumId = kisi.KisiBagliOlduguKurumId
                    };
                    kisiListModel.Add(kisiListItem);
                }
            }
            return kisiListModel.ToResult();
        }

        /// <summary>
        /// Kişinin session bilgilerini günceller
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public Result<bool> KimlikGuncelle(string sessionID)
        {
            var user = _sharedSession.Get<KullaniciSession>(sessionID).Result;

            var yetkiList = this.KisiYetkilerListGetir(user.KisiID);
            var token = Guid.NewGuid().ToString();
            var _kurumService = _serviceProvider.GetService<IKurumTemelBilgilerService>();
            var kurum = _kurumService.SingleOrDefault(user.KisiID);
            var kurumId = user.KurumID;
            var yetkiliKisiler = AmirTemsilciyeGoreKisiListesi(user.KisiID, kurumId);
            var yetkiliKurumlar = _kurumService.AmirTemsilciyeGoreKurumListesi(user.KisiID, kurumId);
            var yetkiliKurumlarIds = yetkiliKurumlar.Value.Select(x => x.TabloID).ToList();
            var yetkiliKisilerIds = yetkiliKisiler.Value.Select(x => x.TabloID).ToList();
            user.YetkiliKurumIdleri = yetkiliKurumlarIds;
            user.YetkiliKisiIdleri = yetkiliKisilerIds;
            DistributedCacheEntryOptions options = new();
            var kureselParamModel = new KureselParametreModel()
            {
                ParamTanim = "Session Timeout",
                KurumID = user.KurumID,
            };


            var sessionTimeoutParamModel = _kureselParametrelerService.SessionTimeoutParamGetir("Session Timeout").Value;
            options = options.SetAbsoluteExpiration(TimeSpan.FromSeconds(Convert.ToInt32(sessionTimeoutParamModel.ParametreBaslangicDegeri)));


            _sharedSession.SetString(sessionID, JsonConvert.SerializeObject(user), options);

            var userm = _sharedSession.Get<KullaniciSession>(sessionID).Result;
            return true.ToResult();
        }

        public Result<string> ZiyaretciLogin(LoginModel login)
        {
            //Kişinin mailine göre hassas ve temel bilgileri çeken sorgu
            var kisi = (from temelBilgiler in List().Value
                        join hassasBilgiler in _kisiHassasBilgilerService.ListForQuery() on temelBilgiler.TabloID equals hassasBilgiler.KisiTemelBilgiId
                        where temelBilgiler.AktifMi == 1 &&
                        (temelBilgiler.KisiEposta.ToLower() == login.EmailOrUserName.ToLower().Trim() || temelBilgiler.KisiKullaniciAdi.ToLower() == login.EmailOrUserName.ToLower().Trim())
                        select new { hassasBilgiler, temelBilgiler }
                        ).FirstOrDefault();
            if (kisi == null)
            {
                _logger.LogWarning("Geçersiz Kullanıcı Adı: {0}, İPAdress: {1}, UserAgent: {2}", login.EmailOrUserName, login.IpAdress, login.UserAgent);

                return Results.Fail("Geçersiz kullanıcı adı!", ResultStatusCode.ReadError);
            }
           
            //Kişinin şifre doğrulaması
            var r = HashSalt.VerifyPassword(login.Password, kisi.hassasBilgiler.HashValue, kisi.hassasBilgiler.SaltValue) || (login.Google || login.Facebook) && !string.IsNullOrEmpty(login.ExternalId);

            if (!r)
            {
                _logger.LogWarning("Hatalı Giriş: {0}, İPAdress: {1}, UserAgent: {2}", login.EmailOrUserName, login.IpAdress, login.UserAgent);

                return Results.Fail("Geçersiz kullanıcı adı şifre!", ResultStatusCode.ReadError);
            }
            //// kişinin yetkilendirildiği sayfalar (Ziyareetciİçin gerekirse bunlar kullanılabilir)
            //var yetkiList = this.KisiYetkilerListGetir(kisi.temelBilgiler.TabloID);

            var token = Guid.NewGuid().ToString();
            var _kurumService = _serviceProvider.GetService<IKurumTemelBilgilerService>();
            //var kurum = _kurumService.SingleOrDefault(kisi.temelBilgiler.KurumID);

            var kurumId = kisi != null && kisi.temelBilgiler.KisiBagliOlduguKurumId.HasValue
                ? kisi.temelBilgiler.KisiBagliOlduguKurumId.Value
                : 0;

            //Lisans ve session bilgileri
            KullaniciSession kullaniciSession = new()
            {
                AktifMi = kisi.temelBilgiler.AktifMi,
                DilID = kisi.temelBilgiler.DilID,
                KisiID = kisi.temelBilgiler.TabloID,
                KurumID = kurumId,
                //YetkiliKisiIdleri = yetkiliKisilerIds,
            };




            kullaniciSession.KurumID = kisi.temelBilgiler.KurumID;
            //kullaniciSession.KurumAdi = kurum.Value.KurumKisaUnvan;
            kullaniciSession.IpAdress = login.IpAdress;
            kullaniciSession.UserAgent = login.UserAgent;
            kullaniciSession.Ad = kisi.temelBilgiler.KisiAdi;
            kullaniciSession.SoyAd = kisi.temelBilgiler.KisiSoyadi;
            kullaniciSession.KurumAdminMi = false;

            
            DistributedCacheEntryOptions options = new();

            var kureselParamModel = new KureselParametreModel()
            {
                ParamTanim = "Session Timeout",
                KurumID = kisi.temelBilgiler.KisiBagliOlduguKurumId.Value,
            };

            var sessionTimeoutParamModel = new KureselParametreModel();

            sessionTimeoutParamModel.ParametreBaslangicDegeri = 5000;
            sessionTimeoutParamModel.ParametreBitisDegeri = 8500;
            sessionTimeoutParamModel.ParametreMetinDegeri = "";
            sessionTimeoutParamModel.ParametreTarihBaslangicDegeri = null;
            sessionTimeoutParamModel.ParametreTarihBitisDegeri = null;
            //sessionTimeoutParamModel.ParametreKurumsalMiSistemMi = 0;


            options = options.SetAbsoluteExpiration(TimeSpan.FromSeconds(Convert.ToInt32(sessionTimeoutParamModel.ParametreBaslangicDegeri)));

            //Session ataması ve log kaydı
            _sharedSession.SetString(token, JsonConvert.SerializeObject(kullaniciSession), options);
            _logger.LogInformation("Başarılı Giriş: {0}, İPAdress: {1}, UserAgent: {2}", login.EmailOrUserName, login.IpAdress, login.UserAgent);

            return token.ToResult();
        }
    }
}