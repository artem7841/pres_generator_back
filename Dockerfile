# СТАДИЯ СБОРКИ
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Копируем файлы проектов для кэширования слоев восстановления
COPY PresentationCreator.sln ./
COPY PresentationApi/*.csproj ./PresentationApi/
COPY PresentationApi.Core/*.csproj ./PresentationApi.Core/
COPY PresentationApi.Infrastructure/*.csproj ./PresentationApi.Infrastructure/
COPY TestPresentation/*.csproj ./TestPresentation/

RUN dotnet restore


COPY . ./
RUN dotnet publish PresentationApi/PresentationApi.csproj -c Release -o /app/out


FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app


RUN apt-get update && apt-get install -y --no-install-recommends \
    fonts-dejavu \
    fonts-liberation \
    && rm -rf /var/lib/apt/lists/*


COPY pres.pptx prompt.txt ./

COPY --from=build /app/out ./

VOLUME [ "/app/Data" ]

# Настройки сети
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "PresentationApi.dll"]
