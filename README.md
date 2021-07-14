# [OpenEdge™](openedge) Connector
An RPC endpoint for interfacing with [OpenEdge™](openedge)/[RAW](raw). The goal is to provide decoupled RPC connectivity with [OpenEdge™](openedge) regardless of what ecosystem the microservice runs on.
This should vastly increase development speed and production resource usage through the use of docker containers.

## Features
* Centralized access to [OpenEdge™](openedge)/[RAW](raw)
* Caching of procedures using in-memory storage via [Redis](redis)
* Logging to the EK-stack
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

## Input structure
The following structure is expressed in JSON, though through RPC this will be sent in the MessagePack format using our libraries.
```jsonc
{
  "proc": "{{Procedure Name}}", // Name of the procedure to be called in OpenEdge
  "parm": [ // Parameters to provide with the procedure
    { "pos": 1, "value": "{{Value}}" },
    { "pos": 2, "value": "{{Value}}" },
    { "pos": 3, "out": true } // "out": Optional parameter, specifies the output field. No value is required
  ],
  "cache": 0 // Time the procedure should be cached if not cached already, 0 bypasses caching
}
```

## Output structure
The following structure is expressed in JSON, though through RPC this will be sent in the MessagePack format using our libraries.
```jsonc
{
  "proc": "{{Procedure Name}}", // Name of the procedure that was called in OpenEdge
  "status": 0, // Response statuscode, TBD: Whether or not this is required is to be verified when researching SignalR.
  "result": { // null if none
    "3": { // "3" is the output position entered in the input
      "type": "{{Value Type}}",
      "value": "{{Any Type: Result}}"
    }
  },
  "cache": 0 // Time the provided result is still being cached.
}
```


[openedge]: (https://www.progress.com/openedge)
[raw]: (https://www.realdolmen.com/en/solution/raw)
[redis]: (https://redis.io/)
[signalr]: (https://en.wikipedia.org/wiki/SignalR)