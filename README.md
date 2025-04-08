# Screen Brightness By Battery

A lightweight Windows application that automatically adjusts your screen brightness based on your device's power state (battery or AC power) and prevents sleep mode when an external monitor is connected.

## Features

- **Automatic Brightness Control**: Adjusts screen brightness automatically when switching between battery and AC power.
- **Smart Sleep Prevention**: Optionally prevents your device from entering sleep mode when an external monitor is connected.
- **System Tray Integration**: Runs silently in the system tray for easy access to settings.
- **Adaptive Brightness Support**: Supports Windows adaptive brightness feature.
- **Auto-start Option**: Can be set to start automatically with Windows.
- **Simple Configuration**: Uses an easy-to-edit INI file for settings.

## How It Works

### Brightness Control

The application automatically detects when your device switches between battery power and AC power, then adjusts the screen brightness according to your preferences:

- When running on **battery power**: Sets brightness to a lower value to save energy
- When plugged into **AC power**: Sets brightness to a higher value for better visibility
- Supports **adaptive brightness** setting ("auto" mode)

### Sleep Prevention

When enabled, the application will:

- Detect when an external monitor is connected
- Prevent the system from entering sleep mode while the external monitor remains connected
- Allow normal sleep behavior when the external monitor is disconnected

## Settings

The application stores your preferences in a `settings.ini` file with the following sections:

### [Brightness] Section

- `Enabled`: Whether the brightness control feature is enabled ("on" or "off")
- `Battery`: Brightness level when on battery power (0-100 or "auto")
- `AC`: Brightness level when on AC power (0-100 or "auto")

### [Sleep] Section

- `Enabled`: Whether to prevent sleep mode when external monitor is connected ("on" or "off")

## System Requirements

- Windows 10/11 (10.0.17763.0 or higher)
- Compatible with x64 and ARM64 architectures

## Installation

1. Download and run installer of the latest release from the Releases page
2. Run `Screen Brightness By Battery` from start menu
3. The application will appear in your system tray

## Usage

- **Right-click** the system tray icon to access settings
- Select **"Screen Brightness by Battery: Enabled/Disabled"** to toggle the brightness control feature
- Select **"Prevent Sleep (External Monitor): Enabled/Disabled"** to toggle the sleep prevention feature
- Select **"Open Settings File"** to manually edit your configuration
- Select **"Add to Startup Process"** to make the application start with Windows

## Building from Source

This project uses:

- .NET 9.0
- Windows App SDK
- WinUI 3
- Target Windows version 10.0.22621.0 (minimum 10.0.17763.0)

### Requirements

- Visual Studio 2022 or newer
- Windows App SDK development tools
- .NET 9.0 SDK

### Build Steps

1. Clone the repository
2. Open `ScreenBrightnessByBattery.sln` in Visual Studio
3. Build the solution for your target architecture (x64 or ARM64)

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Author

`이호원 (Howon Lee) a.k.a hoyo321 or kck4156, airtaxi`
