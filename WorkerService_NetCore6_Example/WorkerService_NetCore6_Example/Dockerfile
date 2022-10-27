#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WorkerService_NetCore6_Example/WorkerService_NetCore6_Example.csproj", "WorkerService_NetCore6_Example/"]
RUN dotnet restore "WorkerService_NetCore6_Example/WorkerService_NetCore6_Example.csproj"
COPY . .
WORKDIR "/src/WorkerService_NetCore6_Example"
RUN dotnet build "WorkerService_NetCore6_Example.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WorkerService_NetCore6_Example.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkerService_NetCore6_Example.dll"]