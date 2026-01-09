# Med-Patient Management System

A backend ASP.NET Core Web API project to manage patients, caregivers, medications, and dose logs.  
The system is designed so that caregivers (doctors) can track and manage only their assigned patients.

---

## 🧾 Project Overview

This system provides:

- Patient management with chronic conditions.
- Caregiver (doctor) assignment and management *(feature for linking doctors to patients is in progress)*.
- Medication tracking per patient with detailed dose logs.
- JWT authentication with refresh token support for secure access.
- Password management with change password functionality.
- Admin functionality to pre-register users and manage accounts.
- Logging of user activities and system events.
- Efficient data retrieval using AutoMapper.
- In-memory caching & pagination for optimized performance.

---

## 🛠 Technologies Used

-ASP.NET Core Web API
- Entity Framework Core
- AutoMapper
- JWT Authentication with Refresh Tokens
- SQL Server
- Swagger for API documentation
