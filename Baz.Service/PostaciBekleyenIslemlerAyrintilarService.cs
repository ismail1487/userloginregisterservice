using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.Repository.Pattern;
using Baz.Service.Base;
using Microsoft.Extensions.Logging;
using System;

namespace Baz.Service
{
    /// <summary>
    /// Mail işlemleri bu talimat tablolarından takip edilecekler.
    /// </summary>
    /// <seealso cref="PostaciBekleyenIslemlerAyrintilar" />
    public interface IPostaciBekleyenIslemlerAyrintilarService : IService<PostaciBekleyenIslemlerAyrintilar>
    {
    }

    /// <summary>
    /// Şifre yenileme ve aktivasyon için gerekli methodların yer aldığı servis Class'dır
    /// </summary>
    public class PostaciBekleyenIslemlerAyrintilarService : Service<PostaciBekleyenIslemlerAyrintilar>, IPostaciBekleyenIslemlerAyrintilarService
    {
        /// <summary>
        /// Şifre yenileme ve aktivasyon için gerekli methodların yer aldığı servis Classın yapıcı metodu
        /// </summary>
        public PostaciBekleyenIslemlerAyrintilarService(IRepository<PostaciBekleyenIslemlerAyrintilar> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<PostaciBekleyenIslemlerAyrintilarService> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
        }
    }
}