﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette.Persistence
{
    public class CachedAsset : IAsset
    {
        public CachedAsset(byte[] hash, IEnumerable<IAsset> children, Func<Stream> openStream)
        {
            this.hash = hash;
            this.children = children.ToArray();
            this.openStream = openStream;
        }

        readonly byte[] hash;
        readonly IEnumerable<IAsset> children;
        readonly Func<Stream> openStream;
        readonly List<AssetReference> references = new List<AssetReference>();

        public void Accept(IAssetVisitor visitor)
        {
            foreach (var child in children)
            {
                visitor.Visit(child);
            }
        }

        public string SourceFilename
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<AssetReference> References
        {
            get { return references; }
        }

        public void AddReference(string path, int lineNumber)
        {
            references.Add(new AssetReference(path, this, lineNumber, AssetReferenceType.DifferentModule));
        }

        public void AddRawFileReference(string filename)
        {
            throw new NotImplementedException();
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            throw new NotImplementedException();
        }

        public Stream OpenStream()
        {
            return openStream();
        }

        public byte[] Hash
        {
            get { return hash; }
        }


        public IFileSystem Directory
        {
            get { throw new NotImplementedException(); }
        }
    }
}
