# HMER API - Handwritten Mathematical Expression Recognition

## Overview

HMER API is a RESTful service for recognizing and processing handwritten mathematical expressions. Upload an image containing handwritten math, and the system will return the recognized LaTeX expression along with a human-readable description.

## Features

- Image upload for handwritten mathematical expressions
- Automatic recognition using advanced computer vision
- Conversion to LaTeX format
- Natural language descriptions of mathematical expressions
- Persistence of results for future reference

## Technologies

- **.NET 9.0**: Modern framework for building APIs
- **Entity Framework Core**: ORM for database operations
- **SQL Server**: Database for storing recognition results
- **Python**: Integration for the mathematical expression recognition model
- **LaTeX to MathML**: Conversion for standard math representation
- **Repository Pattern**: Clean architecture principles

## Prerequisites

- .NET 9.0 SDK
- SQL Server
- Python 3.x with required packages
- Node.js (for speech conversion)

## Setup

1. Clone the repository
2. Configure your SQL Server connection string in appsettings.json
3. Run database migrations:
   ```
   dotnet ef database update
   ```
4. Ensure Python environment is set up with required dependencies
5. Create an Uploads directory in the project root

## API Endpoints

- **POST /Product**: Upload an image and get recognition results
- **GET /Product/{id}**: Retrieve a specific recognition result
- **GET /Product**: Retrieve all recognition results
- **PUT /Product/{id}**: Update an existing recognition entry
- **DELETE /Product/{id}**: Delete a recognition entry

## Usage

Upload a handwritten mathematical expression image (JPG, PNG, BMP) up to 3MB in size. The API will process the image and return:

- The LaTeX representation of the expression
- A natural language description of the expression
- A unique ID for future reference

The image and recognition results are stored in the database for later retrieval.

## Limitations

- Maximum image size: 3MB
- Supported formats: JPG, JPEG, PNG, BMP