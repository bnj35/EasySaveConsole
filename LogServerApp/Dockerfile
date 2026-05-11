# Use a base image with the .NET SDK to build the application (minimum .NET 6)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

# Copy both projects (EasyLog is a local dependency of LogServerApp)
COPY EasyLog/ ./EasyLog/
COPY LogServerApp/ ./LogServerApp/

RUN dotnet restore LogServerApp/LogServerApp.csproj
RUN dotnet publish LogServerApp/LogServerApp.csproj -c Release -o out

# Use a smaller base image with the .NET runtime to run the application
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime

WORKDIR /app

# Copy the built application from the build stage
COPY --from=build /app/out .

# Expose the port the server listens on
EXPOSE 5000

# Define the application entry point
ENTRYPOINT ["dotnet", "LogServerApp.dll"]