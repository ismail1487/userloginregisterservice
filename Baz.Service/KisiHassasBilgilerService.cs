using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.Repository.Pattern;
using Baz.SharedSession;
using Microsoft.Extensions.Logging;
using System;

namespace Baz.Service
{
    /// <summary>
    /// KisiHassasBilgileri ile ilgili işlemleti yöneten interface
    /// </summary>
    public interface IKisiHassasBilgilerService : Baz.Service.Base.IService<KisiHassasBilgiler>
    {
    }

    /// <summary>
    /// KisiHassasBilgilerService Tablosu ile ilgili servislerin yazılacağı class
    /// </summary>
    public class KisiHassasBilgilerService : Baz.Service.Base.Service<KisiHassasBilgiler>, IKisiHassasBilgilerService
    {
        /// <summary>
        /// KisiHassasBilgilerService Tablosu ile ilgili servislerin yazılacağı classın yapıcı metodu
        /// </summary>
        public KisiHassasBilgilerService(IRepository<KisiHassasBilgiler> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<KisiTemelBilgiler> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
        }
    }
}