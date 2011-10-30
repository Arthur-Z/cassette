﻿using System;
using System.Collections.Generic;
using Cassette.Utilities;

namespace Cassette.Configuration
{
    abstract class BundleContainerFactoryBase : IBundleContainerFactory
    {
        protected BundleContainerFactoryBase(IDictionary<Type, IBundleFactory<Bundle>> bundleFactories)
        {
            this.bundleFactories = bundleFactories;
        }

        readonly IDictionary<Type, IBundleFactory<Bundle>> bundleFactories;

        public abstract IBundleContainer Create(IEnumerable<Bundle> unprocessedBundles, ICassetteApplication application);

        protected void ProcessAllBundles(IEnumerable<Bundle> bundles, ICassetteApplication application)
        {
            foreach (var bundle in bundles)
            {
                bundle.Process(application);
            }
        }

        protected IEnumerable<Bundle> CreateExternalBundlesFromReferences(IEnumerable<Bundle> bundlesArray, ICassetteApplication application)
        {
            var referencesAlreadyCreated = new HashSet<string>();
            foreach (var bundle in bundlesArray)
            {
                foreach (var reference in bundle.References)
                {
                    if (reference.IsUrl() == false) continue;
                    if (referencesAlreadyCreated.Contains(reference)) continue;

                    var externalBundle = CreateExternalBundle(reference, bundle, application);
                    referencesAlreadyCreated.Add(externalBundle.Path);
                    yield return externalBundle;
                }
                foreach (var asset in bundle.Assets)
                {
                    foreach (var assetReference in asset.References)
                    {
                        if (assetReference.Type != AssetReferenceType.Url ||
                            referencesAlreadyCreated.Contains(assetReference.Path)) continue;

                        var externalBundle = CreateExternalBundle(assetReference.Path, bundle, application);
                        referencesAlreadyCreated.Add(externalBundle.Path);
                        yield return externalBundle;
                    }
                }
            }
        }

        Bundle CreateExternalBundle(string reference, Bundle referencer, ICassetteApplication application)
        {
            var bundleFactory = GetBundleFactory(referencer.GetType());
            var externalBundle = bundleFactory.CreateExternalBundle(reference);
            externalBundle.Process(application);
            return externalBundle;
        }

        IBundleFactory<Bundle> GetBundleFactory(Type bundleType)
        {
            IBundleFactory<Bundle> factory;
            if (bundleFactories.TryGetValue(bundleType, out factory))
            {
                return factory;
            }
            throw new ArgumentException(string.Format("Cannot find bundle factory for {0}", bundleType.FullName));
        }
    }
}