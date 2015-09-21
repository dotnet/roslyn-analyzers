### CA2153: Do not catch corrupted state exceptions in general handlers. ###

Do not author general catch handlers in code that receives corrupted state exceptions. Code that receives and intends to handle corrupted state exceptions should author distinct handlers for each exception type.

Category: Microsoft.Security
Severity: Warning