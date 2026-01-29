# NServiceBus Web App Example

A simple ASP.NET web application that demonstrates how to send messages to an NServiceBus endpoint using the QuickHostedEndpoint library.

## Overview

This web app provides a user interface and API endpoints to place orders, which are sent as NServiceBus commands to the `OrderProcessing` endpoint. It demonstrates:

- Configuring NServiceBus in an ASP.NET application
- Using Learning Transport for local development
- Sending commands to a separate worker endpoint
- Routing configuration

## Prerequisites

- .NET 10.0 SDK
- The `NServiceBusEndpoint` worker running (to process the messages)

## Running the Example

### Step 1: Start the NServiceBus Endpoint Worker

In one terminal, start the order processing endpoint:

```bash
cd examples/NServiceBusEndpoint
dotnet run
```

You should see output indicating the endpoint is running:
```
Starting NServiceBus endpoint: OrderProcessing
NServiceBus endpoint started successfully: OrderProcessing
```

### Step 2: Start the Web App

In another terminal, start the web application:

```bash
cd examples/NServiceBusWebApp
dotnet run
```

The web app will start on `http://localhost:5000` (or similar).

### Step 3: Place Orders

#### Using the Web UI

1. Open your browser to `http://localhost:5000`
2. Fill in the order form
3. Click "Place Order"
4. Check the NServiceBusEndpoint console for message processing output

#### Using the API

```bash
# Place an order via GET request
curl http://localhost:5000/api/orders
```

## Architecture

```
???????????????????????         ???????????????????????????
?   NServiceBusWebApp ?         ?  OrderProcessingEndpoint ?
?   (ASP.NET)         ?         ?  (Worker Service)        ?
???????????????????????         ???????????????????????????
?                     ?         ?                         ?
?  POST /orders       ???????>  ?  PlaceOrderHandler      ?
?  GET /api/orders    ?  (msg)  ?                         ?
?                     ?         ?  Publishes OrderPlaced  ?
???????????????????????         ???????????????????????????
         ?                                 ?
         ???????????????????????????????????
                      ?
              Learning Transport
              (File-based queue)
              %TEMP%\NServiceBusEndpointExample
```

## Configuration

Both the web app and the worker endpoint use the same Learning Transport storage directory, enabling message exchange:

```csharp
var learningTransportDirectory = Path.Combine(
    Path.GetTempPath(),
    "NServiceBusEndpointExample",
    ".learningtransport");
```

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/` | HTML form to place orders |
| POST | `/orders` | Submit order from form |
| GET | `/api/orders` | Place a test order via API |

## Learning Transport Note

?? **Important**: The Learning Transport is for development and testing only. For production, use a real transport like:

- Azure Service Bus
- RabbitMQ  
- Amazon SQS
- SQL Transport

## Troubleshooting

### Messages not being processed?

1. Ensure both applications are running
2. Verify they're using the same transport directory
3. Check the temp directory for `.learningtransport` folder

### Port already in use?

Change the port in `Program.cs` or use:
```bash
dotnet run --urls "http://localhost:5001"
```

## Related Examples

- [NServiceBusEndpoint](../NServiceBusEndpoint/README.md) - The worker endpoint that processes messages
