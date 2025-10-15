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
    public interface ISayfaYetkiGurubuAyrintilarService : IService<SayfaYetkiGurubuAyrintilar>
    {
        
        public List<int> SayfalariGetir(int ParamYetkiGurubuID);
    }

    public class SayfaYetkiGurubuAyrintilarService : Service<SayfaYetkiGurubuAyrintilar>, ISayfaYetkiGurubuAyrintilarService
    {
        private readonly IRequestHelper _requestHelper;

        public SayfaYetkiGurubuAyrintilarService(IRepository<SayfaYetkiGurubuAyrintilar> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<SayfaYetkiGurubuAyrintilarService> logger, IRequestHelper requestHelper) : base(repository, dataMapper, serviceProvider, logger)
        {
            _requestHelper = requestHelper;
        }

        public List<int> SayfalariGetir(int ParamYetkiGurubuID)
        {
            var list = List(a => a.YetkiGurubuId == ParamYetkiGurubuID && a.AktifMi == 1).Value.Select(a => Convert.ToInt32(a.SayfaId)).ToList(); 
            return list;
        }
    }
}