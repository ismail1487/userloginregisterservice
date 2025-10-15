using Baz.AOP.Logger.ExceptionLog;
using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.Model.Entity.Constants;
using Baz.Model.Entity.ViewModel;
using Baz.ProcessResult;
using Baz.Repository.Pattern;
using Baz.RequestManager;
using Baz.RequestManager.Abstracts;
using Baz.Service.Base;
using Decor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Baz.Model.Pattern;

namespace Baz.Service
{
    /// <summary>
    /// Kurum Temel Bilgiler için oluşturulmuş Interface'dir.
    /// </summary>
    public interface IKurumTemelBilgilerService : IService<KurumTemelBilgiler>
    {
        /// <summary>
        ///  Kurum kaydı esnasında, kurum bilgileri, kurum ile ilgili kişi bilgileri, ilgili kişiye dair organizasyon tanımlarının kaydı ile ve ilgili kişiye ilgili organizasyon tanımlarının atanması işlemlerini gerçekleştiren method.
        /// </summary>
        /// <param name="_kurumModel"> Kurum bilgilerini içeren parametre.</param>
        /// <param name="kisiModel"> Kişi bilgileri ile ilgili kişiye tanımlanan organizasyon birimlerini barındıran parametre.</param>
        /// <returns>Kaydedilen kişi ve kurum bilgilerini JSON formatında string olarak döndürür.</returns>
        Result<KurumKisiPostModel> KurumKaydet(BasicKurumModel _kurumModel, BasicKisiModel kisiModel);

        /// <summary>
        /// Dinamik kurum kaydetme işleminde, girilen kurum ismi mevcut kurumlar arasında bulunuyor mu kontrolünü gerçekleştiren method.
        /// </summary>
        /// <param name="_vergiNo"></param>
        /// <returns>kontrol sonucu true veya false döndürür.</returns>
        Result<bool> KurumMevcutMu(string _vergiNo);

        /// <summary>
        /// Kurum ismine göre kurum verilerini getiren method.
        /// </summary>
        /// <param name="kurumAdi">kurum adi parametresi.</param>
        /// <returns>kurum bilgilerini döndürür.</returns>
        Result<KurumTemelBilgiler> IsmeGoreKurumGetir(string kurumAdi);

        //Result<KurumTemelBilgiler> VergiNosunaGoreAktifKurumGetir(string vergiNo)

        /// <summary>
        /// Amir temsilciye göre kurum listesi getiren metod
        /// </summary>
        /// <returns></returns>
        public Result<List<KurumTemelBilgiler>> AmirTemsilciyeGoreKurumListesi(int kisiId, int kurumId);

        /// <summary>
        /// Kuruma bağlı kurumların tabloID'lerini getiren metot
        /// </summary>
        /// <param name="kurumId">kurum ID</param>
        /// <returns>int Id listesi</returns>
        public Result<List<int>> KurumaBagliKurumIdleriList(int kurumId);
        /// <summary>
        /// VarOlan kuruma yeni ziyaretçi kaydı yapan metot
        /// </summary>
        /// <param name="kurumId">kurum ID</param>
        /// <returns>int Id listesi</returns>
        Result<BasicKisiModel> ZiyaretciRegister(BasicKisiModel model);
    }

    /// <summary>
    /// Kurum bilgilerinin tanımlandığı KurumTemelBilgiler tablosuyla ilgili işlemleri barındıran class.
    /// </summary>
    public class KurumTemelBilgilerService : Service<KurumTemelBilgiler>, IKurumTemelBilgilerService
    {
        private readonly IKurumlarKisilerService _kurumlarKisilerService;
        private readonly IKurumOrganizasyonBirimTanimlariService _kurumOrganizasyonBirimTanimlariService;
        private readonly IKisiTemelBilgilerService _kisiTemelBilgilerService;
        private readonly IRequestHelper _requestHelper;
        private readonly IKisiService _kisiService;
        private readonly IModulService _modulService;
        private readonly IModulDetayService _modulDetayService;
        private readonly ISayfaYetkiGurubuAyrintilarService _sayfaYetkiGurubuAyrintilarService;


        /// <summary>
        /// Kurum bilgilerinin tanımlandığı KurumTemelBilgiler tablosuyla ilgili işlemleri barındıran class yapıcı metodu
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="helper"></param>
        /// <param name="modulService"></param>
        /// <param name="modulDetayService"></param>
        /// <param name="kurumOrganizasyonBirimTanimlariService"></param>
        /// <param name="kurumlarKisilerService"></param>
        /// <param name="kisiTemelBilgilerService"></param>
        /// <param name="dataMapper"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="kisiService"></param>
        /// <param name="kurumLisansService"></param>
        public KurumTemelBilgilerService(IRepository<KurumTemelBilgiler> repository, IRequestHelper helper, IModulService modulService, IModulDetayService modulDetayService, IKurumOrganizasyonBirimTanimlariService kurumOrganizasyonBirimTanimlariService, IKurumlarKisilerService kurumlarKisilerService, IKisiTemelBilgilerService kisiTemelBilgilerService, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<KurumTemelBilgiler> logger, IKisiService kisiService, ISayfaYetkiGurubuAyrintilarService sayfaYetkiGurubuAyrintilarService) : base(repository, dataMapper, serviceProvider, logger)
        {
            _kurumlarKisilerService = kurumlarKisilerService;
            _kurumOrganizasyonBirimTanimlariService = kurumOrganizasyonBirimTanimlariService;
            _kisiTemelBilgilerService = kisiTemelBilgilerService;
            _requestHelper = helper;
            _kisiService = kisiService;
            _modulService = modulService;
            _modulDetayService = modulDetayService;
            _sayfaYetkiGurubuAyrintilarService = sayfaYetkiGurubuAyrintilarService;
        }

        /// <summary>
        ///  Kurum kaydı esnasında, kurum bilgileri, kurum ile ilgili kişi bilgileri, ilgili kişiye dair organizasyon tanımlarının kaydı ile ve ilgili kişiye ilgili organizasyon tanımlarının atanması işlemlerini gerçekleştiren method.
        /// </summary>
        /// <param name="_kurumModel"> Kurum bilgilerini içeren parametre.</param>
        /// <param name="kisiModel"> Kişi bilgileri ile ilgili kişiye tanımlanan organizasyon birimlerini barındıran parametre.</param>
        /// <returns>Kaydedilen kişi ve kurum bilgilerini JSON formatında string olarak döndürür.</returns>
        public Result<KurumKisiPostModel> KurumKaydet(BasicKurumModel _kurumModel, BasicKisiModel kisiModel)
        {
            // Yöntemde değişiklik yapıyoruz. Kurum zaten var ise, kişiyi kurumun altına genel olarak kaydetip admin onayında bekletiyruz.


            //Method içinde global değişkenler
            var kurumTModel = new KurumTemelBilgiler();
            var newOrgTanimlari = new List<KurumOrganizasyonBirimTanimlari>();
            var kurumKisi = new List<KurumlarKisiler>();
            var mevcutKurumKontrol = KurumMevcutMu(_kurumModel.KurumVergiNo).Value;
            var kurumAdmin = new KurumOrganizasyonBirimTanimlari();
            var calisan = new KurumOrganizasyonBirimTanimlari();
            //KurumKaydet
            var kurumvarmi = false;
            if (!kurumvarmi)
            {
                //Transaction başlat
                _repository.DataContextConfiguration().BeginNewTransactionIsNotFound();
                if (!mevcutKurumKontrol)
                {
                    var tempKurumTemelBilgiler = new KurumTemelBilgiler()
                    {
                        KurumTicariUnvani = _kurumModel.KurumTicariUnvani,
                        KurumTipiId = _kurumModel.KurumTipiId,
                        KurumAdres = 1,
                        KayitTarihi = DateTime.Now,
                        GuncellenmeTarihi = DateTime.Now,
                        AktifMi = 1,
                        KurumKisaUnvan = _kurumModel.KurumKisaUnvan,
                        DilID = kisiModel.KisiDilID,
                        KayitEdenID = 1,
                        AktifEdenKisiID = 1,
                        KurumHesabiAktifMi = false,
                        KurumUlkeId = _kurumModel.UlkeID,
                        KurumVergiNo = _kurumModel.KurumVergiNo
                    };
                    kurumTModel = Add(tempKurumTemelBilgiler).Value;
                    kurumTModel.KurumID = kurumTModel.TabloID;
                    //kurumTModel.AktifKurumHesabıID = kurumTModel.TabloID;// Bu alan kaldırıldı.
                    this.Update(kurumTModel);


                    // Kuruma default olarak Müşteri temsilcisi,Çalışan ve KurmAdmin birim kayıtları yapılması

                    var KurumOrgModel = new KurumOrganizasyonBirimTanimlari()
                    {
                        OrganizasyonBirimTipiId = (int)OrganizasyonBirimTipi.Rol,//3
                        BirimTanim = "Sistem Admin",
                        DilID = 1,
                        KayitTarihi = DateTime.Now,
                        UstId = 0,
                        GuncellenmeTarihi = DateTime.Now,
                        AktifMi = 1,
                        KurumID = kurumTModel.TabloID,
                        BirimKisaTanim = Guid.NewGuid().ToString() + "level1",
                        IlgiliKurumId = kurumTModel.TabloID
                    };
                    _kurumOrganizasyonBirimTanimlariService.Add(KurumOrgModel);
                    newOrgTanimlari.Add(KurumOrgModel);



                    var KurumOrgModel3 = new KurumOrganizasyonBirimTanimlari()
                    {
                        OrganizasyonBirimTipiId = (int)OrganizasyonBirimTipi.Pozisyon, //2
                        BirimTanim = "Genel",
                        DilID = 1,
                        KayitTarihi = DateTime.Now,
                        UstId = 0,
                        GuncellenmeTarihi = DateTime.Now,
                        AktifMi = 1,
                        KurumID = kurumTModel.TabloID,
                        BirimKisaTanim = Guid.NewGuid().ToString() + "level1",
                        IlgiliKurumId = kurumTModel.TabloID
                    };
                    _kurumOrganizasyonBirimTanimlariService.Add(KurumOrgModel3);
                    newOrgTanimlari.Add(KurumOrgModel3);

                    var KurumOrgModelDepartman = new KurumOrganizasyonBirimTanimlari()
                    {
                        OrganizasyonBirimTipiId = (int)OrganizasyonBirimTipi.Departman, //1
                        BirimTanim = "Genel",
                        DilID = 1,
                        KayitTarihi = DateTime.Now,
                        UstId = 0,
                        GuncellenmeTarihi = DateTime.Now,
                        AktifMi = 1,
                        KurumID = kurumTModel.TabloID,
                        BirimKisaTanim = Guid.NewGuid().ToString() + "level1",
                        IlgiliKurumId = kurumTModel.TabloID
                    };
                    _kurumOrganizasyonBirimTanimlariService.Add(KurumOrgModelDepartman);
                    newOrgTanimlari.Add(KurumOrgModelDepartman);

                }
                else
                {
                    kurumTModel = List(x => x.KurumVergiNo == _kurumModel.KurumVergiNo).Value.FirstOrDefault();
                    if (kurumTModel == null)
                    {
                        throw new ArgumentException("Kurum Kaydı oluşturulamadı. Vergi numarasının kontrol ediniz.");
                    }

                }

                _kurumModel.TabloID = kurumTModel.TabloID;
                kisiModel.KisiBagliOlduguKurumId = _kurumModel.TabloID;

                // Kuruma kaydolan ilk kişi ise, admin yetkisinde olacak.

                var KurumdaKacKisiKayitli = _kisiTemelBilgilerService.List(x => x.KisiBagliOlduguKurumId == kisiModel.KisiBagliOlduguKurumId).Value.Count();


                //Kişinin kayıt edilmesi

                var tempKisiTemelBilgiler = _kisiTemelBilgilerService.KisiKaydet(kisiModel, !mevcutKurumKontrol).Value;

                kisiModel.TabloID = tempKisiTemelBilgiler.TabloID;
                foreach (var item in newOrgTanimlari)
                {
                    var model = new KurumlarKisiler()
                    {
                        IlgiliKurumId = kurumTModel.TabloID,
                        IlgiliKisiId = kisiModel.TabloID,
                        KurumID = kurumTModel.TabloID,
                        KisiID = kisiModel.TabloID,
                        KurumOrganizasyonBirimTanimId = item.TabloID,
                        AtanmaZamani = DateTime.Now,
                        AtanmaninSonlanmaZamani = DateTime.Now.AddDays(360),
                        AktifMi = 1,//Convert.ToInt32(!mevcutKurumKontrol), // mevcut kuruma kaydolan birisi ise, otomatik false olur.
                        SilindiMi = 0,//Convert.ToInt32(mevcutKurumKontrol), // mevcut kuruma kaydolan birisi ise, otomatik false olur.
                        KayitTarihi = DateTime.Now,
                        GuncellenmeTarihi = DateTime.Now,
                    };
                    var tempKurumlarKisiler = _kurumlarKisilerService.Add(model).Value;
                    if (tempKurumlarKisiler == null)
                    {
                        throw new ArgumentException("KurumlarKisiler insert failed.");
                    }
                    kurumKisi.Add(tempKurumlarKisiler);
                }

                // Burada SayfaYetkiGurubuAyrintilar tablosundan otomatik yetkisi olunan sayfaları ekleyeceğiz.

                var sayfayetkilistesiservis = _sayfaYetkiGurubuAyrintilarService.SayfalariGetir(2);

                //var AdminSayfaListesi = new List<int>() {
                //        2,3,10,11,12,13,14,15,16,17,18,19,20,21,24,25,26,27,28,29,
                //        38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,
                //        66,67,68,69,70,71,72,73,74,75,76,77,78,102,103,104,105,106,107,108,109,110,111,112,
                //        113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128,129,130,131,132,133,
                //        134,135,136,137,138,139,140,141,142,143,144,145,146,147,148,149,150,151,152,153,154,
                //        155,156,157,158,159,160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,
                //        176,177,178,179,180,181,182,183,184,185,186,187,188,189,190,191,192,193,194,195,196,
                //        197,198,199,200,201,202,203,204,205,206,207,208,209,210,211,214,215,216,217,218,219,
                //        220,221,222,223,224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,240,
                //        241,242,243,244,245,246,247,248,249,250,251,252,253,254,255,256,257,258,259,260,261
                //};
                //var UyeSayfaListesi = new List<int>() {
                //        2,3,10,11,12,13,18,19
                //};


                var KurumAdmin = 1;
                var KurumUyesi = 2;

                var ParamErisimGurupID = KurumdaKacKisiKayitli == 0 ? KurumAdmin : KurumUyesi;
                //var sayfalar = KurumdaKacKisiKayitli == 0 ? AdminSayfaListesi : UyeSayfaListesi;

                if (mevcutKurumKontrol) // Mevcut kuruma kaydoluyorsa artık üyedir.
                    ParamErisimGurupID = KurumUyesi;

                var sayfalar = _sayfaYetkiGurubuAyrintilarService.SayfalariGetir(ParamErisimGurupID);

                var list = new List<ErisimYetkilendirmeTanimlari>();

                foreach (var item in newOrgTanimlari)
                {
                    foreach (var sayfa in sayfalar)
                    {
                        var yetkiTanimi = new ErisimYetkilendirmeTanimlari()
                        {
                            IlgiliKurumOrganizasyonBirimTanimiId = item.TabloID,
                            ErisimYetkisiVerilenSayfaId = sayfa,
                            KayitTarihi = DateTime.Now,
                            ErisimYetkisiVerilmeTarihi = DateTime.Now,
                            GuncellenmeTarihi = DateTime.Now,
                            AktifMi = 1,
                            SilindiMi = 0,
                            KayitEdenID = kisiModel.TabloID,
                            KurumID = kurumTModel.TabloID,
                            KisiID = kisiModel.TabloID,
                            ErisimYetkisiVerenKisiId = kisiModel.TabloID,
                            IlgiliKurumId = kurumTModel.TabloID,
                        };
                        list.Add(yetkiTanimi);
                    }
                }


                var _yetkiMerkeziService = _serviceProvider.GetService<IYetkiMerkeziService>();
                _yetkiMerkeziService.ErisimYetkilendirmeTanimlariKaydet(list);


                var calisanSayfalar = new ModulDetayKayitModel();

                calisanSayfalar.Name = "Anasayfa";
                calisanSayfalar.TabloID = 1;
                calisanSayfalar.SayfaId = new List<int>();
                calisanSayfalar.SayfaId.Add(1);


                if (calisanSayfalar != null)
                {
                    var pages = calisanSayfalar.SayfaId;
                    pages.Add(64);
                    var tanimlar = new List<ErisimYetkilendirmeTanimlari>();
                    foreach (var sayfa in pages)
                    {
                        var yetkiTanimi = new ErisimYetkilendirmeTanimlari()
                        {
                            IlgiliKurumOrganizasyonBirimTanimiId = calisan.TabloID,
                            ErisimYetkisiVerilenSayfaId = sayfa,
                            KayitTarihi = DateTime.Now,
                            ErisimYetkisiVerilmeTarihi = DateTime.Now,
                            GuncellenmeTarihi = DateTime.Now,
                            AktifMi = 1,
                            SilindiMi = 0,
                            KayitEdenID = kisiModel.TabloID,
                            KurumID = kurumTModel.TabloID,
                            KisiID = kisiModel.TabloID,
                            ErisimYetkisiVerenKisiId = kisiModel.TabloID,
                            IlgiliKurumId = kurumTModel.TabloID,
                        };

                        tanimlar.Add(yetkiTanimi);
                    }

                    _yetkiMerkeziService.ErisimYetkilendirmeTanimlariKaydet(tanimlar);
                }

                KurumKisiPostModel returnModel = new()
                {
                    KisiModel = kisiModel,
                    KurumModel = _kurumModel
                };

                DataContextConfiguration().Commit();

                return returnModel.ToResult();

                throw new ArgumentException("Kurum Kaydı oluşturulamadı.");
            }

            // Kurum mevcuttur, ve kurumun altına kişiyi kaydetip admin onayına alacağız.

            throw new ArgumentException("Kaydınız gerçeleşmiştir. Lütfen kurum yöneticinizin onaylamasını bekleyiniz veya sistem yöneticinizle  görüşünüz.");
        }

        /// <summary>
        /// Dinamik kurum kaydetme işleminde, girilen kurum ismi mevcut kurumlar arasında bulunuyor mu kontrolünü gerçekleştiren method.
        /// </summary>
        /// <param name="_vergiNo"></param>
        /// <returns>kontrol sonucu true veya false döndürür.</returns>

        public Result<bool> KurumMevcutMu(string _vergiNo)
        {
            var result = List(x => x.KurumVergiNo == _vergiNo).Value.FirstOrDefault();
            if (result == null)
            {
                return false.ToResult();
            }
            return true.ToResult();
        }

        /// <summary>
        /// Kurum ismine göre kurum verilerini getiren method.
        /// </summary>
        /// <param name="kurumAdi">kurum adi parametresi.</param>
        /// <returns>kurum bilgilerini döndürür.</returns>

        public Result<KurumTemelBilgiler> IsmeGoreKurumGetir(string kurumAdi)
        {
            var result = List(x => x.KurumTicariUnvani == kurumAdi).Value.FirstOrDefault();
            return result.ToResult();
        }

        /// <summary>
        /// Pozisyona bağlı hiyerarşik ağaçta ast-üst ilişkisi bulunmayan ancak ilgili kurumlara erişmesi gereken kullanıcılar için kullanılacak kurum listesi metodu.
        /// </summary>
        /// <returns>KisiListeModel listesi döndürür. <see cref="KurumTemelBilgiler"></see></returns>
        public Result<List<KurumTemelBilgiler>> HiyerarsiDisiKisilerKurumListesi(int kurumId)
        {
            var result = List(a => a.AktifMi == 1 && a.KurumID == kurumId);
            return result;
        }

        /// <summary>
        /// Musteri temsilcisi Idye göre bağlı kurumlarını getiren method
        /// </summary>
        /// <param name="musteriTemsilcisiId"></param>
        /// <returns></returns>
        public Result<List<KurumTemelBilgiler>> MusteriTemsilcisiBagliKurumlarList(int musteriTemsilcisiId)
        {
            var _kisiService = _serviceProvider.GetService<IKisiService>();
            var _iliskiler = _serviceProvider.GetService<IKurumIliskiService>();
            var kurumIdList = _iliskiler.MusteriTemsilcisiBagliKurumIdGetir(musteriTemsilcisiId);
            var kurumlar = List(a => kurumIdList.Value.Contains(a.TabloID)).Value.ToList();

            var kisi = _kisiService.SingleOrDefault(musteriTemsilcisiId).Value;
            var kurum = SingleOrDefault(kisi.KurumID).Value;
            kurumlar.Add(kurum);

            return kurumlar.Distinct().ToList().ToResult();
        }

        /// <summary>
        /// Amir temsilciye göre kurum listesi getiren metod
        /// </summary>
        /// <returns></returns>
        public Result<List<KurumTemelBilgiler>> AmirTemsilciyeGoreKurumListesi(int kisiId, int kurumId)
        {
            var MusteriTemsilcisiIdList = _kisiService.AmireBagliMusteriTemsilcileriList(kisiId);
            if (MusteriTemsilcisiIdList != null && MusteriTemsilcisiIdList.Value.Count > 0)
            {
                var result = AmirlereAstMusteriTemsilcisiKurumlariniGetir(kisiId);
                return result;
            }
            else
            {
                var kontrol = _kurumlarKisilerService.KisiMusteriTemsilcisiMi(kisiId).Value;
                if (kontrol)
                {
                    var result = MusteriTemsilcisiBagliKurumlarList(kisiId);
                    return result;
                }
                else
                {
                    var result = HiyerarsiDisiKisilerKurumListesi(kurumId);
                    return result;
                }
            }
        }

        /// <summary>
        /// Amirlere Ast Musteri Temsilcisi Kurumlarini Getiren method
        /// </summary>
        /// <param name="amirId"></param>
        /// <returns></returns>
        public Result<List<KurumTemelBilgiler>> AmirlereAstMusteriTemsilcisiKurumlariniGetir(int amirId)
        {
            var _kisiService = _serviceProvider.GetService<IKisiService>();
            var _kurumlarKisiler = _serviceProvider.GetService<IKurumlarKisilerService>();
            Result<List<KisiOrganizasyonBirimView>> astlarList = _kisiService.KisiAstlarListGetir(amirId);
            List<KurumTemelBilgiler> result = new();
            var kisi = _kisiService.SingleOrDefault(amirId).Value;
            var kurum = SingleOrDefault(kisi.KurumID).Value;
            if (_kurumlarKisiler.KisiMusteriTemsilcisiMi(amirId).Value)
            {
                var temp = MusteriTemsilcisiBagliKurumlarList(amirId);
                result.AddRange(temp.Value);
            }
            result.Add(kurum);
            //Kişinin ast müşteri temsilcilerine bağlı kurumları getiren döngü
            foreach (var ast in astlarList.Value)
            {
                var kontrol = _kurumlarKisiler.KisiMusteriTemsilcisiMi(ast.KisiId);
                if (kontrol.Value)
                {
                    var temp1 = MusteriTemsilcisiBagliKurumlarList(ast.KisiId);
                    if (ast.KisiAstlar != null && ast.KisiAstlar.Count > 0)
                    {
                        var temp2 = AmirlereAstMusteriTemsilcisiKurumlariniGetir(ast.KisiId);
                        if (temp2.Value != null)
                        {
                            result.AddRange(temp2.Value);
                        }
                    }
                    if (temp1.Value[0] != null)
                    {
                        result.AddRange(temp1.Value);
                    }
                }
            }
            return result.Distinct().ToList().ToResult();
        }

        /// <summary>
        /// Kuruma bağlı kurumların tabloID'lerini getiren metot
        /// </summary>
        /// <param name="kurumId">kurum ID</param>
        /// <returns>int Id listesi</returns>
        public Result<List<int>> KurumaBagliKurumIdleriList(int kurumId)
        {
            var result = List(a => a.AktifMi == 1 && a.KurumID == kurumId).Value.Select(x => x.TabloID).ToList();
            return result.ToResult();
        }
        /// <summary>
        /// Web Site ziyaretçisi olarak kayıt yaptıran kullanıcının kayıt işlemlerini gerçekleştiren metot
        /// </summary>
        /// <param name="model">kurum ID</param>
        /// <returns>int Id listesi</returns>
        public Result<BasicKisiModel> ZiyaretciRegister(BasicKisiModel model)
        {
            try
            {
                var kurumBilgileri = List(x => x.AktifMi == 1).Value.FirstOrDefault();
                var kurumID = kurumBilgileri != null ? kurumBilgileri.TabloID : 0;
                model.MeslekiUnvan = "Ziyaretci";
                var kisiVarmi = _kisiTemelBilgilerService.MailveTelefonKontrolu(model.KisiEposta, model.KisiTelefon);
                if (kisiVarmi == true)
                {
                    return Results.Fail<BasicKisiModel>("Bu e-posta veya telefon numarası ile zaten bir kayıt bulunmaktadır.");
                }

                _repository.DataContextConfiguration().BeginNewTransactionIsNotFound();
                var tempKisiTemelBilgiler = _kisiTemelBilgilerService.KisiKaydet(model).Value;

                model.TabloID = tempKisiTemelBilgiler.TabloID;
                var kurumOrganizasyonBirimTanimlariID = _kurumOrganizasyonBirimTanimlariService.List(x => x.BirimTanim == "Ziyaretçi").Value.Select(x => x.TabloID);
                foreach (var item in kurumOrganizasyonBirimTanimlariID)
                {
                    //departman,pozisyon,rol için ziyaretciye uygun kurumarkisiler kaydı yapılıyor.
                    var Ziyaretci = new KurumlarKisiler()
                    {
                        IlgiliKurumId = kurumID,
                        IlgiliKisiId = model.TabloID,
                        KurumID = kurumID,
                        KisiID = model.TabloID,
                        KurumOrganizasyonBirimTanimId = item,
                        AtanmaZamani = DateTime.Now,
                        AtanmaninSonlanmaZamani = DateTime.Now.AddDays(360),
                        AktifMi = 1,
                        SilindiMi = 0,
                        KayitTarihi = DateTime.Now,
                        GuncellenmeTarihi = DateTime.Now,
                    };
                    var tempKurumlarKisiler = _kurumlarKisilerService.Add(Ziyaretci).Value;
                }
                DataContextConfiguration().Commit();

                return model.ToResult();
            }
            catch (Exception ex)
            {
                DataContextConfiguration().Commit();
                throw new ArgumentException("Ziyaretçi kaydı sırasında hata oluştu");
            }
        }
    }
}