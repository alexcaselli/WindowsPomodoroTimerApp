# Pomodoro Timer App

## Overview

The Pomodoro Timer App is a productivity tool designed to help users manage their time effectively using the Pomodoro Technique. This technique involves breaking work into intervals, traditionally 25 minutes in length, separated by short breaks. This app is built using .NET 8 and targets Windows platforms.

## Features

* **Pomodoro Timer:** Set and manage work intervals and breaks.
* **Customizable Intervals:** Adjust the length of work sessions and breaks to fit your needs.
* **Notifications:** Receive notifications when it's time to take a break or start a new work session.
* **User Interface:** Intuitive and user-friendly interface built with WinUI.
* **MSIX Packaging:** Easy installation and updates through MSIX packaging.
* **Multi-Platform Support:** Supports x86, x64, and ARM64 architectures.

## Project Structure

The project is structured as follows:

* **Assets:** Contains images and icons used in the application.
    * **ControlIcons:** Icons for play, pause, stop, and disabled stop controls.
    * Various logo and splash screen images.
* **Properties:** Contains project properties and settings.
* **Main Application:** Core logic and UI components of the Pomodoro Timer App.

## Build and Publish

The project is configured to build and publish using the following settings:

* **Target Framework:** .NET 8.0
* **Output Type:** Windows Executable (WinExe)
* **Platforms:** x86, x64, ARM64
* **Runtime Identifiers:** `win-x86`, `win-x64`, `win-arm64`
* **MSIX Tooling:** Enabled for easy packaging and publishing.
* **Nullable Reference Types:** Enabled for better null safety.

## Dependencies

The project relies on the following NuGet packages:

* [Microsoft.Toolkit.Uwp.Notifications](https://www.nuget.org/packages/Microsoft.Toolkit.Uwp.Notifications) (Version 7.1.3)
* [Microsoft.Windows.SDK.BuildTools](https://www.nuget.org/packages/Microsoft.Windows.SDK.BuildTools) (Version 10.0.26100.1742)
* [Microsoft.WindowsAppSDK](https://www.nuget.org/packages/Microsoft.WindowsAppSDK) (Version 1.6.250108002)

## Installation

To install the Pomodoro Timer App, download the MSIX package from the release page and follow the installation instructions.

## Usage

1.  Launch the Pomodoro Timer App.
2.  Set your desired work interval and break duration.
3.  Start the timer and focus on your task.
4.  Receive notifications when it's time to take a break or start a new session.
5.  Use the control icons to play, pause, or stop the timer as needed.

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request with your changes. Ensure that your code adheres to the project's coding standards and includes appropriate tests.

## License

This project is licensed under the [MIT License](LICENSE). See the `LICENSE` file for more details.

## Contact

For any questions or feedback, please open an issue on the GitHub repository.

---

Thank you for using the Pomodoro Timer App! We hope it helps you stay productive and manage your time effectively.