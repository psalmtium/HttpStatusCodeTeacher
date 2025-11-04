# HTTP Status Code Teacher

**HTTP Status Code Teacher** is an intelligent ASP.NET Core Web API service that provides educational content about HTTP status codes using advanced AI models (Google Gemini and Claude AI).

Learn about HTTP status codes with detailed explanations, use cases, best practices, and real-world examples.

---

## Features

- **Comprehensive Status Code Explanations**: Learn about any HTTP status code (100-599)
- **AI-Powered Teaching**: Choose between **Gemini AI** or **Claude AI** for intelligent, context-aware explanations
- **Multiple Learning Endpoints**:
  - Explain specific status codes
  - Browse status codes by category (1xx, 2xx, 3xx, 4xx, 5xx)
  - A2A protocol support for agent integration
- **Azure Application Insights**: Comprehensive telemetry and monitoring
- **Redis Caching**: Optional performance optimization
- **Swagger Documentation**: Interactive API documentation
- **Factory Pattern**: Clean separation of AI service implementations

---

## Project Structure

```
HttpStatusCodeTeacher/
├── Controllers/              # API endpoints
│   ├── StatusCodeController.cs
│   ├── HealthController.cs
│   └── TelexWebhookController.cs
├── Models/                   # Data models
│   ├── StatusCodeModels.cs
│   ├── A2AModels.cs
│   └── AgentCard.cs
├── Services/                 # Business logic
│   ├── IAIService.cs
│   ├── GeminiService.cs
│   ├── ClaudeService.cs
│   ├── AIServiceFactory.cs
│   └── CacheService.cs
├── Program.cs               # Application entry point
├── appsettings.json         # Configuration
└── HttpStatusCodeTeacher.csproj
```

---

## Installation and Setup

### Prerequisites

- .NET 8.0 SDK or later
- Redis (optional, only required if using Redis caching)

### 1. Clone the Repository

```bash
git clone https://github.com/psalmtium/HttpStatusCodeTeacher.git
cd HttpStatusCodeTeacher
```

### 2. Configure Environment Variables

Update `appsettings.json` or set environment variables:

```json
{
  "AI": {
    "Provider": "gemini"
  },
  "Cache": {
    "Type": "memory"
  },
  "Gemini": {
    "ApiKey": "your-gemini-api-key-here"
  },
  "Claude": {
    "ApiKey": "your-claude-api-key-here"
  },
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key;IngestionEndpoint=https://your-region.in.applicationinsights.azure.com/"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

Or use environment variables:
```bash
export AI__Provider=gemini
export Cache__Type=memory
export Gemini__ApiKey=your_gemini_api_key_here
export Claude__ApiKey=your_claude_api_key_here
export ApplicationInsights__ConnectionString=your_app_insights_connection_string
export Redis__ConnectionString=localhost:6379
```

**Configuration Notes:**
- `AI:Provider`: Set to `"gemini"` or `"claude"` to choose your AI service
- `Cache:Type`: Set to `"memory"`, `"redis"`, or `"none"` to choose caching strategy (see below)

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Run the Application

```bash
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`.

---

## API Endpoints

### Explain Status Code
- **GET** `/api/v1/explain?code={code}`
- Returns comprehensive explanation of the specified HTTP status code

**Example:**
```bash
curl "https://localhost:5001/api/v1/explain?code=404"
```

**Response:**
```json
{
  "status": "success",
  "explanation": {
    "code": 404,
    "name": "Not Found",
    "category": "4xx Client Error",
    "description": "The server cannot find the requested resource...",
    "when_to_use": "When a requested resource doesn't exist on the server...",
    "common_scenarios": "Requesting a non-existent page, deleted resource...",
    "best_practices": "Return helpful error messages, suggest alternatives...",
    "example_response": "HTTP/1.1 404 Not Found...",
    "related_codes": "400, 410, 403..."
  }
}
```

### List Status Codes
- **GET** `/api/v1/codes?category={category}`
- Lists common HTTP status codes, optionally filtered by category

**Example:**
```bash
curl "https://localhost:5001/api/v1/codes?category=4xx"
```

### Health Check
- **GET** `/api/v1/health`
- Returns the health status of the API

### Telex Webhook (A2A Protocol)
- **GET** `/api/v1/.well-known/agent.json` - Agent card
- **POST** `/api/v1/a2a/status-code-teacher` - A2A webhook endpoint

---

## Swagger Documentation

Access the interactive API documentation at:
- `https://localhost:5001/swagger` (Development mode)

---

## Tech Stack

| Category | Tools Used |
|-----------|-------------|
| **Framework** | ASP.NET Core 8.0 |
| **AI Integration** | Google Gemini API, Claude AI (Anthropic) |
| **Monitoring** | Azure Application Insights |
| **Caching** | In-Memory, Redis (StackExchange.Redis), or None |
| **Language** | C# 12 |
| **Serialization** | System.Text.Json, Newtonsoft.Json |
| **Documentation** | Swagger/OpenAPI |

---

## Dependencies

- **StackExchange.Redis** - Redis client for .NET
- **Microsoft.Extensions.Caching.Memory** - In-memory caching
- **Microsoft.Extensions.Http** - HTTP client factory
- **Newtonsoft.Json** - JSON serialization
- **Anthropic.SDK** - Claude AI client SDK
- **Microsoft.ApplicationInsights.AspNetCore** - Application Insights telemetry
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI generation

---

## AI Provider Configuration

The application supports two AI providers:

### Google Gemini
- Set `AI:Provider` to `"gemini"`
- Configure `Gemini:ApiKey` with your Google API key
- Uses `gemini-2.5-flash-preview-09-2025` model
- Fast and efficient responses
- Great for quick lookups

### Claude AI (Anthropic)
- Set `AI:Provider` to `"claude"`
- Configure `Claude:ApiKey` with your Anthropic API key
- Uses `claude-3-5-sonnet-20241022` model
- Detailed, educational responses
- Excellent for learning and understanding

---

## Cache Configuration

The application supports three caching strategies to optimize performance:

### In-Memory Caching (Default)
- **Configuration**: Set `Cache:Type` to `"memory"` or `"inmemory"`
- **Best For**: Single-instance deployments, development, testing
- **Advantages**:
  - No external dependencies
  - Fast access times
  - Simple setup
- **Limitations**:
  - Not shared across multiple instances
  - Lost on application restart

### Redis Caching
- **Configuration**: Set `Cache:Type` to `"redis"`
- **Best For**: Production deployments, multi-instance scenarios
- **Advantages**:
  - Shared across multiple app instances
  - Persists across application restarts
  - Scalable and distributed
- **Requirements**: Redis server must be running and configured
- **Connection**: Set `Redis:ConnectionString` in configuration

### No Caching
- **Configuration**: Set `Cache:Type` to `"none"`
- **Best For**: Debugging, always-fresh data requirements
- **Advantages**:
  - Always returns fresh AI-generated content
  - Simplest setup
- **Limitations**:
  - Higher latency
  - More API calls to AI providers
  - Higher costs

**Example Configuration:**

```json
{
  "Cache": {
    "Type": "memory"  // Options: "memory", "redis", "none"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"  // Only needed if using Redis
  }
}
```

---

## Azure Application Insights

The application automatically sends telemetry data to Azure Application Insights when configured:

- **Logs**: All application logs are sent to Application Insights
- **Requests**: HTTP request telemetry including response times
- **Dependencies**: External API calls (Gemini, Claude, Redis)
- **Exceptions**: Detailed exception tracking and stack traces
- **Custom Metrics**: Track API usage patterns

To enable, set the `ApplicationInsights:ConnectionString` in your configuration.

---

## Development

### Build the Project
```bash
dotnet build
```

### Run Tests (if available)
```bash
dotnet test
```

### Publish for Production
```bash
dotnet publish -c Release -o ./publish
```

---

## Example Use Cases

### Learn About Common Codes
```bash
# Get explanation for 200 OK
curl "https://localhost:5001/api/v1/explain?code=200"

# Get explanation for 500 Internal Server Error
curl "https://localhost:5001/api/v1/explain?code=500"

# Learn about redirect codes
curl "https://localhost:5001/api/v1/codes?category=3xx"
```

### Educational Purposes
Perfect for:
- Web development students learning HTTP
- Developers debugging API responses
- Technical writers documenting APIs
- Interview preparation

---

## Troubleshooting

| Issue | Possible Fix |
|-------|--------------|
| Redis not connecting | Ensure Redis server is running and `Cache:Type` is set to "redis" |
| Cache not working | Check `Cache:Type` is set to "memory" or "redis" (not "none") |
| Unsupported cache type error | Ensure `Cache:Type` is one of: "memory", "redis", or "none" |
| 500 Internal Server Error | Check logs in Application Insights or console for API issues |
| Missing API Key | Set the API key for your chosen provider in appsettings.json |
| AI Provider not found | Ensure `AI:Provider` is set to either "gemini" or "claude" |
| Application Insights not working | Verify the ConnectionString is correctly formatted |
| Invalid status code error | Ensure the code is between 100 and 599 |

---
