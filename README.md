# AsyncNet

A .NET library for **deterministic testing of time-dependent async code** through injectable time and scheduling abstractions.

Instead of making tests wait for real timers, inject `MockTimeLibrary` and advance the clock manually. Tests run synchronously, instantly, and without flakiness.

## Packages

| Package | Description |
|---|---|
| `AsyncNet` | Core interfaces: `ITimeLibrary`, `ICancellationToken`, `ITimer`, `IEventHandle` |
| `AsyncNet.Mock` | Test doubles: `MockTimeLibrary`, manual clock control |
| `AsyncNet.Real` | Production implementations wrapping `Task.Delay` and .NET BCL |
| `AsyncNet.Scheduler` | Single-threaded async scheduler with `SynchronizationContext` |

## Installation

```bash
# Core abstractions (required in production code)
dotnet add package AsyncNet

# Test doubles (reference from test projects only)
dotnet add package AsyncNet.Mock

# Production implementations
dotnet add package AsyncNet.Real

# Async scheduler
dotnet add package AsyncNet.Scheduler
```

## Quick Start

### 1. Write production code against the interfaces

```csharp
public class MyService
{
    public async Task RunAsync(ICancellationToken timeout)
    {
        await Time.Delay(5000, timeout);   // uses the injected ITimeLibrary
        DoWork();
    }
}
```

### 2. In tests, inject the mock and control time manually

```csharp
[TestInitialize]
public void Setup()
{
    _scheduler   = new ControlledContextAsyncScheduler();
    _timeLibrary = new MockTimeLibrary();
    Time.SetTimeLibrary(_timeLibrary);
    AsyncScheduler.SetScheduler(_scheduler);
}

[TestCleanup]
public void Cleanup()
{
    _timeLibrary.Dispose();
    _scheduler.Dispose();
}

[TestMethod]
public async Task ShouldFault_WhenTimeoutElapses()
{
    var started = new TaskCompletionSource();

    var task = AsyncScheduler.Post(async () =>
    {
        var ct = Time.GetCancellationToken(2000);
        started.SetResult();
        await ct.Task;             // waits for the mock token
    });

    await started.Task;            // ensure the async task has reached its await

    _timeLibrary.Advance(2000);    // instantly advance the virtual clock

    await Assert.ThrowsExactlyAsync<TimeoutException>(() => task);
}
```

## Core Concepts

### ITimeLibrary / Time

The central abstraction. All time-dependent operations go through it.

```csharp
// Production setup
Time.SetTimeLibrary(new RealTimeLibrary());

// Test setup
Time.SetTimeLibrary(new MockTimeLibrary());
```

| Method | Description |
|---|---|
| `Time.CurrentTime` | Current time in milliseconds |
| `Time.Delay(ms)` | Async delay |
| `Time.Delay(ms, token)` | Async delay with cancellation |
| `Time.GetCancellationToken(ms)` | Token that times out after `ms` milliseconds |
| `Time.GetTimer(dueTime)` | Repeating timer |
| `Time.GetEventHandle(dueTime)` | Manual-reset event with optional timeout |

### MockTimeLibrary

Provides full control over the virtual clock in tests.

```csharp
var lib = new MockTimeLibrary();
Time.SetTimeLibrary(lib);

var delayTask = Time.Delay(1000);       // registers a pending sleep expiring at t=1000

lib.Advance(500);   // t=500 — delayTask still pending
lib.Advance(500);   // t=1000 — delayTask completes
await delayTask;
```

**Key behaviour:**
- `Advance(ms)` notifies all pending observers (sleeps, tokens, timers, event handles) synchronously
- Observers that expire are automatically removed
- Thread-safe

### ICancellationToken

A testable replacement for .NET's `CancellationToken` that integrates with the virtual clock.

```csharp
var ct = Time.GetCancellationToken(3000);  // expires at CurrentTime + 3000 ms

// Await the timeout — throws TimeoutException when the clock reaches the expiry
await ct.Task;

// Or cancel manually — throws TaskCanceledException
ct.Cancel();

// Inspect state
bool cancelled = ct.IsCancellationRequested;
```

### IAsyncScheduler / AsyncScheduler

A single-threaded scheduler that serialises async tasks for deterministic execution.

```csharp
var task = AsyncScheduler.Post(async () =>
{
    await Time.Delay(100);
    return 42;
});

_timeLibrary.Advance(100);
var result = await task;   // == 42
```

### Waiting for a task to start before advancing time

Always ensure the posted async work has reached its first `await` before calling `Advance`. Two patterns:

**Pattern 1 — started TCS** (clearest intent, preferred when you control the lambda):
```csharp
var started = new TaskCompletionSource();

var task = AsyncScheduler.Post(async () =>
{
    var delay = Time.Delay(1000);
    started.SetResult();      // signal before awaiting
    await delay;
});

await started.Task;           // wait until the delay is registered
_timeLibrary.Advance(1000);
await task;
```

## Building and Testing

```bash
# Build
dotnet build AsyncNet.sln

# Run all tests
dotnet test AsyncNet.sln

# Run a single test
dotnet test AsyncNet.Test/AsyncNet.Test.csproj --filter "FullyQualifiedName~WorkflowA"
```

## Project Structure

```
AsyncNet/
├── AsyncNet/              Core abstractions (ITimeLibrary, ICancellationToken, …)
├── AsyncNet.Mock/         Test doubles (MockTimeLibrary, MockedSleep, …)
├── AsyncNet.Real/         Production implementations (RealTimeLibrary, …)
├── AsyncNet.Scheduler/    Async scheduler (ControlledContextAsyncScheduler, …)
└── AsyncNet.Test/         Unit tests
```

**Dependency graph (no circular dependencies):**
```
AsyncNet
├── AsyncNet.Mock      → AsyncNet
├── AsyncNet.Real      → AsyncNet
└── AsyncNet.Scheduler
```

## License

This project is licensed under the GNU General Public License v3.0. See [LICENSE](LICENSE) for details.
