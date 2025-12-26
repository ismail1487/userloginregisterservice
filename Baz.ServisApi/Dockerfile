# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["bazuserloginregisterservis/Baz.ServisApi/Baz.UserLoginServiceApi.csproj", "bazuserloginregisterservis/Baz.ServisApi/"]
COPY ["bazuserloginregisterservis/Baz.Service/Baz.Service.csproj", "bazuserloginregisterservis/Baz.Service/"]
COPY packages /src/packages
RUN dotnet restore "bazuserloginregisterservis/Baz.ServisApi/Baz.UserLoginServiceApi.csproj" --source https://https://nuget.cdn.azure.cn/v3/index.json --source /src/packages

COPY bazuserloginregisterservis/Baz.ServisApi bazuserloginregisterservis/Baz.ServisApi
COPY bazuserloginregisterservis/Baz.Service bazuserloginregisterservis/Baz.Service
WORKDIR "/src/bazuserloginregisterservis/Baz.ServisApi"
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
