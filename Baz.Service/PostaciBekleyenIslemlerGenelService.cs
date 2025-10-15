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
    /// <seealso cref="PostaciBekleyenIslemlerGenel" />
    public interface IPostaciBekleyenIslemlerGenelService : IService<PostaciBekleyenIslemlerGenel>
    {
    }

    /// <summary>
    /// Şifre yenileme ve aktivasyon için gerekli methodların yer aldığı servis Class'dır
    /// </summary>
    public class PostaciBekleyenIslemlerGenelService : Service<PostaciBekleyenIslemlerGenel>, IPostaciBekleyenIslemlerGenelService
    {
        /// <summary>
        /// Şifre yenileme ve aktivasyon için gerekli methodların yer aldığı servis Classın yapıcı metodu
        /// </summary>
        public PostaciBekleyenIslemlerGenelService(IRepository<PostaciBekleyenIslemlerGenel> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<PostaciBekleyenIslemlerGenelService> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
        }
    }
}