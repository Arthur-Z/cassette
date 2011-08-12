﻿using System;
using System.IO;
using Cassette.Less;
using Cassette.Utilities;

namespace Cassette.ModuleProcessing
{
    public class CompileAsset : IAssetTransformer
    {
        public CompileAsset(ICompiler compiler, Module module)
        {
            this.compiler = compiler;
            this.module = module;
        }

        readonly ICompiler compiler;
        readonly Module module;

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                using (var input = new StreamReader(openSourceStream()))
                {
                    var directory = Path.GetDirectoryName(asset.SourceFilename);
                    var fileSystem = directory.Length > 0 ? module.FileSystem.NavigateTo(directory, false)
                                                          : module.FileSystem;
                    var css = compiler.Compile(input.ReadToEnd(), asset.SourceFilename, fileSystem);
                    return css.AsStream();
                }
            };
        }
    }
}