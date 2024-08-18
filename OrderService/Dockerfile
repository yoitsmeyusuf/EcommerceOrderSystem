# .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy and restore packages
COPY *.csproj ./
RUN dotnet restore

# Copy the project files and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "OrderService.dll"]