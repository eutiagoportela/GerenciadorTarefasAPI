# Gerenciador de Carteiras Digitais</br>
API RESTful para gerenciamento de carteiras digitais, permitindo transferências entre usuários e controle de saldo.</br>

Características:</br>

Clean Architecture: Organização em camadas (Domínio, Aplicação, Infraestrutura e API)</br>
Autenticação JWT: Segurança baseada em tokens</br>
PostgreSQL: Banco de dados relacional robusto</br>
Entity Framework Core: ORM para acesso a dados</br>
Tratamento de Exceções: Middleware global para tratamento consistente de erros</br>
Docker: Configuração pronta para containers</br>

Requisitos:</br>

Docker Desktop (Windows/Mac) ou Docker Engine + Docker Compose (Linux)</br>
Git</br>
Editor de texto (para criar o arquivo .env)</br>


# Estrutura do Projeto
```Gerenciador/
├── src/
│   ├── Gerenciador.Api/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── CarteiraController.cs
│   │   │   ├── TransferenciaController.cs
│   │   │   └── UsuarioController.cs
│   │   ├── Extensions/
│   │   │   └── DependencyInjectionExtensions.cs
│   │   ├── Middleware/
│   │   │   └── ErrorHandlingMiddleware.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── Gerenciador.Aplicacao/
│   │   ├── Services/
│   │   │   ├── TokenService.cs
│   │   │   └── HashService.cs
│   │   └── UseCases/
│   │       ├── Login/
│   │       │   └── DoLogin/ DoLoginUseCase.cs e IDoLoginUseCase.cs
│   │       ├── Carteira/
│   │       │   ├── AdicionarSaldo/ AdicionarSaldoUseCase.cs e IAdicionarSaldoUseCase.cs
│   │       │   └── ConsultarSaldo/ ConsultarSaldoUseCase.cs e IConsultarSaldoUseCase.cs
│   │       ├── Transferencia/
│   │       │   ├── CriarTransferencia/
│   │       │   └── ListarTransferencias/
│   │       └── Usuario/
│   │           └── CriarUsuario/
│   ├── Gerenciador.Dominio/
│   │   ├── Entidades/
│   │   │   ├── CarteiraEntity.cs
│   │   │   ├── TransferenciaEntity.cs
│   │   │   └── UsuarioEntity.cs
│   │   ├── Exceptions/
│   │   │   └── UsuarioNaoEncontradoException.cs
│   │   └── Repositorios/
│   │       ├── ICarteiraRepositorio.cs
│   │       ├── ITransferenciaRepositorio.cs
│   │       └── IUsuarioRepositorio.cs
│   ├── Gerenciador.Infraestrutura/
│   │   ├── Database/
│   │   │   ├── GerenciadorDbContext.cs
│   │   │   └── Migrations/
│   │   └── Repositorios/
│   │       ├── CarteiraRepositorio.cs
│   │       ├── TransferenciaRepositorio.cs
│   │       └── UsuarioRepositorio.cs
│   ├── Gerenciador.Comunicacao/
│   │   ├── Requests/
│   │   │   ├── Carteira/ AdicionarSaldoRequest.cs
│   │   │   ├── Transferencia/ CriarTransferenciaRequest
│   │   │   └── Usuario/ CriarUsuarioRequest.cs e LoginRequest.cs
│   │   └── Responses/
│   │       ├── Carteira/CarteiraResponse.cs
│   │       ├── Transferencia/TransferenciaResponse.cs
│   │       └── Usuario/LoginResponse.cs e UsuarioResponse.cs
│   ├── docker-compose.yml
│   └── .env
└── tests/
    └── Gerenciador.Tests/
        ├── Controllers/
        │   ├── AuthController.cs
        │   ├── CarteiraController.cs
        │   ├── TransferenciaController.cs
        │   └── UsuarioController.cs
        └── UseCases/
            ├── AdicionarSaldoUseCase.cs
            ├── CriarUsuarioUseCaseTests.cs
            └── DoLoginUseCase.cs
```
# Fluxo da Aplicação</br>
Request HTTP → Controller → Request (DTO) → UseCase → Domínio → Repositório → Banco de Dados
                                                   ↓
Response HTTP ← Controller ← Response (DTO) ← UseCase


Controller: Recebe a requisição HTTP, valida o modelo e chama o UseCase apropriado</br>
Comunicação: Contém DTOs (Data Transfer Objects) para Requests e Responses</br>
Aplicação: Contém a lógica de negócios nos UseCases e serviços de suporte</br>
Domínio: Define as entidades, interfaces de repositório e regras de negócio</br>
Infraestrutura: Implementa os repositórios e contém o contexto do banco de dados</br>

📚 Endpoints da API</br>
![image](https://github.com/user-attachments/assets/eaf9dba2-e700-4347-a075-1155729a694b)

# Como Baixar e Executar
1. Clonar o Repositório
bashgit clone https://github.com/seu-usuario/gerenciador-carteiras.git</br>
cd gerenciador-carteiras
2. Navegar para o Diretório Correto</br>
bash# Navegar para a pasta onde está o docker-compose.yml</br>
cd Solution/src

# Verificar se o arquivo docker-compose.yml está presente
dir
# ou
ls</br></br>
3. Configurar Variáveis de Ambiente
Crie um arquivo .env no diretório Solution/src (mesmo diretório do docker-compose.yml) com o seguinte conteúdo:
# Configurações do PostgreSQL
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres

# Configurações JWT
JWT_SECRET=2W3Jj4Gd5Lk7Rr8S6t9Zb1XqCk8Yv3F5Zc9J8Rk1Q0N=</br>
JWT_ISSUER=GerenciadorApi</br>
JWT_AUDIENCE=GerenciadorClient</br>
JWT_EXPIRATION=60</br>

# Configuração CORS
FRONTEND_URL=http://localhost:3000</br>
Nota: Este arquivo é essencial para o funcionamento correto da aplicação, pois fornece variáveis de ambiente necessárias para o Docker Compose.</br></br>
4. Iniciar os Containers
bash# Certifique-se que o Docker Desktop está em execução (ou Docker Engine no Linux)
# No diretório Solution/src, execute:
docker-compose up -d
Este comando irá:

Construir a imagem da API usando o Dockerfile</br>
Iniciar o container do PostgreSQL (gerenciadordb-postgres)
Iniciar o container da API (gerenciador-api-prod) quando o PostgreSQL estiver saudável
Aplicar automaticamente as migrações do banco de dados

5. Verificar Status dos Containers</br>
bash docker-compose ps</br>
Certifique-se de que ambos os serviços (postgres e api) estão no estado "Up".</br></br>
A API estará disponível em: http://localhost:80 e https://localhost:443</br>
Fluxo de Uso da API</br>
A API segue um fluxo de autenticação baseado em JWT (JSON Web Token):</br>

Registro de Usuário (Endpoint Público)</br>

Primeiro, o usuário deve se cadastrar usando o endpoint /api/Usuario/registrar</br>
Não é necessário autenticação para este endpoint</br>


Login (Endpoint Público)</br>

Após o cadastro, o usuário faz login em /api/Auth/login</br>
Ao fazer login com sucesso, recebe um token JWT</br>


Funcionalidades Protegidas (Requerem Autenticação)</br>

Todas as demais funcionalidades requerem o token JWT</br>
O token deve ser enviado no header Authorization: Bearer {token}</br>
Funcionalidades disponíveis:</br>

Consultar saldo da carteira</br>
Adicionar saldo à carteira</br>
Realizar transferências para outros usuários</br>
Listar transferências com filtro por período</br>

Testando a API no Postman</br>
Após iniciar os containers, você pode testar a API seguindo o fluxo de autenticação:</br>
1. Registrar um Usuário</br></br>

Método: POST</br>
URL: http://localhost/api/Usuario/registrar</br>
```Headers: Content-Type: application/json
Body:

json{
  "nome": "Usuário Teste",
  "email": "usuario@teste.com",
  "senha": "Senha@123"
}
```
2. Fazer Login e Obter Token

Método: POST</br>
URL: http://localhost/api/Auth/login</br>
```Headers: Content-Type: application/json
Body:

json{
  "email": "usuario@teste.com",
  "senha": "Senha@123"
}
```
</br>
Resposta: Copie o token JWT retornado</br>

3. Acessar Funcionalidades Protegidas</br>
Para todas as requisições abaixo, inclua o header:</br>

Authorization: Bearer {token}</br>

Consultar Saldo</br>

Método: GET</br>
URL: http://localhost/api/Carteira/saldo</br>

Adicionar Saldo</br>

Método: POST</br>
URL: http://localhost/api/Carteira/adicionar-saldo</br>
```Headers: Content-Type: application/json
Body:

json{
  "valor": 100.00
}
```
Realizar Transferência</br>

Método: POST</br>
URL: http://localhost/api/Transferencia</br>
```Headers: Content-Type: application/json
Body:

json{
  "emailDestinatario": "outro@usuario.com", 
  "valor": 50.00
}
```
</br>
Listar Transferências</br>

Método: GET</br>
URL: http://localhost/api/Transferencia?dataInicio=2025-01-01&dataFim=2025-12-31</br>

Notas sobre Migrações</br>
As migrações do banco de dados são aplicadas automaticamente quando a API inicia. Se você precisar executá-las manualmente por algum motivo, use:</br>
bash# Entrar no container da API</br>
docker exec -it gerenciador-api-prod /bin/bash</br>

# Navegar para o diretório da aplicação
cd /app

# Executar migrações (ajuste o comando conforme necessário)
dotnet ef database update
