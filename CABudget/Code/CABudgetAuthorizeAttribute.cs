using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CABudget.Code {
    public class CABudgetAuthorizeAttribute : AuthorizeAttribute {
        private string[] allowedUsers = { "heitman\\morelockc" };

        

        protected override bool AuthorizeCore(HttpContextBase httpContext) {
            var logon = httpContext.User.Identity == null ? "" : httpContext.User.Identity.Name.ToLower();
            return base.AuthorizeCore(httpContext) || allowedUsers.Contains(logon);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext) {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
                base.HandleUnauthorizedRequest(filterContext);
            else {
                filterContext.Result = new ViewResult { ViewName = "Unauthorized" };
                filterContext.HttpContext.Response.StatusCode = 403;
            }
        }
    }
}