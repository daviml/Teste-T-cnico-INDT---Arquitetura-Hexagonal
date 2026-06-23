# Imagem do ContratacaoService. Build a partir da RAIZ do repositório
# (context: .) — precisa do Directory.Build.props e dos projetos referenciados.
# Uso no docker-compose: dockerfile: src/ContratacaoService/ContratacaoService.API/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1) Restore isolado: copia só os manifestos (.csproj + props) antes do código.
#    Enquanto as dependências não mudam, esta camada fica em cache e o restore
#    (passo lento) não é refeito a cada alteração de código-fonte.
COPY Directory.Build.props ./
COPY src/Contracts/Contracts.csproj src/Contracts/
COPY src/ContratacaoService/ContratacaoService.Domain/ContratacaoService.Domain.csproj src/ContratacaoService/ContratacaoService.Domain/
COPY src/ContratacaoService/ContratacaoService.Application/ContratacaoService.Application.csproj src/ContratacaoService/ContratacaoService.Application/
COPY src/ContratacaoService/ContratacaoService.Infrastructure/ContratacaoService.Infrastructure.csproj src/ContratacaoService/ContratacaoService.Infrastructure/
COPY src/ContratacaoService/ContratacaoService.API/ContratacaoService.API.csproj src/ContratacaoService/ContratacaoService.API/
RUN dotnet restore "src/ContratacaoService/ContratacaoService.API/ContratacaoService.API.csproj"

# 2) Copia o restante do código e publica sem refazer o restore.
COPY . .
RUN dotnet publish "src/ContratacaoService/ContratacaoService.API/ContratacaoService.API.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Roda como usuário não-root (UID fornecido pela própria imagem .NET) — princípio do
# menor privilégio, alinhado à postura de segurança do ADR 0001.
USER $APP_UID

# A imagem aspnet escuta em 8080 por padrão; o mapeamento de porta fica no compose.
EXPOSE 8080
ENTRYPOINT ["dotnet", "ContratacaoService.API.dll"]
