# Screen Brightness By Battery

**[한국어](README.ko.md)** | English

A Windows application that adjusts screen brightness based on power state (battery or AC) and can prevent sleep mode when an external monitor is connected.

![image](https://github.com/user-attachments/assets/9a6db803-8424-4cba-a9e3-1d2b48b3c14b)

## Features

- **Automatic Brightness Control**: Adjusts screen brightness when switching between battery and AC power.
- **Sleep Prevention**: Optionally prevents sleep mode when an external monitor is connected.
- **System Tray Integration**: Runs in the system tray with quick access to settings.
- **Adaptive Brightness Support**: Supports Windows adaptive brightness.
- **Auto-start Option**: Can start automatically with Windows.
- **INI File Configuration**: Settings stored in an editable INI file.

## How It Works

### Brightness Control

The application detects when your device switches between battery and AC power, then adjusts screen brightness according to your settings:

- On **battery power**: Sets brightness to a lower value to save energy
- On **AC power**: Sets brightness to a higher value for better visibility
- Supports **adaptive brightness** ("auto" mode)

### Sleep Prevention

When enabled, the application will:

- Detect when an external monitor is connected
- Prevent sleep mode while the external monitor remains connected
- Allow normal sleep behavior when the external monitor is disconnected

## Settings

The application stores preferences in a `settings.ini` file with the following sections:

### [Brightness] Section

- `Enabled`: Whether brightness control is enabled ("on" or "off")
- `Battery`: Brightness level on battery power (0-100 or "auto")
- `AC`: Brightness level on AC power (0-100 or "auto")

### [Sleep] Section

- `Enabled`: Whether to prevent sleep when an external monitor is connected ("on" or "off")

## System Requirements

- Windows 10/11 (10.0.17763.0 or higher)
- x64 and ARM64

## Installation

1. Download and run the installer from the Releases page
2. Run `Screen Brightness By Battery` from the Start menu
3. The application appears in the system tray

## Usage

- **Right-click** the system tray icon to access settings
- Select **"Screen Brightness by Battery: Enabled/Disabled"** to toggle brightness control
- Select **"Prevent Sleep (External Monitor): Enabled/Disabled"** to toggle sleep prevention
- Select **"Open Settings File"** to manually edit your configuration
- Select **"Add to Startup Process"** to start the application with Windows

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

MIT License. See [LICENSE.txt](LICENSE.txt) for details.

## Author

`이호원 (Howon Lee) a.k.a hoyo321 or kck4156, airtaxi`