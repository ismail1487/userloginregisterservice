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
    /// Kişinin son login işlemine dair ayrıntıların kaydedilmesini yöneten interface.
    /// </summary>
    public interface ISistemLoginSonDurumService : IService<SistemLoginSonDurum>
    {
        /// <summary>
        /// Kişiye dair son login durumu verilerini ilgili kişiID değerine göre getiren method.
        /// </summary>
        /// <param name="id"> ilgili kişiye ait ID değeri</param>
        /// <returns> Kişiye ait son login verilerini SistemLoginSonDurum modelinde döndürür.</returns>
        Result<SistemLoginSonDurum> LoginOlanKisiIDyeGoreGetir(int id);

        /// <summary>
        /// Başarısız login sayısını sıfırlar
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Result<SistemLoginSonDurum> BasarisizLoginSifirla(int id);
    }

    /// <summary>
    ///  Kişinin son login işlemine dair ayrıntıların kaydedilmesini yöneten sevice class'ı.
    /// </summary>
    public class SistemLoginSonDurumService : Service<SistemLoginSonDurum>, ISistemLoginSonDurumService
    {
        /// <summary>
        ///  Kişinin son login işlemine dair ayrıntıların kaydedilmesini yöneten sevice class'ın yapıcı metodu
        /// </summary>
        public SistemLoginSonDurumService(IRepository<SistemLoginSonDurum> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger<SistemLoginSonDurumService> logger) : base(repository, dataMapper, serviceProvider, logger)
        {
        }

        /// <summary>
        /// Başarısız login sayısını sıfırlar
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Result<SistemLoginSonDurum> BasarisizLoginSifirla(int id)
        {
            var result = List(t => t.LoginOlanKisiId == id).Value.FirstOrDefault();
            result.KacinciBasarisizLogin = 0;
            result.GuncellenmeTarihi = DateTime.Now;
            var upd = Update(result).Value;
            return upd.ToResult();
        }

        /// <summary>
        /// Kişiye dair son login durumu verilerini ilgili kişiID değerine göre getiren method.
        /// </summary>
        /// <param name="id"> ilgili kişiye ait ID değeri</param>
        /// <returns> Kişiye ait son login verilerini SistemLoginSonDurum modelinde döndürür.</returns>
        public Result<SistemLoginSonDurum> LoginOlanKisiIDyeGoreGetir(int id)
        {
            var result = List(x => x.LoginOlanKisiId == id).Value.FirstOrDefault();
            if (result == null)
            {
                return Results.Fail("login verileri getirme başarısız", ResultStatusCode.ReadError);
            }
            return result.ToResult();
        }
    }
}