FROM nginx:alpine AS base
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Valtuutus.Playground/Valtuutus.Playground.csproj", "Valtuutus.Playground/"]
RUN dotnet restore "Valtuutus.Playground/Valtuutus.Playground.csproj"
COPY . .
WORKDIR "/src/Valtuutus.Playground"
RUN dotnet build "Valtuutus.Playground.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Valtuutus.Playground.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM  base AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY nginx.conf /etc/nginx/nginx.conf
