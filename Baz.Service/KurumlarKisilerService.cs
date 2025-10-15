using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.Model.Entity.ViewModel;
using Baz.ProcessResult;
using Baz.Repository.Pattern;
using Baz.Service.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Baz.Service
{
    /// <summary>
    /// Kurumlar ve Kişiler için oluşturulmuş Interface'dir.
    /// </summary>
    /// <seealso cref="KurumlarKisiler" />
    public interface IKurumlarKisilerService : IService<KurumlarKisiler>
    {
        /// <summary>
        /// KurumKisiler tablosunda kişi Id'ye göre bilgileri getiren metot
        /// </summary>
        /// <param name="kisiID"></param>
        /// <returns></returns>
        Result<List<KurumlarKisiler>> KisiIDileListGetir(int kisiID);

        Result<List<KurumOrganizasyonBirimView>> PozisyonlarList(int kurumID);

        /// <summary>
        /// Kişi musteri temsilcisi mi kontrolü sağlayan metot.
        /// </summary>
        /// <param name="kisiId">kişi Id</param>
        /// <returns>sonucu true veya false olarak döndürür.</returns>
        Result<bool> KisiMusteriTemsilcisiMi(int kisiId);

        /// <summary>
        /// Kişi kurum admin mi kontrolü sağlayan metot.
        /// </summary>
        /// <param name="kisiId">kişi Id</param>
        /// <returns>sonucu true veya false olarak döndürür.</returns>
        public Result<bool> KisiKurumAdminMi(int kisiId);
    }

    /// <summary>
    /// Kişilerin bir kurumda hangi organizasyon birimi bünyesinde bulunduklarının (Departman/Rol/Pozisyon/Lokasyon/vs) tanımlandığı KurumlarKisiler tablosuyla ilgili işlemleri barındıran class.
    /// </summary>
    public class KurumlarKisilerService : Service<KurumlarKisiler>, IKurumlarKisilerService
    {
        private readonly IKurumOrganizasyonBirimTanimlariService _kurumOrganizasyonBirimTanimlariService;

        /// <summary>
        /// Kişilerin bir kurumda hangi organizasyon birimi bünyesinde bulunduklarının (Departman/Rol/Pozisyon/Lokasyon/vs) tanımlandığı KurumlarKisiler tablosuyla ilgili işlemleri barındıran classın yapıcı metodu
        /// </summary>
        public KurumlarKisilerService(IRepository<KurumlarKisiler> repository, IDataMapper dataMapper, IKurumOrganizasyonBirimTanimlariService kurumOrganizasyonBirimTanimlariService, IServiceProvider serviceProvider, ILogger<KurumlarKisiler> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
            _kurumOrganizasyonBirimTanimlariService = kurumOrganizasyonBirimTanimlariService;
        }

        /// <summary>
        /// KurumKisiler tablosunda kişi Id'ye göre bilgileri getiren metot
        /// </summary>
        /// <param name="kisiID"></param>
        /// <returns></returns>
        public Result<List<KurumlarKisiler>> KisiIDileListGetir(int kisiID)
        {

            var list = this.List(x => x.IlgiliKisiId == kisiID); 
            return list;
        }

        /// <summary>
        /// Kuruma ait pozisyonları kurumId'ye göre listeleyen method
        /// </summary>
        /// <param name="kurumID"></param>
        /// <returns></returns>
        public Result<List<KurumOrganizasyonBirimView>> PozisyonlarList(int kurumID)
        {
            var pozisyonRequest = new KurumOrganizasyonBirimRequest()
            {
                KurumId = kurumID,
                Name = "pozisyon"
            };
            var pozisyonList = _kurumOrganizasyonBirimTanimlariService.ListTip(pozisyonRequest);
            return pozisyonList;
        }

        /// <summary>
        /// Kişi müşteri temsilcisi mi kontrolü sağlayan metot.
        /// </summary>
        /// <param name="kisiId">kişi Id</param>
        /// <returns>sonucu true veya false olarak döndürür.</returns>
        public Result<bool> KisiMusteriTemsilcisiMi(int kisiId)
        {
            var temsilciOrgTanim = _kurumOrganizasyonBirimTanimlariService.List(a => a.BirimTanim.ToLower().Contains("müşteri temsilcisi") && a.AktifMi == 1).Value.Select(a => a.TabloID);
            var kontrol = List(a => a.IlgiliKisiId == kisiId && a.AktifMi == 1 && temsilciOrgTanim.Contains(a.KurumOrganizasyonBirimTanimId)).Value.Any();
            if (kontrol)
                return true.ToResult();
            return false.ToResult();
        }

        /// <summary>
        /// Kişi kurum admin mi kontrolü sağlayan metot.
        /// </summary>
        /// <param name="kisiId">kişi Id</param>
        /// <returns>sonucu true veya false olarak döndürür.</returns>
        public Result<bool> KisiKurumAdminMi(int kisiId)
        {
            var temsilciOrgTanim = _kurumOrganizasyonBirimTanimlariService.List(a => a.BirimTanim.ToLower().Contains("kurum admin") && a.AktifMi == 1).Value.Select(a => a.TabloID);
            var kontrol =List(a => a.IlgiliKisiId == kisiId && a.AktifMi == 1 && temsilciOrgTanim.Contains(a.KurumOrganizasyonBirimTanimId)).Value.Any();
            if (kontrol)
                return true.ToResult();
            return false.ToResult();
        }
    }
}