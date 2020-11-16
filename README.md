Jarvis is lightweight admin framework

I. Functions
---
1. Login (multi-tenant, switch tenant)
2. Logout
3. Register
4. Reset password
5. Profile
6. Tenant information
7. Tenant management (CRUD)
8. Orgniazation management (CRUD, permission data)
9. Account management (CRUD, role, lock/unlock)
10. Role management (CRUD, permission function)
11. Label management (CRUD)
12. Setting magagement (RU)

II. Technology
---
1. Framework: ASP.NET Core 3.1
2. ORM: Entity Framework Core 3.1, Dapper
3. Database: MySQL, MSSQL
4. Auth: JWT
5. Cache: Memory, Redis
6. Message: RabbitMQ
7. Front-end: AngularJS 1.6 component base (multi-theme, override controller/template/service)
8. API document: Swagger
9. Default theme: AdminLTE
10. RestfulAPI

III. Architecture
---
1. Design pattern
- UnitOfWork, Repository pattern
- Observer pattern
- Abstract factory pattern
- Singleton pattern
- Chain of Responsibility
2. Dependency Injection base on Microsoft
