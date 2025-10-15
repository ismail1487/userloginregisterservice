using Baz.Attributes;
using Baz.Model.Entity;
using Baz.Model.Entity.ViewModel;
using Baz.ProcessResult;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Baz.Service;
using Microsoft.AspNetCore.Authorization;

namespace Baz.UserLoginServiceApi.Controllers
{
    /// <summary>
    /// Kurum Kişi Kayıt kontrol methodlarının bulunduğu Classdır.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route("api/[Controller]")]
    [ApiController]
    public class KurumKisiKayitController : Controller
    {
        private readonly IKurumOrganizasyonBirimTanimlariService _kurumOrganizasyonBirimTanimlariService;
        private readonly IKurumTemelBilgilerService _kurumTemelBilgilerService;
        private readonly IKisiTemelBilgilerService _kisiTemelBilgilerService;
        private readonly IKisiService _kisiService;
        private readonly IParamOrganizasyonBirimleriService _paramOrganizasyonBirimleriService;
        private readonly ISayfaYetkiGurubuAyrintilarService _sayfaYetkiGurubuAyrintilarService;

        /// <summary>
        /// Kurum kişi kayıt kontrol metodlarının yer aldığı konst.
        /// </summary>
        /// <param name="kurumTemelBilgilerService"></param>
        /// <param name="kurumOrganizasyonBirimTanimlariService"></param>
        /// <param name="kisiTemelBilgilerService"></param>
        /// <param name="paramOrganizasyonBirimleriService"></param>
        public KurumKisiKayitController(IKurumTemelBilgilerService kurumTemelBilgilerService, ISayfaYetkiGurubuAyrintilarService sayfaYetkiGurubuAyrintilarService, IKurumOrganizasyonBirimTanimlariService kurumOrganizasyonBirimTanimlariService, IKisiTemelBilgilerService kisiTemelBilgilerService, IParamOrganizasyonBirimleriService paramOrganizasyonBirimleriService, IKisiService kisiService)
        {
            _kisiTemelBilgilerService = kisiTemelBilgilerService;
            _kurumTemelBilgilerService = kurumTemelBilgilerService;
            _kurumOrganizasyonBirimTanimlariService = kurumOrganizasyonBirimTanimlariService;
            _paramOrganizasyonBirimleriService = paramOrganizasyonBirimleriService;
            _kisiService = kisiService;
            _sayfaYetkiGurubuAyrintilarService = sayfaYetkiGurubuAyrintilarService;
        }

        [ProcessName(Name = "Yetki Gurup Listesi")]
        [Route("yetkigurupliste")]
        [HttpPost]
        [AllowAnonymous]
        public Result<List<int>> yetkigurupliste(int gurupid)
        {
            var result = _sayfaYetkiGurubuAyrintilarService.SayfalariGetir(gurupid);
            return result.ToResult();
        }


        /// <summary>
        /// Kurum ve kişi kaydı işlemlerini yöneten HTTPPost türünde method
        /// </summary>
        /// <param name="model"></param>
        /// <returns>kaydedilen verileri JSON formatında döndürür.</returns>
        [ProcessName(Name = "Kurum ve kişi kaydı işlemlerinin gerçekleştirilmesi")]
        [Route("KurumKisiKaydet")]
        [HttpPost]
        [AllowAnonymous]
        public Result<KurumKisiPostModel> KurumKisikaydet(KurumKisiPostModel model)
        {
            var result = _kurumTemelBilgilerService.KurumKaydet(model.KurumModel, model.KisiModel);
            return result;
        }

        /// <summary>
        ///  Kurum Organizasyon Birim Tanımlarının kaydedilmesi işlemini gerçekleştiren method.
        /// </summary>
        /// <param name="model"> kaydedilmek istenen parametreleri barındıran <see cref="KurumOrganizasyonBirimTanimlari"/> türünde model </param>
        /// <returns>kaydedilen model verisini JSON formatında döndürür.</returns>
        [ProcessName(Name = "Kurum Organizasyon birim tanımlaması kaydı")]
        [Route("KurumOrganizasyonKaydet")]
        [HttpPost]
        public Result<KurumOrganizasyonBirimTanimlari> KurumOrganizasyonKaydet(KurumOrganizasyonBirimTanimlari model)
        {
            var result = _kurumOrganizasyonBirimTanimlariService.Add(model);
            return result;
        }



        /// <summary>
        /// Hesap aktivasyon linki oluşturan ve ilgili linki mail ile gönderen method
        /// </summary>
        /// <param name="KisiID">hesap aktivasyon talebi oluşturulacak kişinin Id değeri</param>
        /// <returns>sonuc değerini json string halinde döndürür.</returns>
        [ProcessName(Name = "Aktivasyon maili oluşturulması")]
        [Route("HesapAktivasyonMailiOlustur")]
        [HttpPost]
        [AllowAnonymous]
        public Result<SistemLoginSifreYenilemeAktivasyonHareketleri> HesapAktivasyonMailiOlustur([FromBody] int KisiID)
        {
            var result = _kisiTemelBilgilerService.HesapAktivasyonMailiOlustur(KisiID);
            return result;
        }

        /// <summary>
        /// Hesap aktivasyon işlemini gerçekleştiren method.
        /// </summary>
        /// <param name="guid">hesap aktivasyon talebi oluşturulacak kişinin Id değeri</param>
        /// <returns>sonuc değerini json string halinde döndürür.</returns>
        [ProcessName(Name = "Hesap aktivasyon işlemi")]
        [Route("HesapAktivasyonIslemi")]
        [HttpPost]
        [AllowAnonymous]
        public Result<KisiTemelBilgiler> HesapAktivasyonIslemi([FromBody] string guid)
        {
            var result = _kisiTemelBilgilerService.KisiAktiveEtme(guid);
            return result;
        }

        /// <summary>
        /// İlgili aktivasyon linki geçerli mi kontrolü sağlayan HTTPPost methodu.
        /// </summary>
        /// <param name="guid">aktivasyon için gereken GUID değerini içeren string parametre.</param>
        /// <returns>Geçerliyse true, değilse false döndürür.</returns>
        [ProcessName(Name = "İlgili GUID ile oluşturulan hesap aktifleştirme talebi halen geçerli mi kontrol edilmesi")]
        [Route("HesapAktivasyonLinkiGecerliMi")]
        [HttpPost]
        [AllowAnonymous]
        public Result<bool> HesapAktivasyonLinkiGecerliMi([FromBody] string guid)
        {
            var result = _kisiTemelBilgilerService.HesapAktivasyonLinkiGecerliMi(guid);
            return result;
        }

        /// <summary>
        /// Kurum adina göre kurum bilgilerini getiren metod
        /// </summary>
        /// <param name="kurumAdi"></param>
        /// <returns></returns>
        [ProcessName(Name = "İsme göre kurum getir")]
        [Route("IsmeGoreGetir/{kurumAdi}")]
        [HttpGet]
        public Result<KurumTemelBilgiler> IsmeGoreKurumGetir(string kurumAdi)
        {
            var result = _kurumTemelBilgilerService.IsmeGoreKurumGetir(kurumAdi);
            return result;
        }

        /// <summary>
        /// Sistemdeki üyeleri getirir
        /// </summary>
        /// <returns></returns>
        [ProcessName(Name = "Sistemdeki üyeleri getirir")]
        [Route("UyeleriGetir")]
        [HttpGet]
        public Result<List<UyeListViewModel>> UyeleriGetir()
        {
            var result = _kisiService.UyeleriGetir();
            return result;
        }
    }
}