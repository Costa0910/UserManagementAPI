# UserManagementAPI

Minimal in-memory user management API with logging, error handling, and token-based authentication middleware.

## Prerequisites

- .NET 10.0 SDK or later
- Git

## Quickstart

1. **Clone and navigate to the project:**

```bash
git clone https://github.com/costa0910/UserManagementAPI.git
cd UserManagementAPI
```

2. **Restore dependencies:**

```bash
dotnet restore
```

3. **Run the API:**

```bash
dotnet run
```

The API will start on `http://localhost:5045` (HTTPS enabled).

4. **Get a Token:**

Make a POST request to `/auth/token`:

```bash
curl -X POST http://localhost:5045/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","secret":"admin-secret"}'
```

Response:
```json
{
  "token": "changeme-token"
}
```

5. **Use the Token:**

Add the token to subsequent requests as a Bearer token:

```bash
curl http://localhost:5045/users \
  -H "Authorization: Bearer changeme-token"
```

## Configuration

Edit `UserManagementAPI/appsettings.json` to configure authentication users and tokens:

```json
{
  "Auth": {
    "Users": {
      "admin": "admin-secret",
      "user": "user-password"
    },
    "Tokens": ["changeme-token", "another-token"]
  }
}
```

## API Endpoints

### Authentication
- **POST** `/auth/token` - Issue a bearer token (no auth required)

### Users (require valid bearer token)
- **GET** `/users` - Retrieve all users
- **GET** `/users/{id}` - Retrieve a specific user by ID
- **POST** `/users` - Create a new user
- **PUT** `/users/{id}` - Update an existing user
- **DELETE** `/users/{id}` - Delete a user by ID

## Architecture

### Middleware Pipeline (in order)
1. **ErrorHandlingMiddleware** - Catches unhandled exceptions and returns consistent JSON error responses
2. **TokenAuthenticationMiddleware** - Validates bearer tokens for all protected endpoints (skips `/auth/token`)
3. **RequestResponseLoggingMiddleware** - Logs HTTP method, path, status code, and elapsed time

### Project Structure
```
UserManagementAPI/
├── Models/                 # Data models and request/response DTOs
├── Repositories/           # In-memory data store abstraction
├── Validation/             # Input validation logic
├── Middleware/             # Custom middleware components
├── Services/               # Helper services (TokenStore)
├── Program.cs              # Application entry point and route definitions
├── appsettings.json        # Configuration
└── UserManagementAPI.http  # HTTP request examples for testing
```

## Testing

Use the included `UserManagementAPI.http` file with your IDE's REST client (e.g., Rider, VS Code REST Client):

1. Call `POST /auth/token` with credentials to get a token
2. Set the `@AuthToken` variable to the returned token value
3. Execute remaining requests with the valid token

Example workflow in `UserManagementAPI.http`:
```http
POST http://localhost:5045/auth/token
Content-Type: application/json

{
  "username": "admin",
  "secret": "admin-secret"
}

###

GET http://localhost:5045/users
Authorization: Bearer changeme-token
Accept: application/json
```

## Development

To build the project:

```bash
dotnet build
```

## Notes

- Tokens and users are stored in-memory and will reset on application restart
- All endpoints validate tokens through middleware;
