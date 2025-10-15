using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.Model.Entity.ViewModel;
using Baz.ProcessResult;
using Baz.Repository.Pattern;
using Baz.Service.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Baz.Service
{
    /// <summary>
    /// ModulDetayService servisi arayüzü
    /// </summary>
    public interface IModulDetayService : IService<ModulDetay>
    {
      
    }

    /// <summary>
    /// ModulDetayService servisi sınıfı
    /// </summary>
    public class ModulDetayService : Service<ModulDetay>, IModulDetayService
    {
        /// <summary>
        /// ModulDetayService servisi yapıcı metodu
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="dataMapper"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        public ModulDetayService(IRepository<ModulDetay> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<ModulDetayService> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
        }

      
    }
}