# Camunda Platform 7 Setup Guide

## 🚀 Quick Start

### Prerequisites
- Docker Desktop installed and running
- .NET 10.0 SDK
- SQL Server or PostgreSQL (for application database)

### Step 1: Start Camunda with Docker Compose

```bash
# Navigate to project root
cd "c:\Users\user\Desktop\New folder\BPMN Workflow Designer"

# Start all services (Camunda, PostgreSQL, Redis)
docker-compose up -d

# Check if services are running
docker-compose ps

# View logs
docker-compose logs -f camunda
```

### Step 2: Verify Camunda is Running

Open your browser and navigate to:
- **Camunda Cockpit**: http://localhost:8080/camunda/app/cockpit/default/
- **Camunda Tasklist**: http://localhost:8080/camunda/app/tasklist/default/
- **Camunda Admin**: http://localhost:8080/camunda/app/admin/default/

**Default Credentials:**
- Username: `demo`
- Password: `demo`

### Step 3: Test Camunda REST API

```bash
# Health check
curl http://localhost:8080/engine-rest/engine

# Get process definitions
curl http://localhost:8080/engine-rest/process-definition
```

### Step 4: Run Database Migration

```bash
# Navigate to API project
cd "Server\BpmnWorkflow.API"

# Add migration for Camunda fields
dotnet ef migrations add AddCamundaIntegration --project ..\BpmnWorkflow.Infrastructure

# Update database
dotnet ef database update
```

### Step 5: Start the Application

```bash
# Terminal 1: Start Backend API
cd "Server\BpmnWorkflow.API"
dotnet run

# Terminal 2: Start Blazor Client
cd "Client\BpmnWorkflow.Client"
dotnet run
```

## 📋 Available Services

| Service | URL | Purpose |
|---------|-----|---------|
| Camunda Platform | http://localhost:8080/camunda | Web UI for process management |
| Camunda REST API | http://localhost:8080/engine-rest | REST API endpoint |
| PostgreSQL | localhost:5432 | Database (both app and Camunda) |
| Redis | localhost:6379 | Caching (optional) |
| Backend API | https://localhost:7225 | Your application API |
| Blazor Client | https://localhost:5001 | Your application frontend |

## 🔧 Configuration

### Camunda Settings (appsettings.json)

```json
{
  "Camunda": {
    "BaseUrl": "http://localhost:8080/engine-rest/",
    "Username": "demo",
    "Password": "demo"
  }
}
```

### Environment Variables (docker-compose.yml)

You can customize Camunda by editing `docker-compose.yml`:

```yaml
environment:
  DB_DRIVER: org.postgresql.Driver
  DB_URL: jdbc:postgresql://postgres:5432/camunda
  DB_USERNAME: postgres
  DB_PASSWORD: postgres123
  CAMUNDA_BPM_ADMIN_USER_ID: admin
  CAMUNDA_BPM_ADMIN_USER_PASSWORD: admin
```

## 📚 Usage Examples

### 1. Deploy a Workflow

```csharp
// From your application code
var deployRequest = new DeployWorkflowRequest
{
    WorkflowId = workflowId,
    DeploymentName = "My Process",
    BpmnXml = bpmnXmlContent
};

var result = await camundaService.DeployWorkflowAsync(deployRequest);
```

### 2. Start a Process Instance

```csharp
var startRequest = new StartProcessInstanceRequest
{
    ProcessDefinitionKey = "myProcess",
    BusinessKey = "ORDER-12345",
    Variables = new Dictionary<string, object>
    {
        { "orderId", "12345" },
        { "amount", 1000.50 },
        { "customer", "John Doe" }
    }
};

var instance = await camundaService.StartProcessInstanceAsync(startRequest);
```

### 3. Complete a User Task

```csharp
// Get user tasks
var tasks = await camundaService.GetUserTasksAsync(assignee: "john");

// Complete a task
var completeRequest = new CompleteTaskRequest
{
    Variables = new Dictionary<string, object>
    {
        { "approved", true },
        { "comment", "Looks good!" }
    }
};

await camundaService.CompleteTaskAsync(taskId, completeRequest);
```

## 🔍 Monitoring & Debugging

### View Running Process Instances

```bash
# Via REST API
curl http://localhost:8080/engine-rest/process-instance

# Or use Camunda Cockpit
# Navigate to: http://localhost:8080/camunda/app/cockpit/default/
```

### Check Camunda Logs

```bash
# View Camunda container logs
docker logs bpmn-camunda -f

# View all services logs
docker-compose logs -f
```

### Database Access

```bash
# Connect to PostgreSQL
docker exec -it bpmn-postgres psql -U postgres -d camunda

# List Camunda tables
\dt act_*

# View process definitions
SELECT * FROM act_re_procdef;

# View running instances
SELECT * FROM act_ru_execution;
```

## 🛠️ Troubleshooting

### Camunda Not Starting

```bash
# Check if port 8080 is already in use
netstat -ano | findstr :8080

# Restart Camunda container
docker-compose restart camunda

# View detailed logs
docker-compose logs camunda
```

### Database Connection Issues

```bash
# Check if PostgreSQL is running
docker-compose ps postgres

# Test database connection
docker exec -it bpmn-postgres psql -U postgres -c "SELECT version();"

# Recreate database
docker-compose down -v
docker-compose up -d
```

### API Connection Issues

1. Check Camunda base URL in `appsettings.json`
2. Verify Camunda is accessible: http://localhost:8080/engine-rest/engine
3. Check firewall settings
4. Review application logs for detailed error messages

## 📖 API Endpoints

### Deployment

```
POST   /api/camunda/deploy/{workflowId}           # Deploy workflow to Camunda
GET    /api/camunda/process-definitions           # List all process definitions
DELETE /api/camunda/deployments/{deploymentId}   # Delete deployment
```

### Process Instances

```
POST   /api/camunda/processes/start               # Start process instance
GET    /api/camunda/processes                     # List running instances
GET    /api/camunda/processes/{id}                # Get instance details
DELETE /api/camunda/processes/{id}                # Cancel instance
```

### User Tasks

```
GET    /api/camunda/tasks                         # List all tasks
GET    /api/camunda/tasks/{taskId}                # Get task details
POST   /api/camunda/tasks/{taskId}/claim          # Claim task
POST   /api/camunda/tasks/{taskId}/complete       # Complete task
```

### Health Check

```
GET    /api/camunda/health                        # Check Camunda status
```

## 🔐 Security Notes

### Production Deployment

1. **Change Default Passwords**
   ```yaml
   CAMUNDA_BPM_ADMIN_USER_PASSWORD: <strong-password>
   DB_PASSWORD: <strong-password>
   ```

2. **Use HTTPS**
   - Configure SSL certificates for Camunda
   - Use reverse proxy (nginx/traefik)

3. **Network Security**
   - Don't expose Camunda directly to internet
   - Use VPN or private network
   - Configure firewall rules

4. **Authentication**
   - Integrate with your application's auth system
   - Use OAuth2/OIDC for Camunda
   - Implement API key authentication

## 📊 Performance Tips

1. **Database Optimization**
   - Add indexes on frequently queried tables
   - Configure connection pooling
   - Regular database maintenance

2. **Camunda Tuning**
   ```yaml
   JAVA_OPTS: -Xmx2g -XX:MaxMetaspaceSize=512m
   ```

3. **Caching**
   - Enable Redis for session management
   - Cache process definitions
   - Use HTTP client connection pooling

## 🧪 Testing

### Integration Tests

```csharp
[Fact]
public async Task Should_Deploy_Workflow_Successfully()
{
    // Arrange
    var request = new DeployWorkflowRequest { ... };
    
    // Act
    var result = await _camundaService.DeployWorkflowAsync(request);
    
    // Assert
    Assert.NotNull(result.DeploymentId);
    Assert.NotNull(result.ProcessDefinitionId);
}
```

### Manual Testing with Postman

Import the Camunda REST API collection:
https://docs.camunda.org/manual/latest/reference/rest/overview/

## 📚 Additional Resources

- [Camunda Platform 7 Documentation](https://docs.camunda.org/manual/latest/)
- [Camunda REST API Reference](https://docs.camunda.org/manual/latest/reference/rest/)
- [BPMN 2.0 Tutorial](https://camunda.com/bpmn/)
- [Camunda Best Practices](https://camunda.com/best-practices/)
- [Community Forum](https://forum.camunda.io/)

## 🆘 Support

If you encounter issues:

1. Check the [troubleshooting section](#-troubleshooting)
2. Review Camunda logs: `docker-compose logs camunda`
3. Check application logs
4. Consult [Camunda documentation](https://docs.camunda.org/)
5. Ask in [Camunda forum](https://forum.camunda.io/)

---

**Happy Process Automation! 🚀**
