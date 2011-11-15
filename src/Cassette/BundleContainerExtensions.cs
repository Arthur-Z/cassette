namespace Cassette
{
    static class BundleContainerExtensions
    {
        public static bool TryGetAssetByPath(this IBundleContainer bundleContainer, string path, out IAsset asset, out Bundle bundle)
        {
            bundle = bundleContainer.FindBundleContainingPath<Bundle>(path);
            if (bundle == null)
            {
                asset = null;
                return false;
            }
            else
            {
                asset = bundle.FindAssetByPath(path);
                return asset != null;
            }
        }
    }
}