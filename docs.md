# Product Requirements Document (PRD): Cloud Infrastructure Provisioning Portal

## Table of Contents
- [1. Doc Overview](#1-doc-overview)
- [2. Technical Stack & Architecture](#2-technical-stack--architecture)
- [3. Folder Structure](#3-folder-structure)
- [4. UI & Layout Strategy](#4-ui--layout-strategy)
- [5. Collaboration & Workflow](#5-collaboration--workflow)
- [6. Database Schema (Entities)](#6-database-schema-entities)
- [7. Core Views (Feature Scope)](#7-core-views-feature-scope)
- [Future](#future)

## 1. Doc Overview
This document outlines the architecture, database schema, and feature scope for an internal ASP.NET Core web portal. The system streamlines the process of requesting and approving virtual server instances. By decoupling the authentication engine from the MVC infrastructure logic, the application allows developers to rapidly request resources while providing administrators a unified queue to manage infrastructure deployments.

## 2. Technical Stack & Architecture
- **Frontend:** HTML5, CSS3, Bootstrap 5
- **Backend:** ASP.NET Core (.NET 10 environment)
- **Database:** SQLite (file-based, rapid local development)
- **ORM:** Entity Framework Core (Code-First Migrations)
- **Architecture Pattern:** MVC (Model-View-Controller) hybrid. Feature-specific logic lives in MVC Controllers, while the Authentication module leverages isolated Razor Pages.
- **Styling Strategy:** Global Flexbox Master Layout (`_Layout.cshtml`). Strict prohibition on modifying the master skeleton from feature views.

## 3. Folder Structure
The project utilizes a standard ASP.NET Core directory tree, customized to isolate Identity concerns from core infrastructure logic to minimize merge conflicts.

```text
cloud-infrastructure/
├── Areas/
│   └── Identity/                 # Isolated Microsoft Identity UI
│       └── Pages/Account/        # Custom Register.cshtml (FullName capture) & Login
├── Controllers/
│   ├── HomeController.cs         # Base routing
│   └── InfrastructureController.cs # Server data fetching and approval logic
├── Models/
│   ├── ApplicationUser.cs        # Extends IdentityUser (FullName property)
│   └── ServerInstance.cs         # Blueprint for tbl_ProvisionedServers
├── Views/
│   ├── Home/                     # Dashboard & Queue UI
│   └── Shared/
│       └── _Layout.cshtml        # Global dark-mode Flexbox skeleton
├── appsettings.json              # SQLite Connection string ("DefaultConnection")
└── Program.cs                    # Dependency Injection, DB Context, & Routing setup
```

## 4. UI & Layout Strategy

To ensure a consistent, modern interface without duplicating code, the UI relies on a centralized Layout pattern.

### 4.1 Architecture Components

* **The Skeleton (`_Layout.cshtml`):** Contains the `<html>`, `<head>`, persistent dark-mode sidebar, and top navigation header.
* **The Render Body (`@RenderBody()`):** The designated white-space container where feature-specific HTML is injected.
* **Script Landing Zones:** Enforces `@await RenderSectionAsync("Scripts", required: false)` at the bottom of the skeleton to catch validation scripts from child pages without crashing the DOM.

### 4.2 Implementation Rules

* **Strict UI Compliance:** Developers must build feature cards and tables using Bootstrap 5 utility classes (e.g., `card shadow-sm`, `badge bg-primary`).
* **Layout Isolation:** Feature developers (Frontend) must never edit `_Layout.cshtml`. All UI work happens inside standard `Views/` and is injected automatically.

## 5. Collaboration & Workflow

* **Work Distribution:** Feature ownership is divided by discipline.
* **Hamza:** Project architecture, Auth logic, Provisioning modals.
* **Mohammad Natsheh:** Frontend UI, Bootstrap execution, data visualization.
* **Mohammad Aqeel:** Backend Controller routing, MVC data passing.
* **Suhaib:** EF Core Database logic, LINQ queries, state updates.


* **Git Strategy:** Strict branch-per-feature workflow (e.g., `feature/admin-dashboard`, `feature/server-models`). Direct pushes to `main` are prohibited.
* **Phase 1 Execution (Active):** UI Controllers must be initialized with hardcoded `List<ServerInstance>` dummy data. Frontend development must not block on database integration.
* **Phase 2 Execution:** Swap dummy lists in Controllers with `_context.ServerInstances.Where(...)` Entity Framework database queries.

## 6. Database Schema (Entities)

The data model utilizes Entity Framework Code-First constraints to enforce data integrity before it reaches the SQLite file.

### AspNetUsers (Identity)

| Column | Type | Description |
| --- | --- | --- |
| Id | nvarchar | Primary key, default Identity generation |
| Email | nvarchar | User login and contact |
| FullName | nvarchar | Custom property for dashboard display |
| PasswordHash | nvarchar | Hashed security key |

### tbl_ProvisionedServers (`ServerInstance`)

| Column | Type | Constraints & Validation | Description |
| --- | --- | --- | --- |
| ServerInstanceId | int | Primary Key, Auto-increment | Unique identifier for the provisioned server. |
| Hostname | string | Required, MaxLength(63), Regex | Enforces lowercase letters, digits, and hyphens only. |
| RamGb | int | Required, Range(1, 128) | Memory allocation in Gigabytes. |
| InstanceSize | enum | Required | Standardized mapping (e.g., Small, Medium, Large). |
| Status | string | Required, Default: "Pending" | Tracks workflow state (`Pending`, `Approved`, `Rejected`). |
| DeveloperId | string | Foreign Key (Nullable) | Links instance to requesting `AspNetUsers.Id`. |

## 7. Core Views (Feature Scope)

### 7.1 Authentication System

**Use Case Description:** Secure entry point bypassing default email confirmation to allow rapid onboarding.
**Use Cases:**

* User registers an account inputting standard credentials plus `FullName`.
* Framework maps `FullName` securely to the Identity context.
* System automatically redirects authenticated sessions to the Dashboard.

### 7.2 Admin Dashboard (Approval Queue)

**Use Case Description:** Primary operational interface for infrastructure administrators to execute decisions on pending resources.
**Use Cases:**

* Administrator views a grid/list of instances where `Status == "Pending"`.
* UI displays core specs (Hostname, RamGb, InstanceSize) via Bootstrap badging.
* Administrator executes state changes via "Approve" (turns green, updates DB) or "Reject" (turns red, updates DB) actions.
* System dynamically updates the visual queue without requiring a hard refresh where possible.

### 7.3 The Provisioning Modal

**Use Case Description:** The primary input mechanism for standard developers to request new infrastructure.
**Use Cases:**

* User triggers a modal overlay from the master layout.
* User inputs desired `Hostname` (validated via Regex in real-time).
* User selects `RamGb` and `InstanceSize` configurations.
* System saves the request to `tbl_ProvisionedServers` with an automatic `Status` of "Pending" and assigns the current `DeveloperId`.

## Future

* Automated execution scripts: Triggering real cloud APIs (e.g., AWS EC2, Azure VMs) upon clicking "Approve".
* System Health Monitoring: Polling live metrics from active servers to display in the dashboard.
