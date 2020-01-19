﻿using System.Collections.Generic;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.Tests.Stubs
{
    public class StubDocument : Document
    {
        public virtual void SetVersions(List<DocumentVersion> versions)
        {
            Versions = versions;
        }

        public override string UrlSegment { get; set; }
    }
    public class StubUniquePage : IUniquePage
    {
    }
}