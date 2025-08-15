# MCP Server Configuration Guide

This guide explains how to configure the MSSQL MCP servers for both Azure SQL and local database access.

## Authentication Methods

Both the Node.js and .NET implementations support two authentication types:

1. **Azure AD Authentication** (default)
2. **SQL Authentication** (username/password)

## Environment Variables

### Common Configuration

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `AUTH_TYPE` | Authentication type: `azure-ad` or `sql` | `azure-ad` | No |
| `SERVER_NAME` | Database server hostname or IP (without port) | - | Yes |
| `SERVER_PORT` | Database server port (separate from SERVER_NAME) | `1433` | No |
| `DATABASE_NAME` | Database name | - | Yes |
| `TRUST_SERVER_CERTIFICATE` | Trust self-signed certificates | `false` | No |
| `CONNECTION_TIMEOUT` | Connection timeout in seconds | `30` | No |
| `READONLY` | Enable read-only mode (no write operations) | `false` | No |

### SQL Authentication

When `AUTH_TYPE=sql`, these additional variables are required:

| Variable | Description | Required |
|----------|-------------|----------|
| `SQL_USERNAME` | SQL Server username | Yes |
| `SQL_PASSWORD` | SQL Server password | Yes |

### .NET Specific

The .NET implementation also supports a direct connection string:

| Variable | Description | Required |
|----------|-------------|----------|
| `CONNECTION_STRING` | Full SQL Server connection string | No* |

*If `CONNECTION_STRING` is provided, it takes precedence over individual environment variables.

## Configuration Examples

### Local SQL Server with SQL Authentication

```bash
# Node.js
export AUTH_TYPE=sql
export SERVER_NAME=localhost
export SERVER_PORT=1433
export DATABASE_NAME=mydb
export SQL_USERNAME=sa
export SQL_PASSWORD=YourPassword123!
export TRUST_SERVER_CERTIFICATE=true  # Required for local development

# .NET (Option 1: Individual variables)
export AUTH_TYPE=sql
export SERVER_NAME=localhost
export SERVER_PORT=1433
export DATABASE_NAME=mydb
export SQL_USERNAME=sa
export SQL_PASSWORD=YourPassword123!
export TRUST_SERVER_CERTIFICATE=true  # Required for local development

# .NET (Option 2: Connection string)
export CONNECTION_STRING="Server=localhost,1433;Database=mydb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True"
```

**Note**: `TRUST_SERVER_CERTIFICATE=true` is essential for local SQL Server instances that use self-signed certificates.

### Local SQL Server with Windows Authentication (.NET only)

```bash
export CONNECTION_STRING="Server=.;Database=mydb;Integrated Security=True;TrustServerCertificate=True"
```

### Azure SQL with Azure AD Authentication

```bash
export AUTH_TYPE=azure-ad
export SERVER_NAME=myserver.database.windows.net
export DATABASE_NAME=mydb
```

### Custom Port Example

```bash
export AUTH_TYPE=sql
export SERVER_NAME=192.168.1.100  # Do NOT include port here
export SERVER_PORT=1434            # Set port separately
export DATABASE_NAME=mydb
export SQL_USERNAME=appuser
export SQL_PASSWORD=SecurePass456!
```

**Important**: The `SERVER_NAME` should contain only the hostname or IP address. The port must be specified separately using `SERVER_PORT`. Do not include the port in the server name (e.g., don't use `server.example.com,1434` as SERVER_NAME).

## MCP Client Configuration

### Claude Code CLI

Use the `claude mcp add` command to configure the MCP server:

#### Local SQL Server Example
```bash
claude mcp add mssql-local \
  --env AUTH_TYPE=sql \
  --env SERVER_NAME=localhost \
  --env SERVER_PORT=1433 \
  --env DATABASE_NAME=mydb \
  --env SQL_USERNAME=sa \
  --env SQL_PASSWORD=YourPassword123! \
  --env TRUST_SERVER_CERTIFICATE=true \
  -- node /path/to/MssqlMcp/Node/dist/index.js
```

#### Custom Port Example
```bash
claude mcp add mssql-custom \
  --env AUTH_TYPE=sql \
  --env SERVER_NAME=192.168.1.100 \
  --env SERVER_PORT=1434 \
  --env DATABASE_NAME=mydb \
  --env SQL_USERNAME=appuser \
  --env SQL_PASSWORD=SecurePass456! \
  --env TRUST_SERVER_CERTIFICATE=true \
  -- node /path/to/MssqlMcp/Node/dist/index.js
```

#### Azure SQL Example
```bash
claude mcp add mssql-azure \
  --env AUTH_TYPE=azure-ad \
  --env SERVER_NAME=myserver.database.windows.net \
  --env DATABASE_NAME=mydb \
  -- node /path/to/MssqlMcp/Node/dist/index.js
```

#### Read-Only Mode Example
```bash
claude mcp add mssql-readonly \
  --env AUTH_TYPE=sql \
  --env SERVER_NAME=localhost \
  --env DATABASE_NAME=mydb \
  --env SQL_USERNAME=readonly_user \
  --env SQL_PASSWORD=ReadOnlyPass123! \
  --env TRUST_SERVER_CERTIFICATE=true \
  --env READONLY=true \
  -- node /path/to/MssqlMcp/Node/dist/index.js
```

### Claude Desktop Configuration

Alternatively, add to your Claude Desktop configuration file:

```json
{
  "mcpServers": {
    "mssql-local": {
      "command": "node",
      "args": ["/path/to/MssqlMcp/Node/dist/index.js"],
      "env": {
        "AUTH_TYPE": "sql",
        "SERVER_NAME": "localhost",
        "DATABASE_NAME": "mydb",
        "SQL_USERNAME": "sa",
        "SQL_PASSWORD": "YourPassword123!",
        "TRUST_SERVER_CERTIFICATE": "true"
      }
    }
  }
}
```

### VS Code Agent Configuration

Similar configuration can be used with the VS Code Agent extension.

## Security Considerations

1. **Never commit credentials** to version control
2. Use environment variables or secure secret management
3. Enable `READONLY=true` for read-only access when appropriate
4. Use strong passwords for SQL authentication
5. Prefer Azure AD authentication for production Azure SQL databases
6. **Important**: Set `TRUST_SERVER_CERTIFICATE=true` only for development/local databases
   - Required for local SQL Server instances with self-signed certificates
   - Should be `false` for production environments with proper SSL certificates

## Troubleshooting

### Connection Issues

1. **Self-signed certificate error**: Set `TRUST_SERVER_CERTIFICATE=true` for local development
2. Verify the server is accessible: `telnet SERVER_NAME SERVER_PORT`
3. Check firewall rules allow the connection
4. Ensure SQL Server is configured to accept the authentication type
5. For SQL authentication, verify SQL Server allows mixed mode authentication

### Authentication Errors

- **Azure AD**: Ensure you're logged in with appropriate Azure credentials
- **SQL Auth**: Verify username/password are correct and user has database access
- **Windows Auth (.NET)**: Ensure running user has SQL Server access

### Port Configuration

- Default SQL Server port is 1433
- For named instances, you may need to specify a different port
- Docker containers often use non-standard ports (e.g., 1434, 14330)