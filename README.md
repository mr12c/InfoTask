 # Workflow Engine API

This project is a simple, in-memory workflow engine built using ASP.NET Core Minimal APIs. It allows you to define workflow blueprints (States and Actions), create instances of those workflows, and transition them through their lifecycle.

This API is designed with a two-step process:
1.  **Define Global Components**: First, you create a global pool of reusable `States` and `Actions`.
2.  **Build Definitions**: Then, you create a `WorkflowDefinition` by selecting specific states and actions from the global pool and linking them together.

## 🚀 Setup and Running the Project

### Prerequisites
* [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later.
* A code editor like [Visual Studio Code](https://code.visualstudio.com/) or Visual Studio.
* An API testing tool like [Postman](https://www.postman.com/) or the built-in Swagger UI.

### Running the Application
1.  **Clone the repository:**
    ```bash
    git clone https://github.com/mr12c/InfoTask
    cd <your-project-directory>
    ```
2.  **Run the project from the command line:**
    ```bash
    dotnet run
    ```
3.  **Access the Swagger UI:**
    Once the application is running, open your web browser and navigate to:
    **http://localhost:5000/swagger/index.html** (the port may vary; check your console output).

    You can use the Swagger UI to test all the API endpoints directly.

---

## 📖 API Documentation

The API is divided into three main parts: Global Components (States/Actions), Workflow Definitions, and Workflow Instances.

### Base URL
* `http://localhost:5000` (or as specified in your `launchSettings.json`)

### 1. Global Components

Endpoints for creating the reusable building blocks of your workflows.

#### `POST /states/create`
Creates a new global state.

* **Request Body:** `application/json`
    ```json
    {
      "id": "draft",
      "name": "Draft",
      "isFinal": false,
      "isInitial": true,
      "enabled": true
    }
    ```
* **Success Response (200 - OK):**
    ```json
    {
      "id": "draft",
      "name": "Draft",
      "isFinal": false,
      "isInitial": true,
      "enabled": true
    }
    ```
* **Error Response (400 - Bad Request):**
    ```json
    "State ID already exists."
    ```

#### `POST /actions/create`
Creates a new global action that defines a transition between states.

* **Request Body:** `application/json`
    ```json
    {
      "id": "submit_for_review",
      "name": "Submit for Review",
      "fromStates": ["draft"],
      "toState": "in_review",
      "enabled": true
    }
    ```
* **Success Response (200 - OK):**
    ```json
    {
      "id": "submit_for_review",
      "name": "Submit for Review",
      "fromStates": ["draft"],
      "toState": "in_review",
      "enabled": true
    }
    ```
* **Error Response (400 - Bad Request):**
    ```json
    "FromState ID 'draft' not found."
    ```

### 2. Workflow Definitions

Endpoints for building and managing workflow blueprints.

#### `POST /workflowdef`
Creates a new, empty workflow definition.

* **Request Body:** `application/json`
    ```json
    {
      "id": "doc_approval",
      "description": "A workflow for approving official documents."
    }
    ```
* **Success Response (200 - OK):**
    ```json
    {
      "id": "doc_approval",
      "description": "A workflow for approving official documents.",
      "states": [],
      "actions": []
    }
    ```

#### `GET /workflowdef/{id}`
Retrieves a specific workflow definition by its ID.

* **Success Response (200 - OK):**
    ```json
    {
      "id": "doc_approval",
      "description": "A workflow for approving official documents.",
      "states": [ /* ... */ ],
      "actions": [ /* ... */ ]
    }
    ```
* **Error Response (404 - Not Found):**
    ```json
    "Definition not found."
    ```

#### `POST /workflowdef/{defId}/states/add`
Adds an existing global state to a workflow definition.

* **URL Parameter:** `defId` (e.g., `doc_approval`)
* **Request Body:** `application/json`
    ```json
    {
      "stateId": "draft"
    }
    ```
* **Success Response (200 - OK):**
    ```json
    {
      "id": "draft",
      "name": "Draft",
      "isFinal": false,
      "isInitial": true,
      "enabled": true
    }
    ```

#### `POST /workflowdef/{defId}/actions/add`
Adds an existing global action to a workflow definition.

* **URL Parameter:** `defId` (e.g., `doc_approval`)
* **Request Body:** `application/json`
    ```json
    {
      "actionId": "submit_for_review"
    }
    ```
* **Success Response (200 - OK):**
    ```json
    {
      "id": "submit_for_review",
      "name": "Submit for Review",
      "fromStates": ["draft"],
      "toState": "in_review",
      "enabled": true
    }
    ```

### 3. Workflow Instances

Endpoints for creating and running instances of workflows.

#### `POST /instances/create`
Creates and starts a new instance of a workflow.

* **Request Body:** `application/json`
    ```json
    {
      "defId": "doc_approval",
      "instanceId": "instance_123"
    }
    ```
* **Success Response (200 - OK):**
    ```json
    {
      "id": "instance_123",
      "definitionId": "doc_approval",
      "currentStateId": "draft",
      "history": []
    }
    ```

#### `GET /instances/{id}`
Gets the current status and history of an instance.

* **Success Response (200 - OK):**
    ```json
    {
      "id": "instance_123",
      "definitionId": "doc_approval",
      "currentStateId": "in_review",
      "history": [
        {
          "actionId": "submit_for_review",
          "timestamp": "2025-07-18T03:08:00.123Z"
        }
      ]
    }
    ```

#### `POST /instances/{id}/actions/{actionId}`
Performs an action on a running workflow instance, causing a state transition.

* **URL Parameters:** `id` (e.g., `instance_123`), `actionId` (e.g., `submit_for_review`)
* **Success Response (200 - OK):**
    ```json
    {
      "id": "instance_123",
      "definitionId": "doc_approval",
      "currentStateId": "in_review",
      "history": [
        {
          "actionId": "submit_for_review",
          "timestamp": "2025-07-18T03:08:00.123Z"
        }
      ]
    }
    ```
* **Error Response (400 - Bad Request):**
    ```json
    "Action not valid from current state."
    ```

---
