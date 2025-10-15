using Baz.AOP.Logger.ExceptionLog;
using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.Model.Entity.ViewModel;
using Baz.ProcessResult;
using Baz.Repository.Pattern;
using Baz.Service.Base;
using Decor;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Baz.Service
{
    /// <summary>
    /// Param Küresel parametreler servisi için gerekli methodların yer aldığı sınıftır.
    /// </summary>
    public interface IParamKureselParametrelerService : IService<ParamKureselParametreler>
    {
        /// <summary>
        ///İsme göre parametre getiren method.
        /// </summary>
        /// <param name="paramTanim">paramTanim</param>
        /// <returns></returns>
        Result<KureselParametreModel> IsmeGoreParamGetir(string paramTanim);
    }

    /// <summary>
    /// ParamKureselParametreler ile ilgili işlemleri yöneten servıs sınıfı
    /// </summary>
    public class ParamKureselParametrelerService : Service<ParamKureselParametreler>, IParamKureselParametrelerService
    {
        /// <summary>
        /// ParamKureselParametreler ile ilgili işlemleri yöneten servıs sınıfının yapıcı metodu
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="dataMapper"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        public ParamKureselParametrelerService(IRepository<ParamKureselParametreler> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<ParamKureselParametrelerService> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
        }

        /// <summary>
        ///İsme göre parametre getiren method.
        /// </summary>
        /// <param name="paramTanim">paramTanim</param>
        /// <returns></returns>
        public Result<KureselParametreModel> IsmeGoreParamGetir(string paramTanim)
        {
            var paramModel = List(p => p.ParamTanim == paramTanim).Value.FirstOrDefault();

            var paramWmodel = new KureselParametreModel()
            {
                KureselParametreId = paramModel.TabloID,
                ParamTanim = paramModel.ParamTanim,
                SistemParamMi = paramModel.SistemParamMi,
            };
            return paramWmodel.ToResult();
        }
    }
}