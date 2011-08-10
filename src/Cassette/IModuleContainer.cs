﻿using System;
using System.Collections.Generic;

namespace Cassette
{
    public interface IModuleContainer<T> //: IEnumerable<T>
        where T : Module
    {
        IEnumerable<T> Modules { get; }
        void ValidateAndSortModules();
        T FindModuleByPath(string path);
        IEnumerable<T> AddDependenciesAndSort(IEnumerable<T> modules);
    }
}