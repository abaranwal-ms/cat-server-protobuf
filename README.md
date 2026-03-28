# cat-server-protobuf

A simple demo of **REST + Protobuf** using .NET 9 — no gRPC required.

## What it does

The cat is hungry. You feed it via HTTP, and it tells you its mood.

- **CatClient** sends an HTTP `PUT /food` request whose body is binary Protobuf.
- **CatServer** deserializes the request, picks a mood based on the food, and replies with a Protobuf-encoded `CatResponse`.
- **CatProto** holds the shared `.proto` schema that both projects reference.

## Project structure

```
CatServer.sln
├── CatProto/          # Shared Protobuf schema (cat.proto) + generated C# classes
├── CatServer/         # ASP.NET Core Minimal API — listens on http://localhost:5099
└── CatClient/         # Console app that sends a PUT /food request
```

## How REST + Protobuf works

```
Client                              Server
  │                                   │
  │── HTTP PUT /food ────────────────>│
  │   Content-Type: application/x-protobuf
  │   Body: [0A 04 74 75 6E 61]  ← "tuna" in protobuf binary
  │                                   │
  │<── HTTP 200 ──────────────────────│
  │    Body: protobuf CatResponse     │
  │    { status_code:200, message:"meow meow", cat_mood:"ecstatic 😻" }
```

HTTP provides the verb (`PUT`) and path (`/food`). Protobuf only carries the business data — making the payload compact and schema-validated.

## Getting started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)

### Run the server

```bash
dotnet run --project CatServer
```

### Run the client (in a separate terminal)

```bash
dotnet run --project CatClient
```

### Expected client output

```
🐱 Cat Client — feeding the cat via REST + Protobuf

── Outgoing protobuf bytes ──────────────────
   Total size : 6 bytes
   Hex dump   : 0A-04-74-75-6E-61
   (compare to JSON: 15 bytes)
📡 Sending PUT /food …

── Response from Cat Server ─────────────────
   HTTP status  : 200
   Proto status : 200
   Message      : meow meow
   Cat mood     : ecstatic 😻
   Wire size    : 22 bytes

✅ Done! The cat has been fed.
```

## Cat moods

| Food     | Mood           |
|----------|----------------|
| tuna     | ecstatic 😻    |
| chicken  | happy 😺       |
| kibble   | meh 😼         |
| anything else | curious 🙀 |
