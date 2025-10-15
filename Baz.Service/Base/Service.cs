using Baz.Mapper.Pattern;
using Baz.ProcessResult;
using Baz.Repository.Common;
using Baz.Repository.Pattern;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Baz.Model.Pattern;
using Baz.Model.Entity;

namespace Baz.Service.Base
{
    /// <summary>
    /// Ekleme,düzenleme,silme listeleme vb işlemlerin yer aldığı sınftır.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Baz.Service.Base.IService{TEntity}" />
    public class Service<TEntity> : IService<TEntity> where TEntity : class, Baz.Model.Pattern.IBaseModel
    {
        /// <summary>
        /// Reposiitory değişkeni
        /// </summary>
        protected readonly IRepository<TEntity> _repository;

        /// <summary>
        /// Model mapper
        /// </summary>
        protected IDataMapper _dataMapper;

        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger _logger;

        /// <summary>
        /// Servis collector
        /// </summary>
        protected IServiceProvider _serviceProvider;

        private readonly ILoginUser _loginUser;

        private bool _disposed;

        /// <summary>
        /// Ekleme,düzenleme,silme listeleme vb işlemlerin yer aldığı sınfın yapıcı metodu
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="dataMapper"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        public Service(IRepository<TEntity> repository, IDataMapper dataMapper, IServiceProvider serviceProvider, ILogger logger)
        {
            _repository = repository;
            _dataMapper = dataMapper;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _loginUser = _serviceProvider.GetService<ILoginUser>();
            //#if DEBUG

            //            _loginUser.KisiID = 129;
            //            _loginUser.KurumID = 82;
            //            _loginUser.YetkiliKurumIdleri = new() { 82, 85, 2497, 3448, 3593, 3771 };
            //            _loginUser.YetkiliKisiIdleri = new() { 129, 130, 210, 3603, 395, 401, 4019 };
            //            _loginUser.LisansId = 1031;
            //#endif
        }

        /// <summary>
        /// Ekleme işleminin yapıldığı methodtur.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual Result<TEntity> Add(TEntity entity)
        {
            if (IslemYetkisiVarMi(entity))
            {
                var result = _repository.Add(entity);
                if (_repository.SaveChanges() > 0)
                    return result.ToResult();
            }
            return Results.Fail("Bu işleme yetkiniz yoktur.");
        }

        /// <summary>
        /// Silme işleminin yapıldığı methodtur.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public virtual Result<TEntity> Delete(int id)
        {
            var entity = _repository.SingleOrDefault(id).ToResult();
            if (IslemYetkisiVarMi(entity.Value))
            {
                var result = _repository.Delete(id);
                if (_repository.SaveChanges() > 0)
                    return result.ToResult();
            }
            return Results.Fail("Bu işleme yetkiniz yoktur.");
        }

        /// <summary>
        /// Listeleme yapılan methodtur.
        /// </summary>
        /// <returns></returns>
        public virtual Result<List<TEntity>> List()
        {
            return _repository.List().ToList().ToResult();
        }

        /// <summary>
        /// Alınan parametreye göre listelemenin yapıldığı methodtur.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public virtual Result<List<TEntity>> List(Expression<Func<TEntity, bool>> expression)
        {
            return _repository.List(expression).ToList().ToResult();
        }

        /// <summary>
        /// Id'ye göre sonucun döndürüldüğü methodtur.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public virtual Result<TEntity> SingleOrDefault(int id)
        {
            var entity = _repository.SingleOrDefault(id).ToResult();
            if (IslemYetkisiVarMi(entity.Value))
            {
                return entity;
            }
            return Results.Fail("Bu işleme yetkiniz yoktur.");
        }

        /// <summary>
        /// Düzenleme işleminin yapıldığı methodtur.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual Result<TEntity> Update(TEntity entity)
        {
            if (IslemYetkisiVarMi(entity))
            {
                TEntity result = null;
                int saveResult = 0;
                _repository.DataContextConfiguration().AutoDetectChangesEnable();
                var dbItem = _repository.SingleOrDefault(entity.TabloID);
                if (dbItem != null)
                {
                    result = _dataMapper.Map(dbItem, entity);
                    saveResult = _repository.SaveChanges();
                }
                _repository.DataContextConfiguration().AutoDetectChangesDisable();
                if (saveResult > 0)
                    return result.ToResult();
            }
            return Results.Fail("Bu işleme yetkiniz yoktur.");
        }

        /// <summary>
        /// Kullanılmayan kaynakları boşa çıkardıktan sonra sonucu true veya false döndüren methodtur.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed && disposing)
            {
                _repository.Dispose();
            }
            this._disposed = true;
        }

        /// <summary>
        /// Kullanılmayan kaynakları boşa çıkaran methodtur.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Veritabanı bağlantı ayarlarının yapıldığı method.
        /// </summary>
        /// <returns></returns>
        public DataContextConfiguration DataContextConfiguration()
        {
            return _repository.DataContextConfiguration();
        }

        /// <summary>
        /// Sorguların listesini veren methodtur..
        /// </summary>
        /// <returns></returns>
        public IQueryable<TEntity> ListForQuery()
        {
            return _repository.List();
        }

        /// <summary>
        /// Listeleme yapılan methodtur.
        /// </summary>
        /// <returns></returns>
        public virtual Result<IQueryable<TEntity>> ListResultQueryable()
        {
            return _repository.List().ToResult();
        }

        /// <summary>
        /// Geri dönüş biçimi IQueryable olan listeleme metodu
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<TEntity> ListQueryable()
        {
            return _repository.List();
        }

        /// <summary>
        /// verileri IQueryable şeklinde listeleyen metot.
        /// </summary>
        /// <returns></returns>
        public IQueryable<TEntity> AsQuery()
        {
            return _repository.List();
        }

        [ExcludeFromCodeCoverage]
        private bool IslemYetkisiVarMi(TEntity entity)
        {
            if (entity != null)
            {
                if (_loginUser.KisiID == 0) // Kisi login değilse login işlemi için yetki kontrolü
                    return true;
                var gecicikisiList = _loginUser.YetkiliKisiIdleri;
                var gecicikurumList = _loginUser.YetkiliKurumIdleri;
                if (_loginUser.YetkiliKisiIdleri==null &&_loginUser.KisiID!=0)
                {
                    _loginUser.YetkiliKisiIdleri.Add(_loginUser.KisiID);
                }
                if (_loginUser.YetkiliKurumIdleri == null &&_loginUser.KurumID!=0)
                {
                    _loginUser.YetkiliKurumIdleri.Add(_loginUser.KurumID);
                }
                switch (entity.GetType().Name)
                {
                    case nameof(ErisimYetkilendirmeTanimlari):
                        {
                            var prop = entity.GetType().GetProperty("IlgiliKurumId");
                            if (gecicikurumList.Any(a => a == entity.KurumID)
                                &&
                                gecicikisiList.Any(a => a == entity.KisiID)
                                &&
                                gecicikurumList.Any(a => a == entity.KurumID) && gecicikurumList.Any(a => a == (int)prop.GetValue(entity))
                                )
                            {
                                return true;
                            }
                            return false;
                        }

                    case nameof(KisiHassasBilgiler):
                        {
                            var prop = entity.GetType().GetProperty("KisiTemelBilgiId");
                            if (gecicikurumList.Any(a => a == entity.KurumID))
                            {
                                return true;
                            }
                            return false;
                        }

                    case nameof(Iliskiler):
                        {
                            var propBununKisi = entity.GetType().GetProperty("BununKisiId");
                            var propBuKisi = entity.GetType().GetProperty("BuKisiId");
                            var propBununKurum = entity.GetType().GetProperty("BununKurumId");
                            var propBuKurum = entity.GetType().GetProperty("BuKurumId");
                            if (gecicikurumList.Any(a => a == entity.KurumID)
                                && gecicikisiList.Any(a => a == entity.KisiID)
                                && ((gecicikurumList.Any(a => a == (int?)propBununKurum.GetValue(entity)) && gecicikurumList.Any(a => a == (int?)propBuKurum.GetValue(entity)))
                                || (gecicikisiList.Any(a => a == (int?)propBuKisi.GetValue(entity)) && gecicikisiList.Any(a => a == (int?)propBununKisi.GetValue(entity)))
                                || (gecicikisiList.Any(a => a == (int?)propBuKisi.GetValue(entity)) && gecicikurumList.Any(a => a == (int?)propBuKurum.GetValue(entity))))
                                )
                            {
                                return true;
                            }
                            return false;
                        }

                    case nameof(KisiTemelBilgiler):
                        {
                            var propKurum = entity.GetType().GetProperty("KisiBagliOlduguKurumId");
                            if (gecicikurumList.Any(a => a == entity.KurumID)
                                && gecicikurumList.Any(a => a == (int?)propKurum.GetValue(entity)))
                            {
                                return true;
                            }
                            return false;
                        }

                    case nameof(KurumlarKisiler):
                        {
                            if (gecicikurumList.Any(a => a == entity.KurumID))
                            {
                                return true;
                            }
                            return false;
                        }

                    case nameof(KurumOrganizasyonBirimTanimlari):
                        {
                            var propKurum = entity.GetType().GetProperty("IlgiliKurumID");
                            if (gecicikurumList.Any(a => a == entity.KurumID) && gecicikurumList.Any(a => a == (int)propKurum.GetValue(entity)) && gecicikisiList.Any(a => a == entity.KisiID))
                            {
                                return true;
                            }
                            return false;
                        }

                    case nameof(KurumTemelBilgiler):
                        {
                            if (gecicikurumList.Any(a => a == entity.KurumID))
                            {
                                return true;
                            }
                            return false;
                        }

                    //case nameof(LisansKurumKisiAbonelikTanimlari):
                    //    {
                    //        var propKisi = entity.GetType().GetProperty("LisansAboneKurumId");
                    //        var propKurum = entity.GetType().GetProperty("LisansAboneKisiId");
                    //        if (gecicikurumList.Any(a => a == entity.KurumID) && gecicikisiList.Any(a => a == (int?)propKisi.GetValue(entity)) && gecicikurumList.Any(a => a == (int?)propKurum.GetValue(entity)) && gecicikisiList.Any(a => a == entity.KisiID))
                    //        {
                    //            return true;
                    //        }
                    //        return false;
                    //    }
                    case nameof(ModulDetay):
                    //case nameof(Lisans):
                    //    {
                    //        if (_loginUser.LisansId == 1031)
                    //        {
                    //            return true;
                    //        }
                    //        return false;
                    //    }

                    //case nameof(ParamKureselParametreler):
                    //    {
                    //        if (_loginUser.LisansId == 1031)
                    //        {
                    //            return true;
                    //        }
                    //        return false;
                    //    }

                    //case nameof(ParamOrganizasyonBirimleri):
                    //    {
                    //        if (_loginUser.LisansId == 1031)
                    //        {
                    //            return true;
                    //        }
                    //        return false;
                    //    }

                    

                    case nameof(SistemLoginSonDurum):
                    case nameof(SistemLoginTarihce):
                    case nameof(SistemLoginCariDurum):
                        {
                            var propKisi = entity.GetType().GetProperty("LoginOlanKisiId");
                            if (gecicikisiList.Any(a => a == (int?)propKisi.GetValue(entity)))
                            {
                                return true;
                            }
                            return false;
                        }

                    case nameof(SistemLoginSifreYenilemeAktivasyonHareketleri):
                        {
                            var propKisi = entity.GetType().GetProperty("SifreYenilemeAktivasyonLinkiYollananKisiId");
                            if (
                                gecicikurumList.Any(a => a == entity.KurumID)
                                &&
                                gecicikisiList.Any(a => a == entity.KisiID)
                                &&
                                gecicikisiList.Any(a => a == (int?)propKisi.GetValue(entity)))
                            {
                                return true;
                            }
                            return false;
                        }

                    //case nameof(SistemLoginTarihce):
                    //    {
                    //        var propKisi = entity.GetType().GetProperty("LoginOlanKisiId");
                    //        if (
                    //            //(gecicikurumList.Any(a => a == entity.KurumID)
                    //            //&&
                    //            //gecicikisiList.Any(a => a == entity.KisiID)
                    //            //&&
                    //            gecicikisiList.Any(a => a == (int?)propKisi.GetValue(entity)))
                    //        {
                    //            return true;
                    //        }
                    //        return false;
                    //    }

                    default:
                        return true;
                }
            }
            return true;
        }
    }
}