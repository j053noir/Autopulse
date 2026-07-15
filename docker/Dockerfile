FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["AutoPulse.Api/AutoPulse.Api.csproj", "AutoPulse.Api/"]
COPY ["AutoPulse.Application/AutoPulse.Application.csproj", "AutoPulse.Application/"]
COPY ["AutoPulse.Domain/AutoPulse.Domain.csproj", "AutoPulse.Domain/"]
COPY ["AutoPulse.Infrastructure/AutoPulse.Infrastructure.csproj", "AutoPulse.Infrastructure/"]

RUN dotnet restore "AutoPulse.Api/AutoPulse.Api.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish the API
WORKDIR "/src/AutoPulse.Api"
RUN dotnet publish "AutoPulse.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AutoPulse.Api.dll"]
