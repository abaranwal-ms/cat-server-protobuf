// ──────────────────────────────────────────────────────────────
// CatClient — HttpClient + Protobuf (no gRPC)
// ──────────────────────────────────────────────────────────────
//
// HOW IT WORKS
// ─────────────
// 1. Build a CatRequest protobuf object  (business data only!)
// 2. Serialize it to bytes  → catRequest.ToByteArray()
// 3. Send HTTP PUT /food with those bytes as the body
//    ↑ verb + path live in HTTP, NOT inside the protobuf
// 4. Read response bytes   → CatResponse.Parser.ParseFrom(bytes)
// 5. Print the cat's reply
//
// ──────────────────────────────────────────────────────────────

using System.Net.Http.Headers;
using Google.Protobuf;
using CatProto;

Console.WriteLine("🐱 Cat Client — feeding the cat via REST + Protobuf\n");

// ── Build the protobuf request ──────────────────────────────
//
// Notice: NO method or path here! Those are HTTP's job.
// The protobuf payload only carries the business data
// that ISN'T already expressed by the HTTP verb + URL.
//
var catRequest = new CatRequest
{
    Item = "tuna"        // ← the ONLY thing the body needs to say
};

// ── Serialize to binary ─────────────────────────────────────
byte[] requestBytes = catRequest.ToByteArray();

// Let's peek at the raw bytes so you can see how compact protobuf is
string jsonEquivalent = System.Text.Json.JsonSerializer.Serialize(
    new { catRequest.Item });

Console.WriteLine("── Outgoing protobuf bytes ──────────────────");
Console.WriteLine($"   Total size : {requestBytes.Length} bytes");
Console.WriteLine($"   Hex dump   : {BitConverter.ToString(requestBytes)}");
Console.WriteLine($"   (compare to JSON: {jsonEquivalent.Length} bytes)");

// ── Send HTTP PUT /food ─────────────────────────────────────
using var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5099") };

var content = new ByteArrayContent(requestBytes);
content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");

Console.WriteLine("📡 Sending PUT /food …");
var httpResponse = await httpClient.PutAsync("/food", content);

// ── Read & deserialize the response ─────────────────────────
byte[] responseBytes = await httpResponse.Content.ReadAsByteArrayAsync();
var catResponse = CatResponse.Parser.ParseFrom(responseBytes);

Console.WriteLine();
Console.WriteLine("── Response from Cat Server ─────────────────");
Console.WriteLine($"   HTTP status  : {(int)httpResponse.StatusCode}");
Console.WriteLine($"   Proto status : {catResponse.StatusCode}");
Console.WriteLine($"   Message      : {catResponse.Message}");
Console.WriteLine($"   Cat mood     : {catResponse.CatMood}");
Console.WriteLine($"   Wire size    : {responseBytes.Length} bytes");
Console.WriteLine($"   Hex dump     : {BitConverter.ToString(responseBytes)}");
Console.WriteLine();
Console.WriteLine("✅ Done! The cat has been fed.");
