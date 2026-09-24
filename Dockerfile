FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY runner.slnx .editorconfig Directory.Build.props Directory.Packages.props ./
COPY src/Runner.Core/Runner.Core.csproj src/Runner.Core/
COPY src/Runner.Host/Runner.Host.csproj src/Runner.Host/
RUN dotnet restore src/Runner.Host/Runner.Host.csproj

COPY src/ src/
COPY definitions/ definitions/
RUN dotnet publish src/Runner.Host/Runner.Host.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Runner.Host.dll"]
