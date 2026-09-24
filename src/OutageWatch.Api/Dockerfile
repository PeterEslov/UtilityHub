FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/OutageWatch.Api/OutageWatch.Api.csproj", "src/OutageWatch.Api/"]
RUN dotnet restore "src/OutageWatch.Api/OutageWatch.Api.csproj"
COPY . .
WORKDIR "/src/src/OutageWatch.Api"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "OutageWatch.Api.dll"]
