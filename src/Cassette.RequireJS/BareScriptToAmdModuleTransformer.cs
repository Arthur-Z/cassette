﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Ajax.Utilities;

namespace Cassette.RequireJS
{
    /// <summary>
    /// Converts plain JavaScript source into an AMD module. The script should contain a top-level
    /// variable that matches the file name of the script. This variable will be returned as the
    /// module's value from the generated <code>define</code> call.
    /// </summary>
    public class BareScriptToAmdModuleTransformer
    {
        readonly IAmdModuleCollection modules;
        readonly IJsonSerializer jsonSerializer;

        public BareScriptToAmdModuleTransformer(IAmdModuleCollection modules, IJsonSerializer jsonSerializer)
        {
            this.modules = modules;
            this.jsonSerializer = jsonSerializer;
        }

        public string Transform(string source, string scriptPath, IEnumerable<string> referencePaths)
        {
            var path = PathHelpers.ConvertCassettePathToModulePath(scriptPath);
            var dependencyPaths = DependencyPaths(referencePaths);
            var dependencyAliases = DependencyAliases(referencePaths);
            var export = ExportedVariableName(scriptPath);

            var output = JavaScriptContainsTopLevelVar(source, export)
                ? ModuleWithReturn(path, dependencyPaths, dependencyAliases, source, export)
                : ModuleWithoutReturn(path, dependencyPaths, dependencyAliases, source);

            return output;
        }

        string ModuleWithoutReturn(string path, IEnumerable<string> dependencyPaths, IEnumerable<string> dependencyAliases, string source)
        {
            return string.Format(
                "define({0},{1},function({2}){{{3}\r\n}});",
                jsonSerializer.Serialize(path),
                jsonSerializer.Serialize(dependencyPaths),
                string.Join(",", dependencyAliases),
                source
            );
        }

        string ModuleWithReturn(string path, IEnumerable<string> dependencyPaths, IEnumerable<string> dependencyAliases, string source, string export)
        {
            Diagnostics.Trace.Source.TraceInformation("AMD module {0} does not return a value.", path);
            return string.Format(
                "define({0},{1},function({2}){{{3}\r\nreturn {4};}});",
                jsonSerializer.Serialize(path),
                jsonSerializer.Serialize(dependencyPaths),
                string.Join(",", dependencyAliases),
                source,
                export
            );
        }

        IEnumerable<string> DependencyPaths(IEnumerable<string> referencePaths)
        {
            return referencePaths.Select(p => modules[p].ModulePath);
        }

        IEnumerable<string> DependencyAliases(IEnumerable<string> referencePaths)
        {
            return referencePaths.Select(p => modules[p].Alias);
        }

        string ExportedVariableName(string assetPath)
        {
            var name = Path.GetFileNameWithoutExtension(assetPath);
            if (!char.IsLetter(name[0]) && name[0] != '_') name = "_" + name;
            var safeName = Regex.Replace(name, "[^a-zA-Z0-9_]", match => "_");
            return safeName;
        }

        bool JavaScriptContainsTopLevelVar(string source, string var)
        {
            var parser = new JSParser(source);
            var tree = parser.Parse(new CodeSettings());
            var finder = new TopLevelVarFinder(var);
            tree.Accept(finder);
            return finder.Found;
        }

        class TopLevelVarFinder : TreeVisitor
        {
            readonly string varName;

            public TopLevelVarFinder(string varName)
            {
                this.varName = varName;
            }

            public override void Visit(VariableDeclaration node)
            {
                if (node.EnclosingScope is GlobalScope && node.Identifier == varName)
                {
                    Found = true;
                }

                base.Visit(node);
            }

            public bool Found { get; private set; }
        }
    }
}