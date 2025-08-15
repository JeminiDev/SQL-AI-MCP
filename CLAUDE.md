# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

This repository contains Azure SQL AI samples and implementations, with a focus on Model Context Protocol (MCP) servers for MSSQL databases. The main components include:

- **MssqlMcp/Node**: TypeScript/Node.js MCP server implementation
- **MssqlMcp/dotnet**: .NET 8 MCP server implementation  
- Sample notebooks and scripts for Azure SQL AI integrations

## Common Development Commands

### Node.js MCP Server (MssqlMcp/Node)

```bash
# Install dependencies
cd MssqlMcp/Node
npm install

# Build the TypeScript project
npm run build

# Watch mode for development
npm run watch

# Start the server
npm start
```

### .NET MCP Server (MssqlMcp/dotnet)

```bash
# Build the project
cd MssqlMcp/dotnet
dotnet build

# Run tests
dotnet test

# Run the server
cd MssqlMcp
dotnet run
```

## Architecture Overview

### MCP Server Architecture

Both Node.js and .NET implementations follow the Model Context Protocol pattern:

1. **Tool Registration**: Each server registers tools (CreateTable, ReadData, UpdateData, etc.) that can be called by AI assistants
2. **Connection Management**: Handles MSSQL database connections with support for Azure AD authentication
3. **Request Handling**: Processes MCP protocol requests and routes them to appropriate tool implementations
4. **Security**: Supports read-only mode via READONLY environment variable

### Node.js Implementation Structure

- `src/index.ts`: Main server entry point, handles MCP protocol and connection pooling
- `src/tools/`: Individual tool implementations (CreateTableTool, ReadDataTool, etc.)
- Uses Azure Identity SDK for authentication with Azure SQL
- Implements connection pooling with token refresh logic

### .NET Implementation Structure  

- `Program.cs`: Main entry point with host configuration
- `Tools/`: Tool implementations following MCP protocol
- `ISqlConnectionFactory`: Abstraction for database connections
- Uses Microsoft.Data.SqlClient for database operations

## Key Technical Details

### Authentication
- Node server uses Azure AD Interactive Browser authentication
- .NET server supports both Windows Authentication and Azure AD
- Connection strings can be configured via environment variables

### MCP Protocol Integration
- Both servers implement standard MCP tools for database operations
- Tools are conditionally exposed based on READONLY mode
- Supports integration with Claude Desktop and VS Code Agent

### Database Operations
- All tools include proper error handling and result formatting
- Read operations require WHERE clauses to prevent full table scans
- Update/Delete operations require explicit WHERE clauses for safety