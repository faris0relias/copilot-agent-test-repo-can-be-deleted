---
applyTo: "**"
---
# Relias .NET Coding Standards and Best Practices
1. Use Clean Architecture
    1. Separate concerns using layers: Presentation, Application, Domain, and Infrastructure.
    2. Enforce dependencies inward: higher layers (UI, Infrastructure) depend on lower ones (Domain).
2. Use Dependency Injection (DI)
    1. Favor constructor injection for services and configurations.
    2. Register interfaces with their implementations in the `Startup.cs` or `Program.cs`.
3. Naming Conventions
    1. Code must follow .NET naming conventions
    2. Use PascalCase for class, method, and property names.
    3. Use camelCase for local variables and method parameters.
    4. Prefix interfaces with `I` (e.g., `IUserService`).
    5. Use var only when the type is obvious or improves readability
    6. Avoid using magic numbers/strings—use constants or enums
    7. Prefer string interpolation over string.Format
    8. Avoid unnecessary comments; use XML comments for public APIs
    9. Code must be organized into regions if large or logically separable
    <!-- Below are the Relias specific Instructions(insturction 10-21) -->
    10. Namespace names should start with 'Relias'.
    11. Class and struct names should be nouns.
    12. Class and struct names should in PascalCase.
    13. Exception class names must end with the suffix 'Exception'.
    14. Interface names must be in PascalCase and prefixed with 'I'.
    15. Method names should be verbs or verb/noun pairs,
    16. Method names should be in PascalCase.
    17. Property names must be written in PascalCase.
    18. Property names should not be prefixed with 'get' or 'set'.
    19. Constant names must be written in PascalCase.
    20. Local variable and parameter names should be in camelCase.
    21. Enum Type and Value Name should be in Pascal Case.
4. Class and Method Design
    1. Each method should do one thing only.
    2. Limit method length to ideally < 20 lines.
    3. Use access modifiers explicitly (e.g., public, private)
    4. Avoid static state in classes unless truly stateless
    <!-- Relias specific Instructions(5-16) -->
    5. All member variables should be declared at the top of the class.
    6. Public types must have doc comments. No need for doc comments on non-public types.
    7. Consider using good inheritance to avoid repetition.
    8. Make use of C# partial classes for organization and refactoring
    9. Don’t prefix or personalize naming of members or files in an application with the developer’s name or initials
    10. Consider prefixing Boolean properties with Is, Has, Can, Allows, or Supports
    11. Consider using Any() to determine whether an IEnumerable<T> is empty instead of Count(). Count() iterates over the entire      collection first, which in the case of IQueryable<T> in LINQ-to-SQL can significantly impact performance.
    12. Use C# type alias, like use int instead of Int32, string instead of String, bool instead of Boolean, etc.
    13. Keep class using statements orderly: generally in alphabetical order and “unused usings” removed.
    14. Take the Single Responsibility Principle seriously. Aim to keep classes, methods, and any functional unit small and within its own contextual boundary. Smaller constructs are easier to understand, test, and debug.
    15. Avoid static classes, especially in web applications. Static classes are not thread safe (ex.Data passed through a static utility that should be protected. IIS thread pool is a case where this might occur). An exception to this rule is in the case of C# Extension
    Methods.
    16. Use “null conditional” and “null coalescing” operators instead of ternary operator.
5. Use Asynchronous Programming
    1. Prefer `async` and `await` with `Task`-based methods.
    2. Avoid blocking calls like `.Result` or `.Wait()`.
    3. All async methods have Async suffix
    4. No deadlocks caused by mixing sync/async code
 
    5. All async methods must have the Async suffix.    <!-- Relias specific Instructions -->
6. Graceful Error Handling
    1. Use try-catch blocks wisely and avoid catching general exceptions like `Exception`.
    2. Use custom exception types for better context.
    3. Avoid throw ex; (use throw; to preserve stack trace)
7. Logging
    1. Use a logging framework like Serilog, NLog, or built-in `ILogger<T>`.
    2. Log at appropriate levels: `Information`, `Warning`, `Error`, `Critical`.
8. Efficient Querying
    1. Use `AsNoTracking()` when entities don't need to be updated.
    2. Avoid `Include()` overload—project into DTOs instead.
    3. Use `FirstOrDefaultAsync()` or `SingleOrDefaultAsync()` for async-safe queries.
9. Migrations and Schema Management
    1. Use code-based migrations (`Add-Migration`, `Update-Database`).
    2. Keep migration names meaningful (e.g., `AddAuditLogTable`).
    3. Migrations are named and committed with context
10. Follow RESTful Principles
    1. Use HTTP methods correctly: GET (read), POST (create), PUT/PATCH (update), DELETE (remove).
    2. Use appropriate response codes (200, 201, 400, 404, 500).
    3. Use [ApiController] and model validation
    4. Use DTOs and never expose domain entities directly
    5. Use CreatedAtAction, NoContent, etc. for responses
11. Use DTOs and AutoMapper
    1. Use Data Transfer Objects (DTOs) to decouple API contracts from domain models.
    2. Use AutoMapper for clean object mapping.
    3. Use DbContext per scope (e.g., via DI)
    4. Use AsNoTracking() when not updating entities
    5. Avoid lazy loading unless explicitly needed
    6. Use explicit joins or projections instead of Include() chains
12. Versioning APIs
    1. Use route-based versioning: `/api/v1/users`.
    2. Support multiple versions via `[ApiVersion]` attribute.
13. Unit Testing
    1. Use xUnit or NUnit for unit tests.
    2. Mock dependencies using Moq or similar libraries.
    3. Write tests for both success and error scenarios for each unit.
    4. Use `WebApplicationFactory<T>` for ASP.NET Core test servers.
    5. Cover critical business flows in tests.
14. Common Security Practices
    1. Validate all user inputs (e.g., with FluentValidation).
    2. Use built-in Identity for authentication or OAuth2/JWT for APIs.
    3. Store secrets in Azure Key Vault, AWS Secrets Manager, or `appsettings.{Environment}.json`.
    4. Enable CSRF protection.
    5. Sanitize user inputs to prevent XSS and SQL Injection.
    6. Use HTTPS for all communications.
    7. Ensure sensitive data is stored securely (hashed, encrypted)
    8. Ensure secrets are not hardcoded—use environment/config providers
    9. Ensure HTTPS is enforced in production environments
15. Performance and Scalability
    1. Cache responses using in-memory cache, Redis, or ResponseCaching middleware.
    2. Use `IAsyncEnumerable<T>` for streaming large datasets.
    3. Profile bottlenecks using Application Insights or dotTrace.
16. Configuration and Environment Management
    1. Use `IOptions<T>` pattern for strongly typed configuration.
    2. Separate settings by environment (`appsettings.Development.json`, `appsettings.Production.json`).
    3. Never commit secrets—use User Secrets or Environment Variables during development.
    4. Ensure no environment-specific logic in code (use config abstraction)
17. Code Quality and DevOps
    1. Use tools like SonarQube, StyleCop, and FxCop for static analysis.
    2. Set up CI/CD pipelines with GitHub Actions, Azure DevOps, or GitLab CI.
    3. Automate tests and deployment to ensure reliability.
18. Code Quality and DevOps
    1. Use tools like SonarQube, StyleCop, and FxCop for static analysis.
    2. Set up CI/CD pipelines with GitHub Actions, Azure DevOps, or GitLab CI.
    3. Automate tests and deployment to ensure reliability.
    <!--Relias Specific(Formatting and CQRS-->
19. Formatting
    1. Consistent layout, format, and organization are keys to creating maintainable code.
    2. Make sure each code statement block has its opening brace at the next line and on the same indention level as its header. This is often referred to as Allman brace placement style.
    3. Use parenthesis in groups to make expressions clearer.
    4. Always properly indent code blocks.
    5. Keep one white space between keywords like if and the expression, but don’t add white spaces after left parens ( and before right parens ) .
    6. To aid clarity for reading add a white space around operators, like +, -, ==, etc.
    7. Always succeed the keywords if, else, do, while, for, and foreach, with opening and closing parentheses, even though the language does not require it.
    8. Prefer multi-line object initializers.
    9. Consider multi-line lambda statements.
    10. Don’t make explicit comparisons to true or false.
    11. Line up LINQ query statement keywords at the same indention
20. CQRS
    1. Use DTOs for request and response models
    2. Implement CQRS pattern with Command and Query
    3. Create a Service layer encapsulating all business logic.
    4. Implement a Repository layer for data persistence, using Entity Framework Core with a Microsoft SQL Server database.
    5. Define a Domain Entity
    6. The Controller should communicate with business logic through MediatR Command and Query handlers.
    7. Use FluentValidation to validate incoming requests.
    8. Include robust error handling across all layers.
    9. Write Unit test classes for CommandHandler and Service with positive and negative tests.
<!--Relias Specific(Performance and Security)-->
21. Performance
    1.Parallel computation or distributed processing shoulb used where ever it is beneficial.
    2. Principles of modularity, cohesion, and coupling can be applied.
    3. Read and Write operations shoulbe separated out where ever its applicable.
    4. Implemented appropriate use of caching.
    5. Define performance parameters like Processing Speed, Memory Utilization, Network Latency, Concurrency and Parallelism, Response Time, and Load Balancing.
    6. Use algorithms optimal for the problems.
    7. Choose data structures to minimize time and space complexity.
    8. Optimize the methods of sorting, searching, and iteration.
    9. Use the built-in functions that are faster than custom logic.
    10. Minimize the appropriate usage of memory.
    11. Reuse the large objects and arrays where ever possible instead of duplicating.
    12. Clear up the temporary variables when no longer needed.
    13. Use object pooling (if applicable in high-frequency object creation).
    14. Abstract duplicate logic into functions or helpers.
    15. Minimize and optimize loops or recursive calls.
    16. Use caching where appropriate to avoid repeated calculations.
    17. Properly release the resources like file handles, database connections, and sockets.
    18. Manage asynchronous operations to avoid blocking calls.
    19. Use lazy loading or pagination used for large datasets.
    20. Minimize or batch expensive operations (e.g., database calls, file I/O, network requests).
    21. Minimize the number of operations within nested loops.
    22. Use tail-recursion or iteration instead of deep recursion (if applicable).
    23. Code should able to handle large volumes of data or high traffic.
    24. Parallelize and distribute operations  where it is necessary.
    25. Designe the code to work efficiently under load or stress.
    26. Deferr computations until needed (e.g., generators).
    27. Short-circuite the logical operations (e.g., `&&` or `||`) to skip unnecessary evaluation.
    28. The code should be profiled with tools (e.g., `perf`, `cProfile`, or Chrome DevTools).
    29. The performance-critical paths shoul be tested with real-world inputs.
    30. The metrics should be collected for load times, memory, CPU usage, and throughput.
    31. The third-party libraries can be chosen based on performance benchmarks.
    32. Minimize unnecessary overhead from frameworks/libraries.
    33. Use updated versions of libraries for performance improvements.
    34. Clearly document the performance trade-offs.
    35. Provide comments for non-obvious optimizations.
    36. Add TODOs for potential improvements.
22. Security
    1. Code should follow secure coding practices and OWASP Top 10 guidelines.
    2. Use security libraries and frameworks where applicable.
    3. The codebase should be free from known vulnerabilities (e.g., using tools like Snyk, OWASP Dependency-Check).
    4. Security headers shoul be implemented (e.g., Content Security Policy, X-Content-Type-Options).
    5. Security patches should be applied to the codebase and dependencies.
    6. Security best practices should be followed for the programming language and framework.
    7. The code should be reviewed for security vulnerabilities by peers or security experts.
    8. The security-related issues should be tracked and managed in the issue tracker.
    9. Follow principle of least privilege to manage identity and access.
    10. The strong access control should be implemented.
    11. The strong encryption techniques should be implemented to protect data in transit and at rest.
    12. The network should be secured and endpoints should be protected.
    13. The proper logging with ability to trace the user session with masking/removing sensitive data must be implemented.
    14. The proper monitoring with alerting should be implemented.
    15. The PII must be defined and is PII data should get captured in the application.
    16. The sensitive information must be hidden on the screen (e.g., using input type as "password").
    17. The obfuscated data should be passed where possible.
    18. The PII data should be avoided from being sent to the third-party service as part of integration.
    19. The PII data should be avoided from being logged.
    20. The PII data should not retained once it is no longer needed.
    21. The sensitive data access and modification should be audited.
    22. All of the external inputs (user, API, file, etc.) should be validated properly.
    23. All the inputs should be sanitized before usage (e.g., SQL queries, HTML output).
    24. The input constraints should be enforced (type, length, format, range).
    25. The authentication should be handled securely (e.g., password hashing, MFA).
    26. All the roles and permissions should be checked properly before sensitive operations.
    27. Session expiration and proper session management should be there.
    28. The sensitive data (e.g., PII, credentials, tokens) should be encrypted at rest and in transit.
    29. The sensitive data should be masked or omitted from logs.
    30. The secrets should be stored securely (e.g., environment variables, secret managers).
    31. The SQL queries should be parameterized or using ORM to prevent SQL injection.
    32. The command-line input should be safely escaped or avoided altogether.
    33. The LDAP, XML, and other types of injection should be considered and mitigated.
    34. The Cross-Site Scripting (XSS) protections should be implemented.
    35. The security headers should be implemented (e.g., Content Security Policy, X-Content-Type-Options).
    36. The strong access control should be implemented.
    37. The output should be escaped properly when rendering data into HTML/JavaScript.
    38. The CSP (Content Security Policy) headers can be used.
    39. The untrusted scripts should be blocked.
    40. The CSRF tokens should be implemented on state-changing requests.
    41. The SameSite cookie attributes should be used appropriately.
    42. The user intent should be confirmed before sensitive actions.
    43. The error messages should be generic and not expose internal details.
    44. The exceptions should be properly caught and handled.
    45. The logs should be protected and not contain sensitive information.
    46. All dependencies should be kept up to date.
    47. The third-party packages should be audited for known vulnerabilities.
    48. The unused dependencies should be removed.
    49. The debug mode should be disabled in production.
    50. The security settings like CORS, CSP, HSTS should be configured properly.
    51. The default credentials or unnecessary services should be disabled.
    52. The API endpoints should be authenticated and authorized.
    53. The rate limits or throttling should be applied.
    54. The error responses should be consistent and non-revealing.
    55. The unit and integration tests should be in place for security-critical code.
    56. The static code analysis tools (e.g., SonarQube, Semgrep) should be used.
    57. The penetration tests or dynamic scans should be performed.
    58. The risky areas or accepted risks should be documented.
    59. The security-specific implementation details should be commented.
    60. The README or security.md files should be updated with secure usage guidelines.
    61. A custom authentication handler should be implemented for API requests.
    62. The application should support claims-based identity and authorization.
    63. The user actions and access should be tracked in domain entities and database configurations.
    64. The environment variables should be used for secret management in Docker and Functions.
    65. The logging of errors, warnings, and information should be performed using ILogger and Serilog.
    66. The input validation should be enforced using validators such as FluentValidation.
    67. The explicit exception handling should be implemented for invalid input and error states.
    68. The secure database access should be ensured through entity property and index configuration.
    69. The security logic should be separated into dedicated files and layers.