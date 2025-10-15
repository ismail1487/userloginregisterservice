using Baz.Model.Entity;
using Baz.Model.Entity.ViewModel;
using Baz.ProcessResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Baz.Repository.Pattern;

namespace Baz.Service
{
    /// <summary>
    /// Yetki Merkezi işlemlerini gerçekleştiren methodları barındıran interface.
    /// </summary>
    public interface IYetkiMerkeziService
    {
        /// <summary>
        /// KişiID değeri ile ilgili kişinin yetkilendirildiği sayfa tanımlarını getiren method.
        /// </summary>
        /// <param name="kisiID">ilgili kişiID değeri</param>
        /// <returns>yetkilendirilen sayfa tanımları listesiin döndürür.</returns>

        Result<List<string>> KisiYetkilerListGetir(int kisiID);

        /// <summary>
        /// Erişim yetkilendirme tanımlarını kaydeden method.
        /// </summary>
        /// <param name="list"> kaydedilecek erişim yetkilendirme tanımları listesi</param>
        /// <returns>kaydedilen verileri döndürür.</returns>
        public Result<List<ErisimYetkilendirmeTanimlari>> ErisimYetkilendirmeTanimlariKaydet(List<ErisimYetkilendirmeTanimlari> list);
    }

    /// <summary>
    /// Yetki Merkezi işlemlerini gerçekleştiren methodları barındıran class.
    /// </summary>
    public class YetkiMerkeziService : IYetkiMerkeziService
    {
        private readonly IErisimYetkilendirmeTanimlariService _erisimYetkilendirmeTanimlariService;
        private readonly IKurumlarKisilerService _kurumlarKisilerService;
        private readonly ISistemSayfalariService _sistemSayfalariService;

        /// <summary>
        /// Yetki Merkezi işlemlerini gerçekleştiren methodları barındıran classın yapıcı metodu
        /// </summary>
        /// <param name="erisimYetkilendirmeTanimlariService"></param>
        /// <param name="kurumlarKisilerService"></param>
        /// <param name="sistemSayfalariService"></param>
        public YetkiMerkeziService(IRepository<ErisimYetkilendirmeTanimlari> repository, IErisimYetkilendirmeTanimlariService erisimYetkilendirmeTanimlariService, IKurumlarKisilerService kurumlarKisilerService, ISistemSayfalariService sistemSayfalariService)
        {
            _erisimYetkilendirmeTanimlariService = erisimYetkilendirmeTanimlariService;
            _kurumlarKisilerService = kurumlarKisilerService;
            _sistemSayfalariService = sistemSayfalariService;
        }

        /// <summary>
        /// KişiID değeri ile ilgili kişinin yetkilendirildiği sayfa tanımlarını getiren method.
        /// </summary>
        /// <param name="kisiID">ilgili kişiID değeri</param>
        /// <returns>yetkilendirilen sayfa tanımları listesiin döndürür.</returns>
        public Result<List<string>> KisiYetkilerListGetir(int kisiID)
        {
            //var kisiLisansVerisi = _lisansKurumKisiAbonelikTanimlariService.KisiLisansAbonelikGetir(kisiID);
            var strList = new List<string>();
            //Kişinin lisansına bağlı sayfaları getirir.
            //var lisansSayfalar = _lisansService.GetLisansSayfaId(Convert.ToInt32(kisiLisansVerisi.Value.LisansGenelTanimId));

            var kisiOrgList = _kurumlarKisilerService.KisiIDileListGetir(kisiID).Value;

            List<ErisimYetkilendirmeTanimlari> yetkiTanimlari = new();

            //Kişinin yetkili olduğu sayfalar
            yetkiTanimlari = _erisimYetkilendirmeTanimlariService.List(a => kisiOrgList.Select(b => b.KurumOrganizasyonBirimTanimId).Contains(a.IlgiliKurumOrganizasyonBirimTanimiId) && a.AktifMi == 1 && a.ErisimYetkisiVerilenSayfaId != null).Value;

            yetkiTanimlari = yetkiTanimlari.GroupBy(x => x.ErisimYetkisiVerilenSayfaId).Select(x => x.FirstOrDefault()).ToList();
            var erisimSayfalar = yetkiTanimlari.Where(a => a.ErisimYetkisiVerilenSayfaId != null).Select(a => a.ErisimYetkisiVerilenSayfaId);

            //Sayfaların Urllerini döndüren döngü.
            var finalSayfalar = erisimSayfalar;
            foreach (var item in finalSayfalar)
            {
                var sayfa = _sistemSayfalariService.SingleOrDefault(Convert.ToInt32(item)).Value;
                if (sayfa != null)
                {
                    strList.Add(sayfa.SayfaTanimi);
                }
            }
            
            return strList.ToResult();
        }

        /// <summary>
        /// Erişim yetkilendirme tanımlarını kaydeden method.
        /// </summary>
        /// <param name="list"> kaydedilecek erişim yetkilendirme tanımları listesi</param>
        /// <returns>kaydedilen verileri döndürür.</returns>
        public Result<List<ErisimYetkilendirmeTanimlari>> ErisimYetkilendirmeTanimlariKaydet(List<ErisimYetkilendirmeTanimlari> list)
        {
            if (list.Count == 0)
            {
                return Results.Fail("Kaydetme işleminiz gerçekleşmemiştir!", ResultStatusCode.CreateError);
            }
            var result1 = _erisimYetkilendirmeTanimlariService.ErisimYetkilendirmeTanimlariListesi();

            bool benzerKayitVarMi = false;
            var returnList = new List<ErisimYetkilendirmeTanimlari>();
            foreach (var item in list)
            {
                var varMi = result1.Any(x =>
                    x.ErisimYetkisiVerilenSayfaId == item.ErisimYetkisiVerilenSayfaId &&
                    x.IlgiliKurumOrganizasyonBirimTanimiId == item.IlgiliKurumOrganizasyonBirimTanimiId);

                if (varMi)
                {
                    benzerKayitVarMi = true;
                }
                else
                {
                    var result = _erisimYetkilendirmeTanimlariService.Add(item);
                    returnList.Add(result.Value);
                }
            }

            if (benzerKayitVarMi)
            {
                if (list.Count == 1)
                {
                    return returnList.ToResult().WithSuccess(new Success("Benzer kayıt mevcuttur. İşleminiz gerçekleştirilemedi."));
                }
                return returnList.ToResult().WithSuccess(new Success("Benzer kayıtlar mevcuttur. Haricindekiler kayıt edilmiştir."));
            }

            return returnList.ToResult().WithSuccess(new Success("Kayıt başarılı."));
        }
    }
}