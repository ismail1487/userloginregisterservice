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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Baz.Service
{
    /// <summary>
    /// ModulService servisi arayüzü
    /// </summary>
    public interface IModulService : IService<Modul>
    {
        /// <summary>
        /// ismine göre modul detayı getiren metot
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Result<ModulDetayKayitModel> IsmeGoreModulGetir(string name);
    }

    /// <summary>
    /// ModulService servisi sınıfı
    /// </summary>
    public class ModulService : Service<Modul>, IModulService
    {
        private readonly IRequestHelper _requestHelper;

        /// <summary>
        /// ModulService servisi yapıcı metodu.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="dataMapper"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        public ModulService(IRepository<Modul> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<ModulService> logger, IRequestHelper requestHelper) : base(repository, dataMapper, serviceProvider, logger)
        {
            _requestHelper = requestHelper;
        }

        /// <summary>
        /// ismine göre modul detayı getiren metot
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //[HttpGet]
        //[Route("IsmeGoreModulGetir/{name}")]
        public Result<ModulDetayKayitModel> IsmeGoreModulGetir(string name)
        {
            var result = _requestHelper.Get<Result<ModulDetayKayitModel>>(LocalPortlar.LisansServis + "/api/Modul/IsmeGoreModulGetir/" + name);
            return result.Result;
        }
    }
}