# ZSS Books API (SQL Server, .NET 8)

A REST API for managing **Books** and **Categories**, and for purchasing books through an external **mock transaction API**.

- **Framework:** .NET 8 Web API  
- **Database:** SQL Server + Entity Framework Core  
- **Docs:** Swagger (OpenAPI 3.x) at `/swagger`  
- **No frontend** — API validated with Postman or Swagger UI  

---

## Features

- Create and retrieve **categories**
- Create and retrieve **books**, with optional filter by category
- **Purchase endpoint** (`/api/purchases`) that integrates with the provided ZSS mock API
- Swagger/OpenAPI 3 documentation

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)  
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, Developer, or full edition)  

---

## Setup

### 1. Clone the repository
```bash
git clone https://github.com/Brianmutsau/zssApi.git
cd zssApi/WebApplication1


### 2.Configure secrets

dotnet user-secrets init

dotnet user-secrets set "ConnectionStrings:Default" "Server=.;Database=ZssBooks;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "PurchaseApi:BaseUrl" "https://secure.v.co.zw/interview/"
dotnet user-secrets set "PurchaseApi:Token" "<TOKEN>"

3 Apply migrations

dotnet ef database update

4 dotnet run


