// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace NuGet
{
    public interface IVersionSpec
    {
        SemanticVersion MinVersion { get; }
        bool IsMinInclusive { get; }
        SemanticVersion MaxVersion { get; }
        bool IsMaxInclusive { get; }
    }
}
