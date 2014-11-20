﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DependencyAnalyzer.Util;
using Microsoft.Framework.Runtime;

namespace DependencyAnalyzer.Commands
{
    /// <summary>
    /// Command to build the minimal TPA list
    /// </summary>
    public class BuildTpaCommand
    {
        private readonly IApplicationEnvironment _environment;
        private readonly string _assemblyFolder;
        private readonly string _sourceFile;

        public BuildTpaCommand(IApplicationEnvironment environment, string assemblyFolder, string sourceFile)
        {
            _environment = environment;
            _assemblyFolder = assemblyFolder;
            _sourceFile = sourceFile;
        }

        /// <summary>
        /// Execute the command 
        /// </summary>
        /// <returns>Returns 0 for success, otherwise 1.</returns>
        public int Execute()
        {
            if (!ValidateSourceFile())
            {
                return 1;
            }

            var accessor = new CacheContextAccessor();
            var cache = new Cache(accessor);

            var finder = new DependencyFinder(accessor, cache, _environment, _assemblyFolder);

            // TODO: accept project other than klr.core45.managed, for example, klr.net45.managed?
            ICollection<string> tpa = finder.GetContractDependencies("klr.core45.managed");

            UpdateSourceFile(tpa.ToArray());

            return 0;
        }

        private bool ValidateSourceFile()
        {
            if (string.IsNullOrEmpty(_sourceFile))
            {
                return false;
            }

            if (!File.Exists(_sourceFile))
            {
                return false;
            }

            return true;
        }

        private bool UpdateSourceFile(string[] tpa)
        {
            var content = new List<string>();
            content.AddRange(File.ReadAllLines(_sourceFile));

            FillTpaCount(tpa, content);

            FillTpaList(
                tpa,
                content,
                ".ni.dll",
                "// MARK: begin tpa native image list",
                "// MARK: end tpa native image list");

            FillTpaList(
                tpa,
                content,
                ".dll",
                "// MARK: begin tpa list",
                "// MARK: end tpa list");

            File.WriteAllLines(_sourceFile, content.ToArray());

            return true;
        }

        private void FillTpaCount(string[] tpa, List<string> content)
        {
            var head =  FindLineOfCode(content, "// MARK: begin tpa list size") + 1;
            var tail =  FindLineOfCode(content, "// MARK: end tpa list size");

            content.RemoveRange(head, tail - head);
            content.Insert(
                head++,
                "    // updated on UTC " + DateTime.UtcNow.ToString("yyyy/MM/dd hh:mm:ss"));
            content.Insert(
                head++,
                string.Format("    const size_t count = {0};", tpa.Length));
        }

        private void FillTpaList(string[] tpa, List<string> content, string suffix, string beginMark, string endMark)
        {
            var head =  FindLineOfCode(content,  beginMark) + 1;
            var tail =  FindLineOfCode(content, endMark);
            content.RemoveRange(head, tail - head);
            content.Insert(
                head++,
                "        // updated on UTC " + DateTime.UtcNow.ToString("yyyy/MM/dd hh:mm:ss"));

            for (int i = 0; i < tpa.Length; ++i)
            {
                content.Insert(
                    head + i,
                    string.Format("        pArray[{0}] = _wcsdup(L\"{1}{2}\");", i, tpa[i], suffix));
            }
        }

        private int FindLineOfCode(List<string> content, string pattern)
        {
            return content.FindIndex(line => line.Trim().Equals(pattern, StringComparison.Ordinal));
        }
    }
}