using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.Repository.Pattern;
using Baz.Service.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Baz.AOP.Logger.ExceptionLog;
using Baz.Model.Entity.Constants;
using Baz.Model.Entity.ViewModel;
using Baz.Model.Pattern;
using Baz.ProcessResult;
using Baz.RequestManager.Abstracts;
using BazWebApp.Services;
using Decor;
using Microsoft.Extensions.DependencyInjection;

namespace Baz.Service
{
    /// <summary>
    /// KisiService ile kişilere dair işlemlerin yönetileceği interface.
    /// </summary>
    public interface IKisiService : IService<KisiTemelBilgiler>
    {
        /// <summary>
        /// kisi Id ile müsteri temsilcileri getirme
        /// </summary>
        /// <param name="kisiId"></param>
        /// <returns></returns>
        Result<List<int>> AmireBagliMusteriTemsilcileriList(int kisiId);

        /// <summary>
        /// Kurum organizasyon birimlerine göre kişinin astlarını getiren method
        /// </summary>
        /// <param name="kisiID"> astları getirilecek kişiId</param>
        /// <returns>kişinin astları listesi.</returns>
        Result<List<KisiOrganizasyonBirimView>> KisiAstlarListGetir(int kisiID);

        /// <summary>
        /// Sistemdeki tüm üyeleri getirir
        /// </summary>
        /// <returns></returns>
        Result<List<UyeListViewModel>> UyeleriGetir();
    }

    /// <summary>
    /// KisiService ile kişilere dair işlemlerin yönetileceği servis classı.
    /// IKisiService interface'ini ve Service class'ını baz alır.
    /// </summary>
    public class KisiService : Service<KisiTemelBilgiler>, IKisiService
    {
        private readonly IKurumlarKisilerService _kurumlarKisilerService;

        /// <summary>
        /// KisiService ile kişilere dair işlemlerin yönetileceği servis classının yapıcı metodu
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="dataMapper"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="kurumlarKisilerService"></param>
        public KisiService(IRepository<KisiTemelBilgiler> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<KisiService> logger, IKurumlarKisilerService kurumlarKisilerService) : base(repository, dataMapper, serviceProvider, logger)
        {
            _kurumlarKisilerService = kurumlarKisilerService;
        }

        /// <summary>
        /// kisi Id ile müsteri temsilcileri getirme
        /// </summary>
        /// <param name="kisiId"></param>
        /// <returns></returns>
        public Result<List<int>> AmireBagliMusteriTemsilcileriList(int kisiId)
        {
            var _kurumlarKisilerService = _serviceProvider.GetService<IKurumlarKisilerService>();
            List<int> result = new();
            var astlar = KisiAstlarListGetir(kisiId);
            foreach (var ast in astlar.Value)
            {
                var kontrol = _kurumlarKisilerService.KisiMusteriTemsilcisiMi(ast.KisiId);
                if (kontrol.Value)
                    result.Add(ast.KisiId);
            }
            return result.ToResult();
        }

        /// <summary>
        /// Kurum organizasyon birimlerine göre kişinin astlarını getiren method
        /// </summary>
        /// <param name="kisiID"> astları getirilecek kişiId</param>
        /// <returns>kişinin astları listesi.</returns>

        public Result<List<KisiOrganizasyonBirimView>> KisiAstlarListGetir(int kisiID)
        {
            List<KisiOrganizasyonBirimView> altbirimler = new();
            Aslar(kisiID, altbirimler);
            return altbirimler.ToResult();
        }

        /// <summary>
        /// Sistemdeki tüm üyeleri getirir
        /// </summary>
        /// <returns></returns>
        public Result<List<UyeListViewModel>> UyeleriGetir()
        {
            var _kisiHassaService = _serviceProvider.GetService<IKisiHassasBilgilerService>();

            //var response = from lisans in _lisansService.ListResultQueryable().Value
            //               from kurumLisans in _kurumLisansService.ListResultQueryable().Value
            //               from abonelik in _lisansKisiAbonelikService.ListQueryable()
            //               from kisiHassas in _kisiHassaService.ListQueryable()
            //               from kisi in ListResultQueryable().Value
            //               where kisi.TabloID == kisiHassas.KisiTemelBilgiId &&
            //                     kisi.TabloID == abonelik.LisansAboneKisiId &&
            //                     abonelik.LisansGenelTanimId == kurumLisans.TabloID &&
            //                     kurumLisans.LisansId == lisans.TabloID


            var response = from kisiHassas in _kisiHassaService.ListQueryable()
                           from kisi in ListResultQueryable().Value
                           where kisi.TabloID == kisiHassas.KisiTemelBilgiId

                           select new UyeListViewModel
                           {
                               KisiTabloId = kisi.TabloID,
                               Ad = kisi.KisiAdi,
                               Soyad = kisi.KisiSoyadi,
                               Eposta = kisi.KisiEposta,
                               Aktiflik = kisiHassas.HesabiAktifMi == true ? "Aktif" : "Pasif",
                               UyeTarihi = kisi.KayitTarihi,
                           };
            return response.ToList().ToResult();
        }

        /// <summary>
        /// Kurum organizasyon birimlerine göre kişinin astlarının astlarını getiren method
        /// </summary>
        /// <param name="kisiID"> astları getirilecek kişiId</param>
        /// <param name="altbirimler"></param>
        /// <returns>kişinin astları listesi.</returns>
        private void Aslar(int kisiID, List<KisiOrganizasyonBirimView> altbirimler)
        {
            var _kurumlarKisilerService = _serviceProvider.GetService<IKurumlarKisilerService>();

            var kisiBilgileri = this.SingleOrDefault(kisiID).Value;
            if (kisiBilgileri == null)
            {
                return;
            }
            var kurumPozisyonlari = _kurumlarKisilerService.PozisyonlarList(kisiBilgileri.KurumID).Value;
            if (kurumPozisyonlari == null)
            {
                return;
            }
            var kisiPozisyonTanimId = _kurumlarKisilerService.List(s => s.IlgiliKisiId == kisiID && s.AktifMi == 1 && s.KurumOrganizasyonBirimTanimId != 0).Value.Select(a => a.KurumOrganizasyonBirimTanimId).ToList();
            if (kisiPozisyonTanimId == null)
            {
                return;
            }
            //Kişinin organizasyon ağacındaki pozisyona göre altındaki roldeki kişileri getirir.
            foreach (var item in kurumPozisyonlari.Where(a => kisiPozisyonTanimId.Contains(a.UstId)))
            {
                var altKisiPozisyonlar = _kurumlarKisilerService.List(w => w.KurumOrganizasyonBirimTanimId == item.TabloId && w.AktifMi == 1 && !altbirimler.Select(a => a.KisiId).Contains(w.IlgiliKisiId)).Value;
                foreach (var altKisiPozisyon in altKisiPozisyonlar)
                {
                    if (altKisiPozisyon != null)
                    {
                        if (altKisiPozisyon.IlgiliKisiId == kisiID)
                            break;
                        var kisiTemelBilgi = this.SingleOrDefault(altKisiPozisyon.IlgiliKisiId).Value;
                        if (kisiTemelBilgi != null)
                        {
                            var altbirim = new KisiOrganizasyonBirimView()
                            {
                                KisiAdi = kisiTemelBilgi.KisiAdi,
                                KisiSoyadi = kisiTemelBilgi.KisiSoyadi,
                                KurumId = kisiTemelBilgi.KurumID,
                                TipId = item.TabloId,
                                Tanim = item.Tanim,
                                UstId = item.UstId,
                                KisiId = kisiTemelBilgi.TabloID,
                                KisiUstler = new List<KisiOrganizasyonBirimView>(),
                                KisiAstlar = new()
                            };
                            altbirimler.Add(altbirim);
                            Aslar(kisiTemelBilgi.TabloID, altbirimler);
                        }
                    }
                }
            }
        }
    }
}