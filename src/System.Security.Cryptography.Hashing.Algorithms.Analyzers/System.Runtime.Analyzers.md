### CA5350: Do not use insecure cryptographic algorithm SHA1. ###

This type implements SHA1, a cryptographically insecure hashing function. Hash collisions are computationally feasible for the SHA-1 and SHA-0 algorithms. Replace this usage with a SHA-2 family hash algorithm (SHA512, SHA384, SHA256).

Category: Microsoft.Security

Severity: Warning

### CA5351: Do not use insecure cryptographic algorithm MD5. ###

This type implements MD5, a cryptographically insecure hashing function. Hash collisions are computationally feasible for the MD5 and HMACMD5 algorithms. Replace this usage with a SHA-2 family hash algorithm (SHA512, SHA384, SHA256).

Category: Microsoft.Security

Severity: Warning