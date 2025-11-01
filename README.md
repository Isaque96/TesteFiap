### Execução rápida — Docker & docker-compose

Antes de tudo: o Docker precisa estar instalado e operacional na sua máquina (o daemon do Docker deve estar rodando).

Verificações rápidas:

* Verifique se o Docker está instalado e rodando:

  ```bash
  docker --version
  docker info   # mostra se o daemon está ativo
  ```

* Verifique a versão do Docker Compose (existem duas formas comuns):

  ```bash
  docker compose version      # Compose v2 (recomendado)
  docker-compose --version    # Compose v1 (antigo)
  ```

* Garanta que as portas 5000 e 5001 não estejam em uso por outro processo.

Rodando tudo com o nosso docker-compose (build + up):

```bash
# na raiz do projeto (onde está o docker-compose.yml)
docker-compose up --build
# ou, se usar Compose v2:
docker compose up --build
```

* Para rodar em modo destacado (background):

  ```bash
  docker-compose up --build -d
  # ou:
  docker compose up --build -d
  ```

* Acompanhar logs:

  ```bash
  docker-compose logs -f
  docker-compose logs -f api
  docker-compose logs -f sqlserver
  ```

* Parar e remover containers:

  ```bash
  docker-compose down
  ```

* Remover volumes ( ATENÇÃO: isso apaga dados persistidos ):

  ```bash
  docker-compose down -v
  ```

Dicas:

* Se estiver no Windows com Docker Desktop, confirme que o Docker Desktop está aberto e o WSL2 (quando aplicável) habilitado.

* O container do SQL Server pode demorar a ficar disponível — por isso o compose aqui foi pensado para usar healthchecks / scripts de espera (wait-for) para que a API aplique migrations/seed somente quando o banco estiver pronto.

* Caso ocorra erro no start do SQL Server, verifique os logs do container e a variável `SA_PASSWORD` (ela deve obedecer às regras de complexidade do SQL Server).

---
 
### Sistema Administrativo Escola (AdmSchoolApp)

#### Visão geral

Sistema em .NET (API RESTful com Minimal APIs) + Frontend em Razor Pages para gerenciar alunos, turmas e matrículas.\
Autenticação via JWT com roles, banco de dados relacional SQL Server, testes com xUnit + NSubstitute + FluentAssertions, Swagger para documentação e tudo orquestrado via Docker Compose para facilitar execução em qualquer ambiente.

#### Principais características

* API RESTful (Minimal API)

* Autenticação JWT com roles (Admin, User)

* Banco: SQL Server (container)

* Frontend: Razor Pages

* Testes unitários e de integração com xUnit, NSubstitute e FluentAssertions

* Swagger (OpenAPI) para documentação interativa

* Docker Compose para orquestração de API, Frontend e SQL Server

#### Portas

* Backend (API): [http://localhost:5000](http://localhost:5000)

* Frontend (Razor Pages): [http://localhost:5001](http://localhost:5001)

#### Usuário

> Observação: Para facilitar testes, a rota de criação de usuário está liberada

#### Requisitos

* Docker & Docker Compose

* .NET SDK (só necessário se for executar local sem Docker)

* (Opcional) SSMS / Azure Data Studio para inspecionar o banco

#### Estrutura sugerida do repositório

* Arquivos Bases

  * /Api       -> Projeto .NET Minimal API

  * /Web       -> Frontend Razor Pages

  * /Domain    -> DTOs/Models compartilhados (opcional)

* Testes

  * /Api.UnitTests

* docker-compose.yml

* [README.md](http://README.md)

* .env (opcional)

#### Endpoints (exemplos)

* POST /api/auth/login — autenticação (retorna JWT)

* POST /api/auth/register — registro (se aplicável)

* GET /api/students — lista de alunos

* GET /api/students/{id} — obter aluno

* POST /api/students — criar aluno (role: Admin)

* PUT /api/students/{id} — atualizar aluno (role: Admin)

* DELETE /api/students/{id} — deletar aluno (role: Admin)

* GET /api/classes — listar turmas

* POST /api/enrollments — matricular aluno

* GET /swagger — UI do Swagger

> Ajuste os endpoints conforme a implementação real.

#### Autenticação & Roles

* Uso de JWT Bearer tokens.

* Roles comuns: `Admin`, `User`.

* Header de autenticação:

```
Authorization: Bearer <token>
```

#### Como rodar o projeto (com o docker-compose que orquestra tudo)

1. Clone o repositório e entre na pasta raiz (onde está o `docker-compose.yml`):

```bash
git clone <seu-repo.git>
cd NomeDoRepositorio
```

1. (Opcional) Crie/edite um arquivo `.env` para variáveis de ambiente (exemplo):

```
SA_PASSWORD=Your_strong@Passw0rd
MSSQL_PID=Express
ASPNETCORE_ENVIRONMENT=Development
```

1. Inicie os serviços (build + up):

```bash
docker-compose up --build
```

* O `docker-compose` está preparado para:

  * subir o container do SQL Server;

  * aplicar migrations/seed (se configurado);

  * iniciar a API na porta 5000;

  * iniciar o frontend na porta 5001.

1. Acesse a aplicação:

* Swagger (API): [http://localhost:5000/swagger](http://localhost:5000/swagger)

* Frontend (Razor Pages): [http://localhost:5001](http://localhost:5001)

1. Parar e remover containers:

```bash
docker-compose down
```

1. Para remover volumes (cuidado: apaga dados persistidos):

```bash
docker-compose down -v
```

#### Migrations & Seeding

* Para aplicar migrations manualmente (sem Docker):

```bash
cd src/Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

* Observação sobre seed de usuários:

  * Se inserir usuários via SQL direto, converta corretamente o hash de senha para `VARBINARY`. Exemplo (preferível criar via código):

```csharp
var user = new ApplicationUser { UserName = "isaque.silva@fiap.com.br", Email = "isaque.silva@fiap.com.br" };
await userManager.CreateAsync(user, "Senha*forte1");
```

* Exemplo T-SQL genérico (quando tiver hash em HEX):

```sql
INSERT INTO AspNetUsers (Id, UserName, Email, PasswordHash, ...)
VALUES ('...', 'isaque.silva@fiap.com.br', 'isaque.silva@fiap.com.br', 0x<HEX_HERE>, ...);
```

#### Docker Compose — dicas

* O container do SQL Server pode demorar para ficar pronto; use healthchecks e scripts de espera para que a API aplique migrations somente quando o banco estiver saudável.

* Se o SQL Server não iniciar:

  * Verifique logs: `docker-compose logs sqlserver`;

  * Cheque se `SA_PASSWORD` atende requisitos de complexidade;

  * Remova volumes e recrie se houver corrupção.

#### Execução de testes

```bash
dotnet test ./Api.UnitTests
```

#### Scaffolding do DbContext (a partir de DB existente)

```bash
dotnet ef dbcontext scaffold "Server=host,1433;Database=NomeDB;User Id=sa;Password=YourPassword;" Microsoft.EntityFrameworkCore.SqlServer -o Models -c ApplicationDbContext --schema dbo --use-database-names
```

#### Debug & Troubleshooting rápido

* Ver logs:

```bash
docker-compose logs -f
docker-compose logs api
docker-compose logs sqlserver
```

* Remover containers/volumes e recriar:

```bash
docker-compose down -v --remove-orphans
docker-compose up --build
```

* Se seed não for aplicado automaticamente, execute a rotina de seed no startup da API ou rode o script SQL via `sqlcmd`.

#### Boas práticas

* Siga SOLID, Clean Code e TDD.

* Isole lógica de negócio em services e use DI.

* Separe secrets em variáveis de ambiente.

#### Contribuições

* Abra issues e PRs; inclua testes e documentação para mudanças relevantes.

#### Licença

* Adicione um arquivo `LICENSE` conforme sua escolha (MIT, GPL, etc.).

---

**Resumo rápido**

* Backend: porta `5000`

* Frontend: porta `5001`

* Usuário padrão: `isaque.silva@fiap.com.br` / `Senha*forte1`

Obrigado — o arquivo `README.md` foi criado na workspace. Se quiser, eu anexo aqui para download ou faço alguma alteração de layout/linguagem.
