FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["OpenFTTH.Address.Service/OpenFTTH.Address.Service.csproj", "OpenFTTH.Address.Service/"]
RUN dotnet restore "OpenFTTH.Address.Service/OpenFTTH.Address.Service.csproj"
COPY . .
WORKDIR "/src/OpenFTTH.Address.Service"
RUN dotnet build "OpenFTTH.Address.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenFTTH.Address.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenFTTH.Address.Service.dll"]
