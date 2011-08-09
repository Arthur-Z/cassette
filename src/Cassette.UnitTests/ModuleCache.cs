﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ModuleCache_IsUpToDate_Tests
    {
        public ModuleCache_IsUpToDate_Tests()
        {
            fileSystem = new Mock<IFileSystem>();
            cache = new ModuleCache<Module>(fileSystem.Object, Mock.Of<IModuleFactory<Module>>());
        }

        readonly ModuleCache<Module> cache;
        readonly Mock<IFileSystem> fileSystem;

        [Fact]
        public void WhenContainerFileDoesNotExist_ThenIsUpToDateReturnsFalse()
        {
            fileSystem.Setup(fs => fs.FileExists("container.xml"))
                      .Returns(false);
            cache.IsUpToDate(new DateTime(2000, 1, 2)).ShouldEqual(false);
        }

        [Fact]
        public void WhenContainerFileIsOlder_ThenIsUpToDateReturnsFalse()
        {
            fileSystem.Setup(fs => fs.FileExists("container.xml"))
                      .Returns(true);
            fileSystem.Setup(fs => fs.GetLastWriteTimeUtc("container.xml"))
                      .Returns(new DateTime(2000, 1, 1));

            cache.IsUpToDate(new DateTime(2000, 1, 2)).ShouldEqual(false);
        }

        [Fact]
        public void WhenContainerFileIsNewer_ThenIsUpToDateReturnsTrue()
        {
            fileSystem.Setup(fs => fs.FileExists("container.xml"))
                      .Returns(true);
            fileSystem.Setup(fs => fs.GetLastWriteTimeUtc("container.xml"))
                      .Returns(new DateTime(2000, 1, 2));

            cache.IsUpToDate(new DateTime(2000, 1, 1)).ShouldEqual(true);
        }

        [Fact]
        public void WhenContainerFileIsSameAge_ThenIsUpToDateReturnsTrue()
        {
            fileSystem.Setup(fs => fs.FileExists("container.xml"))
                      .Returns(true);
            fileSystem.Setup(fs => fs.GetLastWriteTimeUtc("container.xml"))
                      .Returns(new DateTime(2000, 1, 1));

            cache.IsUpToDate(new DateTime(2000, 1, 1)).ShouldEqual(true);
        }
    }

    public class ModuleCache_LoadModuleContainer_Tests
    {
        public ModuleCache_LoadModuleContainer_Tests()
        {
            var containerXml = new XDocument(new XElement("container",
                new XAttribute("lastWriteTime", DateTime.UtcNow.Ticks),
                new XElement("module",
                    new XAttribute("directory", "module-a"),
                    new XElement("asset",
                        new XAttribute("filename", "asset-1.js")
                    ),
                    new XElement("asset",
                        new XAttribute("filename", "asset-2.js")
                    ),
                    new XElement("reference", new XAttribute("module", "module-b"))
                ),
                new XElement("module",
                    new XAttribute("directory", "module-b"),
                    new XElement("asset",
                        new XAttribute("filename", "asset-3.js")
                    ),
                    new XElement("asset",
                        new XAttribute("filename", "asset-4.js")
                    )
                )
            ));
            var fileStreams = new Dictionary<string, Stream>
            {
                { "container.xml", containerXml.ToString().AsStream() },
                { "module-a", "module-a".AsStream() },
                { "module-b", "module-b".AsStream() }
            };
            var fileSystem = new StubFileSystem(fileStreams);

            var moduleFactory = new Mock<IModuleFactory<Module>>();
            moduleFactory.Setup(f => f.CreateModule(It.IsAny<string>()))
                         .Returns<string>(path => new Module(path, Mock.Of<IFileSystem>()));

            cache = new ModuleCache<Module>(fileSystem, moduleFactory.Object);
        }

        readonly ModuleCache<Module> cache;

        [Fact]
        public void LoadModuleContainer_ReturnsModuleContainer()
        {
            var container = cache.LoadModuleContainer();

            var moduleA = container.First(m => m.Directory.EndsWith("module-a"));
            moduleA.Assets.Count.ShouldEqual(1);
            moduleA.Assets[0].References.Single().ReferencedFilename.ShouldEqual("module-b");
            moduleA.ContainsPath("module-a\\asset-1.js");
            moduleA.ContainsPath("module-a\\asset-2.js");
            moduleA.ContainsPath("module-a");

            var moduleB = container.First(m => m.Directory.EndsWith("module-b"));
            moduleB.Assets.Count.ShouldEqual(1);
            moduleB.Assets[0].References.Count().ShouldEqual(0);
            moduleB.ContainsPath("module-b\\asset-3.js");
            moduleB.ContainsPath("module-b\\asset-4.js");
            moduleB.ContainsPath("module-b");
        }
    }

    public class ModuleCache_SaveModuleContainer_Tests
    {
        public ModuleCache_SaveModuleContainer_Tests()
        {
            fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(fs => fs.OpenFile(It.IsAny<string>(), FileMode.OpenOrCreate, FileAccess.Write))
                      .Returns(Stream.Null);

            cache = new ModuleCache<Module>(fileSystem.Object, Mock.Of<IModuleFactory<Module>>());
        }

        readonly ModuleCache<Module> cache;
        readonly Mock<IFileSystem> fileSystem;

        [Fact]
        public void SaveWritesContainerXmlFile()
        {
            var module = new Module("", Mock.Of<IFileSystem>());
            var asset1 = new Mock<IAsset>();
            asset1.SetupGet(a => a.SourceFilename).Returns("asset.js");
            asset1.Setup(a => a.OpenStream()).Returns(Stream.Null);
            module.Assets.Add(asset1.Object);
            var container = new ModuleContainer<Module>(new[] { module });

            cache.SaveModuleContainer(container);

            fileSystem.Verify(fs => fs.OpenFile("container.xml", FileMode.OpenOrCreate, FileAccess.Write));
            fileSystem.Verify(fs => fs.OpenFile(".module", FileMode.OpenOrCreate, FileAccess.Write));
        }
    }
}