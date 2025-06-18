# Etapa de construção
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copia todos os arquivos do diretório src (sem o src na frente)
COPY . .

# Listar arquivos após a cópia
RUN ls -al /app

# Configura o diretório de trabalho para o projeto da API
WORKDIR /app/Gerenciador.Api

# Restaura as dependências
RUN dotnet restore

# Publica o projeto
RUN dotnet publish -c Release -o /app/out

# Imagem final
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

# Copia os arquivos publicados da etapa anterior
COPY --from=build-env /app/out .

# Define o ponto de entrada da aplicação
ENTRYPOINT ["dotnet", "Gerenciador.Api.dll"]
