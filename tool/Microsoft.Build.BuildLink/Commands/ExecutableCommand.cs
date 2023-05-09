﻿using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.Build.BuildLink.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Build.BuildLink.Commands;

public interface ICommandExecutor
{ }

public interface ICommandExecutor<in TArgs> : ICommandExecutor where TArgs : class
{
    Task<BuildLinkErrorCode> ExecuteAsync(TArgs args, CancellationToken cancellationToken);
}

internal abstract class ExecutableCommand<TArgs, THandler> : Command, ICommandHandler
    where TArgs : class
    where THandler : ICommandExecutor<TArgs>
{
    protected ExecutableCommand(string name, string? description = null)
        : base(name, description)
    {
        Handler = this;
    }

    /// <summary>
    /// Parses the context from <see cref="ParseResult"/>.
    /// </summary>
    protected internal abstract TArgs ParseContext(ParseResult parseResult);

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        ParseResult parseResult = context.ParseResult;
        TArgs arguments = ParseContext(parseResult);

        IHost host = context.GetHost();
        BuildLinkErrorCode returnCode;
        try
        {
            THandler handler = host.Services.GetRequiredService<THandler>();
            returnCode = await handler.ExecuteAsync(arguments, context.GetCancellationToken()).ConfigureAwait(false);

            //returnCode = await ExecuteAsync(arguments, host, context.GetCancellationToken()).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            ILogger? logger = host.Services.GetService<ILogger<ExecutableCommand<TArgs, THandler>>>();

            logger?.LogError("Executed command failed");
            logger?.LogError(e.Message);
            BuildLinkException? ex = e as BuildLinkException;
            returnCode = ex?.BuildLinkErrorCode ?? BuildLinkErrorCode.InternalError;
        }

        if (returnCode != BuildLinkErrorCode.Success && parseResult.GetConsoleVerbosityOptionOrDefault().ToLogLevel() < LogLevel.None)
        {
            IStderrWriter? writer = host.Services.GetService<IStderrWriter>();

            writer?.WriteLine();
            writer?.WriteLine(
                $"For details on the exit code, refer to https://aka.ms/build-link/exit-codes#{(int)returnCode}");
        }

        return (int)returnCode;
    }

    /// <inheritdoc/>
    public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();
}
