FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore TimeIsNow.slnx
RUN dotnet publish src/TimeIsNow.API/TimeIsNow.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5099
EXPOSE 5099
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TimeIsNow.API.dll"]
