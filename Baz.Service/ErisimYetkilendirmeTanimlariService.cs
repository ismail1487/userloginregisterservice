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
using System.Text;
using System.Threading.Tasks;

namespace Baz.Service
{
    /// <summary>
    /// Erişim yetkilendirme tanimlarına  ait metotların yer aldığı servis sınıfıdır
    /// </summary>
    public interface IErisimYetkilendirmeTanimlariService : IService<ErisimYetkilendirmeTanimlari>
    {
        /// <summary>
        /// Erişim yetkilendirme listeleme metodu
        /// </summary>
        /// <returns></returns>
        public List<ErisimYetkilendirmeTanimlari> ErisimYetkilendirmeTanimlariListesi();
    }

    /// <summary>
    /// Erişim yetkilendirme işlemlerini yöneten servis sınıfı
    /// </summary>
    public class ErisimYetkilendirmeTanimlariService : Service<ErisimYetkilendirmeTanimlari>, IErisimYetkilendirmeTanimlariService
    {
        /// <summary>
        /// Erişim yetkilendirme işlemlerini yöneten servis sınıfının yapıcı metodu
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="dataMapper"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        public ErisimYetkilendirmeTanimlariService(IRepository<ErisimYetkilendirmeTanimlari> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<ErisimYetkilendirmeTanimlariService> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
        }

        /// <summary>
        /// Erişim yetkilendirme listeleme metodu
        /// </summary>
        /// <returns></returns>
        public List<ErisimYetkilendirmeTanimlari> ErisimYetkilendirmeTanimlariListesi()
        {
            var list = this.List(a => a.AktifMi == 1 && a.SilindiMi == 0).Value;
            return list;
        }
    }
}