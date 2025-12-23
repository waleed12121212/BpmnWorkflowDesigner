@echo off
echo ========================================
echo Starting BPMN Workflow Designer
echo ========================================
echo.

echo Starting API Server (Backend)...
start "BPMN API Server" cmd /k "cd Server\BpmnWorkflow.API && dotnet run --launch-profile https"

echo Waiting for API to start...
timeout /t 5 /nobreak > nul

echo.
echo Starting Client (Frontend)...
start "BPMN Client" cmd /k "cd Client\BpmnWorkflow.Client && dotnet run"

echo.
echo ========================================
echo Both servers are starting!
echo ========================================
echo API Server: https://localhost:7225
echo Client App: https://localhost:7096
echo.
echo Press any key to stop all servers...
pause > nul

echo Stopping servers...
taskkill /FI "WindowTitle eq BPMN API Server*" /T /F
taskkill /FI "WindowTitle eq BPMN Client*" /T /F
