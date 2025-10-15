using Baz.ProcessResult;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace Baz.UserLoginServiceApi.Handlers
{
    /// <summary>
    /// Model validation handler class'ı.
    /// </summary>
    public class ModelValidationFilter : IActionFilter
    {
        /// <summary>
        /// IActionFilter nesnesinden kalıtılan metot, Action başlamadan önce, model binding sonrasında çalışır ve ModelState.IsValid kontrollerini gerçekleştirir.
        /// </summary>
        /// <param name="context">ActionExecutingContext nesnesi.</param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var err = new Error();
                foreach (var entry in context.ModelState)
                {
                    if (entry.Value.ValidationState != Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid)
                    {
                        err.Metadata.Add(entry.Key, entry.Value.Errors.FirstOrDefault().ErrorMessage);
                    }
                }
                var x = Results.Fail(err);
                var requestObject = new ObjectResult(x);
                requestObject.StatusCode = 200;
                context.Result = requestObject;
                //context.Result = new RedirectToRouteResult(RouteToDictionaryValue());     //properties ve private methods regionları açılırsa bu satır da açılabilir.
            }
        }

        /// <summary>
        /// IActionFilter nesnesinden kalıtılan metot, Action işlemi bittikten sonra çalışır. Şu an kullanılmadığı için içi boş ancak kalıtım nedeniyle eklenmesi gerektiği için buradadır.
        /// </summary>
        /// <param name="context">ActionExecutingContext nesnesi.</param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //something can be done after the action executes
        }

        #region properties

        //public string RedirectToController { get; set; }

        //public string RedirectToAction { get; set; }

        //public string RedirectToPage { get; set; }

        #endregion properties

        #region private methods

        //private RouteValueDictionary RouteToDictionaryValue()
        //{
        //    var dict = new RouteValueDictionary();

        //    if (!string.IsNullOrWhiteSpace(RedirectToPage))
        //    {
        //        dict.Add("page", RedirectToPage);
        //    }
        //    //if RedirectToController & RedirectToAction are set
        //    else
        //    {
        //        dict.Add("controller", RedirectToController);
        //        dict.Add("action", RedirectToAction);
        //    }
        //    return dict;
        //}

        #endregion private methods
    }
}