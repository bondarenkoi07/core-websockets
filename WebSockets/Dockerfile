FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["WebSockets/WebSockets.csproj", "WebSockets/"]
RUN dotnet restore "WebSockets/WebSockets.csproj"
COPY . .
WORKDIR "/src/WebSockets"
RUN dotnet build "WebSockets.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebSockets.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebSockets.dll"]
