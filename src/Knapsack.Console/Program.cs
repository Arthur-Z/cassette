﻿using System;
using System.IO;
using System.Linq;
using Knapsack.CoffeeScript;

namespace Knapsack
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: knapsack {path}");
                return;
            }

            var path = Path.GetFullPath(args[0]);

            var scriptFilenames = Directory.GetFiles(path, "*.js", SearchOption.AllDirectories);
            var scripts = from filename in scriptFilenames
                          select LoadScript(filename);

            var unresolvedModule = new UnresolvedModule("", scripts.ToArray());
            var module = unresolvedModule.Resolve(_ => "");

            var writer = new ModuleWriter(Console.Out, p => File.ReadAllText(Path.Combine(path, p)), new CoffeeScriptCompiler(File.ReadAllText));
            writer.Write(module);
        }

        static UnresolvedScript LoadScript(string filename)
        {
            var scriptParser = new ScriptParser();
            using (var stream = File.OpenRead(filename))
            {
                return scriptParser.Parse(stream, filename);
            }
        }
    }
}
