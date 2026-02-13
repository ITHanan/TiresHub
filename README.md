# 🚗 Tire Service Platform

A fullstack web application that connects **vehicle owners** with **tire workshops**, enabling booking, tire storage management, inspections, and operational coordination.

The platform is designed to reflect **real-world workshop workflows**, including role-based access, warehouse capacity management, inspection reporting, and post-service tire data updates.

This project is developed as part of **Kunskapskontroll 2 – Grupprojekt (Fullstack, Live Deployment)**.

---

## 🎯 Project Goals

The purpose of this project is to demonstrate the ability to:

- Develop and deploy a **complete fullstack application**
- Build a **distributed client–server architecture**
- Apply **Clean Architecture** principles in a .NET backend
- Create a modern **React frontend** that communicates with a REST API
- Use a **cloud-hosted database (Azure SQL)**
- Implement **CI/CD pipelines** using GitHub Actions
- Work collaboratively using **sprints, pull requests, and code reviews**

---

## 🧩 Core Features

### 👥 Roles & Permissions

- **Vehicle Owner** – registers vehicles, books services, views tire information
- **Shop Owner** – registers workshop, manages warehouses and capacity, invites managers
- **Shop Manager** – handles bookings, assigns employees, communicates with customers, updates tire data
- **Employee** – inspects stored tires, uploads photos and reports

All staff access (shop managers and employees) is **invitation-based**.

---

### 🛞 Tire & Booking Workflow

- Vehicle owners book tire change or tire purchase services
- Stored tires are inspected **after booking**
- Employees upload inspection photos and reports
- Shop managers review reports and contact vehicle owners
- After service completion, tire information is updated in the system

---

### 📦 Warehouse & Storage Management

- Shops define multiple warehouses (A / B / C, etc.)
- Each warehouse has a defined capacity
- Storage availability affects booking decisions
- Tire storage location is tracked and updated after service

---

## 🏗️ Technical Architecture

### Backend

- **.NET Web API**
- Clean Architecture (Controllers, Services, Repositories, DTOs)
- Validation, logging, and global error handling
- Azure SQL database with EF Core migrations

### Frontend

- **React**
- API-driven UI
- Role-based dashboards
- Responsive design
- Environment-based configuration

### Infrastructure

- **Azure App Service** (Backend API)
- **Azure SQL Database**
- **Azure Static Web App** (Frontend)
- **GitHub Actions** for CI/CD

---

## 🗄️ Database Overview

The database uses a relational model and includes the following main entities:

- User (role-based)
- Shop, Warehouse
- Vehicle, TireSet
- Booking
- InspectionReport, InspectionPhoto

ER diagrams and architecture diagrams are included in the documentation.

---

## ⚙️ Local Development Setup

### Prerequisites

- .NET SDK (latest LTS)
- Node.js (LTS)
- npm or yarn
- SQL Server / Azure SQL
- Git

---

### 1️⃣ Clone the Repository

```bash
git clone https://github.com/your-org/tire-service-platform.git
cd tire-service-platform
2️⃣ Backend Setup (.NET API)
bash
Copy code
cd backend
dotnet restore
dotnet ef database update
dotnet run
Environment Variables
Create appsettings.Development.json or use environment variables:

env
Copy code
ConnectionStrings__DefaultConnection=your-azure-sql-connection-string
3️⃣ Frontend Setup (React)
bash
Copy code
cd frontend
npm install
npm run dev
Environment Variables
Create a .env file in the frontend root:

env
Copy code
VITE_API_BASE_URL=https://localhost:5001/api
4️⃣ Access the Application
Frontend: http://localhost:5173

Backend API: https://localhost:5001

🔄 CI/CD Pipelines
Backend Pipeline
Restore dependencies

Build

Run tests

Deploy to Azure App Service

Frontend Pipeline
Build React application

Deploy to Azure Static Web App

Pipelines are automatically triggered on merge to the main branch.

📚 Documentation
The documentation includes:

Activity Diagram (PlantUML)

ER Diagram

API endpoint overview

CI/CD pipeline overview

Role & permission descriptions

See the /docs directory for details.

👥 Team & Process
Agile workflow with weekly sprints

GitHub Projects for task tracking

Pull Requests with code reviews

Clearly defined team roles (frontend, backend, DevOps)

🏁 Project Status
✔ Core functionality implemented

✔ Clean architecture applied

✔ CI/CD pipelines configured

✔ Live deployment completed

📌 Notes for Examination
This project demonstrates:

Distributed system design

Realistic business workflows

Role-based access control

Cloud deployment and CI/CD

Professional team collaboration

📄 License
This project is developed for educational purposes as part of a fullstack course.

### 📋 UC-13: Branch-Scoped Booking Visibility (Shop Manager)

**Description:**
Shop managers can view and access only bookings assigned to their specific branch. This ensures data isolation and prevents unauthorized access to bookings from other branches.

**Key Features:**
- **Branch-Scoped Access:** Shop managers see only bookings for their assigned branch
- **Authorization Enforcement:** Attempts to access bookings from other branches are blocked and logged
- **Read-Only at Intake Stage:** Booking data cannot be modified at this stage (assignment and inspection happen later)
- **Ordered by Appointment Date:** Bookings are displayed in chronological order (ascending)
- **Empty State Handling:** Clean UI when no bookings exist for the branch

**API Endpoints:**
- `GET /api/bookings` - Retrieve all bookings for the authenticated shop manager's branch
- `GET /api/bookings/{bookingId}` - View details of a specific booking (with branch authorization check)

**Security:**
- Single-branch access enforcement for shop managers
- Audit logging of unauthorized access attempts
- Branch ownership validation on every request

**Technical Implementation:**
- Database index on `(BranchId, AppointmentDate)` for efficient queries
- Clean Architecture with CQRS pattern (MediatR)
- Repository pattern for data access
- Comprehensive unit tests covering authorization scenarios

---

**Description:**
Shop managers can assign an active employee from their branch to handle a booking. This assignment establishes ownership and responsibility for tire inspection, preparation, and service tasks.

**Key Features:**
- **Employee Assignment:** Shop managers assign employees to bookings
- **Branch-Scoped Assignment:** Only employees from the same branch as the booking can be assigned
- **Active Employee Validation:** Inactive employees cannot be assigned
- **Reassignment Support:** Previously assigned employees can be changed with full audit trail
- **Authorization Enforcement:** Only shop managers can perform assignments
- **Audit Logging:** All assignments, reassignments, and unauthorized attempts are logged

**API Endpoints:**
- `POST /api/bookings/{bookingId}/assign-employee/{employeeId}` - Assign or reassign an employee to a booking

**Business Rules:**
- Employee must have `Employee` role
- Employee must be active (`IsActive = true`)
- Employee must belong to the same branch as the booking
- Only shop managers can assign employees
- Manager must belong to the same branch as the booking
- Reassignments are tracked with previous employee ID in audit logs

**Security:**
- Role-based authorization (Shop Manager only)
- Branch boundary enforcement
- Audit logging of all assignment attempts
- Unauthorized cross-branch assignment attempts are blocked and logged

**Notification:**
- Employee notification is triggered on assignment (mocked implementation)
- In production, this would send email, push, or in-app notifications

**Technical Implementation:**
- Clean Architecture with CQRS pattern (MediatR)
- Command: `AssignEmployeeCommand`
- Handler: `AssignEmployeeCommandHandler`
- Comprehensive unit tests covering all scenarios
- Audit actions: `EmployeeAssigned`, `EmployeeReassigned`, `UnauthorizedEmployeeAssignment`

---

### 📋 UC-15A: Employee Registration and Branch Assignment (Shop Manager)

**Description:**
Shop managers can create employee accounts and manage their access within their assigned branch. Employees are automatically linked to the manager's branch and cannot be assigned to multiple branches, ensuring clear responsibility and data isolation.

**Key Features:**
- **Employee Account Creation:** Shop managers create employee accounts with name and email/phone
- **Automatic Branch Assignment:** Employees are automatically assigned to the manager's branch (immutable after creation)
- **Single-Branch Restriction:** Employees belong to exactly one branch and cannot be reassigned
- **Employee Status Management:** Shop managers can activate/deactivate employees to control access
- **Employee List View:** View all employees (active and inactive) assigned to the branch
- **Authorization Enforcement:** Only shop managers can create and manage employees in their own branch

**API Endpoints:**
- `POST /api/employees` - Create a new employee account for the manager's branch
- `GET /api/employees` - List all employees in the manager's branch
- `POST /api/employees/{employeeId}/deactivate` - Deactivate an employee account
- `POST /api/employees/{employeeId}/reactivate` - Reactivate an employee account

**Business Rules:**
- Employee accounts can only be created by shop managers
- Employee's branch is set to the manager's branch automatically (preselected and read-only)
- Employee cannot be assigned to multiple branches
- Branch assignment is immutable after account creation
- Required fields: Name, Email or Phone
- Employee role is automatically assigned (cannot be changed to other roles)
- Deactivated employees cannot log in (authentication blocked immediately)
- Only the branch's manager can activate/deactivate employees in that branch
- Self-registration for employee accounts is blocked

**Employee Status Management:**
- Active employees can log in and perform their duties
- Inactive employees are blocked at login with clear error message
- Status changes take effect immediately (no caching)
- Status changes are fully audited

**Security:**
- Role-based authorization (Shop Manager only)
- Branch-scoped access enforcement
- Cross-branch employee management is blocked
- All employee operations are audit logged
- Login blocked for inactive employees in both staff auth and regular auth flows
- Unauthorized attempts are logged with user and branch information

**Audit Logging:**
All employee management actions are tracked with full audit trail:
- `EMPLOYEE_CREATED` - New employee account created
- `EMPLOYEE_ASSIGNED` - Employee assigned to branch
- `EMPLOYEE_DEACTIVATED` - Employee account deactivated
- `EMPLOYEE_REACTIVATED` - Employee account reactivated
- Audit logs include: userId, action, entityType, entityId, timestamp, metadata

**Technical Implementation:**
- Clean Architecture with CQRS pattern (MediatR)
- Commands: `CreateEmployeeCommand`, `DeactivateEmployeeCommand`, `ReactivateEmployeeCommand`
- Queries: `GetBranchEmployeesQuery`
- Handlers with comprehensive validation and authorization
- Repository methods: `GetEmployeesByBranchIdAsync`, `GetActiveEmployeesByBranchIdAsync`
- Database: Uses existing User entity with `BranchId` (nullable), `IsActive` (boolean), `Role` (enum)
- No database migration needed - schema already supports employee management
- Comprehensive unit tests (22 tests covering all scenarios)
- API Controller: `EmployeesController` with role-based authorization

**Error Handling:**
- "Not authenticated" - User not logged in
- "Only shop managers can create employee accounts" - Wrong role
- "Shop manager must be assigned to a branch" - Manager has no branch
- "Name is required" - Missing required field
- "Email or phone is required" - Missing contact information
- "Employee is already assigned to another branch" - Cannot reassign employee
- "You can only manage employees in your own branch" - Cross-branch access attempt
- "User is not an employee" - Attempting to manage non-employee user
- "Your account has been deactivated" - Inactive employee login attempt

```

