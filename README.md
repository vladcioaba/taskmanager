# TaskManager

A full-stack task management application built with .NET Web API backend and React frontend, featuring comprehensive CRUD operations, priority management, and a modern responsive interface.

## Features

- **CRUD Operations**: Complete Create, Read, Update, and Delete functionality for tasks
- **CQRS Architecture**: Clean separation of read (Queries) and write (Commands) operations
- **Priority Management**: Three-level priority system (Low, Medium, High)
- **Task Status Tracking**: Mark tasks as completed or active with automatic timestamp management
- **Advanced Filtering**: Filter tasks by completion status and priority level
- **Responsive Design**: Modern, clean interface that adapts to all screen sizes
- **Real-time Updates**: Immediate UI feedback for all operations
- **Comprehensive Validation**: Both client-side and server-side data validation
- **In-memory Database**: Fast development setup using Entity Framework Core
- **Feature-Based Organization**: Organized by business features with Commands/Queries/Handlers

## System Architecture

```
┌──────────────────┐    HTTP/JSON     ┌──────────────────────────────────────┐
│   React Frontend │ ◄──────────────► │          .NET Web API                │
│                  │                  │           Backend                    │
│  • TaskManager   │                  │                                      │
│  • TaskCard      │                  │  ┌─────────────────┐                 │
│  • TaskForm      │                  │  │ TasksController │                 │
│  • API Service   │                  │  └────────┬────────┘                 │
└──────────────────┘                  │           │                          │
                                      │           ▼                          │
                                      │  ┌─────────────────┐                 │
                                      │  │   IDispatcher   │                 │
                                      │  └────────┬────────┘                 │
                                      │           │                          │
                   ┌──────────────────┼───────────┼───────────┬──────────────┤
                   │     QUERIES      │           │  COMMANDS │              │
                   │                  │           │           │              │
                   │ ┌──────────────┐ │           │ ┌───────────────────┐    │
                   │ │GetTasksQuery │ │           │ │CreateTaskCommand  │    │
                   │ │GetTaskByIdQ. │ │           │ │UpdateTaskCommand  │    │
                   │ └──────┬───────┘ │           │ │DeleteTaskCommand  │    │
                   │        │         │           │ └─────────┬─────────┘    │
                   │        ▼         │           │           ▼              │
                   │ ┌──────────────┐ │           │ ┌───────────────────┐    │
                   │ │Query Handlers│ │           │ │Command Handlers   │    │
                   │ └──────┬───────┘ │           │ └─────────┬─────────┘    │
                   └────────┼─────────┼───────────┼───────────┼──────────────┤
                            │         │           │           │              │
                            └─────────┼───────────┼───────────┘              │
                                      │           │                          │
                                      │           ▼                          │
                                      │  ┌─────────────────┐                 │
                                      │  │  TaskItem Model │                 │
                                      │  │  EF DbContext   │                 │
                                      └──┴─────────────────┴─────────────────┘
                                               │
                                               ▼
                                     ┌───────────────────┐
                                     │   In-Memory DB    │
                                     │ (Entity Framework)│
                                     └───────────────────┘
```

### Backend Architecture (.NET Web API)

**CQRS Pattern Implementation**
- **Controller**: `TasksController` handles HTTP requests and delegates to dispatcher
- **Dispatcher**: `IDispatcher` routes queries and commands to appropriate handlers
- **Queries**: Read operations (`GetTasksQuery`, `GetTaskByIdQuery`) 
- **Commands**: Write operations (`CreateTaskCommand`, `UpdateTaskCommand`, `DeleteTaskCommand`)
- **Handlers**: Separate handlers for each query/command with single responsibility
- **Models**: `TaskItem` entity with validation attributes
- **DTOs**: Clean separation with Create/Update/Response DTOs
- **Database**: Entity Framework Core with In-Memory provider
- **Validation**: Data annotations and model validation
- **Error Handling**: Global exception handling and structured error responses
- **CORS**: Configured for frontend communication

**CQRS Benefits**
- **Separation of Concerns**: Clear distinction between read and write operations
- **Testability**: Easy to mock and unit test individual handlers
- **Maintainability**: Feature-based organization with Commands/Queries/Handlers
- **Scalability**: Can optimize read and write operations independently

### Frontend Architecture (React + TypeScript)
- **Components**: 
  - `TaskManager` - Main container component
  - `TaskCard` - Individual task display
  - `TaskForm` - Create/Edit task modal
  - `ErrorBoundary` - Global error handling
- **Services**: Axios-based API client with interceptors
- **Types**: TypeScript interfaces for type safety
- **Hooks**: Custom hooks for API state management
- **Styling**: Tailwind CSS for responsive design

## Technology Stack

### Backend
- **.NET 9.0** - Web API framework
- **CQRS Pattern** - Custom implementation for separation of concerns
- **Entity Framework Core** - ORM with In-Memory database
- **System.ComponentModel.DataAnnotations** - Model validation
- **CORS** - Cross-origin resource sharing

### Frontend
- **React 18** - UI library
- **TypeScript** - Type safety and better development experience
- **Vite** - Fast development build tool
- **Tailwind CSS** - Utility-first CSS framework
- **Axios** - HTTP client for API communication
- **Lucide React** - Modern icon library

## Project Structure

```
TaskManager/
├── backend/
│   ├── TaskManagerApi/
│   │   ├── Controllers/
│   │   │   └── TasksController.cs        # HTTP API endpoints
│   │   ├── CQRS/                         # CQRS Infrastructure
│   │   │   ├── IQuery.cs                 # Query interface
│   │   │   ├── ICommand.cs               # Command interfaces
│   │   │   ├── IDispatcher.cs            # Dispatcher interface
│   │   │   └── Dispatcher.cs             # Command/Query dispatcher
│   │   ├── Features/                     # Feature-based organization
│   │   │   └── Tasks/
│   │   │       ├── Commands/             # Write operations
│   │   │       │   ├── CreateTaskCommand.cs
│   │   │       │   ├── UpdateTaskCommand.cs
│   │   │       │   └── DeleteTaskCommand.cs
│   │   │       ├── Queries/              # Read operations
│   │   │       │   ├── GetTasksQuery.cs
│   │   │       │   └── GetTaskByIdQuery.cs
│   │   │       └── Handlers/             # Business logic
│   │   │           ├── CreateTaskCommandHandler.cs
│   │   │           ├── UpdateTaskCommandHandler.cs
│   │   │           ├── DeleteTaskCommandHandler.cs
│   │   │           ├── GetTasksQueryHandler.cs
│   │   │           └── GetTaskByIdQueryHandler.cs
│   │   ├── Models/
│   │   │   └── TaskItem.cs               # Domain entity
│   │   ├── DTOs/
│   │   │   └── TaskDTOs.cs               # Data transfer objects
│   │   ├── Data/
│   │   │   └── TaskManagerContext.cs     # EF DbContext
│   │   └── Program.cs                    # Application startup
│   └── TaskManagerApi.Tests/             # Comprehensive Test Suite
│       ├── Controllers/
│       │   └── TasksControllerTests.cs   # Unit tests with CQRS mocking
│       ├── Integration/
│       │   └── TasksIntegrationTests.cs  # End-to-end API tests
│       └── Models/
│           └── TaskValidationTests.cs    # Model validation tests
└── frontend/
    └── task-manager-ui/
        ├── src/
        │   ├── components/
        │   │   ├── TaskManager.tsx
        │   │   ├── TaskCard.tsx
        │   │   ├── TaskForm.tsx
        │   │   └── ErrorBoundary.tsx
        │   ├── services/
        │   │   └── taskService.ts
        │   ├── types/
        │   │   └── Task.ts
        │   ├── hooks/
        │   │   └── useApi.ts
        │   └── App.tsx
        └── package.json
```

## Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Git](https://git-scm.com/)

### Installation & Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd TaskManager
   ```

2. **Setup Backend**
   ```bash
   cd backend/TaskManagerApi
   
   # Restore dependencies
   dotnet restore
   
   # Build the project
   dotnet build
   
   # Run the API (will start on http://localhost:5215)
   dotnet run
   ```

3. **Setup Frontend** (in a new terminal)
   ```bash
   cd frontend/task-manager-ui
   
   # Install dependencies
   npm install
   
   # Start development server (will start on http://localhost:5173)
   npm run dev
   ```

4. **Access the Application**
   - Frontend: http://localhost:5173
   - Backend API: http://localhost:5215
   - API Documentation: http://localhost:5215/swagger (if Swagger is enabled)

### Development Commands

#### Backend
```bash
# Run in development mode with hot reload
dotnet watch run

# Run comprehensive test suite
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run specific test project
cd backend/TaskManagerApi.Tests && dotnet test

# Build for production
dotnet build --configuration Release
```

#### Frontend
```bash
# Development server with hot reload
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Lint code
npm run lint
```

## Configuration

### Backend Configuration
- **Database**: Configured to use Entity Framework In-Memory database
- **CORS**: Allows requests from `http://localhost:3000` and `http://localhost:5173`
- **API Base URL**: `http://localhost:5215/api` (configure in `taskService.ts`)

### Frontend Configuration
- **API Endpoint**: Update `API_BASE_URL` in `src/services/taskService.ts` if backend runs on different port
- **Styling**: Tailwind CSS configured with default theme

## API Reference

### Tasks API
- `GET /api/tasks` - Get all tasks (supports ?isCompleted filter)
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update existing task
- `DELETE /api/tasks/{id}` - Delete task

### Request/Response Examples

**Create Task**
```json
POST /api/tasks
{
  \"title\": \"Learn React\",
  \"description\": \"Complete React tutorial and build a project\",
  \"priority\": 2
}
```

**Response**
```json
{
  \"id\": 1,
  \"title\": \"Learn React\",
  \"description\": \"Complete React tutorial and build a project\",
  \"isCompleted\": false,
  \"createdAt\": \"2024-01-15T10:30:00Z\",
  \"completedAt\": null,
  \"priority\": 2
}
```

## Testing

The TaskManager includes a comprehensive test suite ensuring code quality and reliability across all backend functionality.

### Test Architecture

```
backend/TaskManagerApi.Tests/
├── Controllers/
│   └── TasksControllerTests.cs    # Unit tests for API controller
├── Integration/
│   └── TasksIntegrationTests.cs   # End-to-end API tests
└── Models/
    └── TaskValidationTests.cs     # Model and DTO validation tests
```

### Test Coverage

#### **1. Unit Tests (`TasksControllerTests`)**
Comprehensive testing of the TasksController with CQRS architecture using **8 focused test methods**:

- **CQRS Query Testing** (IDispatcher mocking):
  - ✅ Get all tasks - mocks `GetTasksQuery` dispatch
  - ✅ Get single task by ID - mocks `GetTaskByIdQuery` dispatch
  - ✅ Handle non-existent task IDs (404 responses)

- **CQRS Command Testing** (IDispatcher mocking):
  - ✅ Create tasks - mocks `CreateTaskCommand` dispatch
  - ✅ Update existing tasks - mocks `UpdateTaskCommand` dispatch
  - ✅ Handle updates to non-existent tasks
  - ✅ Delete tasks - mocks `DeleteTaskCommand` dispatch
  - ✅ Handle deletion of non-existent tasks

- **CQRS Architecture Benefits in Testing**:
  - ✅ Clean separation: Controller logic vs business logic
  - ✅ Easy mocking of IDispatcher interface
  - ✅ Focused testing without database dependencies
  - ✅ Verification of correct command/query dispatching

#### **2. Integration Tests (`TasksIntegrationTests`)**
Full-stack API testing with real HTTP requests using **12+ test scenarios**:

- **HTTP Client Testing**:
  - ✅ Content-Type validation (`application/json`)
  - ✅ CORS policy validation
  - ✅ Status code verification for all endpoints
  - ✅ Request/Response serialization

- **End-to-End Workflows**:
  - ✅ Complete CRUD workflow (Create → Read → Update → Delete)
  - ✅ Task filtering via query parameters
  - ✅ Error scenarios and edge cases
  - ✅ API behavior under different conditions

- **Data Persistence**:
  - ✅ Verify data is actually saved to database
  - ✅ Test data consistency across operations
  - ✅ Validate database state changes

#### **3. Model Validation Tests (`TaskValidationTests`)**
Validation logic testing for **25+ validation scenarios**:

- **Entity Validation (`TaskItem`)**:
  - ✅ Required field validation (Title)
  - ✅ String length constraints (Title: 200 chars, Description: 1000 chars)
  - ✅ Priority enum value validation
  - ✅ DateTime field validation

- **DTO Validation**:
  - ✅ `TaskCreateDto` validation rules
  - ✅ `TaskUpdateDto` validation rules
  - ✅ Optional vs required field handling
  - ✅ Type safety and constraint enforcement

- **Priority Enum Tests**:
  - ✅ Enum value verification (`Low=1, Medium=2, High=3`)
  - ✅ String parsing and serialization
  - ✅ Invalid value handling

### Test Framework & Tools

- **xUnit**: Primary testing framework for .NET
- **Moq**: Mocking framework for dependencies
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing with `WebApplicationFactory`
- **Entity Framework In-Memory**: Isolated test database
- **Custom Test Helpers**: Validation utilities and test data builders

### Running Tests

```bash
cd backend/TaskManagerApi.Tests

# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run with coverage (requires coverage tools)
dotnet test --collect:"XPlat Code Coverage"

# Run specific test categories
dotnet test --filter "TestCategory=Unit"
dotnet test --filter "TestCategory=Integration"
```

### Test Results Summary
- **Total Tests**: 56 comprehensive test cases
- **Unit Tests**: 18 controller-focused tests
- **Integration Tests**: 13 end-to-end API tests  
- **Validation Tests**: 25 model and DTO tests
- **Coverage**: Complete API surface area coverage

### Test Benefits

1. **Regression Prevention**: Catch breaking changes before deployment
2. **Documentation**: Tests serve as living documentation of API behavior
3. **Refactoring Safety**: Confidently modify code knowing tests will catch issues
4. **Quality Assurance**: Validate business logic and edge cases
5. **Integration Confidence**: Ensure frontend-backend compatibility

### Test Data Management

Tests use isolated in-memory databases with:
- **Seeded Test Data**: Consistent test scenarios
- **Clean State**: Each test runs in isolation
- **Realistic Scenarios**: Tests mirror real-world usage patterns

## Deployment

### Backend Deployment
```bash
# Build for production
dotnet publish -c Release -o ./publish

# The app will be in ./publish folder
```

### Frontend Deployment
```bash
# Build for production
npm run build

# The build artifacts will be in ./dist folder
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Commit your changes (`git commit -m 'Add amazing feature'`)
5. Push to the branch (`git push origin feature/amazing-feature`)
6. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## References

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [React Documentation](https://reactjs.org/docs/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Tailwind CSS](https://tailwindcss.com/docs)

## Support

If you encounter any issues or have questions, please open an issue on GitHub or contact the maintainers.