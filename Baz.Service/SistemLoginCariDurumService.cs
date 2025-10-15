using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.ProcessResult;
using Baz.Repository.Pattern;
using Baz.Service.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Baz.Service
{
    /// <summary>
    /// Login ve logout işlemleri sonrasında SistemLoginCariDurum tablosuna kayıt eklenmesi/güncellenmesi işlemlerini gerçekleştiren methodları barındıran service interface'i.
    /// </summary>
    public interface ISistemLoginCariDurumService : IService<SistemLoginCariDurum>
    {
        /// <summary>
        /// Logout işlemi sonrasında  ilgili kaydı sistem token'i ile bulup SistemLoginCariDurum tablosundan silen method.
        /// </summary>
        /// <param name="token">Silinecek kaydın sessionId değeri</param>
        /// <returns>SistemLoginCariDurum türünde model döndürür.</returns>
        Result<SistemLoginCariDurum> TokenIleKaydiSil(string token);
    }

    /// <summary>
    /// Login ve logout işlemleri sonrasında SistemLoginCariDurum tablosuna kayıt eklenmesi/güncellenmesi işlemlerini gerçekleştiren methodları barındıran service class'ı.
    /// </summary>
    public class SistemLoginCariDurumService : Service<SistemLoginCariDurum>, ISistemLoginCariDurumService
    {
        /// <summary>
        /// Login ve logout işlemleri sonrasında SistemLoginCariDurum tablosuna kayıt eklenmesi/güncellenmesi işlemlerini gerçekleştiren methodları barındıran service class'ının yapıcı metodu
        /// </summary>
        public SistemLoginCariDurumService(IRepository<SistemLoginCariDurum> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<SistemLoginCariDurumService> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
        }

        /// <summary>
        /// Logout işlemi sonrasında  ilgili kaydı sistem token'i ile bulup SistemLoginCariDurum tablosundan silen method.
        /// </summary>
        /// <param name="token">Silinecek kaydın sessionId değeri</param>
        /// <returns>SistemLoginCariDurum türünde model döndürür.</returns>
        public Result<SistemLoginCariDurum> TokenIleKaydiSil(string token)
        {
            var kayitModel = List(x => x.LoginSistemToken == token || x.LoginDisSistemToken == token).Value.FirstOrDefault();
            //kayitModel.SilindiMi = 1
            //kayitModel.SilinmeTarihi = DateTime.Now
            if (kayitModel == null)
                return Results.Fail("logout kaydı oluşturma başarısız", ResultStatusCode.DeleteError);
            Delete(kayitModel.TabloID);
            return kayitModel.ToResult();
        }
    }
}