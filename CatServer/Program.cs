// ──────────────────────────────────────────────────────────────
// CatServer — ASP.NET Core Minimal API + Protobuf (no gRPC)
// ──────────────────────────────────────────────────────────────
//
// HOW IT WORKS
// ─────────────
// 1. Client sends an HTTP PUT to /food
// 2. The request BODY is raw protobuf bytes (Content-Type: application/x-protobuf)
// 3. Server reads those bytes → deserializes into a CatRequest object
// 4. Server builds a CatResponse, serializes to bytes, sends back
//
// This is "REST + Protobuf":
//   • HTTP verbs & paths   → REST
//   • Binary body          → Protobuf (instead of JSON)
//
// ──────────────────────────────────────────────────────────────

using Google.Protobuf;
using CatProto;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ── PUT /food endpoint ──────────────────────────────────────
app.MapPut("/food", async (HttpContext ctx) =>
{
    // ── STEP 1: Read raw bytes from the HTTP request body ──
    using var ms = new MemoryStream();
    await ctx.Request.Body.CopyToAsync(ms);
    byte[] requestBytes = ms.ToArray();

    // ── STEP 2: Deserialize protobuf bytes → CatRequest ──
    //
    // Under the hood protobuf stores data like this (simplified):
    //
    //   [field‑tag + wire‑type] [length] [UTF‑8 data]
    //
    // For CatRequest { item="tuna" } the wire bytes are:
    //   0A 04 74 75 6E 61      ← field 1 (item), length 4, "tuna"
    //
    // That's it — just 6 bytes! HTTP already tells us it's a PUT
    // to /food, so the protobuf body only carries business data.
    //
    var catRequest = CatRequest.Parser.ParseFrom(requestBytes);

    Console.WriteLine($"🐱 Cat received: PUT /food → \"{catRequest.Item}\"");

    // ── STEP 3: Build the response ──────────────────────────
    var catResponse = new CatResponse
    {
        StatusCode = 200,
        Message    = "meow meow",
        CatMood    = catRequest.Item.ToLower() switch
        {
            "tuna"    => "ecstatic 😻",
            "chicken" => "happy 😺",
            "kibble"  => "meh 😼",
            _         => "curious 🙀"
        }
    };

    // ── STEP 4: Serialize CatResponse → bytes & send ────────
    byte[] responseBytes = catResponse.ToByteArray();

    Console.WriteLine($"🐱 Responding: {catResponse.Message} (mood: {catResponse.CatMood})");
    Console.WriteLine($"📦 Response size: {responseBytes.Length} bytes (protobuf binary)");

    ctx.Response.ContentType = "application/x-protobuf";
    ctx.Response.StatusCode  = 200;
    await ctx.Response.Body.WriteAsync(responseBytes);
});

// ── Health check (plain text, for browser testing) ──────────
app.MapGet("/", () => "🐱 Cat Server is running! PUT /food to feed the cat.");

Console.WriteLine("🐱 Cat Server listening on http://localhost:5099");
Console.WriteLine("   PUT /food  →  send protobuf CatRequest, get protobuf CatResponse");
Console.WriteLine();

app.Run("http://localhost:5099");
