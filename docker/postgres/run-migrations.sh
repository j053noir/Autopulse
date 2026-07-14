#!/bin/bash
set -e

echo "Installing dotnet-ef tool..."
dotnet tool install --global dotnet-ef
export PATH="$PATH:/root/.dotnet/tools"

echo "Restoring packages..."
dotnet restore --no-cache

echo "Running EF migrations on Master database..."
dotnet ef database update --project AutoPulse.Infrastructure --startup-project AutoPulse.Api --connection "Host=postgres-master;Database=autopulse;Port=5432;Username=autopulse;Password=mysecretpassword"

echo "Running EF migrations on Slave database..."
dotnet ef database update --project AutoPulse.Infrastructure --startup-project AutoPulse.Api --connection "Host=postgres-slave;Database=autopulse-slave;Port=5432;Username=autopulse;Password=mysecretpassword"

echo "Migrations completed successfully!"
