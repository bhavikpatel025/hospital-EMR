# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file
COPY *.sln ./

# Copy all project files and restore as distinct layers
COPY EMR.API/*.csproj EMR.API/
COPY EMR.Application/*.csproj EMR.Application/
COPY EMR.Domain/*.csproj EMR.Domain/
COPY EMR.Infrastructure/*.csproj EMR.Infrastructure/
COPY EMR.Shared/*.csproj EMR.Shared/
RUN dotnet restore

# Copy the remaining files and build the app
COPY . ./
WORKDIR /app/EMR.API
RUN dotnet publish -c Release -o /out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out ./

# Expose port (Render sets PORT environment variable dynamically)
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
EXPOSE 8080

ENTRYPOINT ["dotnet", "EMR.API.dll"]
