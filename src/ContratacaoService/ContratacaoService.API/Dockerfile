# Imagem do ContratacaoService. Build a partir da RAIZ do repositório
# (context: .) — precisa do Directory.Build.props e dos projetos referenciados.
# Uso no docker-compose: dockerfile: src/ContratacaoService/ContratacaoService.API/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish "src/ContratacaoService/ContratacaoService.API/ContratacaoService.API.csproj" \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# A imagem aspnet escuta em 8080 por padrão; o mapeamento de porta fica no compose.
EXPOSE 8080
ENTRYPOINT ["dotnet", "ContratacaoService.API.dll"]
