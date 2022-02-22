﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace vstest.ProgrammerTests;

using System.Reflection;

internal class Program
{
    static void Main()
    {
        Intent.Console.Program.Main(new[] { Assembly.GetExecutingAssembly().Location });
    }
}
