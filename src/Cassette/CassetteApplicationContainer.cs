﻿#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.IO;

namespace Cassette
{
    public static class CassetteApplicationContainer
    {
        static Func<ICassetteApplication> _getApplication;

        public static void SetAccessor(Func<ICassetteApplication> getApplication)
        {
            _getApplication = getApplication;
        }

        public static ICassetteApplication Application
        {
            get
            {
                if (_getApplication == null)
                {
                    throw new InvalidOperationException("Cassette infrastructure library missing. Make sure Cassette.Web has been added to the web application.");
                }
                return _getApplication();
            }
        }
    }

    class CassetteApplicationContainer<T> : IDisposable
        where T : ICassetteApplication
    {
        readonly Func<T> createApplication;
        FileSystemWatcher watcher;
        Lazy<T> application;
        bool creationFailed;

        public CassetteApplicationContainer(Func<T> createApplication)
        {
            this.createApplication = createApplication;
            application = new Lazy<T>(CreateApplication);
        }

        public CassetteApplicationContainer(Func<T> createApplication, string rootDirectoryToWatch)
            : this(createApplication)
        {

            // In production mode we don't expect the asset files to change
            // while the application is running. Changes to assets will involve a 
            // re-deployment and restart of the app pool. So new assets are loaded then.

            // In development mode, asset files will likely change while application is
            // running. So watch the file system and recycle the application object 
            // when files are created/changed/deleted/etc.
            StartWatchingFileSystem(rootDirectoryToWatch);
        }

        public T Application
        {
            get
            {
                return application.Value;
            }
        }

        void StartWatchingFileSystem(string rootDirectoryToWatch)
        {
            watcher = new FileSystemWatcher(rootDirectoryToWatch)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            watcher.Created += RecycleApplication;
            watcher.Changed += RecycleApplication;
            watcher.Renamed += RecycleApplication;
            watcher.Deleted += RecycleApplication;
        }

        void RecycleApplication(object sender, FileSystemEventArgs e)
        {
            if (IsPendingCreation) return; // Already recycled, awaiting first creation.

            lock (this)
            {
                if (IsPendingCreation) return;

                if (creationFailed)
                {
                    creationFailed = false;
                }
                else
                {
                    application.Value.Dispose();
                }
                // Re-create the lazy object. So the application isn't created until it's asked for.
                application = new Lazy<T>(CreateApplication);
            }
        }

        bool IsPendingCreation
        {
            get { return creationFailed == false && application.IsValueCreated == false; }
        }

        T CreateApplication()
        {
            try
            {
                var app = createApplication();
                creationFailed = false;
                return app;
            }
            catch
            {
                creationFailed = true;
                throw;
            }
        }

        public void Dispose()
        {
            if (watcher != null)
            {
                watcher.Dispose();
            }
            if (application.IsValueCreated)
            {
                application.Value.Dispose();
            }
        }

        public void Initialize()
        {
            CreateApplication();
        }
    }
}