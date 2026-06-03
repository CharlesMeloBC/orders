# Orders API

API RESTful em .NET 8 para gerenciamento de pedidos de um e-commerce.

## Stack

- .NET 8
- Minimal APIs
- Entity Framework Core
- SQL Server
- MediatR
- Swagger
- Docker / Docker Compose
- xUnit

## Como executar

### 1. Configure a senha do SQL Server

Crie um arquivo `.env` na raiz do projeto:

```env
MSSQL_SA_PASSWORD=YourStrong!Passw0rd
```

A senha precisa atender a política do SQL Server: letras maiúsculas, minúsculas, número, caractere especial e pelo menos 8 caracteres.

### 2. Suba a API e o banco

```powershell
docker compose up --build
```

A API ficará disponível em:

```text
http://localhost:8080
```

O SQL Server ficará disponível em:

```text
localhost,1433
```

As migrations são aplicadas automaticamente no startup da API.

### 3. Acesse a documentação

```text
http://localhost:8080/swagger
```

### 4. Verifique o health check

```bash
curl --request GET \
  --url http://localhost:8080/health
```

Resposta esperada:

```text
Healthy
```

## Como rodar os testes

```powershell
dotnet test Orders.sln
```

## Collection

Para facilitar os testes manuais, o projeto inclui uma collection do Insomnia na raiz do repositório:

```text
Collection.yaml
```

Importe esse arquivo no Insomnia e ajuste o ambiente para apontar para a API local antes de executar os requests.

## Autenticação

Os endpoints de pedidos exigem Bearer Token.

### Gerar token

```bash
curl --request POST \
  --url http://localhost:8080/api/v1/auth/token \
  --header 'Content-Type: application/json' \
  --data '{
    "name": "Charles",
    "document": "30030030030"
  }'
```

Resposta esperada:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2026-06-03T03:00:00Z"
}
```

Use o valor de `token` nos demais endpoints:

```http
Authorization: Bearer SEU_TOKEN
```

### Cenário triste: payload inválido

```bash
curl --request POST \
  --url http://localhost:8080/api/v1/auth/token \
  --header 'Content-Type: application/json' \
  --data '{
    "name": "",
    "document": ""
  }'
```

Resposta esperada:

```http
400 Bad Request
```

```json
{
  "message": "name and document are required."
}
```

## Status do pedido

```text
Initiated  -> pedido criado e recebido
Processed  -> pedido processado pelo sistema
Shipped    -> pedido enviado ao comprador
Cancelled  -> pedido cancelado pelo comprador
```

Regras:

- Apenas pedidos `Initiated` podem ser alterados.
- Apenas pedidos `Initiated` ou `Processed` podem ser cancelados.
- Apenas pedidos `Processed` podem ser enviados.
- Cancelar um pedido não exclui o registro.
- Excluir um pedido preenche `deletedAt` e remove o pedido das consultas comuns.

## Endpoints

### Criar pedido

```bash
curl --request POST \
  --url http://localhost:8080/api/v1/orders \
  --header 'Authorization: Bearer SEU_TOKEN' \
  --header 'Content-Type: application/json' \
  --data '{
    "products": [
      {
        "name": "Produto A",
        "price": 29.90,
        "quantity": 2
      }
    ]
  }'
```

Resposta esperada:

```http
201 Created
```

```json
{
  "id": "bab324b6-7099-4f9c-8d14-5d1c89099ea6",
  "buyerId": "14c027e0-ae5c-8918-c995-9eff32a10b8d",
  "buyerName": "Charles",
  "status": "Initiated",
  "createdAt": "2026-06-03T00:00:00-03:00",
  "items": [
    {
      "productName": "Produto A",
      "unitPrice": 29.90,
      "quantity": 2,
      "lineTotal": 59.80
    }
  ],
  "total": 59.80
}
```

### Cenários tristes da criação

Sem token:

```http
401 Unauthorized
```

Sem produtos:

```json
{
  "products": []
}
```

Resposta esperada:

```http
400 Bad Request
```

Produto com preço inválido:

```json
{
  "products": [
    {
      "name": "Produto A",
      "price": 0,
      "quantity": 1
    }
  ]
}
```

Resposta esperada:

```http
400 Bad Request
```

```json
{
  "title": "Validation error",
  "status": 400,
  "errors": {
    "products[0].price": [
      "price must be greater than zero."
    ]
  }
}
```

## Listar pedidos

```bash
curl --request GET \
  --url http://localhost:8080/api/v1/orders \
  --header 'Authorization: Bearer SEU_TOKEN'
```

Resposta esperada:

```http
200 OK
```

```json
[
  {
    "id": "bab324b6-7099-4f9c-8d14-5d1c89099ea6",
    "buyerId": "14c027e0-ae5c-8918-c995-9eff32a10b8d",
    "buyerName": "Charles",
    "status": "Initiated",
    "createdAt": "2026-06-03T00:00:00-03:00",
    "updatedAt": "2026-06-03T00:00:00-03:00",
    "deletedAt": null,
    "items": [
      {
        "productName": "Produto A",
        "unitPrice": 29.90,
        "quantity": 2,
        "lineTotal": 59.80
      }
    ],
    "total": 59.80
  }
]
```

### Filtrar por status

```bash
curl --request GET \
  --url 'http://localhost:8080/api/v1/orders?status=Initiated' \
  --header 'Authorization: Bearer SEU_TOKEN'
```

Também são aceitos:

```text
Processed
Shipped
Cancelled
```

### Cenários tristes da listagem

Sem token:

```http
401 Unauthorized
```

Status inválido:

```bash
curl --request GET \
  --url 'http://localhost:8080/api/v1/orders?status=InvalidStatus' \
  --header 'Authorization: Bearer SEU_TOKEN'
```

Resposta esperada:

```http
400 Bad Request
```

## Buscar pedido por ID

```bash
curl --request GET \
  --url http://localhost:8080/api/v1/orders/bab324b6-7099-4f9c-8d14-5d1c89099ea6 \
  --header 'Authorization: Bearer SEU_TOKEN'
```

Resposta esperada:

```http
200 OK
```

```json
{
  "id": "bab324b6-7099-4f9c-8d14-5d1c89099ea6",
  "buyerId": "14c027e0-ae5c-8918-c995-9eff32a10b8d",
  "buyerName": "Charles",
  "status": "Initiated",
  "createdAt": "2026-06-03T00:00:00-03:00",
  "updatedAt": "2026-06-03T00:00:00-03:00",
  "deletedAt": null,
  "items": [],
  "total": 0
}
```

### Cenários tristes da busca

Pedido inexistente, excluído ou de outro comprador:

```http
404 Not Found
```

```json
{
  "title": "Not found",
  "status": 404,
  "detail": "Order not found."
}
```

ID fora do formato GUID:

```http
404 Not Found
```

## Alterar pedido

Só pedidos `Initiated` podem ser alterados.

```bash
curl --request PUT \
  --url http://localhost:8080/api/v1/orders/bab324b6-7099-4f9c-8d14-5d1c89099ea6 \
  --header 'Authorization: Bearer SEU_TOKEN' \
  --header 'Content-Type: application/json' \
  --data '{
    "products": [
      {
        "name": "Produto B",
        "price": 50,
        "quantity": 2
      }
    ]
  }'
```

Resposta esperada:

```http
200 OK
```

```json
{
  "id": "bab324b6-7099-4f9c-8d14-5d1c89099ea6",
  "buyerId": "14c027e0-ae5c-8918-c995-9eff32a10b8d",
  "buyerName": "Charles",
  "status": "Initiated",
  "createdAt": "2026-06-03T00:00:00-03:00",
  "updatedAt": "2026-06-03T00:10:00-03:00",
  "deletedAt": null,
  "items": [
    {
      "productName": "Produto B",
      "unitPrice": 50,
      "quantity": 2,
      "lineTotal": 100
    }
  ],
  "total": 100
}
```

### Cenários tristes da alteração

Pedido inexistente, excluído ou de outro comprador:

```http
404 Not Found
```

Pedido já processado, enviado ou cancelado:

```http
422 Unprocessable Entity
```

```json
{
  "title": "Business rule violation",
  "status": 422,
  "detail": "Only orders with status Initiated can be updated."
}
```

Payload sem produtos:

```http
400 Bad Request
```

## Processar pedido

Só pedidos `Initiated` podem ser processados.

```bash
curl --request POST \
  --url http://localhost:8080/api/v1/orders/bab324b6-7099-4f9c-8d14-5d1c89099ea6/process \
  --header 'Authorization: Bearer SEU_TOKEN'
```

Resposta esperada (campos principais; a API também retorna `buyerId`, `buyerName`, datas e itens completos):

```http
200 OK
```

```json
{
  "id": "bab324b6-7099-4f9c-8d14-5d1c89099ea6",
  "status": "Processed",
  "deletedAt": null,
  "items": [],
  "total": 0
}
```

### Cenários tristes do processamento

Pedido inexistente:

```http
404 Not Found
```

Pedido que não está `Initiated`:

```http
422 Unprocessable Entity
```

```json
{
  "title": "Business rule violation",
  "status": 422,
  "detail": "Only orders with status Initiated can be processed."
}
```

## Enviar pedido

Só pedidos `Processed` podem ser enviados.

```bash
curl --request POST \
  --url http://localhost:8080/api/v1/orders/bab324b6-7099-4f9c-8d14-5d1c89099ea6/ship \
  --header 'Authorization: Bearer SEU_TOKEN'
```

Resposta esperada (campos principais; a API também retorna `buyerId`, `buyerName`, datas e itens completos):

```http
200 OK
```

```json
{
  "id": "bab324b6-7099-4f9c-8d14-5d1c89099ea6",
  "status": "Shipped",
  "deletedAt": null,
  "items": [],
  "total": 0
}
```

### Cenários tristes do envio

Pedido inexistente:

```http
404 Not Found
```

Pedido que não está `Processed`:

```http
422 Unprocessable Entity
```

```json
{
  "title": "Business rule violation",
  "status": 422,
  "detail": "Only orders with status Processed can be shipped."
}
```

## Cancelar pedido

Só pedidos `Initiated` ou `Processed` podem ser cancelados.

```bash
curl --request POST \
  --url http://localhost:8080/api/v1/orders/bab324b6-7099-4f9c-8d14-5d1c89099ea6/cancel \
  --header 'Authorization: Bearer SEU_TOKEN'
```

Resposta esperada (campos principais; a API também retorna `buyerId`, `buyerName`, datas e itens completos):

```http
200 OK
```

```json
{
  "id": "bab324b6-7099-4f9c-8d14-5d1c89099ea6",
  "status": "Cancelled",
  "deletedAt": null,
  "items": [],
  "total": 0
}
```

### Cenários tristes do cancelamento

Pedido inexistente:

```http
404 Not Found
```

Pedido enviado:

```http
422 Unprocessable Entity
```

```json
{
  "title": "Business rule violation",
  "status": 422,
  "detail": "Only orders with status Initiated or Processed can be cancelled."
}
```

## Excluir pedido

Exclusão lógica. O pedido deixa de aparecer nas consultas comuns por causa do filtro de `deletedAt`.

```bash
curl --request DELETE \
  --url http://localhost:8080/api/v1/orders/bab324b6-7099-4f9c-8d14-5d1c89099ea6 \
  --header 'Authorization: Bearer SEU_TOKEN'
```

Resposta esperada:

```http
204 No Content
```

### Cenários tristes da exclusão

Pedido inexistente, já excluído ou de outro comprador:

```http
404 Not Found
```

Pedido já excluído enquanto estava carregado para a operação:

```http
422 Unprocessable Entity
```

```json
{
  "title": "Business rule violation",
  "status": 422,
  "detail": "Order has already been deleted."
}
```

## Ordem sugerida para testar manualmente

1. Gerar token.
2. Criar pedido.
3. Listar pedidos.
4. Buscar pedido por ID.
5. Alterar pedido.
6. Processar pedido.
7. Enviar pedido ou cancelar outro pedido.
8. Excluir pedido.

## Observações

- O comprador é identificado pelo token.
- O mesmo `document` gera sempre o mesmo `buyerId`.
- A listagem retorna apenas pedidos do comprador autenticado.
- Os horários são retornados com offset `-03:00`.
- Swagger fica disponível quando a API roda em ambiente `Development`.
