# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish ITransitionTask5.Blazor/ITransitionTask5.Blazor.csproj -c Release -o /app/publish

# Serve stage — Blazor WASM is static files, use nginx
FROM nginx:alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY ITransitionTask5.Blazor/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80