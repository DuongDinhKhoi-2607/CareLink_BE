# ==========================================
# Multi-stage build for CareLinkAPI (.NET 10)
# ==========================================

# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file first to take advantage of Docker layer caching
COPY ["CareLinkAPI/CareLinkAPI.csproj", "CareLinkAPI/"]
RUN dotnet restore "CareLinkAPI/CareLinkAPI.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish application in Release mode
WORKDIR "/src/CareLinkAPI"
RUN dotnet publish "CareLinkAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Default port for container (Render will route via PORT or EXPOSE)
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CareLinkAPI.dll"]
