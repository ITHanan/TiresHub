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

