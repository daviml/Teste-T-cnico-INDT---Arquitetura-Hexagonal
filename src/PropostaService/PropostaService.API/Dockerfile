# Imagem do PropostaService. Build a partir da RAIZ do repositório
# (context: .) — precisa do Directory.Build.props e dos projetos referenciados.
# Uso no docker-compose: dockerfile: src/PropostaService/PropostaService.API/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1) Restore isolado: copia só os manifestos (.csproj + props) antes do código.
#    Enquanto as dependências não mudam, esta camada fica em cache e o restore
#    (passo lento) não é refeito a cada alteração de código-fonte.
COPY Directory.Build.props ./
COPY src/Contracts/Contracts.csproj src/Contracts/
COPY src/PropostaService/PropostaService.Domain/PropostaService.Domain.csproj src/PropostaService/PropostaService.Domain/
COPY src/PropostaService/PropostaService.Application/PropostaService.Application.csproj src/PropostaService/PropostaService.Application/
COPY src/PropostaService/PropostaService.Infrastructure/PropostaService.Infrastructure.csproj src/PropostaService/PropostaService.Infrastructure/
COPY src/PropostaService/PropostaService.API/PropostaService.API.csproj src/PropostaService/PropostaService.API/
RUN dotnet restore "src/PropostaService/PropostaService.API/PropostaService.API.csproj"

# 2) Copia o restante do código e publica sem refazer o restore.
COPY . .
RUN dotnet publish "src/PropostaService/PropostaService.API/PropostaService.API.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Roda como usuário não-root (UID fornecido pela própria imagem .NET) — princípio do
# menor privilégio, alinhado à postura de segurança do ADR 0001.
USER $APP_UID

# A imagem aspnet escuta em 8080 por padrão; o mapeamento de porta fica no compose.
EXPOSE 8080
ENTRYPOINT ["dotnet", "PropostaService.API.dll"]
