# Payment Portal

A full-stack payment management application built with **ASP.NET Core** (.NET API) and **Angular** (frontend). The application allows users to create, retrieve, update, and delete payment records with built-in validation and duplicate prevention.

## Table of Contents

- [Features](#features)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Setup and Installation](#setup-and-installation)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Configuration](#configuration)
- [Testing](#testing)
- [Development Notes](#development-notes)

## Features

- **Payment Management**: Create, read, update, and delete payments
- **Duplicate Prevention**: Automatically prevents duplicate payments using `ClientRequestId`
- **Sequential Reference Generation**: Auto-generates daily sequential payment references
- **Payment Validation**: Comprehensive validation of payment data
- **Flexible Storage**: Support for both JSON file-based and in-memory storage
- **REST API**: Full RESTful API with Swagger documentation
- **CORS Support**: Pre-configured CORS for Angular frontend
- **Unit Tests**: Comprehensive test coverage for API, Service, and Data layers
- **Modern UI**: Angular-based responsive user interface

## Technology Stack

### Backend
- **Framework**: ASP.NET Core (.NET)
- **API Documentation**: Swagger/OpenAPI
- **Testing**: MSTest
- **Data Storage**: JSON files or In-Memory

### Frontend
- **Framework**: Angular 19.2+
- **Language**: TypeScript
- **Testing**: Karma, Jasmine
- **Build Tool**: Angular CLI 19.2+

## Project Structure

```
Payments-Portal/
├── Payments-Portal.API/              # ASP.NET Core API
│   ├── Controllers/                  # API Controllers
│   ├── Data/                         # Data layer (repositories)
│   ├── Properties/                   # Launch settings
│   ├── Program.cs                    # Application startup
│   └── appsettings.json              # Configuration
│
├── Payments-Portal.Service/          # Business Logic Layer
│   ├── PaymentService.cs             # Payment business logic
│   ├── PaymentValidator.cs           # Payment validation
│   ├── DailySequentialReferenceGenerator.cs  # Reference generation
│   ├── PaymentMapper.cs              # DTO mapping
│   └── DTOs/                         # Data transfer objects
│
├── Payments-Portal.UI/               # Angular Application
│   ├── src/                          # Angular source code
│   ├── public/                       # Static assets
│   └── angular.json                  # Angular configuration
│
├── Payments-Portal.API.Test/         # API Layer Tests
├── Payments-Portal.Service.Test/     # Service Layer Tests
├── Payments-Portal.Data.Test/        # Data Layer Tests
│
└── Payments-Portal.sln               # Solution file
```

## Prerequisites

### Backend
- **.NET Runtime**: .NET 8.0 or higher
- **Visual Studio** 2022 or **Visual Studio Code** with C# extension

### Frontend
- **Node.js**: v18.0 or higher
- **npm**: v9.0 or higher (comes with Node.js)
- **Angular CLI**: v19.2.18+

## Setup and Installation

### Step 1: Clone or Extract the Project

```bash
cd c:\Payment-Portal
```

### Step 2: Backend Setup

1. **Restore NuGet packages**:
   ```bash
   dotnet restore Payments-Portal.sln
   ```

2. **Build the solution**:
   ```bash
   dotnet build Payments-Portal.sln
   ```

### Step 3: Frontend Setup

1. **Navigate to the UI folder**:
   ```bash
   cd Payments-Portal.UI
   ```

2. **Install dependencies**:
   ```bash
   npm install
   ```

3. **Return to root directory**:
   ```bash
   cd ..
   ```

## Running the Application

### Start the Backend API

1. **Navigate to the API project**:
   ```bash
   cd Payments-Portal.API
   ```

2. **Run the API**:
   ```bash
   dotnet run
   ```

   The API will start on `https://localhost:5001` or `http://localhost:5000`.

3. **View Swagger Documentation**:
   Open your browser and navigate to `https://localhost:5001/swagger/index.html`

### Start the Frontend Application

1. **Navigate to the UI folder**:
   ```bash
   cd Payments-Portal.UI
   ```

2. **Run the development server**:
   ```bash
   npm start
   ```

   Or using Angular CLI directly:
   ```bash
   ng serve
   ```

3. **Access the application**:
   Open your browser and navigate to `http://localhost:4200/`

## API Documentation

### Base URL
- **Development**: `https://localhost:5001/api`

### Endpoints

#### Create Payment
- **POST** `/api/payments`
- **Request Body**:
  ```json
  {
    "amount": 100.50,
    "currency": "USD",
    "description": "Payment description",
    "clientRequestId": "unique-request-id"
  }
  ```
- **Response**: `201 Created`

#### Get All Payments
- **GET** `/api/payments`
- **Response**: `200 OK` - Returns array of all payments

#### Get Payment by ID
- **GET** `/api/payments/{id}`
- **Response**: `200 OK` or `404 Not Found`

#### Update Payment
- **PUT** `/api/payments/{id}`
- **Request Body**:
  ```json
  {
    "amount": 150.00,
    "currency": "USD",
    "description": "Updated description"
  }
  ```
- **Response**: `200 OK` or `404 Not Found`

#### Delete Payment
- **DELETE** `/api/payments/{id}`
- **Response**: `204 No Content` or `404 Not Found`

**Interactive API documentation available at**: `https://localhost:5001/swagger/index.html`

## Configuration

### Backend Configuration

Edit `Payments-Portal.API/appsettings.json`:

```json
{
  "StorageProvider": "Json",
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**StorageProvider Options**:
- `"Json"` - Persists payments to `Data/payments.json` file
- `"InMemory"` - Stores payments in memory (lost on application restart)

### Frontend Configuration

The Angular application is pre-configured to communicate with the API at `http://localhost:5001`. Update the API base URL in the service if needed.

## Testing

### Run Backend Tests

```bash
# Run all tests
dotnet test Payments-Portal.sln

# Run specific test project
dotnet test Payments-Portal.API.Test/Payments-Portal.API.Test.csproj
dotnet test Payments-Portal.Service.Test/Payments-Portal.Service.Test.csproj
dotnet test Payments-Portal.Data.Test/Payments-Portal.Data.Test.csproj
```

### Run Frontend Tests

```bash
cd Payments-Portal.UI

# Run unit tests
npm test

# Or using Angular CLI
ng test
```

## Development Notes

### Architecture Principles
- **Layered Architecture**: API → Service → Data
- **Dependency Injection**: Utilized for loose coupling and testability
- **SOLID Principles**: Separation of concerns with clear interfaces
- **DTOs**: Data transfer objects for data mapping between layers

### Key Components

**PaymentService**: Orchestrates business logic including:
- Payment creation with validation
- Duplicate prevention via `ClientRequestId`
- Sequential reference generation
- Payment updates and deletions

**PaymentValidator**: Validates payment data before processing

**DailySequentialReferenceGenerator**: Generates unique daily payment references

**Storage Providers**:
- `JsonPaymentRepository`: File-based persistence
- `InMemoryPaymentRepository`: Volatile in-memory storage

### CORS Configuration

The API allows requests from the Angular frontend at `http://localhost:4200`:

```csharp
options.AddPolicy("AllowAngular", policy =>
{
    policy.WithOrigins("http://localhost:4200")
           .AllowAnyMethod()
           .AllowAnyHeader();
});
```

### Build and Publish

**Publish the backend**:
```bash
dotnet publish -c Release
```

**Build the frontend**:
```bash
cd Payments-Portal.UI
ng build --configuration production
```

## License

This project is provided as-is for educational and development purposes.

## Support

For issues or questions, refer to the project documentation or the respective README files in the project folders.
