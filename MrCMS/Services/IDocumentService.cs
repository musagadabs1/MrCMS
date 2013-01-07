using System;
using System.Collections.Generic;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Multisite;
using MrCMS.Models;
using MrCMS.Paging;

namespace MrCMS.Services
{
    public interface IDocumentService
    {
        void AddDocument<T>(T document) where T : Document;
        T GetDocument<T>(int id) where T : Document;
        T GetUniquePage<T>() where T : UniquePage;
        UniquePage GetUniquePage(Type type);
        T SaveDocument<T>(T document) where T : Document;
        IEnumerable<T> GetAllDocuments<T>() where T : Document;
        bool ExistAny(Type type, Site site);
        IEnumerable<T> GetFrontEndDocumentsByParentId<T>(int? id) where T : Document;
        //IEnumerable<T> GetDocumentsByParentId<T>(int? id) where T : Document;
        IEnumerable<T> GetDocumentsByParentId<T>(int? id, int? siteId = null) where T : Document;
        IEnumerable<T> GetAdminDocumentsByParentId<T>(Site site, int? id) where T : Document, IHaveSite;
        T GetDocumentByUrl<T>(string url, Site site) where T : Document, IHaveSite;
        string GetDocumentUrl(string pageName, int? parentId, Site site, bool useHierarchy = false);
        IEnumerable<SearchResultModel> SearchDocuments<T>(string searchTerm) where T : Document;
        IPagedList<DetailedSearchResultModel> SearchDocumentsDetailed<T>(string searchTerm, int? parentId, int page = 1) where T : Document;
        IPagedList<SearchResult> SiteSearch(string query, int? page, int pageSize = 10);
        Layout GetDefaultLayout(Webpage currentPage);
        void SetTags(string taglist, Document document);
        void SetOrder(int documentId, int order);
        bool AnyPublishedWebpages();
        bool AnyWebpages();
        IEnumerable<Webpage> GetWebPagesByParentIdForNav(int parentId);
        void DeleteDocument<T>(T document) where T : Document;
        void PublishNow(Webpage document);
        void Unpublish(Webpage document);
        void HideWidget(int id, int widgetId);
        void ShowWidget(int id, int widgetId);
        Document Get404Page();
        Document Get500Page();
        DocumentVersion GetDocumentVersion(int id);
        void SetParent(Document document, int? parentId);
        DocumentTypeDefinition GetDefinitionByType(Type type);
    }

    public interface IHaveSite
    {
        Site Site { get; set; }
    }
}