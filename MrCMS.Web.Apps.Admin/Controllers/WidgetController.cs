﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MrCMS.ACL;
using MrCMS.Entities.Widget;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Admin.Helpers;
using MrCMS.Web.Apps.Admin.Models;
using MrCMS.Web.Apps.Admin.Services;
using MrCMS.Website;
using MrCMS.Website.Controllers;

namespace MrCMS.Web.Apps.Admin.Controllers
{
    public class WidgetController : MrCMSAdminController
    {
        private readonly IWidgetAdminService _widgetService;

        public WidgetController(IWidgetAdminService widgetService)
        {
            _widgetService = widgetService;
        }

        [HttpGet]
        public PartialViewResult Add(int id, int? pageId, string returnUrl = null)
        {
            TempData["returnUrl"] = returnUrl;
            var model = new AddWidgetModel
            {
                LayoutAreaId = id,
                WebpageId = pageId
            };
            return PartialView(model);
        }

        [HttpPost]
        [ActionName("Add")]
        public async Task<JsonResult> Add_POST(AddWidgetModel model)
        {
            var additionalPropertyModel = _widgetService.GetAdditionalPropertyModel(model.WidgetType);
            if (additionalPropertyModel != null)
                await TryUpdateModelAsync(additionalPropertyModel, additionalPropertyModel.GetType(), string.Empty);

            var widget = _widgetService.AddWidget(model, additionalPropertyModel);

            return Json(widget.Id);
        }

        [HttpGet]
        //[ValidateInput(false)]
        [ActionName("Edit")]
        [Acl(typeof(Widget), TypeACLRule.Edit)]
        public ViewResult Edit_Get(int id, string returnUrl = null)
        {
            var editModel = _widgetService.GetEditModel(id);
            var widget = _widgetService.GetWidget(id);
            widget.SetViewData(this);
            ViewData["widget"] = widget;

            if (!string.IsNullOrEmpty(returnUrl))
                ViewData["return-url"] = Request.Referer();
            else
                ViewData["return-url"] = returnUrl;

            return View(editModel);
        }

        [HttpPost]
        [Acl(typeof(Widget), TypeACLRule.Edit)]
        public async Task<ActionResult> Edit(UpdateWidgetModel model, string returnUrl = null)
        {
            var additionalPropertyModel = _widgetService.GetAdditionalPropertyModel(model.Id);
            if (additionalPropertyModel != null)
                await TryUpdateModelAsync(additionalPropertyModel, additionalPropertyModel.GetType(), string.Empty);

            var widget = _widgetService.UpdateWidget(model, additionalPropertyModel);

            return string.IsNullOrWhiteSpace(returnUrl)
                ? widget.Webpage != null
                    ? RedirectToAction("Edit", "Webpage", new { id = widget.Webpage.Id })
                    : (ActionResult)RedirectToAction("Edit", "LayoutArea", new { id = widget.LayoutArea.Id })
                : Redirect(returnUrl);
        }

        [HttpGet]
        [ActionName("Delete")]
        [Acl(typeof(Widget), TypeACLRule.Delete)]
        public ActionResult Delete_Get(int id)
        {
            return PartialView(_widgetService.GetEditModel(id));
        }

        [HttpPost]
        [Acl(typeof(Widget), TypeACLRule.Delete)]
        public ActionResult Delete(int id, string returnUrl)
        {
            var widget = _widgetService.DeleteWidget(id);

            int webpageId = 0;
            int layoutAreaId = 0;
            if (widget.Webpage != null)
                webpageId = widget.Webpage.Id;
            if (widget.LayoutArea != null)
                layoutAreaId = widget.LayoutArea.Id;

            return !string.IsNullOrWhiteSpace(returnUrl) &&
                   !returnUrl.Contains("widget/edit/", StringComparison.OrdinalIgnoreCase)
                ? (ActionResult)Redirect(returnUrl)
                : webpageId > 0
                    ? RedirectToAction("Edit", "Webpage", new { id = webpageId, layoutAreaId })
                    : RedirectToAction("Edit", "LayoutArea", new { id = layoutAreaId });
        }

        [HttpGet]
        public ActionResult AddProperties(string type)
        {
            var model = _widgetService.GetAdditionalPropertyModel(type);
            if (model != null)
            {
                // TODO: viewdata
                ViewData["type"] = TypeHelper.GetTypeByName(type);
                return PartialView(model);
            }
            return new EmptyResult();
        }
    }
}