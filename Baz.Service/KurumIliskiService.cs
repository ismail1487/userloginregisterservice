using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.Model.Entity.Constants;
using Baz.Model.Entity.ViewModel;
using Baz.ProcessResult;
using Baz.Repository.Pattern;
using Baz.RequestManager.Abstracts;
using Baz.Service.Base;
using Microsoft.Extensions.Logging;

namespace Baz.Service
{
    /// <summary>
    /// Kurumlar arası ilişkiye ait metotların yer aldığı servis sınıfıdır
    /// </summary>
    public interface IKurumIliskiService : IService<Iliskiler>
    {
        
        /// <summary>
        /// musteri temsilcisine göre bağlı kurum idleri getiren metod
        /// </summary>
        /// <param name="musteriTemsilciId"></param>
        /// <returns></returns>
        Result<List<int>> MusteriTemsilcisiBagliKurumIdGetir(int musteriTemsilciId);
    }

    /// <summary>
    ///  Kurumlar arası ilişkiye ait ilgili işlemleri yöneten servıs sınıfı
    /// </summary>
    public class KurumIliskiService : Service<Iliskiler>, IKurumIliskiService
    {
        private readonly IRequestHelper _requestHelper;

        /// <summary>
        ///  Kurumlar arası ilişkiye ait ilgili işlemleri yöneten servıs sınıfının yapıcı metodu
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="dataMapper"></param>
        /// <param name="requestHelper"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        public KurumIliskiService(IRepository<Iliskiler> repository, IDataMapper dataMapper, IRequestHelper requestHelper, IServiceProvider serviceProvider, ILogger<KurumIliskiService> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
            _requestHelper = requestHelper;
        }


        /// <summary>
        /// musteri temsilcisine göre bağlı kurum idleri getiren metod
        /// </summary>
        /// <param name="musteriTemsilciId"></param>
        /// <returns></returns>
        public Result<List<int>> MusteriTemsilcisiBagliKurumIdGetir(int musteriTemsilciId)
        {
            var list = List(a => a.BuKisiId == musteriTemsilciId && a.IliskiTuruId == 11 && a.AktifMi == 1).Value.Select(a => Convert.ToInt32(a.BuKurumId)).ToList(); // 11, param iliski turleri tablosunda kayıtlı müsteri temsilcisi ID degeri. kayıt işlemi tamamlansın dinamikleştirilecek.
            return list.ToResult();
        }
    }
}