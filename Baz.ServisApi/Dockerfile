# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Baz.ServisApi/Baz.UserLoginServiceApi.csproj", "Baz.ServisApi/"]
COPY ["Baz.Service/Baz.Service.csproj", "Baz.Service/"]
RUN dotnet restore "Baz.ServisApi/Baz.UserLoginServiceApi.csproj" -s http://127.0.0.1:4444/v3/index.json -s https://api.nuget.org/v3/index.json

COPY . .
WORKDIR "/src/Baz.ServisApi"
RUN dotnet build "Baz.UserLoginServiceApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Baz.UserLoginServiceApi.csproj" -c Release -o /app/publish

FROM base AS final

# Change timezone to local time
ENV TZ=Europe/Istanbul
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Baz.UserLoginApi.dll"]