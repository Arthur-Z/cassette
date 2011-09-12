﻿using System;
using System.Linq;
using System.Web;

namespace Cassette.UI
{
    public class PageAssetManager<T> : IPageAssetManager<T>
        where T : Module
    {
        public PageAssetManager(IReferenceBuilder<T> referenceBuilder, IPlaceholderTracker placeholderTracker, IModuleContainer<T> moduleContainer, IUrlGenerator urlGenerator)
        {
            this.referenceBuilder = referenceBuilder;
            this.placeholderTracker = placeholderTracker;
            this.moduleContainer = moduleContainer;
            this.urlGenerator = urlGenerator;
        }

        readonly IReferenceBuilder<T> referenceBuilder;
        readonly IPlaceholderTracker placeholderTracker;
        readonly IUrlGenerator urlGenerator;
        readonly IModuleContainer<T> moduleContainer;
 
        public void Reference(string path, string location = null)
        {
            referenceBuilder.AddReference(path, location);
        }

        public IHtmlString Render(string location = null)
        {
            return placeholderTracker.InsertPlaceholder(
                () => CreateHtml(location)
            );
        }

        public string ModuleUrl(string path)
        {
            var module = moduleContainer.FindModuleContainingPath(path);
            if (module == null)
            {
                throw new ArgumentException("Cannot find module contain path \"" + path + "\".");
            }
            return urlGenerator.CreateModuleUrl(module);
        }

        HtmlString CreateHtml(string location)
        {
            return new HtmlString(string.Join(Environment.NewLine,
                referenceBuilder.GetModules(location).Select(
                    module => module.Render().ToHtmlString()
                )
            ));
        }
    }
}
