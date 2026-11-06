# AsyncProductApi

A small ASP.NET Core Minimal API that shows the **Asynchronous Request-Reply pattern**. Some work takes too long to finish within one HTTP request. With this pattern, the API accepts the request straight away, hands back a way to check on its progress, and later points the client to the finished result.

## The main concept

A normal synchronous API keeps the client waiting until the work is finished. That breaks down when processing takes seconds or minutes: connections time out, threads stay busy, and clients can't tell what's happening.

The Asynchronous Request-Reply pattern breaks the interaction into three steps:

```
 Client                                   API
   │  1. POST /api/v1/products              │
   │ ─────────────────────────────────────► │  Save the request, mark it "ACCEPT"
   │  202 Accepted + Location: status URL   │
   │ ◄───────────────────────────────────── │
   │                                        │
   │  2. GET /api/v1/productstatus/{id}     │   (poll until done)
   │ ─────────────────────────────────────► │
   │  200 OK { status, estimatedTime }      │  while still processing
   │ ◄───────────────────────────────────── │
   │  302 Redirect → result URL             │  once status is "COMPLETE"
   │ ◄───────────────────────────────────── │
   │                                        │
   │  3. GET /api/v1/products/{id}          │
   │ ─────────────────────────────────────► │
   │  200 OK  final result                  │
   │ ◄───────────────────────────────────── │
```

1. **Start.** The client submits work. The API saves it and immediately returns **`202 Accepted`** with a `Location` header that points to a status endpoint.
2. **Poll.** The client calls the status endpoint. While the work is still running, the endpoint returns the current status and an estimated completion time. When the work is done, it **redirects** to the resource holding the result.
3. **Fetch.** The client follows the redirect and gets the final result.

The client never holds a connection open while the work runs, and the server can process requests in the background on its own schedule.

## Tech stack

- .NET 10 / ASP.NET Core Minimal APIs
- Entity Framework Core 10 with SQLite
- EF Core migrations

## Project structure

```
AsyncProductApi/
├── Program.cs              # App setup and the three endpoints
├── Data/
│   └── AppdDbContext.cs    # AppDbContext with the ListingRequests DbSet
├── Models/
│   └── ListingRequest.cs   # Stored request: body, status, ETA, RequestId (GUID)
├── Dtos/
│   └── ListingStatus.cs    # Response shape for the status endpoint
├── Migrations/             # EF Core migrations
└── Properties/
    └── launchSettings.json # Local URLs (http://localhost:5114, https://localhost:7106)
```

## Endpoints

| Method | Route                                  | Purpose                                                                                     |
|--------|----------------------------------------|---------------------------------------------------------------------------------------------|
| POST   | `/api/v1/products`                     | Submits a listing request. Returns `202 Accepted` and the location of the status endpoint.  |
| GET    | `/api/v1/productstatus/{requestId}`    | Returns the request's status, or redirects to the result once it is `COMPLETE`.             |
| GET    | `/api/v1/products/{requestId}`         | Returns the final result (currently a placeholder message).                                 |

### Example request

```http
POST https://localhost:7106/api/v1/products
Content-Type: application/json

{
  "requestBody": "New product listing payload"
}
```

The response is `202 Accepted`. Its body is the stored `ListingRequest`, including its `requestId`, `requestStatus: "ACCEPT"` and `estimatedCompletionTime`.

### Example status response (still processing)

```json
{
  "requestStatus": "ACCEPT",
  "estimatedCompletionTime": "2026-10-31:21:00:00",
  "resourceUrl": ""
}
```

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

### Run it

```bash
# Restore packages
dotnet restore

# Create the SQLite database from the migrations
dotnet ef database update

# Start the API
dotnet run --launch-profile https
```

The API listens on `https://localhost:7106` and `http://localhost:5114`.

## About this demo

This project shows **how the Asynchronous Request-Reply pattern works**. It isn't a production implementation, so a few parts are deliberately simplified:

- **No real processing.** No background work runs. Requests stay `ACCEPT` unless you change the status to `COMPLETE` in the database yourself, which makes the status endpoint redirect to the result.
- **Fixed completion time.** `EstimatedCompletionTime` is a hard-coded example value.
- **Placeholder result.** The final endpoint returns a fixed message to show where the real result would go.
