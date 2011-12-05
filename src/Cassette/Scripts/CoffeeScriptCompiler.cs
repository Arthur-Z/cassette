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
using Cassette.IO;
using Cassette.Utilities;
using Jurassic;
using Jurassic.Library;

namespace Cassette.Scripts
{
    public class CoffeeScriptCompiler : ICompiler
    {
        static CoffeeScriptCompiler()
        {
            scriptEngine = new Lazy<ScriptEngine>(CreateScriptEngineWithCoffeeScriptLoaded);
        }

        readonly static Lazy<ScriptEngine> scriptEngine;

        public string Compile(string coffeeScriptSource, IFile sourceFile)
        {
            var callCoffeeCompile =
                "(function() { try { return CoffeeScript.compile('"
                + JavaScriptUtilities.EscapeJavaScriptString(coffeeScriptSource)
                + "'); } catch (e) { return e; } })()";
            
            object result;
            lock (ScriptEngine) // ScriptEngine is NOT thread-safe, so we MUST lock.
            {
                result = ScriptEngine.Evaluate(callCoffeeCompile);
            }
            var javascript = result as string;
            if (javascript != null)
            {
                return javascript;
            }
            else
            {
                var error = result as ErrorInstance;
                if (error != null)
                {
                    throw new CoffeeScriptCompileException(
                        error.Message + " in " + sourceFile.FullPath,
                        sourceFile.FullPath
                    );
                }
                else
                {
                    throw new CoffeeScriptCompileException(
                        "Unknown CoffeeScript compilation failure.",
                        sourceFile.FullPath
                    );
                }
            }
        }

        static ScriptEngine CreateScriptEngineWithCoffeeScriptLoaded()
        {
            var engine = new ScriptEngine();
            engine.Execute(Properties.Resources.coffeescript);
            return engine;
        }

        ScriptEngine ScriptEngine
        {
            get { return scriptEngine.Value; }
        }
    }
}

