using Baz.ProcessResult;
using Baz.Repository.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Baz.Service.Base
{
    /// <summary>
    /// Ekleme,düzenleme,silme listeleme vb işlemlerin yer aldığı interfacedir.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="System.IDisposable" />
    public interface IService<TEntity> : IDisposable where TEntity : class, Baz.Model.Pattern.IBaseModel
    {
        /// <summary>
        /// Ekleme işleminin yapıldığı methodtur.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Result<TEntity> Add(TEntity entity);

        /// <summary>
        /// Düzenleme işleminin yapıldığı methodtur.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Result<TEntity> Update(TEntity entity);

        /// <summary>
        /// Silme işleminin yapıldığı methodtur.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Result<TEntity> Delete(int id);

        /// <summary>
        /// Id'ye göre sonucun döndürüldüğü methodtur.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Result<TEntity> SingleOrDefault(int id);

        /// <summary>
        /// Listeleme yapılan methodtur.
        /// </summary>
        /// <returns></returns>
        Result<List<TEntity>> List();

        /// <summary>
        /// Alınan parametreye göre listelemenin yapıldığı methodtur.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        Result<List<TEntity>> List(Expression<Func<TEntity, bool>> expression);

        /// <summary>
        /// Listeleme yapılan methodtur.
        /// </summary>
        /// <returns></returns>
        IQueryable<TEntity> ListForQuery();

        /// <summary>
        /// Uygulamamız arasında sorgulama, güncelleme, silme gibi işlemleri yapmamız için olanak sağlar.
        /// </summary>
        /// <returns></returns>
        DataContextConfiguration DataContextConfiguration();

        /// <summary>
        /// verileri IQueryable şeklinde listeleyen metot.
        /// </summary>
        /// <returns></returns>
        IQueryable<TEntity> AsQuery();

        /// <summary>
        /// Queryable oalrak Döndürür.
        /// </summary>
        /// <returns></returns>
        IQueryable<TEntity> ListQueryable();

        /// <summary>
        /// Listeleme yapılan methodtur.
        /// </summary>
        /// <returns></returns>
        Result<IQueryable<TEntity>> ListResultQueryable();
    }
}