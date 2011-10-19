using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette.Configuration
{
    public class BundleContainerFactory : BundleContainerFactoryBase
    {
        public BundleContainerFactory(IDictionary<Type, IBundleFactory<Bundle>> factories)
            : base(factories)
        {
        }

        public override IBundleContainer Create(IEnumerable<Bundle> unprocessedBundles, ICassetteApplication application)
        {
            // The bundles may get altered, so force the evaluation of the enumerator first.
            var bundlesArray = unprocessedBundles.ToArray();

            InitializeAllBundles(bundlesArray, application);
            ProcessAllBundles(bundlesArray, application);

            var externalBundles = CreateExternalBundlesFromReferences(bundlesArray);

            return new BundleContainer(bundlesArray.Concat(externalBundles));
        }
    }
}