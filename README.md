# [OpenEdge™](openedge) Connector
An RPC endpoint for interfacing with [OpenEdge™](openedge)/[RAW](raw). The goal is to provide decoupled RPC connectivity with [OpenEdge™](openedge) regardless of what ecosystem the microservice runs on.
This should vastly increase development speed and production resource usage through the use of docker containers.

## Features
* Centralized access to [OpenEdge™](openedge)/[RAW](raw)
* Caching of procedures using in-memory storage via [Redis](redis)
* Logging to the EK-stack
  * [Serilog](https://serilog.net/) with an [Elasticsearch Sink](https://github.com/serilog/serilog-sinks-elasticsearch)
* Crossplatform access through [SignalR](signalr)

## Technologies
* [SignalR](signalr) - RPC interface
  * [ASP.NET 5 with SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr) - Interface service
    * Compatibility mode - Access to the .NET Framework DLL provided by RAW
  * SignalR DLL - .NET applications to interface
  * SignalR node package - NodeJS communication with OpenEdge
* [MessagePack](https://msgpack.org/index.html) - Fast and efficient transport, it's JSON but even smaller.
* [Redis](redis) - Efficient caching for procedure responses
* [Windows Server Core](https://hub.docker.com/_/microsoft-windows-servercore) on [Docker](https://www.docker.com/)

## Connection requests
After the connection is initialized, the client will need to state the microservice it's representing. The server will then communicate back the connection details
```jsonc
/*
  Request
*/
{
  "application": "{{Microservice App Name}}",
  // Allow test access, all access will be blocked unless it is explicitly allowed here first
  "test": true
}

/*
  Response
*/
{
  // Connection identfier, purely for structured logging on the client.
  "connectionId": "{{ConnectionId}}"
}
```


## Procedure requests
### Input structure
The following structure is expressed in JSON, though through RPC this will be sent in the MessagePack format using our libraries.
```jsonc
{
  // Name of the procedure to be called in OpenEdge
  "proc": "{{Procedure Name}}",
  // Parameters to provide with the procedure
  "parm": [
    { "pos": 1, "value": "{{Value}}" },
     // "redact": Redact the input value from any logging.
    { "pos": 2, "value": "{{Value}}", "redact": true },
    // "out": Optional parameter, specifies the output field. No value is required
    { "pos": 3, "out": true }
  ],
  // Time in milliseconds the procedure should be cached if not cached already, 0 bypasses caching
  // Always bypass caching for confidential information
  "cache": 0
}
```

### Output structure
The following structure is expressed in JSON, though through RPC this will be sent in the MessagePack format using our libraries.
```jsonc
{
  // Name of the procedure that was called in OpenEdge
  "proc": "{{Procedure Name}}",
  // Response statuscode, TBD: Whether or not this is required is to be verified when researching SignalR.
  "status": 0,
  // null if none/at failure
  "result": {
    // The response will always be a binary array
    "3": "{{Result}}"
  },
  // Age of the content, -1 if newly requested/not cached
  "age": -1,
  // Time taken to execute procedure and get result (ms)
  "elapsedTime": 135
}
```


[openedge]: (https://www.progress.com/openedge)
[raw]: (https://www.realdolmen.com/en/solution/raw)
[redis]: (https://redis.io/)
[signalr]: (https://en.wikipedia.org/wiki/SignalR)

## Logging formats
A couple of formats will be specified for structured logging.
The provided forats are subject to additional fields but should at least contain their respective following fields

### New Connections
| Field 	| Type 	| Description 	|
|-------	|------	|-------------	|
| ConnectionId | text | Connection identifier for a specific RPC client connection and request |
| IP | ip | IP Address off the connecting application |
| AppId | text | Application identifier used to authorize the application |


### Procedure execution
| Field 	| Type 	| Description 	|
|-------	|------	|-------------	|
| Procedure | text | The procedure name executed in the context  |
| ConnectionId | text | Connection identifier for a specific RPC client connection |
| RequestId | long | Request identifier identifying a specific task/request, most likely a procedure execution |
| Inputs | text | JSON representation of the input parameters **Warning: The inputs specified as redacted should be filtered out at all times!** |
| Date | date | Time the procedure was executed |

### Procedure completion
| Field 	| Type 	| Description 	|
|-------	|------	|-------------	|
| Procedure | text | The procedure name executed in the context  |
| ConnectionId | text | Connection identifier for a specific RPC client connection |
| RequestId | long | Request identifier identifying a specific task/request, most likely a procedure execution |
| Date | date | Time the procedure was completed |
| ElapsedTime | long | Time in milliseconds that elapsed between execution and completion |

### Procedure failure

| Field 	| Type 	| Description 	|
|-------	|------	|-------------	|
| Procedure | text | The procedure name executed in the context  |
| ConnectionId | text | Connection identifier for a specific RPC client connection |
| RequestId | long | Request identifier identifying a specific task/request, most likely a procedure execution |
| Exception | exception | The generic Serilog exception fields containing the error details provided by .NET |
