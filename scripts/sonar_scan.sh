#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TOOLS_DIR="$HOME/.dotnet/tools"
SONAR_HOST_URL="${SONAR_HOST_URL:-http://localhost:9000}"
SONAR_PROJECT_KEY="${SONAR_PROJECT_KEY:-oficina-mecanica-api}"
SONAR_TOKEN="${SONAR_TOKEN:?SONAR_TOKEN nao informado}"
COVERAGE_DIR="$ROOT_DIR/coverage"

export PATH="$PATH:$TOOLS_DIR"

rm -rf "$COVERAGE_DIR"
mkdir -p "$COVERAGE_DIR"

pushd "$ROOT_DIR" > /dev/null

dotnet-sonarscanner begin \
  /k:"$SONAR_PROJECT_KEY" \
  /d:sonar.host.url="$SONAR_HOST_URL" \
  /d:sonar.token="$SONAR_TOKEN" \
  /d:sonar.cs.opencover.reportsPaths="coverage/*.opencover.xml"

/usr/local/share/dotnet/dotnet build --no-incremental

coverlet "$ROOT_DIR/OficinaMecanica.Api.UnitTests/bin/Debug/net10.0/OficinaMecanica.Api.UnitTests.dll" \
  --target "/usr/local/share/dotnet/dotnet" \
  --targetargs "test $ROOT_DIR/OficinaMecanica.Api.UnitTests/OficinaMecanica.Api.UnitTests.csproj --no-build" \
  -f opencover \
  -o "$COVERAGE_DIR/unit.opencover.xml"

coverlet "$ROOT_DIR/OficinaMecanica.Api.IntegrationTests/bin/Debug/net10.0/OficinaMecanica.Api.IntegrationTests.dll" \
  --target "/usr/local/share/dotnet/dotnet" \
  --targetargs "test $ROOT_DIR/OficinaMecanica.Api.IntegrationTests/OficinaMecanica.Api.IntegrationTests.csproj --no-build" \
  -f opencover \
  -o "$COVERAGE_DIR/integration.opencover.xml"

dotnet-sonarscanner end /d:sonar.token="$SONAR_TOKEN"

popd > /dev/null
