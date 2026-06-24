FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish ITransitionTask5/ITransitionTask5.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update && apt-get install -y \
    espeak-ng \
    fonts-freefont-ttf \
    fontconfig \
    wget \
    && fc-cache -f \
    && rm -rf /var/lib/apt/lists/*

# Install Piper
RUN wget -q https://github.com/rhasspy/piper/releases/download/2023.11.14-2/piper_linux_x86_64.tar.gz \
    && tar -xzf piper_linux_x86_64.tar.gz -C /usr/local/ \
    && rm piper_linux_x86_64.tar.gz

# Download voice models (en, ru, es)
RUN mkdir -p /app/voices \
    && wget -q -O /app/voices/en.onnx https://huggingface.co/rhasspy/piper-voices/resolve/main/en/en_US/lessac/medium/en_US-lessac-medium.onnx \
    && wget -q -O /app/voices/en.onnx.json https://huggingface.co/rhasspy/piper-voices/resolve/main/en/en_US/lessac/medium/en_US-lessac-medium.onnx.json \
    && wget -q -O /app/voices/ru.onnx https://huggingface.co/rhasspy/piper-voices/resolve/main/ru/ru_RU/irina/medium/ru_RU-irina-medium.onnx \
    && wget -q -O /app/voices/ru.onnx.json https://huggingface.co/rhasspy/piper-voices/resolve/main/ru/ru_RU/irina/medium/ru_RU-irina-medium.onnx.json \
    && wget -q -O /app/voices/es.onnx https://huggingface.co/rhasspy/piper-voices/resolve/main/es/es_ES/mls_10246/medium/es_ES-mls_10246-medium.onnx \
    && wget -q -O /app/voices/es.onnx.json https://huggingface.co/rhasspy/piper-voices/resolve/main/es/es_ES/mls_10246/medium/es_ES-mls_10246-medium.onnx.json

WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ITransitionTask5.dll"]