
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY PresentattionCreator.sln ./
COPY PresentattionApi/PresentattionApi.csproj ./PresentattionApi/
COPY PresentattionApi.Core/PresentattionApi.Core.csproj ./PresentattionApi.Core/
COPY PresentattionApi.Infrastructure/PresentattionApi.Infrastructure.csproj ./PresentattionApi.Infrastructure/

RUN dotnet restore

COPY . ./

WORKDIR /src/PresentattionApi
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

COPY --from=build /app/out ./

VOLUME [ "/app/Data" ]

ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000
ENTRYPOINT ["dotnet", "PresentattionApi.dll"]