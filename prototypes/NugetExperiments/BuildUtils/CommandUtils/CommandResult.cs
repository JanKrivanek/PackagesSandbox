﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace BuildUtils.CommandUtils
{
    internal readonly struct CommandResult
    {
        internal static readonly CommandResult Empty;

        internal CommandResult(ProcessStartInfo startInfo, int exitCode, string? stdOut, string? stdErr)
        {
            StartInfo = startInfo;
            ExitCode = exitCode;
            StdOut = stdOut;
            StdErr = stdErr;
        }

        internal ProcessStartInfo StartInfo { get; }

        internal int ExitCode { get; }

        internal string? StdOut { get; }

        internal string? StdErr { get; }

        public override string ToString() => $"""
Exit Code: {ExitCode}
Std Out:
=================================
{StdOut}
=================================
Std Err:
=================================
{StdErr}
=================================
""";
    }
}