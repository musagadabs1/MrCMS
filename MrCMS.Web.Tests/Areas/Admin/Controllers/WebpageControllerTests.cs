using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FakeItEasy;
using FluentAssertions;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Multisite;
using MrCMS.Models;
using MrCMS.Paging;
using MrCMS.Services;
using MrCMS.Web.Application.Pages;
using MrCMS.Web.Areas.Admin.Controllers;
using MrCMS.Web.Areas.Admin.Models;
using MrCMS.Web.Tests.Stubs;
using NHibernate;
using Xunit;

namespace MrCMS.Web.Tests.Areas.Admin.Controllers
{
    public class WebpageControllerTests
    {
        private static IDocumentService documentService;
        private static IFormService formService;
        private static ISiteService siteService;
        private static ISession session;

        [Fact]
        public void WebpageController_AddGet_ShouldReturnAddPageModel()
        {
            WebpageController webpageController = GetWebpageController();

            var actionResult = webpageController.Add(1) as ViewResult;

            actionResult.Model.Should().BeOfType<AddPageModel>();
        }

        private static WebpageController GetWebpageController()
        {
            documentService = A.Fake<IDocumentService>();
            siteService = A.Fake<ISiteService>();
            formService = A.Fake<IFormService>();
            session = A.Fake<ISession>();
            var webpageController = new WebpageController(documentService, siteService, formService, session);
            return webpageController;
        }

        [Fact]
        public void WebpageController_AddGet_ShouldSetParentIdOfModelToIdInMethod()
        {
            WebpageController webpageController = GetWebpageController();
            A.CallTo(() => documentService.GetDocument<Document>(1)).Returns(new TextPage {Id = 1, Site = new Site()});

            var actionResult = webpageController.Add(1) as ViewResult;

            (actionResult.Model as AddPageModel).ParentId.Should().Be(1);
        }

        [Fact]
        public void WebpageController_AddGet_ShouldSetViewDataToSelectListItem()
        {
            WebpageController webpageController = GetWebpageController();

            var result = webpageController.Add(1) as ViewResult;

            webpageController.ViewData["Layout"].Should().BeAssignableTo<IEnumerable<SelectListItem>>();
        }

        [Fact]
        public void WebpageController_AddPost_ShouldCallSaveDocument()
        {
            WebpageController webpageController = GetWebpageController();
            var webpage = new TextPage {Site = new Site()};
            
            webpageController.Add(webpage);

            A.CallTo(() => documentService.AddDocument<Webpage>(webpage)).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_AddPost_ShouldRedirectToView()
        {
            WebpageController webpageController = GetWebpageController();

            var webpage = new TextPage {Id = 1};
            var result = webpageController.Add(webpage) as RedirectToRouteResult;

            result.RouteValues["action"].Should().Be("Edit");
            result.RouteValues["id"].Should().Be(1);
        }

        [Fact]
        public void WebpageController_EditGet_ShouldReturnAViewResult()
        {
            WebpageController webpageController = GetWebpageController();

            ActionResult result = webpageController.Edit_Get(new TextPage());

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void WebpageController_EditGet_ShouldReturnLayoutAsViewModel()
        {
            WebpageController webpageController = GetWebpageController();
            var webpage = new TextPage {Id = 1};

            var result = webpageController.Edit_Get(webpage) as ViewResult;

            result.Model.Should().Be(webpage);
        }

        [Fact]
        public void WebpageController_EditGet_ShouldCallGetAllLayouts()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.Edit_Get(new TextPage());

            A.CallTo(() => documentService.GetAllDocuments<Layout>()).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_EditGet_ShouldSetViewDataToSelectListItem()
        {
            WebpageController webpageController = GetWebpageController();

            var result = webpageController.Edit_Get(new TextPage()) as ViewResult;

            webpageController.ViewData["Layout"].Should().BeAssignableTo<IEnumerable<SelectListItem>>();
        }

        [Fact]
        public void WebpageController_EditGet_ShouldSetLayoutDetailsToSelectListItems()
        {
            WebpageController webpageController = GetWebpageController();
            var layout = new Layout {Id = 1, Name = "Layout Name"};
            A.CallTo(() => documentService.GetAllDocuments<Layout>()).Returns(new List<Layout> {layout});

            webpageController.Edit_Get(new TextPage());

            webpageController.ViewData["Layout"].As<IEnumerable<SelectListItem>>()
                                                .Skip(1)
                                                .First()
                                                .Selected.Should()
                                                .BeFalse();
            webpageController.ViewData["Layout"].As<IEnumerable<SelectListItem>>()
                                                .Skip(1)
                                                .First()
                                                .Text.Should()
                                                .Be("Layout Name");
            webpageController.ViewData["Layout"].As<IEnumerable<SelectListItem>>()
                                                .Skip(1)
                                                .First()
                                                .Value.Should()
                                                .Be("1");
        }

        [Fact]
        public void WebpageController_EditPost_ShouldCallSaveDocument()
        {
            WebpageController webpageController = GetWebpageController();
            Webpage textPage = new TextPage {Id = 1};

            webpageController.Edit(textPage);

            A.CallTo(() => documentService.SaveDocument(textPage)).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_EditPost_ShouldRedirectToEdit()
        {
            WebpageController webpageController = GetWebpageController();
            var textPage = new TextPage {Id = 1};

            ActionResult actionResult = webpageController.Edit(textPage);

            actionResult.Should().BeOfType<RedirectToRouteResult>();
            actionResult.As<RedirectToRouteResult>().RouteValues["action"].Should().Be("Edit");
            actionResult.As<RedirectToRouteResult>().RouteValues["id"].Should().Be(1);
        }

        [Fact]
        public void WebpageController_Sort_ShouldCallGetDocumentsByParentId()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.Sort(1);

            A.CallTo(() => documentService.GetDocumentsByParentId<Webpage>(1, null)).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_Sort_ShouldUseTheResultOfDocumentsByParentIdsAsModel()
        {
            WebpageController webpageController = GetWebpageController();
            var webpages = new List<Webpage> {new TextPage()};
            A.CallTo(() => documentService.GetDocumentsByParentId<Webpage>(1, null)).Returns(webpages);

            var viewResult = webpageController.Sort(1).As<ViewResult>();

            viewResult.Model.As<List<Webpage>>().Should().BeEquivalentTo(webpages);
        }

        [Fact]
        public void WebpageController_SortAction_ShouldCallSortOrderOnTheDocumentServiceWithTheRelevantValues()
        {
            WebpageController webpageController = GetWebpageController();
            webpageController.SortAction(1, 2);

            A.CallTo(() => documentService.SetOrder(1, 2)).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_View_InvalidIdReturnsRedirectToIndex()
        {
            WebpageController webpageController = GetWebpageController();
            A.CallTo(() => documentService.GetDocument<Webpage>(1)).Returns(null);

            ActionResult actionResult = webpageController.Show(null);

            actionResult.As<RedirectToRouteResult>().RouteValues["action"].Should().Be("Index");
        }

        [Fact]
        public void WebpageController_Index_ReturnsViewResult()
        {
            WebpageController webpageController = GetWebpageController();

            ViewResult actionResult = webpageController.Index();

            actionResult.Should().NotBeNull();
        }

        [Fact]
        public void WebpageController_SuggestDocumentUrl_ShouldCallGetDocumentUrl()
        {
            WebpageController webpageController = GetWebpageController();
            var site = new Site();
            A.CallTo(() => siteService.GetSite(1)).Returns(site);

            webpageController.SuggestDocumentUrl(1, "test", 1);

            A.CallTo(() => documentService.GetDocumentUrl("test", 1, site, true)).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_SuggestDocumentUrl_ShouldReturnTheResultOfGetDocumentUrl()
        {
            WebpageController webpageController = GetWebpageController();
            var site = new Site();
            A.CallTo(() => siteService.GetSite(1)).Returns(site);

            A.CallTo(() => documentService.GetDocumentUrl("test", 1, site, true)).Returns("test/result");
            string url = webpageController.SuggestDocumentUrl(1, "test", 1);

            url.Should().BeEquivalentTo("test/result");
        }

        [Fact]
        public void WebpageController_DeleteGet_ReturnsPartialViewResult()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.Delete_Get(null).Should().BeOfType<PartialViewResult>();
        }

        [Fact]
        public void WebpageController_DeleteGet_ReturnsDocumentPassedAsModel()
        {
            WebpageController webpageController = GetWebpageController();

            var textPage = new TextPage();
            webpageController.Delete_Get(textPage).As<PartialViewResult>().Model.Should().Be(textPage);
        }

        [Fact]
        public void WebpageController_Delete_ReturnsRedirectToIndex()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.Delete(null).Should().BeOfType<RedirectToRouteResult>();
            webpageController.Delete(null).As<RedirectToRouteResult>().RouteValues["action"].Should().Be("Index");
        }

        [Fact]
        public void WebpageController_Delete_CallsDeleteDocumentOnThePassedObject()
        {
            WebpageController webpageController = GetWebpageController();

            var textPage = new TextPage();
            webpageController.Delete(textPage);

            A.CallTo(() => documentService.DeleteDocument<Webpage>(textPage)).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_PublishNow_ReturnsRedirectToRouteResult()
        {
            WebpageController webpageController = GetWebpageController();

            var textPage = new TextPage {Id = 1};
            webpageController.PublishNow(textPage).Should().BeOfType<RedirectToRouteResult>();
        }

        [Fact]
        public void WebpageController_PublishNow_RedirectsToEditForId()
        {
            WebpageController webpageController = GetWebpageController();

            var textPage = new TextPage {Id = 1};
            var result = webpageController.PublishNow(textPage).As<RedirectToRouteResult>();

            result.RouteValues["action"].Should().Be("Edit");
            result.RouteValues["id"].Should().Be(1);
        }

        [Fact]
        public void WebpageController_PublishNow_CallsDocumentServicePublishNow()
        {
            WebpageController webpageController = GetWebpageController();

            var textPage = new TextPage();
            webpageController.PublishNow(textPage);

            A.CallTo(() => documentService.PublishNow(textPage)).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_Unpublish_ReturnsRedirectToRouteResult()
        {
            WebpageController webpageController = GetWebpageController();

            var textPage = new TextPage {Id = 1};
            webpageController.Unpublish(textPage).Should().BeOfType<RedirectToRouteResult>();
        }

        [Fact]
        public void WebpageController_Unpublish_RedirectsToEditForId()
        {
            WebpageController webpageController = GetWebpageController();

            var textPage = new TextPage {Id = 1};
            var result = webpageController.Unpublish(textPage).As<RedirectToRouteResult>();

            result.RouteValues["action"].Should().Be("Edit");
            result.RouteValues["id"].Should().Be(1);
        }

        [Fact]
        public void WebpageController_Unpublish_CallsDocumentServicePublishNow()
        {
            WebpageController webpageController = GetWebpageController();

            var textPage = new TextPage();
            webpageController.Unpublish(textPage);

            A.CallTo(() => documentService.Unpublish(textPage)).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_ViewChanges_ShouldReturnPartialViewResult()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.ViewChanges(1).Should().BeOfType<PartialViewResult>();
        }

        [Fact]
        public void WebpageController_ViewChanges_ShouldCallDocumentServiceGetDocumentVersion()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.ViewChanges(1);

            A.CallTo(() => documentService.GetDocumentVersion(1)).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_ViewChanges_NullDocumentVersionRedirectsToIndex()
        {
            WebpageController webpageController = GetWebpageController();

            A.CallTo(() => documentService.GetDocumentVersion(1)).Returns(null);

            ActionResult result = webpageController.ViewChanges(1);

            result.Should().BeOfType<RedirectToRouteResult>();
            result.As<RedirectToRouteResult>().RouteValues["action"].Should().Be("Index");
        }

        [Fact]
        public void WebpageController_GetForm_ShouldReturnAJsonNetResult()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.GetForm(new TextPage()).Should().BeOfType<AdminController.JsonNetResult>();
        }

        [Fact]
        public void WebpageController_GetForm_ShouldCallFormServiceGetFormStructure()
        {
            WebpageController webpageController = GetWebpageController();

            var textPage = new TextPage();
            webpageController.GetForm(textPage);

            A.CallTo(() => formService.GetFormStructure(textPage)).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_SaveForm_ShouldCallFormServiceSaveFormStructure()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.SaveForm(1, "data");

            A.CallTo(() => formService.SaveFormStructure(1, "data")).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_GetUnformattedBodyContent_ReturnsBodyContentOfPassedTextPage()
        {
            WebpageController webpageController = GetWebpageController();

            var textpage = new TextPage {BodyContent = "test body content"};
            string unformattedBodyContent = webpageController.GetUnformattedBodyContent(textpage);

            unformattedBodyContent.Should().Be("test body content");
        }

        [Fact]
        public void WebpageController_ViewPosting_ShouldReturnAPartialViewResult()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.ViewPosting(1).Should().BeOfType<PartialViewResult>();
        }

        [Fact]
        public void WebpageController_ViewPosting_ShouldCallGetFormPostingForPassedId()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.ViewPosting(1);

            A.CallTo(() => formService.GetFormPosting(1)).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_ViewPosting_ReturnsTheResultOfTheCallToGetFormPostingAsTheModel()
        {
            WebpageController webpageController = GetWebpageController();

            var formPosting = new FormPosting();
            A.CallTo(() => formService.GetFormPosting(1)).Returns(formPosting);
            webpageController.ViewPosting(1).As<PartialViewResult>().Model.Should().Be(formPosting);
        }

        [Fact]
        public void WebpageController_SetParent_CallsDocumentServiceSetParentWithPassedArguments()
        {
            WebpageController webpageController = GetWebpageController();

            var textPage = new TextPage();
            webpageController.SetParent(textPage, 2);

            A.CallTo(() => documentService.SetParent(textPage, 2));
        }

        [Fact]
        public void WebpageController_Postings_ReturnsAPartialViewResult()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.Postings(new TextPage(), 1, null).Should().BeOfType<PartialViewResult>();
        }

        [Fact]
        public void WebpageController_Postings_CallsFormServiceGetFormPostingsWithPassedArguments()
        {
            WebpageController webpageController = GetWebpageController();

            var textPage = new TextPage();
            webpageController.Postings(textPage, 1, null);

            A.CallTo(() => formService.GetFormPostings(textPage, 1, null)).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_Posting_ReturnsTheResultOfTheCallToGetFormPostings()
        {
            WebpageController webpageController = GetWebpageController();

            var textPage = new TextPage();
            var postingsModel = new PostingsModel(new PagedList<FormPosting>(new FormPosting[0], 1, 1), 1);
            A.CallTo(() => formService.GetFormPostings(textPage, 1, null)).Returns(postingsModel);
            webpageController.Postings(textPage, 1, null).As<PartialViewResult>().Model.Should().Be(postingsModel);
        }

        [Fact]
        public void WebpageController_Versions_ReturnsPartialViewResult()
        {
            WebpageController webpageController = GetWebpageController();

            var document = new StubDocument();
            document.SetVersions(new List<DocumentVersion>());
            webpageController.Versions(document, 1).Should().BeOfType<PartialViewResult>();
        }

        [Fact]
        public void WebpageController_Versions_CallsGetVersionsOnPassedDocument()
        {
            WebpageController webpageController = GetWebpageController();

            var document = A.Fake<StubDocument>();
            document.SetVersions(new List<DocumentVersion>());
            webpageController.Versions(document, 1);

            A.CallTo(() => document.GetVersions(1)).MustHaveHappened();
        }


        [Fact]
        public void WebpageController_HideWidget_CallsDocumentServiceHideWidgetWithPassedArguments()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.HideWidget(1, 2, 3);

            A.CallTo(() => documentService.HideWidget(1, 2)).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_HideWidget_ReturnsARedirectToRouteResult()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.HideWidget(1, 2, 3).Should().BeOfType<RedirectToRouteResult>();
        }

        [Fact]
        public void WebpageController_HideWidget_SetsRouteValuesForIdAndLayoutAreaId()
        {
            WebpageController webpageController = GetWebpageController();

            var redirectToRouteResult = webpageController.HideWidget(1, 2, 3).As<RedirectToRouteResult>();

            redirectToRouteResult.RouteValues["action"].Should().Be("Edit");
            redirectToRouteResult.RouteValues["id"].Should().Be(1);
            redirectToRouteResult.RouteValues["layoutAreaId"].Should().Be(3);
        }

        [Fact]
        public void WebpageController_ShowWidget_CallsDocumentServiceShowWidgetWithPassedArguments()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.ShowWidget(1, 2, 3);

            A.CallTo(() => documentService.ShowWidget(1, 2)).MustHaveHappened();
        }

        [Fact]
        public void WebpageController_ShowWidget_ReturnsARedirectToRouteResult()
        {
            WebpageController webpageController = GetWebpageController();

            webpageController.ShowWidget(1, 2, 3).Should().BeOfType<RedirectToRouteResult>();
        }

        [Fact]
        public void WebpageController_ShowWidget_SetsRouteValuesForIdAndLayoutAreaId()
        {
            WebpageController webpageController = GetWebpageController();

            var redirectToRouteResult = webpageController.ShowWidget(1, 2, 3).As<RedirectToRouteResult>();

            redirectToRouteResult.RouteValues["action"].Should().Be("Edit");
            redirectToRouteResult.RouteValues["id"].Should().Be(1);
            redirectToRouteResult.RouteValues["layoutAreaId"].Should().Be(3);
        }
    }
}