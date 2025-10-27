using System;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Benchmarks
{
    // Simple benchmark for measuring ParseClaimsFromJwt via reflection
    public class ParseJwtBenchmarks
    {
        private MethodInfo _parseMethod;
        private string _simpleToken = string.Empty;
        private string _arrayRoleToken = string.Empty;

        [GlobalSetup]
        public void Setup()
        {
            // Locate the private static method on the Blazor assembly
            var type = Type.GetType("Blazor.Services.CustomAuthenticationStateProvider, Blazor");
            if (type == null)
                throw new InvalidOperationException("Could not find type Blazor.Services.CustomAuthenticationStateProvider. Ensure the Blazor project is referenced and the assembly name is 'Blazor'.");
            _parseMethod = type.GetMethod("ParseClaimsFromJwt", BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException("Could not find ParseClaimsFromJwt method.");
            // Prepare example JWTs (header.payload.signature). Payloads are simple JSON objects encoded with base64url.
            var simplePayload = JsonSerializer.Serialize(new { nameid = "123", unique_name = "alice", email = "alice@example.com", role = "User" });
            var arrayPayload = JsonSerializer.Serialize(new { nameid = "456", unique_name = "bob", email = "bob@example.com", role = new[] { "Admin", "User" } });
            _simpleToken = BuildJwtWithPayload(simplePayload);
            _arrayRoleToken = BuildJwtWithPayload(arrayPayload);
        }

        private string BuildJwtWithPayload(string payloadJson)
        {
            string header = Base64UrlEncode("{\"alg\":\"none\",\"typ\":\"JWT\"}");
            string payload = Base64UrlEncode(payloadJson);
            string signature = "signature"; // not validated by ParseClaimsFromJwt
            return string.Join('.', header, payload, signature);
        }

        private static string Base64UrlEncode(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var base64 = Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return base64;
        }

        [Benchmark(Baseline = true, Description = "Parse - simple role")]
        public object Parse_SimpleRole()
        {
            return _parseMethod.Invoke(null, new object[] { _simpleToken })!;
        }

        [Benchmark(Description = "Parse - array roles")]
        public object Parse_ArrayRole()
        {
            return _parseMethod.Invoke(null, new object[] { _arrayRoleToken })!;
        }
    }
}