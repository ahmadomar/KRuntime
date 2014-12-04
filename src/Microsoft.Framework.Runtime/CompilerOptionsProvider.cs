// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.Versioning;

namespace Microsoft.Framework.Runtime
{
    /// <summary>
    /// Default implementation for <see cref="ICompilationOptionsProvider"/>.
    /// </summary>
    public class CompilerOptionsProvider : ICompilationOptionsProvider
    {
        /// <inheritdoc />
        public ICompilerOptions GetCompilerOptions(string projectPath, FrameworkName targetFramework, string configurationName)
        {
            Project project;
            if (Project.TryGetProject(projectPath, out project))
            {
                return project.GetCompilerOptions(targetFramework, configurationName);
            }

            return new CompilerOptions();
        }
    }
}