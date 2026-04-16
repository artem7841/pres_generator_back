
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Копируем решение и проекты для кэширования restore
COPY PresentationCreator.sln ./
COPY PresentationApi/PresentationApi.csproj ./PresentationApi/
COPY PresentationApi.Core/PresentationApi.Core.csproj ./PresentationApi.Core/
COPY PresentationApi.Infrastructure/PresentationApi.Infrastructure.csproj ./PresentationApi.Infrastructure/
COPY TestPresentation/ ./TestPresentation/

RUN dotnet restore

COPY . ./

RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y \
    libreoffice \
    libreoffice-writer \
    libreoffice-core \
    fonts-dejavu \
    fonts-liberation \
    fonts-freefont-ttf \
    && rm -rf /var/lib/apt/lists/*

COPY pres.pptx /app/pres.pptx
COPY prompt.txt /app/prompt.txt

COPY --from=build /app/out ./

VOLUME [ "/app/Data" ]

ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000
ENTRYPOINT ["dotnet", "PresentationApi.dll"]