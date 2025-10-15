using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.Model.Entity.Constants;
using Baz.Model.Entity.ViewModel;
using Baz.Model.Pattern;
using Baz.ProcessResult;
using Baz.Repository.Pattern;
using Baz.RequestManager.Abstracts;
using Baz.Service.Base;
using Microsoft.Extensions.Logging;

namespace Baz.Service
{
    /// <summary>
    /// Kişiler arası ilişkiye ait metotların yer aldığı interface
    /// </summary>
    public interface IKisiIliskiService : IService<Iliskiler>
    {
        /// <summary>
        /// müşteri temsilcisine baplı kurumları listeleme
        /// </summary>
        /// <param name="musteriTemsilciId"></param>
        /// <returns></returns>
        public Result<List<int>> MusteriTemsilcisiBagliKurumGetir(int musteriTemsilciId);
    }

    /// <summary>
    /// Kişiler arası ilişkiye ait metotların yer aldığı servis sınıfıdır
    /// </summary>
    public class KisiIliskiService : Service<Iliskiler>, IKisiIliskiService
    {
        private readonly ILoginUser _loginUser;
        private readonly IRequestHelper _requestHelper;

        /// <summary>
        /// işiler arası ilişkiye ait metotların yer aldığı servis sınıfının yapıcı metodu
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="dataMapper"></param>
        /// <param name="loginUser"></param>
        /// <param name="requestHelper"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        public KisiIliskiService(IRepository<Iliskiler> repository, IDataMapper dataMapper, ILoginUser loginUser, IRequestHelper requestHelper, IServiceProvider serviceProvider, ILogger<KisiIliskiService> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
            _loginUser = loginUser;
            _requestHelper = requestHelper;
        }

        /// <summary>
        /// müşteri temsilcisine baplı kurumları listeleme
        /// </summary>
        /// <param name="musteriTemsilciId"></param>
        /// <returns></returns>
        public Result<List<int>> MusteriTemsilcisiBagliKurumGetir(int musteriTemsilciId)
        {
            var list = List(a => a.BuKisiId == musteriTemsilciId && a.IliskiTuruId == 11 && a.AktifMi == 1).Value.Select(a => Convert.ToInt32(a.BuKurumId)).ToList(); // 11, param iliski turleri tablosunda kayıtlı müsteri temsilcisi ID degeri. kayıt işlemi tamamlansın dinamikleştirilecek.
            return list.ToResult();
        }
    }
}