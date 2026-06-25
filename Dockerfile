FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . ./
# Aquí le decimos que entre a la subcarpeta de Visual Studio
WORKDIR /app/backendclientes1
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "backendclientes1.dll"]
