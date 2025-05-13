# Help Desk System - Backend (ASP.NET Core 8)

Help Desk Backend source code using ASP.NET Core (.NET 8)

This is the **backend** of the Help Desk System built with **ASP.NET Core 8 Web API**.  
It provides APIs for managing tickets, users, departments, and more — integrated with **Angular 19 SSR** frontend.

---

## 🏛️ Architecture Overview

```plaintext
Client (Angular 19) ⇄ API Gateway (ASP.NET 8 Web API) ⇄ Services ⇄ Repositories ⇄ SQL Server
```

## ⚙️ Technologies Used

- ASP.NET Core 8 Web API
- Entity Framework Core
- JWT Bearer Authentication
- SQL Server
- SMTP for Emails
- (Optional) Itexmo for SMS Integration

## 🛠️ Backend Setup

### Prerequisites
- .NET 8 SDK
- SQL Server (Local or Azure)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/EchoSNS/AviorITHelpDesk_Backend.git
   ```

2. Navigate to the backend folder:
   ```bash
   cd helpdesk-backend
   ```

3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

4. Configure appsettings.Development.json (for localhost) and appsettings.Production.json (for production):
   - Update ConnectionStrings:DefaultConnection
   - Setup JwtSettings (Issuer and Audience)
   - Setup SmtpSettings for email notifications

5. Apply Database Migrations:
   ```bash
   dotnet ef database update
   ```

6. Run the API:
   ```bash
   dotnet run
   ```

## 🔐 Authentication

- JWT Token-based Authentication
- Issuer refers to backend URL
- Audience refers to frontend URL
- Supports Role-based Authorization: Admin, IT, Staff

## 🧩 Services Implemented

| Service | Description |
|---------|-------------|
| DepartmentService | Manage Departments |
| SubDepartmentService | Manage SubDepartments |
| PositionService | Manage Positions |
| UserRoleService | Manage Role Assignments |
| TokenService | Generate and Validate JWT Tokens |
| EmailService | Send Email Notifications |
| SmsService | (Configured, not yet used) Itexmo SMS Integration |

## 📚 Repositories Implemented

| Repository | Purpose |
|------------|---------|
| PositionRepository | Data access for Positions |
| SubDepartmentRepository | Data access for SubDepartments |
| TicketRepository | Data access for Tickets |

## 📦 DTOs (Data Transfer Objects)

- CreateDTO - For creating new entities
- UpdateDTO - For updating existing entities
- Base/ViewDTO - For listing and viewing data

Entities with DTOs:
- Department
- SubDepartment
- Position

## 📂 Domain Models

- Department
- SubDepartment
- Position
- Ticket
