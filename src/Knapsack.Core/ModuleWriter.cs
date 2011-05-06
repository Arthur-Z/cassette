﻿using System;
using System.IO;
using System.Linq;
using Knapsack.CoffeeScript;
using Microsoft.Ajax.Utilities;

namespace Knapsack
{
    public class ModuleWriter
    {
        readonly TextWriter textWriter;
        readonly Func<string, string> loadSourceFromFile;
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;

        public ModuleWriter(TextWriter textWriter, Func<string, string> loadSourceFromFile, ICoffeeScriptCompiler coffeeScriptCompiler)
        {
            this.textWriter = textWriter;
            this.loadSourceFromFile = loadSourceFromFile;
            this.coffeeScriptCompiler = coffeeScriptCompiler;
        }

        public void Write(Module module)
        {
            var minifier = new Minifier();

            textWriter.Write(
                minifier.MinifyJavaScript(
                    string.Join(
                        "\r\n",
                        module.Scripts.Select(s => GetJavaScript(s.Path))
                    )
                )
            );
        }

        string GetJavaScript(string scriptPath)
        {
            if (scriptPath.EndsWith(".coffee", StringComparison.OrdinalIgnoreCase))
            {
                return coffeeScriptCompiler.CompileFile(scriptPath);
            }
            else // assume it's a regular ".js" file
            {
                return loadSourceFromFile(scriptPath);
            }
        }

    }
}
