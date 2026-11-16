# Sliplane SignalR Demo

Standalone ASP.NET Core 10 SignalR service for the Sliplane infrastructure demo.

## Endpoints

- `GET /` - service information
- `GET /health` - health check
- `/hubs/demo` - SignalR Hub endpoint

## Hub methods

- `SendMessage(user, message)` broadcasts to all connected clients using the `ReceiveMessage` event.
- `JoinRoom(roomName)` joins a group.
- `SendRoomMessage(roomName, user, message)` broadcasts to that group.

## Sliplane configuration

- Repository: `drnserhat/sliplane-demo-api`
- Branch: `master`
- Dockerfile Path: `signalr-demo/Dockerfile`
- Root Directory / Build Context: `/` (repository root)
- Public: enabled for this demo
- Protocol: HTTP
- Healthcheck Path: `/health`
- Container port: `8080`

No database or cache environment variables are required for this service.

The permissive CORS policy is only for this demo. Restrict allowed origins before production use.
