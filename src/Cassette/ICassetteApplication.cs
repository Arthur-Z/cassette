﻿using System;
using Cassette.IO;
using Cassette.UI;

namespace Cassette
{
    public interface ICassetteApplication : IDisposable
    {
        IDirectory RootDirectory { get; }
        bool IsOutputOptimized { get; set; }
        IUrlGenerator UrlGenerator { get; set; }
        bool HtmlRewritingEnabled { get; set; }

        IReferenceBuilder<T> GetReferenceBuilder<T>() where T : Module;
    }
}