using Baz.RequestManager;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Baz.UserLoginServiceApi.Helper
{
    /// <summary>
    /// bir apiden başka bir apiye istek göndermek için oluşturulan sınıf
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RequestManagerHeaderHelperForHttp
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// bir apiden başka bir apiye istek göndermek için oluşturulan sınıf konst.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public RequestManagerHeaderHelperForHttp(IServiceProvider serviceProvider)
        {
            _httpContextAccessor = (IHttpContextAccessor)serviceProvider.GetService(typeof(IHttpContextAccessor));
        }

        /// <summary>
        /// request helper header gönderimi için gerekli metod
        /// </summary>
        /// <returns></returns>
        public RequestHelperHeader SetDefaultHeader()
        {
            var headers = new RequestHelperHeader();
            if (_httpContextAccessor.HttpContext.Request.Headers["sessionid"].Any())
            {
                var sessionId = _httpContextAccessor.HttpContext.Request.Headers["sessionid"][0];
                if (!string.IsNullOrEmpty(sessionId))
                {
                    headers.Add("sessionId", sessionId);
                }
            }

            return headers;
        }
    }
}