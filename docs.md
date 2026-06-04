# Product Requirements Document (PRD): Cloud Infrastructure Provisioning Portal

## Table of Contents
- [1. Doc Overview](#1-doc-overview)
- [2. Technical Stack & Architecture](#2-technical-stack--architecture)
- [3. Folder Structure](#3-folder-structure)
- [4. UI & Layout Strategy](#4-ui--layout-strategy)
- [5. Database Schema (Entities)](#5-database-schema-entities)
- [6. Authentication & Authorization](#6-authentication--authorization)
- [7. Core Views (Feature Scope)](#7-core-views-feature-scope)
- [Future](#future)

## 1. Doc Overview
This document outlines the architecture, database schema, and feature scope for an internal ASP.NET Core web portal. The system streamlines the process of requesting and approving virtual server instances. By decoupling the authentication engine from the MVC infrastructure logic, the application allows developers to rapidly request resources while providing administrators a unified queue to manage infrastructure deployments.

## 2. Technical Stack & Architecture

### Runtime & Framework
- **Runtime:** .NET 10.0
- **Framework:** ASP.NET Core MVC with Razor Pages (Identity)
- **Language:** C# (nullable enabled, implicit usings)

### Packages (EF Core 7.0.20)
| Package | Purpose |
|---------|---------|
| `Microsoft.EntityFrameworkCore` | ORM base |
| `Microsoft.EntityFrameworkCore.Sqlite` | SQLite database provider |
| `Microsoft.EntityFrameworkCore.Design` | CLI migrations support |
| `Microsoft.EntityFrameworkCore.Tools` | VS migrations support |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Identity with EF Core storage |
| `Microsoft.AspNetCore.Identity.UI` | Identity default UI (Razor Pages) |
| `Microsoft.VisualStudio.Web.CodeGeneration.Design` | Scaffolding |

### Frontend
- **Markup:** HTML5, Razor `.cshtml`
- **Styling:** Bootstrap 5.3 (CDN), Bootstrap Icons, custom `site.css`
- **JavaScript:** Bootstrap 5.3 bundle (CDN), jQuery Validation

### Database
- **Engine:** SQLite (file-based: `cloudportal.db`)
- **ORM:** Entity Framework Core (Code-First Migrations)
- **Migration CLI:** `dotnet ef migrations add <name>`
- **Reset DB:** `rm cloudportal.db` (recreated on next `dotnet run`)

### Architecture Pattern
MVC (Model-View-Controller) hybrid. Feature-specific logic lives in MVC Controllers, while the Authentication module leverages isolated Razor Pages under `Areas/Identity/`.

## 3. Folder Structure

```
cloud-infrastructure/
├── Areas/
│   └── Identity/
│       └── Pages/Account/
│           ├── Login.cshtml / .cshtml.cs
│           ├── Register.cshtml / .cshtml.cs
│           └── AccessDenied.cshtml / .cshtml.cs
├── Controllers/
│   ├── AdminController.cs           # Admin dashboard, approve/reject, user management
│   ├── DeveloperController.cs       # VM request form for developers
│   ├── HomeController.cs            # Root routing with role-based redirect
│   ├── InfrastructureController.cs  # Legacy server actions (authorized)
│   └── ServerInstancesController.cs # Admin CRUD for server instances
├── Models/
│   ├── ApplicationDbContext.cs      # EF Core DbContext (extends IdentityDbContext)
│   ├── Developer.cs                 # IdentityUser subclass (FullName, WorkEmail)
│   ├── InstanceSize.cs             # Enum: Micro, Small, Medium, Large
│   ├── ServerInstance.cs           # tbl_ProvisionedServers entity
│   ├── ServerSoftware.cs           # Join table: ServerInstance <-> SoftwarePackage
│   └── SoftwarePackage.cs          # Software catalog entity
├── ViewModels/
│   ├── AdminDashboardViewModel.cs   # Stats + pending requests for admin
│   ├── DeveloperRequestViewModel.cs # VM request form model + recent requests
│   └── ManageUsersViewModel.cs      # User list with roles for admin management
├── Views/
│   ├── Admin/
│   │   ├── Index.cshtml             # Admin dashboard with stats & approval queue
│   │   └── ManageUsers.cshtml       # User list with promote-to-admin
│   ├── Developer/
│   │   └── RequestVM.cshtml         # VM request form + recent requests table
│   ├── Home/
│   │   └── Index.cshtml             # Landing page (post-login redirect)
│   ├── ServerInstances/             # Full CRUD for server instances (admin only)
│   └── Shared/
│       ├── _Layout.cshtml           # Global dark-mode Flexbox skeleton
│       └── _LoginPartial.cshtml     # Login/logout navbar partial
├── Data/
│   └── IdentitySeeder.cs            # Seeds Admin & Developer roles on startup
├── Migrations/                      # EF Core code-first migrations
├── wwwroot/
│   ├── css/site.css                 # Custom styles
│   └── lib/                         # Client-side libraries
├── Program.cs                       # DI, middleware, routing, seeder
└── appsettings.json                 # Configuration & connection strings
```

## 4. UI & Layout Strategy

### 4.1 Architecture Components
- **The Skeleton (`_Layout.cshtml`):** Contains the `<html>`, `<head>`, persistent dark-mode sidebar, and top navigation header.
- **The Render Body (`@RenderBody()`):** White-space container where feature-specific HTML is injected.
- **Script Landing Zones:** `@await RenderSectionAsync("Scripts", required: false)` for validation scripts.
- **Styling:** CSS variables and scoped `<style>` blocks per view for component-specific styles.

### 4.2 Color Palette
| Token | Hex | Usage |
|-------|-----|-------|
| Surface BG | `#1c2128` | Cards, panels |
| Input BG | `#0d1117` | Form inputs |
| Border | `#30363d` | Card & table borders |
| Primary text | `#e6edf3` | Body text on dark surfaces |
| Muted text | `#8b949e` | Labels, secondary info |
| Green (success) | `#3fb950` | Approved badge |
| Yellow (pending) | `#d29922` | Pending badge |
| Red (danger) | `#f85149` | Rejected badge |
| Blue (accent) | `#2f81f7` | Focus rings, links |

## 5. Database Schema (Entities)

### AspNetUsers (Identity — `Developer` model)

| Column | Type | Description |
|--------|------|-------------|
| Id | TEXT (PK) | Identity GUID string |
| Email | TEXT | Login email |
| FullName | TEXT (max 120, required) | Display name |
| WorkEmail | TEXT (max 256, required) | Work contact email |
| PasswordHash | TEXT | Hashed password |
| UserName | TEXT | Defaults to email |
| NormalizedEmail | TEXT | Upper-case email for lookup |
| NormalizedUserName | TEXT | Upper-case username |
| *...other Identity columns* | | ConcurrencyStamp, SecurityStamp, etc. |

### AspNetRoles

| Column | Type | Description |
|--------|------|-------------|
| Id | TEXT (PK) | Role ID |
| Name | TEXT | "Admin" or "Developer" |
| NormalizedName | TEXT | Upper-case role name |

### AspNetUserRoles (Join)

| Column | Type |
|--------|------|
| UserId | TEXT (FK -> AspNetUsers.Id) |
| RoleId | TEXT (FK -> AspNetRoles.Id) |

### tbl_ProvisionedServers (`ServerInstance`)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| ServerInstanceId | INTEGER | PK, auto-increment | Unique ID |
| Hostname | TEXT | Required, Max 63, Regex `^[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9]$` | Server hostname |
| RamGb | INTEGER | Required, Range(1-128) | Memory in GB |
| InstanceSize | INTEGER | Required (enum) | 0=Micro, 1=Small, 2=Medium, 3=Large |
| Status | TEXT | Required, default "Pending" | Pending / Approved / Rejected |
| Purpose | TEXT | Max 500, nullable | Workload description |
| DeveloperId | TEXT | FK -> AspNetUsers.Id, nullable | Requesting user |

### SoftwarePackages

| Column | Type | Constraints |
|--------|------|-------------|
| SoftwarePackageId | INTEGER | PK, auto-increment |
| PackageName | TEXT | Required, Max 150 |
| Version | TEXT | Required, Max 50 |

### ServerSoftwares (Join table — many-to-many)

| Column | Type | Constraints |
|--------|------|-------------|
| ServerInstanceId | INTEGER | PK (composite), FK -> tbl_ProvisionedServers |
| SoftwarePackageId | INTEGER | PK (composite), FK -> SoftwarePackages |

## 6. Authentication & Authorization

### Identity Configuration
- **User class:** `Developer` (extends `IdentityUser`)
- **Storage:** EF Core via `ApplicationDbContext` (extends `IdentityDbContext<Developer>`)
- **Email confirmation:** Disabled (`RequireConfirmedAccount = false`)
- **Roles:** "Admin" and "Developer"

### Role-Based Access
| Controller / Action | Required Role |
|---------------------|---------------|
| `AdminController` (all) | Admin |
| `DeveloperController` (all) | Developer or Admin |
| `ServerInstancesController` (all) | Admin |
| `InfrastructureController.ApproveServer` | Admin |
| `InfrastructureController.RejectServer` | Admin |
| `InfrastructureController` (other) | Authenticated |
| `HomeController.Index` | Authenticated |

### Registration Flow
- Users select "Register as Developer" or "Register as Admin"
- Anyone selecting Admin is assigned the Admin role
- Admins can promote other users via `/Admin/ManageUsers`

### Seeder (`IdentitySeeder`)
Runs on startup:
1. Creates "Admin" and "Developer" roles if absent
2. Promotes the first user to Admin if no admins exist

## 7. Core Views (Feature Scope)

### 7.1 Authentication Pages
- **Login** (`/Identity/Account/Login`): Email + password sign-in with post-login role-based redirect
- **Register** (`/Identity/Account/Register`): Email + password + role selection
- **Access Denied** (`/Identity/Account/AccessDenied`): Friendly 403 page with message

### 7.2 Admin Dashboard (`/Admin`)
- Stats cards: Total Servers, Pending, Approved, Rejected
- Approval queue table with Approve / Reject buttons
- Link to Manage Users

### 7.3 Manage Users (`/Admin/ManageUsers`)
- Lists all users with their current roles
- "Promote to Admin" button for non-admin users

### 7.4 Developer VM Request (`/Developer/RequestVM`)
- Form: Hostname (validated via regex), RAM, Instance Size, Purpose
- Recent requests table showing own submissions with status badges

### 7.5 Server Instances CRUD (`/ServerInstances`)
- Full admin CRUD (Create, Edit, Delete, Details) for server instances
- Approve / Reject actions

## Future

- Automated execution scripts: Triggering real cloud APIs (e.g., AWS EC2, Azure VMs) upon clicking "Approve".
- System Health Monitoring: Polling live metrics from active servers to display in the dashboard.
- Software package assignment during VM requests.
