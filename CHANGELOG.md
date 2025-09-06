# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-09-06

### Added
- Initial release of LibClipboardSharp
- Cross-platform clipboard operations for Windows, Linux, and macOS
- Safe memory management using SafeHandle
- Text and image clipboard operations
- Async clipboard change monitoring with cancellation support
- Comprehensive error handling with specific exception types
- Logging integration with Microsoft.Extensions.Logging
- Configurable options (polling interval, max data size, whitespace trimming)
- Try-pattern methods for safe clipboard access
- XML documentation for all public APIs
- Example console application demonstrating usage

### Features
- `Clipboard` class with IDisposable pattern
- `ClipboardOptions` for configuration
- `ClipboardChangedEventArgs` for change notifications
- Support for text and image data formats
- Thread-safe design with proper resource cleanup
- Dynamic native library loading with platform detection

### Dependencies
- .NET 6.0+
- Microsoft.Extensions.Logging.Abstractions 8.0.0
- Native libclipboard library (runtime dependency)
