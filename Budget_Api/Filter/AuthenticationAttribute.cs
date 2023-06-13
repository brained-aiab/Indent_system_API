using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using DAL;
using System.Web.Http.Controllers;
using System.Web.Http;
using System.IdentityModel.Tokens.Jwt;

namespace Budget_Api.Filter
{
   
    //    public class AuthenticationAttribute : AuthorizationFilterAttribute
    //    {
    //        public override void OnAuthorization(HttpActionContext filterContext)
    //        {
    //            if (SkipAuthorization(filterContext)) return;

    //            //string authorizedToken = string.Empty;
    //            //string userAgent = string.Empty;
    //            //string[] header = HttpContext.Current.Request.Headers.AllKeys;
    //             var req = Request;
    //            var headers = req.Headers;
    //            string token = headers.GetValues("Authorization").First();
    //            var handler = new JwtSecurityTokenHandler();
    //            string authHeader = token;
    //            authHeader = authHeader.Replace("Bearer ", "");
    //            authHeader = authHeader.Replace("\"", "");
    //            var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
    //            objEntity.CreatedBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;

    //            if (header.Contains("AuthToken"))
    //                authorizedToken = HttpContext.Current.Request.Headers["AuthToken"].ToString();

    //            if (header.Contains("UserId"))
    //                userAgent = HttpContext.Current.Request.Headers["UserId"].ToString();

    //            try
    //            {
    //                if (!string.IsNullOrEmpty(authorizedToken))
    //                {
    //                    if (!IsAuthorizedToken(userAgent, authorizedToken))
    //                    {
    //                        filterContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
    //                        return;
    //                    }
    //                }
    //                else
    //                {
    //                    filterContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
    //                    return;
    //                }
    //            }
    //            catch (Exception)
    //            {
    //                filterContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
    //                return;
    //            }
    //            base.OnAuthorization(filterContext);
    //        }

    //        /// <summary>
    //        /// To validate authentication token
    //        /// </summary>
    //        /// <param name="UserId"></param>
    //        /// <param name="token"></param>
    //        /// <returns></returns>
    //        public bool IsAuthorizedToken(string UserId, string token)
    //        {
    //            string action = "VERIFY";
    //            bool IsAuthorized = false;

    //             BudgetDal objDal = new BudgetDal();
    //            DataSet resultDS = new DataSet();

    //            resultDS = objDal.InsertTokenByUserId(UserId, token, action);
    //            if (resultDS != null && resultDS.Tables.Count > 0 && resultDS.Tables[0].Rows.Count > 0)
    //            {
    //                token = Convert.ToString(resultDS.Tables[0].Rows[0]["AuthToken"]);
    //                if (!string.IsNullOrEmpty(token))
    //                    IsAuthorized = true;
    //            }
    //            return IsAuthorized;
    //        }

    //        /// <summary>
    //        /// Skip authorization for anonymous user.
    //        /// [AllowAnonymous] is used to allow any user to perform action.
    //        /// </summary>
    //        /// <returns></returns>
    //        private static bool SkipAuthorization(HttpActionContext actionContext)
    //        {
    //            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
    //                       || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();
    //        }
        
    //}
}