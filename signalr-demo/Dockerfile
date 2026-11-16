FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY signalr-demo/SignalRDemo.csproj ./
RUN dotnet restore SignalRDemo.csproj

COPY signalr-demo/ ./
RUN dotnet publish SignalRDemo.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SignalRDemo.dll"]
