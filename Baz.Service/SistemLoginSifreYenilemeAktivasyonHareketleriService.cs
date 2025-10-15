using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.Repository.Pattern;
using Baz.Service.Base;
using Microsoft.Extensions.Logging;
using System;

namespace Baz.Service
{
    /// <summary>
    /// Şifre yenileme ve aktivasyon için gerekli methodların yer aldığı servis Interface'dir.
    /// </summary>
    /// <seealso cref="SistemLoginSifreYenilemeAktivasyonHareketleri" />
    public interface ISistemLoginSifreYenilemeAktivasyonHareketleriService : IService<SistemLoginSifreYenilemeAktivasyonHareketleri>
    {
    }

    /// <summary>
    /// Şifre yenileme ve aktivasyon için gerekli methodların yer aldığı servis Class'dır
    /// </summary>
    public class SistemLoginSifreYenilemeAktivasyonHareketleriService : Service<SistemLoginSifreYenilemeAktivasyonHareketleri>, ISistemLoginSifreYenilemeAktivasyonHareketleriService
    {
        /// <summary>
        /// Şifre yenileme ve aktivasyon için gerekli methodların yer aldığı servis Classın yapıcı metodu
        /// </summary>
        public SistemLoginSifreYenilemeAktivasyonHareketleriService(IRepository<SistemLoginSifreYenilemeAktivasyonHareketleri> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<SistemLoginSifreYenilemeAktivasyonHareketleriService> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
        }
    }
}