using Baz.Model.Entity.Constants;
using Baz.Model.Entity.ViewModel;
using Baz.ProcessResult;
using Baz.RequestManager.Abstracts;
using Baz.SharedSession;
using System;
using System.Net;

namespace BazWebApp.Services
{
    /// <summary>
    /// Küresel parametreler için oluşturulmuş Interface'dir.
    /// </summary>
    public interface IKureselParametrelerService
    {
        /// <summary>
        /// Session timeout parametresini getiren servis methodudur.
        /// </summary>
        /// <param name="paramTanim">The parameter tanim.</param>
        /// <returns></returns>
        Result<KureselParametreModel> SessionTimeoutParamGetir(string paramTanim = "Session Timeout");

      
    }

    /// <summary>
    /// Küresel parametreler için oluşturulmuş methodların olduğu servis sınıfıdır.
    /// </summary>
    /// <seealso cref="BazWebApp.Services.IKureselParametrelerService" />
    public class KureselParametrelerService : IKureselParametrelerService
    {
        private readonly IRequestHelper _requestHelper;

        /// <summary>
        /// Küresel parametreler için oluşturulmuş methodların olduğu servis sınıfının yapıcı metodu
        /// </summary>
        public KureselParametrelerService(IRequestHelper requestHelper)
        {
            _requestHelper = requestHelper;
        }

        /// <summary>
        /// Session timeout parametresini getiren servis methodudur.
        /// </summary>
        /// <param name="paramTanim">The parameter tanim.</param>
        /// <returns></returns>
        public Result<KureselParametreModel> SessionTimeoutParamGetir(string paramTanim = "Session Timeout")
        {
            var result = _requestHelper.Post<Result<KureselParametreModel>>(LocalPortlar.IYSService + "/api/KureselParametreler/IsmeGoreParamGetir", paramTanim);

            return result.Result;
        }

     
    }
}