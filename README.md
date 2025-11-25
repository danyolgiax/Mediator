# Mediator

A lightweight mediator system that orchestrates C# request handlers for clean, decoupled application logic.

## Features

- Simple and extensible mediator pattern implementation
- Supports request/response and notification handlers
- Decouples business logic from infrastructure
- Targets .NET 8

## Installation

Install via NuGet:

```
dotnet add package Doxo.Mediator
```

Or use the NuGet Package Manager in Visual Studio.

## Getting Started

### 1. Define a Request

Create a request class implementing `IRequest<TResponse>`:

```csharp
public class PingRequest : IRequest<string>
{
    public string Message { get; set; }
}
```

### 2. Create a Handler

Implement a handler for your request:

```csharp
public class PingHandler : IRequestHandler<PingRequest, string>
{
    public Task<string> Handle(PingRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Pong: {request.Message}");
    }
}
```

### 3. Register Handlers

Register your handlers in the dependency injection container (e.g., in `Program.cs`):

```csharp
services.AddMediator(typeof(PingHandler).Assembly);
```

### 4. Send Requests

Inject `IMediator` and send requests:

```csharp
public class MyService
{
    private readonly IMediator _mediator;

    public MyService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<string> SendPing(string message)
    {
        var response = await _mediator.Send(new PingRequest { Message = message });
        return response;
    }
}
```

## Notifications

You can also publish notifications to multiple handlers:

```csharp
public class MyNotification : INotification
{
    public string Info { get; set; }
}

public class MyNotificationHandler : INotificationHandler<MyNotification>
{
    public Task Handle(MyNotification notification, CancellationToken cancellationToken)
    {
        // Handle notification
        return Task.CompletedTask;
    }
}
```

Publish a notification:

```csharp
await _mediator.Publish(new MyNotification { Info = "Hello" });
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

Distributed under the MIT License. See [LICENSE](LICENSE) for details.
