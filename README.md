# Shuttle.Core.Threading

Provides various classes and interfaces to facilitate thread-based processing using task-based asynchronous patterns.

## Installation

```bash
dotnet add package Shuttle.Core.Threading
```

## Overview

This library enables you to create thread pools that continuously execute processor implementations. Each processor runs in a loop, performing work and utilizing configurable idle strategies when no work is available. The library uses dependency injection and supports multiple thread pools with different service keys.

## Core Components

### `IProcessor`

Implement this interface to define the work that will be executed by processor threads:

```c#
public interface IProcessor
{
    ValueTask<bool> ExecuteAsync(CancellationToken cancellationToken = default);
}
```

The return value indicates whether work was performed (`true`) or not (`false`), which is used by the idle strategy to determine thread behavior.

### `IProcessorContext`

Available via dependency injection within your processor implementation, providing context about the current execution:

```c#
public interface IProcessorContext
{
    string ServiceKey { get; }
    int ManagedThreadId { get; }
}
```

### `ProcessorThreadPool`

Manages a pool of processor threads that execute your `IProcessor` implementation. It implements the `IProcessorThreadPool` interface:

```c#
public interface IProcessorThreadPool : IDisposable, IAsyncDisposable
{
    string ServiceKey { get; }
    IEnumerable<ProcessorThread> ProcessorThreads { get; }
    int ThreadCount { get; }
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
```


```c#
public class ProcessorThreadPool(
    string serviceKey,
    int threadCount,
    IServiceScopeFactory serviceScopeFactory,
    ThreadingOptions threadingOptions,
    IProcessorIdleStrategy processorIdleStrategy,
    ILoggerFactory? loggerFactory = null
) : IProcessorThreadPool
```

**Parameters:**
- `serviceKey`: Identifier used for keyed service resolution and configuration
- `threadCount`: Number of processor threads in the pool (must be > 0)
- `serviceScopeFactory`: Factory for creating service scopes for each processor execution
- `threadingOptions`: Configuration options including events and timeouts
- `processorIdleStrategy`: Strategy for handling idle periods when no work is performed

## Configuration

### Service Registration

Register threading services in your dependency injection container:

```c#
services.AddThreading(builder =>
{
    builder.ConfigureThreading(options =>
    {
        options.JoinTimeout = TimeSpan.FromSeconds(30);
    });
    
    builder.ConfigureProcessorIdle("my-processor", options =>
    {
        options.Durations = new List<TimeSpan>
        {
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromSeconds(1)
        };
    });
});

// Register your processor implementation with a service key
services.AddKeyedScoped<IProcessor, MyProcessor>("my-processor");
```

### `ThreadingOptions`

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `JoinTimeout` | `TimeSpan` | `00:00:15` | Duration to wait for processor threads to stop gracefully |
| `ProcessorThreadCreated` | `AsyncEvent` | - | Raised when a processor thread is created |
| `ProcessorThreadActive` | `AsyncEvent` | - | Raised when a processor thread becomes active |
| `ProcessorThreadStarting` | `AsyncEvent` | - | Raised when a processor thread is starting |
| `ProcessorThreadStopping` | `AsyncEvent` | - | Raised when a processor thread is stopping |
| `ProcessorThreadStopped` | `AsyncEvent` | - | Raised when a processor thread has stopped |
| `ProcessorThreadOperationCanceled` | `AsyncEvent` | - | Raised when a processor operation is canceled |
| `ProcessorExecuting` | `AsyncEvent` | - | Raised before processor execution |
| `ProcessorExecuted` | `AsyncEvent` | - | Raised after processor execution |
| `ProcessorException` | `AsyncEvent` | - | Raised when a processor throws an exception |

### `ProcessorIdleOptions`

Configures the idle strategy behavior when processors return `false` (no work performed):

```c#
public class ProcessorIdleOptions
{
    public List<TimeSpan> Durations { get; set; } = [];
}
```

The `Durations` list defines progressive wait times. When no work is performed, the thread waits for increasing durations from this list before retrying.

## Usage Example

### 1. Implement `IProcessor`

```c#
public class MyProcessor : IProcessor
{
    private readonly IProcessorContext _context;
    private readonly ILogger<MyProcessor> _logger;
    private readonly IMyWorkQueue _workQueue;

    public MyProcessor(
        IProcessorContext context,
        ILogger<MyProcessor> logger,
        IMyWorkQueue workQueue)
    {
        _context = context;
        _logger = logger;
        _workQueue = workQueue;
    }

    public async ValueTask<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing on thread {ThreadId} with service key {ServiceKey}",
            _context.ManagedThreadId,
            _context.ServiceKey);

        var workItem = await _workQueue.DequeueAsync(cancellationToken);

        if (workItem == null)
        {
            return false; // No work available
        }

        await ProcessWorkItemAsync(workItem, cancellationToken);
        return true; // Work was performed
    }

    private async Task ProcessWorkItemAsync(WorkItem item, CancellationToken cancellationToken)
    {
        // Process the work item
        await Task.Delay(100, cancellationToken);
    }
}
```

### 2. Register Services

```c#
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddThreading(threadingBuilder =>
{
    threadingBuilder.ConfigureThreading(options =>
    {
        options.JoinTimeout = TimeSpan.FromSeconds(30);
        
        // Subscribe to events
        options.ProcessorException.Subscribe(async args =>
        {
            Console.WriteLine($"Processor exception: {args.Exception.Message}");
        });
    });

    threadingBuilder.ConfigureProcessorIdle("my-processor", options =>
    {
        options.Durations = new List<TimeSpan>
        {
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5)
        };
    });
});

builder.Services.AddKeyedScoped<IProcessor, MyProcessor>("my-processor");
builder.Services.AddSingleton<IMyWorkQueue, MyWorkQueue>();

var host = builder.Build();
await host.RunAsync();
```

### 3. Create and Manage Thread Pool

```c#
public class MyHostedService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ThreadingOptions _threadingOptions;
    private readonly IProcessorIdleStrategy _processorIdleStrategy;
    private ProcessorThreadPool? _threadPool;

    public MyHostedService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<ThreadingOptions> threadingOptions,
        IProcessorIdleStrategy processorIdleStrategy)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _threadingOptions = threadingOptions.Value;
        _processorIdleStrategy = processorIdleStrategy;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _threadPool = new ProcessorThreadPool(
            "my-processor",
            threadCount: 5,
            _serviceScopeFactory,
            _threadingOptions,
            _processorIdleStrategy);

        await _threadPool.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_threadPool != null)
        {
            await _threadPool.StopAsync(cancellationToken);
            await _threadPool.DisposeAsync();
        }
    }
}
```

## Idle Strategies

### `IProcessorIdleStrategy`

Controls thread behavior when processors return `false` (no work performed):

```c#
public interface IProcessorIdleStrategy
{
    Task SignalAsync(string serviceKey, bool workPerformed, CancellationToken cancellationToken = default);
}
```

**Built-in Implementations:**

- **DefaultProcessorIdleStrategy**: Uses progressive wait durations from `ProcessorIdleOptions`. When no work is performed, waits for increasing durations before retrying.
- **NullProcessorIdleStrategy**: No idle behavior; threads immediately retry without waiting.

## Event Handling

Subscribe to events to monitor and react to processor lifecycle and execution:

```c#
services.AddThreading(builder =>
{
    builder.ConfigureThreading(options =>
    {
        options.ProcessorExecuting.Subscribe(async args =>
        {
            Console.WriteLine($"Executing processor on thread {args.ManagedThreadId}");
        });

        options.ProcessorExecuted.Subscribe(async args =>
        {
            Console.WriteLine($"Executed processor. Work performed: {args.WorkPerformed}");
        });

        options.ProcessorException.Subscribe(async args =>
        {
            Console.WriteLine($"Exception on thread {args.ManagedThreadId}: {args.Exception.Message}");
        });
    });
});
```

## Advanced Scenarios

### Multiple Thread Pools

You can create multiple thread pools with different service keys and configurations:

```c#
// Register multiple processors
services.AddKeyedScoped<IProcessor, HighPriorityProcessor>("high-priority");
services.AddKeyedScoped<IProcessor, LowPriorityProcessor>("low-priority");

// Configure different idle strategies
builder.ConfigureProcessorIdle("high-priority", options =>
{
    options.Durations = new List<TimeSpan> { TimeSpan.FromMilliseconds(50) };
});

builder.ConfigureProcessorIdle("low-priority", options =>
{
    options.Durations = new List<TimeSpan> { TimeSpan.FromSeconds(5) };
});

// Create separate thread pools
var highPriorityPool = new ProcessorThreadPool("high-priority", 10, ...);
var lowPriorityPool = new ProcessorThreadPool("low-priority", 2, ...);
```

## Thread Safety

- `ProcessorThreadPool` uses internal locking (`SemaphoreSlim`) for thread-safe start/stop operations
- Each processor execution runs in its own service scope
- `ProcessorContext` is scoped per execution and accessed via `ProcessorContextAccessor`