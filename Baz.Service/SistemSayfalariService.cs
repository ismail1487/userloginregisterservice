using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.Repository.Pattern;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baz.Service
{
    /// <summary>
    /// Sistem sayfaları servisi için gerekli methodların bulunduğu sınıftır.
    /// </summary>
    public interface ISistemSayfalariService : Base.IService<SistemSayfalari>
    {
    }

    /// <summary>
    /// Sistem sayfaları ile ilgili işlemler yöneten  sınıftır.
    /// </summary>
    public class SistemSayfalariService : Base.Service<SistemSayfalari>, ISistemSayfalariService
    {
        /// <summary>
        /// Sistem sayfaları ile ilgili işlemler yöneten  sınıfın yapıcı metodu
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="dataMapper"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        public SistemSayfalariService(IRepository<SistemSayfalari> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<SistemSayfalariService> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
        }
    }
}