﻿using System;
using System.IO;

namespace Cassette.IO
{
    public class IsolatedStorageFile : IFile
    {
        readonly string filename;
        readonly System.IO.IsolatedStorage.IsolatedStorageFile storage;
        readonly IsolatedStorageDirectory directory;
        readonly string systemFilename;

        public IsolatedStorageFile(string filename, System.IO.IsolatedStorage.IsolatedStorageFile storage, IsolatedStorageDirectory directory)
        {
            this.filename = filename;
            this.storage = storage;
            this.directory = directory;
            systemFilename = filename.Substring(2); // Skip the "~/" prefix.
        }

        public IDirectory Directory
        {
            get { return directory; }
        }

        public string FullPath
        {
            get { return filename; }
        }

        public Stream Open(FileMode mode, FileAccess access, FileShare fileShare)
        {
            return storage.OpenFile(systemFilename, mode, access, fileShare);
        }

        public bool Exists
        {
            get { return storage.FileExists(systemFilename); }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return storage.GetLastWriteTime(systemFilename).UtcDateTime; }
        }

        public void Delete()
        {
            storage.DeleteFile(systemFilename);
        }
    }
}

