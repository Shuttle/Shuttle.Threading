using System;
using Microsoft.Extensions.Logging;

namespace Shuttle.Threading;

public static class LogMessage
{
    private static readonly Action<ILogger, string, int, Exception?> ProcessorThreadActiveDelegate =
        LoggerMessage.Define<string, int>(LogLevel.Trace, new(1000, nameof(ProcessorThreadActive)), "Processor thread active for service key {ServiceKey} (managed thread id {ManagedThreadId})");

    private static readonly Action<ILogger, string, int, Exception?> ProcessorThreadStoppedDelegate =
        LoggerMessage.Define<string, int>(LogLevel.Trace, new(1001, nameof(ProcessorThreadStopped)), "Processor thread stopped for service key {ServiceKey} (managed thread id {ManagedThreadId})");

    private static readonly Action<ILogger, string, int, Exception?> ProcessorThreadStartingDelegate =
        LoggerMessage.Define<string, int>(LogLevel.Trace, new(1002, nameof(ProcessorThreadStarting)), "Processor thread starting for service key {ServiceKey} (managed thread id {ManagedThreadId})");

    private static readonly Action<ILogger, string, string, int, Exception?> ProcessorExecutingDelegate =
        LoggerMessage.Define<string, string, int>(LogLevel.Trace, new(1003, nameof(ProcessorExecuting)), "Processor {ProcessorFullName} for service key {ServiceKey} executing (managed thread id {ManagedThreadId})");

    private static readonly Action<ILogger, string, string, int, Exception?> ProcessorExecutedDelegate =
        LoggerMessage.Define<string, string, int>(LogLevel.Trace, new(1004, nameof(ProcessorExecuted)), "Processor {ProcessorFullName} for service key {ServiceKey} executed (managed thread id {ManagedThreadId})");

    private static readonly Action<ILogger, string, int, Exception?> ProcessorThreadOperationCanceledDelegate =
        LoggerMessage.Define<string, int>(LogLevel.Trace, new(1005, nameof(ProcessorThreadOperationCanceled)), "Processor thread operation canceled for service key {ServiceKey} (managed thread id {ManagedThreadId})");

    private static readonly Action<ILogger, string, int, string, Exception?> ProcessorExceptionDelegate =
        LoggerMessage.Define<string, int, string>(LogLevel.Trace, new(1006, nameof(ProcessorException)), "Processor exception for service key {ServiceKey} (managed thread id {ManagedThreadId}): {Exception}");

    private static readonly Action<ILogger, string, int, Exception?> ProcessorThreadStoppingDelegate =
        LoggerMessage.Define<string, int>(LogLevel.Trace, new(1007, nameof(ProcessorThreadStopping)), "Processor thread stopping for service key {ServiceKey} (managed thread id {ManagedThreadId})");

    public static void ProcessorThreadActive(ILogger logger, string serviceKey, int managedThreadId) =>
        ProcessorThreadActiveDelegate(logger, serviceKey, managedThreadId, null);

    public static void ProcessorThreadStopped(ILogger logger, string serviceKey, int managedThreadId) =>
        ProcessorThreadStoppedDelegate(logger, serviceKey, managedThreadId, null);

    public static void ProcessorThreadStarting(ILogger logger, string serviceKey, int managedThreadId) =>
        ProcessorThreadStartingDelegate(logger, serviceKey, managedThreadId, null);

    public static void ProcessorExecuting(ILogger logger, string serviceKey, string processorFullName, int managedThreadId) =>
        ProcessorExecutingDelegate(logger, serviceKey, processorFullName, managedThreadId, null);

    public static void ProcessorExecuted(ILogger logger, string serviceKey, string processorFullName, int managedThreadId) =>
        ProcessorExecutedDelegate(logger, serviceKey, processorFullName, managedThreadId, null);

    public static void ProcessorThreadOperationCanceled(ILogger logger, string serviceKey, int managedThreadId) =>
        ProcessorThreadOperationCanceledDelegate(logger, serviceKey, managedThreadId, null);

    public static void ProcessorException(ILogger logger, string serviceKey, int managedThreadId, Exception exception) =>
        ProcessorExceptionDelegate(logger, serviceKey, managedThreadId, exception.Message, exception);

    public static void ProcessorThreadStopping(ILogger logger, string serviceKey, int managedThreadId) =>
        ProcessorThreadStoppingDelegate(logger, serviceKey, managedThreadId, null);
}
