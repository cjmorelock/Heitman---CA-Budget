using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CABudget.Code {
    public static class ExtentionMethods {
        /// <summary>
        /// This extension method for the MVC Controller class allows you to get the output of a View result as a string.
        /// My use case for this is returning a partial view as part of a JSON result returned from an AJAX request.  It 
        /// allows me to return both the view html and other status information to the ajax caller.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string RenderViewToString(this Controller controller, string viewName, object model) {
            controller.ViewData.Model = model;
            using (var sw = new StringWriter()) {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}