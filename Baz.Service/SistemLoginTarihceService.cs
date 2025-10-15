using Baz.Mapper.Pattern;
using Baz.Model.Entity;
using Baz.ProcessResult;
using Baz.Repository.Pattern;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Baz.Service.Base;

namespace Baz.Service
{
    /// <summary>
    /// Kişilerin login ve logout işlemlerinin tarihçesini yöneten methodları barındıran service interface'i.
    /// </summary>
    public interface ISistemLoginTarihceService : IService<SistemLoginTarihce>
    {
        /// <summary>
        /// kişiye ait ID değeri ile son kaydı getiren method.
        /// </summary>
        /// <param name="id">kişiye ait ID değeri.</param>
        /// <returns> SistemLoginTarihçe türünde model döndürür.</returns>
        Result<SistemLoginTarihce> SonKaydiGetir(int id);

        /// <summary>
        /// Son login işleminde oluşturulan sessşonId değeri ile ilgili kaydı getirip, logout işlemi sonrası logout zamanını ilgili kayda yazan method.
        /// </summary>
        /// <param name="token"> Kişinin son login işleminde oluşturulan sessşonId değeri</param>
        /// <returns> SistemLoginTarihçe türünde model döndürür.</returns>
        Result<SistemLoginTarihce> LogoutZamaniAta(string token);
    }

    /// <summary>
    /// Kişilerin login ve logout işlemlerinin tarihçesini yöneten methodları barındıran service class'ı.
    /// </summary>
    public class SistemLoginTarihceService : Service<SistemLoginTarihce>, ISistemLoginTarihceService
    {
        /// <summary>
        /// Kişilerin login ve logout işlemlerinin tarihçesini yöneten methodları barındıran service class'ın yapıcı metodu
        /// </summary>
        public SistemLoginTarihceService(IRepository<SistemLoginTarihce> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<SistemLoginTarihceService> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
        }

        /// <summary>
        /// kişiye ait ID değeri ile son kaydı getiren method.
        /// </summary>
        /// <param name="id">kişiye ait ID değeri.</param>
        /// <returns> SistemLoginTarihçe türünde model döndürür.</returns>
        public Result<SistemLoginTarihce> SonKaydiGetir(int id)
        {
            var result = this.List().Value.Where(x
                                 => x.LoginOlanKisiId == id)
                             .OrderByDescending(x => x.TabloID)
                             .Take(2).ToList();
            if (result.Count == 0)
                return Results.Fail("son login zamanı bulunamadı.", ResultStatusCode.ReadError);
            if (result.Count == 1)
                return result.FirstOrDefault().ToResult();
            return result[1].ToResult();
        }

        /// <summary>
        /// Son login işleminde oluşturulan sessşonId değeri ile ilgili kaydı getirip, logout işlemi sonrası logout zamanını ilgili kayda yazan method.
        /// </summary>
        /// <param name="token"> Kişinin son login işleminde oluşturulan sessşonId değeri</param>
        /// <returns> SistemLoginTarihçe türünde model döndürür.</returns>
        public Result<SistemLoginTarihce> LogoutZamaniAta(string token)
        {
            var kayitModel = List(x => x.LoginSistemToken == token || x.LoginFirebaseToken == token).Value.FirstOrDefault();
            if (kayitModel == null)
                return Results.Fail("logout zamanı atama işlemi başarısız.", ResultStatusCode.UpdateError);
            kayitModel.LogoutZamani = DateTime.Now;
            var returnModel = Update(kayitModel);
            return returnModel;
        }
    }
}