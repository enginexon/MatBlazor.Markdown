FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["MatBlazor.Markdown.Demo.Server/MatBlazor.Markdown.Demo.Server.csproj", "MatBlazor.Markdown.Demo.Server/"]
RUN dotnet restore "MatBlazor.Markdown.Demo.Server/MatBlazor.Markdown.Demo.Server.csproj"
COPY . .
WORKDIR "/src/MatBlazor.Markdown.Demo.Server"
RUN dotnet build "MatBlazor.Markdown.Demo.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MatBlazor.Markdown.Demo.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MatBlazor.Markdown.Demo.Server.dll"]
