# Unlike Microservice

## Project Overview

This service provides functionality to "unlike" a publication. It is part of a larger microservices ecosystem, handling the specific action of removing a user's like from a publication stored in a MongoDB database. It uses JWT-based authentication to identify the user performing the action.

## Folder Structure

The repository is organized as follows:

-   **`.github/workflows/`**: Contains CI/CD pipeline configurations for GitHub Actions.
    -   `docker-publish.yml`: Workflow for publishing Docker images (likely for production).
    -   `docker-publish_qa.yml`: Workflow for publishing Docker images (likely for a QA environment).
-   **`UnlikeService/`**: Root directory for the .NET Core service.
    -   **`connection/`**:
        -   `mongo.cs`: Contains the logic for establishing and managing the connection to the MongoDB database.
    -   **`controllers/`**:
        -   `unlikecontroller.cs`: Defines the API endpoints. It handles incoming HTTP requests, validates them, extracts user information from JWT tokens, and calls the appropriate services.
    -   **`services/`**:
        -   `functions.cs`: Contains the core business logic for the service, such as the `UnlikePublication` function which interacts with the database to remove a like.
    -   `Program.cs`: The main entry point for the ASP.NET Core application. It configures and starts the web host.
    -   `UnlikeService.csproj`: The .NET project file, defining dependencies and build settings.
    -   `dockerfile`: Instructions for building a Docker image for the service.
    -   `appsettings.json` & `appsettings.Development.json`: Configuration files for the application.
    -   `.env` (expected, not in git): File to store environment variables locally (loaded by `DotNetEnv` in `Program.cs`).

## Backend Design Pattern

The service implements a **Layered Architecture**:

1.  **Presentation Layer (`controllers`):** `UnlikeController` is responsible for handling API requests, basic validation, authentication, and delegating tasks to the service layer. It formats HTTP responses.
2.  **Service Layer (`services`):** `Functions` class encapsulates the business logic (e.g., how to process an "unlike" action).
3.  **Data Access Layer (`connection` and service methods):** The `Mongo` class provides database connection capabilities. The actual database operations (queries, updates) are performed within the service layer methods using the MongoDB C# driver.

This pattern promotes separation of concerns, making the application more maintainable and testable.

## Communication Architecture

The service exposes a **RESTful API** for communication. Clients interact with the service via HTTP requests, and the service responds with standard HTTP status codes and JSON-formatted data.

## Folder Pattern

The `UnlikeService` project primarily uses a **layer-based folder pattern**, where code is organized into folders based on its technical responsibility (e.g., `controllers`, `services`, `connection`).

## Endpoint Instructions

### Unlike a Publication

Removes a user's like from a specified publication.

-   **Method:** `POST`
-   **Path:** `/Unlike`
-   **Authorization:**
    -   Type: `Bearer Token`
    -   Header: `Authorization: Bearer <your_jwt_token>`
    -   The JWT token must contain a `user_id` claim, which will be used to identify the user performing the unlike action.
-   **Request Body:**
    ```json
    {
        "IdPublication": "string" // The ObjectId of the publication to unlike
    }
    ```
-   **Responses:**
    -   **`200 OK`**: Successfully unliked the publication.
        ```json
        {
            "message": "Unlike success"
        }
        ```
    -   **`400 Bad Request`**:
        -   If the `IdPublication` is invalid:
            ```json
            {
                "error": "Invalid publication ID"
            }
            ```
        -   If the user has not liked the publication or the publication doesn't exist:
            ```json
            {
                "error": "You have not liked this publication"
            }
            ```
    -   **`401 Unauthorized`**:
        -   If the token is missing or improperly formatted:
            ```json
            {
                "error": "token missing or invalid"
            }
            ```
        -   If the `user_id` claim is not found in the token:
            ```json
            {
                "error": "user_id not found in token"
            }
            ```
        -   If the token is invalid (e.g., signature mismatch, expired):
            ```json
            {
                "error": "invalid or expired token"
            }
            ```
