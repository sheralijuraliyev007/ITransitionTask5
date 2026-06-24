FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish ITransitionTask5/ITransitionTask5.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update && apt-get install -y \
    espeak-ng \
    fonts-freefont-ttf \
    fontconfig \
    && fc-cache -f \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ITransitionTask5.dll"]