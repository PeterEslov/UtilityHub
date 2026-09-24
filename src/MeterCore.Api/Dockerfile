FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/MeterCore.Api/MeterCore.Api.csproj", "src/MeterCore.Api/"]
RUN dotnet restore "src/MeterCore.Api/MeterCore.Api.csproj"
COPY . .
WORKDIR "/src/src/MeterCore.Api"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "MeterCore.Api.dll"]
