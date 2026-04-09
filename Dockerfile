# 1️⃣ Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Копируем решение и проекты для кэширования restore
COPY PresentationCreator.sln ./
COPY PresentationApi/PresentationApi.csproj ./PresentationApi/
COPY PresentationApi.Core/PresentationApi.Core.csproj ./PresentationApi.Core/
COPY PresentationApi.Infrastructure/PresentationApi.Infrastructure.csproj ./PresentationApi.Infrastructure/

RUN dotnet restore

# Копируем весь проект
COPY . ./

WORKDIR /src/PresentationApi
RUN dotnet publish -c Release -o /app/out

# 2️⃣ Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Копируем сборку
COPY --from=build /app/out ./

# Проброс SQLite файла через volume
VOLUME [ "/app/Data" ]

EXPOSE 5000
ENTRYPOINT ["dotnet", "PresentationApi.dll"]