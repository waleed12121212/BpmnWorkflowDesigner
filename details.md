# BPMN Workflow Designer Project Summary

## Overview
A comprehensive BPMN Workflow Designer and Execution platform built with .NET Core and Blazor, integrated with Camunda Platform 7 for process orchestration.

## Architecture

### Backend (Server)
- **Framework**: .NET 9.0 (ASP.NET Core Web API)
- **Database**: SQL Server (Application Data), PostgreSQL (Camunda Data)
- **Authentication**: JWT Bearer Tokens
- **Key Services**:
    - `CamundaService`: Handles all communication with Camunda REST API (Deployment, Process Start, Task Management).
    - `WorkflowService`: Manages local storage of BPMN designs.
    - `AuthService`: Handles user login and registration.

### Frontend (Client)
- **Framework**: Blazor WebAssembly
- **UI Library**: Radzen Blazor Components
- **Features**:
    - **Dashboard**: Overview of workflow stats.
    - **Process Designer**: Visual BPMN editor.
    - **Process Instances**: Monitor running workflows, view status, and cancel instances.
    - **Task List**: Manage, claim, and complete user tasks.
    - **Details View**: Visualize execution path (Activity Tree) and process variables.

### Infrastructure (Docker)
- **Camunda Platform 7**: Process engine running on port `8081` (mapped to internal 8080).
- **PostgreSQL**: Database for Camunda.
- **Redis**: Caching (configured but currently optional).

## Key Workflows & Features

### 1. Process Execution Flow
1.  **Deploy**: User creates a BPMN diagram and deploys it (`/camunda/deploy`).
2.  **Start**: User starts a new instance from the "Process Instances" page.
    -   *API*: `POST /api/camunda/process-definition/key/{key}/start`
3.  **Monitor**: The new instance appears in the list.
    -   *API*: `GET /api/camunda/process-instance`
    -   *Enhancement*: instances are enriched with Definition Names for better visibility.
4.  **Execute Tasks**: User navigates to "Task List" to see tasks assigned to them or unassigned group tasks.
    -   *Actions*: Claim, Complete, Unclaim.
    -   *API*: `GET /api/camunda/task`, `POST /api/camunda/task/{id}/complete`.

### 2. Camunda Integration Details
- **Connectivity**: The .NET API acts as a gateway/proxy to Camunda.
- **Error Handling**: 
    -   Returns **503 Service Unavailable** if Docker is down.
    -   Handles `DateTime` parsing quirks from Camunda using a custom `JsonConverter`.
- **Configuration**:
    -   **Port**: 8081 (to avoid interfering with local agents on 8080).
    -   **Timeout**: 15 seconds for responsiveness.

## Recent Fixes & Improvements
- **RadzenTreeLevel Error**: Fixed `invalid operation` exception by removing the payload-less `TItem` attribute.
- **Camunda Port Conflict**: Moved Camunda to 8081.
- **Process Names**: Fixed missing process names in the grid by mapping `DefinitionId` to `DefinitionName`.
- **Date Parsing**: Added `CamundaDateTimeConverter` to handle non-standard date formats returned by the engine.

## How to Run
1.  **Start Infrastructure**:
    ```powershell
    docker-compose up -d
    ```
2.  **Start Backend**:
    ```powershell
    cd Server\BpmnWorkflow.API
    dotnet run --launch-profile https
    ```
3.  **Start Frontend**:
    ```powershell
    cd Client\BpmnWorkflow.Client
    dotnet run
    ```
