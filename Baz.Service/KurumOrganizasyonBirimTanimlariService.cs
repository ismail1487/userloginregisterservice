using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.Model.Entity.Constants;
using Baz.Model.Entity.ViewModel;
using Baz.ProcessResult;
using Baz.Repository.Pattern;
using Baz.RequestManager.Abstracts;
using Baz.Service.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Baz.Service
{
    /// <summary>
    /// Kurum Organizasyon Birim Tanımları için oluşturulmuş Interfacedir.
    /// </summary>
    public interface IKurumOrganizasyonBirimTanimlariService : IService<KurumOrganizasyonBirimTanimlari>
    {
        /// <summary>
        /// Erişim yetkilendirme tanımlarını kaydedilmesi için KurumService API'ına ileten method.
        /// </summary>
        /// <param name="list">kaydedilecek tanımlar listesi.</param>
        /// <returns></returns>
        public Result<List<ErisimYetkilendirmeTanimlari>> ErisimYetkilendirmeTanimlariKaydet(List<ErisimYetkilendirmeTanimlari> list);

        public Result<List<KurumOrganizasyonBirimView>> ListTip(KurumOrganizasyonBirimRequest request);
    }

    /// <summary>
    /// Kurumların alt birimlerinin (Departman/Pozisyon/Rol/Lokasyon/Takım/vs) istendiği şekilde tanımlanabildiği KurumOrganizasyonBirimTanimlari tablosuyla ilgili işlemleri barındıran class.
    /// </summary>
    public class KurumOrganizasyonBirimTanimlariService : Service<KurumOrganizasyonBirimTanimlari>, IKurumOrganizasyonBirimTanimlariService
    {
        private readonly IParamOrganizasyonBirimleriService _paramOrg;
        private readonly IRequestHelper _requestHelper;

        /// <summary>
        /// Kurumların alt birimlerinin (Departman/Pozisyon/Rol/Lokasyon/Takım/vs) istendiği şekilde tanımlanabildiği KurumOrganizasyonBirimTanimlari tablosuyla ilgili işlemleri barındıran classın yapıcı metodu
        /// </summary>
        public KurumOrganizasyonBirimTanimlariService(IRepository<KurumOrganizasyonBirimTanimlari> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<KurumOrganizasyonBirimTanimlariService> logger, IParamOrganizasyonBirimleriService paramOrganizasyonBirimleriService, IRequestHelper requestHelper) : base(repository, dataMapper, serviceProvider, logger)
        {
            _requestHelper = requestHelper;
            _paramOrg = paramOrganizasyonBirimleriService;
        }

        /// <summary>
        /// Erişim yetkilendirme tanımlarını kaydedilmesi için KurumService API'ına ileten method.
        /// </summary>
        /// <param name="list">kaydedilecek tanımlar listesi.</param>
        /// <returns></returns>
        public Result<List<ErisimYetkilendirmeTanimlari>> ErisimYetkilendirmeTanimlariKaydet(List<ErisimYetkilendirmeTanimlari> list)
        {
            var result = _requestHelper.Post<Result<List<ErisimYetkilendirmeTanimlari>>>(LocalPortlar.KurumService + "/api/YetkiMerkezi/ErisimYetkilendirmeTanimlariKaydet", list);
            return result.Result;
        }

        /// <summary>
        /// KurumId ve Name e göre listeleme yapan method.
        /// </summary>
        /// <param name="request"></param>
        public Result<List<KurumOrganizasyonBirimView>> ListTip(KurumOrganizasyonBirimRequest request)
        {
            var r = _paramOrg.GetTipId(request.Name);
            int departmanId = r.Value;
            var result = List(p => p.IlgiliKurumId == request.KurumId && p.OrganizasyonBirimTipiId == departmanId && p.AktifMi == 1 && p.SilindiMi == 0).Value.Select(p => new KurumOrganizasyonBirimView()
            {
                IlgiliKurumID = request.IlgiliKurumID,
                Tanim = p.BirimTanim,
                TabloId = p.TabloID,
                TipId = departmanId,
                KurumId = request.KurumId,
                UstId = p.UstId.Value,
                Koordinat = p.Koordinat
            }).ToList();

            var yeniList = result.ToResult().Value.OrderBy(x => x.Tanim).ToList();

            return yeniList.ToResult();
        }
    }
}