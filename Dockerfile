FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Quiz Application.csproj", "./"]
RUN dotnet restore "Quiz Application.csproj"

COPY . .
RUN dotnet publish "Quiz Application.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

COPY --from=build /app/publish .
EXPOSE 10000

ENTRYPOINT ["dotnet", "Quiz Application.dll"]
