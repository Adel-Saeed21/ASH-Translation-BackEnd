# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything and restore dependencies
COPY . .
RUN dotnet restore

# Build and publish the app
RUN dotnet publish -c Release -o out

# Use a lightweight runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port 8080 (Railway default)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Start the app
ENTRYPOINT ["dotnet", "ASH Translation.dll"]
