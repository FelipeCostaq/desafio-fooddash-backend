# FoodDash API
API para cadastro de pratos em um cardápio e criação de pedidos usando arquitetura Minimal API. Desenvolvida em C# com ASP.NET Core e Entity Framework.

## Tecnologias Utilizadas
- ASP.NET Core Web API Minimal API
- Entity Framework Core
- SQLite
- Docker
  
### Pré-requisitos
- [.NET SDK 10+](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Passos para rodar o projeto

Docker

Siga o passo a passo abaixo para subir a aplicação do zero utilizando Docker.

#### 1. Clone o repositório e acesse a pasta
```bash
git clone https://github.com/FelipeCostaq/desafio-fooddash-backend
cd desafio-fooddash-backend
```

#### 2. Prepare o arquivo de banco de dados
O Docker precisa que o arquivo do SQLite exista na sua máquina para realizar o mapeamento de volume.

No Windows (PowerShell):

```bash
New-Item app.db
```

No Linux ou Mac:

```bash
touch app.db
```

#### 3. Build e Execução
Agora, compile a imagem e suba o container com os comandos abaixo:

Build da imagem:

```bash
docker build -t fooddash-api .
```
Executar o container (Porta 8080):

No Windows (PowerShell):

```bash
docker run -d -p 8080:8080 --name fooddash-container -v "${PWD}/app.db:/app/app.db" fooddash-api
```

No Linux ou Mac:

```bash
docker run -d -p 8080:8080 --name fooddash-container -v "$(pwd)/app.db:/app/app.db" fooddash-api
```

#### 4. Acesso
Pronto! A API está rodando em: http://localhost:8080/swagger

# Endpoints

## Menu

- **POST** `/menu` – Adiciona um prato no menu.
- **GET** `/menu/status?=` – Lista todos pratos do menu caso status venha vazio ou inválido, caso receba o valor available lista todos pratos disponiveis, caso venha unavailable lista todos pratos indisponiveis.
- **PUT** `/menu/{id}` – Editar um prato.
- **PATCH** `/menu/{id}/availability` - Editar a disponbiilidade de um prato.
- **DELETE** `/menu/{id}` – Remove um prato.

## Orders

- **GET** `/orders` - Lista todos pedidos.
- **POST** `/orders` - Criar um pedido.
- **GET** `/orders/{id}` - Listar um pedido pelo id.
- **DELETE** `/orders/{id}` - Deleta um pedido.
- **PATCH** `/orders/{id}/status` - Atualiza apenas o status do pedido.
