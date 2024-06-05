# Form Builder - Windows Application for Zonal OCR

## Introduction

Form Builder is a Windows application developed using WPF (Windows Presentation Foundation) that enables users to create forms and define fields with Zonal OCR (Optical Character Recognition) capabilities. With this application, users can streamline the process of extracting information from documents by defining zones within images and assigning OCR-related properties to these zones.

## Features

- **Form Creation:** Users can create forms by defining various fields within them.
- **Zonal OCR:** Define zones within images to extract specific information using OCR.
- **Field Properties:** Assign properties such as regex patterns, whitelist characters, and field types to each zone.
- **Save and Load:** Save created forms and fields configurations for future use.

## Technologies Used

- **WPF (Windows Presentation Foundation):** Framework for building Windows desktop applications.
- **C#:** Primary programming language for backend logic and UI interactions.
- **Json.NET:** Library for JSON serialization and deserialization.

## Getting Started

To run the Form Builder application locally, follow these steps:

1. Clone the repository from [GitHub](https://github.com/mohamedelareeg/FormBuilder).
2. Open the solution file (`FormBuilder.sln`) in Visual Studio.
3. Build the solution to restore dependencies and compile the project.
4. Run the application to start using Form Builder.

## Usage

1. **Create a Form:** Click on the "New Form" button to create a new form.
2. **Add Fields:** Use the UI to add fields to the form by defining zones within images.
3. **Define Properties:** Set properties such as regex patterns, whitelist characters, and field types for each zone.
4. **Save Configuration:** Save the form and fields configuration for future use.
5. **Load Configuration:** Load previously saved configurations to continue working on existing forms.

## Screenshots

### Adding Rectangles
![Adding Rectangles](/screenshots/add-rectangles.PNG)

### Cropping Rectangles
![Cropping Rectangles](/screenshots/crop-rectangles.PNG)

### Customizing Fields
![Customizing Fields](/screenshots/custmize-field.PNG)


## License

This project is licensed under the MIT License. See the [LICENSE](/LICENSE) file for details.

For more details, refer to the [GitHub repository](https://github.com/mohamedelareeg/FormBuilder).

