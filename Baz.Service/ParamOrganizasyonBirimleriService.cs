using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.Model.Entity.ViewModel;
using Baz.ProcessResult;
using Baz.Repository.Pattern;
using Baz.Service.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Baz.Service
{
    /// <summary>
    /// Kurum Organizasyon birimleri parametrelerini tutan ParamOrganizasyonBirimleri tablosuyla ilgili işlemleri barındıran Interface'dir.
    /// </summary>
    public interface IParamOrganizasyonBirimleriService : IService<ParamOrganizasyonBirimleri>
    {
        /// <summary>
        /// Parametre tanımına göre TipId döndüren method
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Result<int> GetTipId(string name);
    }

    /// <summary>
    /// Kurum Organizasyon birimleri parametrelerini tutan ParamOrganizasyonBirimleri tablosuyla ilgili işlemleri barındıran Class'dır.
    /// </summary>

    public class ParamOrganizasyonBirimleriService : Service<ParamOrganizasyonBirimleri>, IParamOrganizasyonBirimleriService
    {
        /// <summary>
        /// Kurum Organizasyon birimleri parametrelerini tutan ParamOrganizasyonBirimleri tablosuyla ilgili işlemleri barındıran Class'ının yapıcı metodu
        /// </summary>
        public ParamOrganizasyonBirimleriService(IRepository<ParamOrganizasyonBirimleri> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<ParamOrganizasyonBirimleri> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
        }


        /// <summary>
        /// Parametre tanımına göre TipId döndüren method
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Result<int> GetTipId(string name)
        {
            var result = List(p => p.ParamTanim.ToLower() == name.ToLower()).Value.FirstOrDefault();
            return result.TabloID.ToResult();
        }
    }
}