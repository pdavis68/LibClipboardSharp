# LibClipboardSharp

A safe, idiomatic C# wrapper for the [jtanx/libclipboard](https://github.com/jtanx/libclipboard) cross-platform clipboard library.

## Features

- **Cross-platform support**: Windows, Linux, and macOS
- **Safe memory management**: Uses `SafeHandle` to prevent memory leaks
- **Thread-safe design**: Proper disposal and resource management
- **Async clipboard monitoring**: Event-driven change detection with cancellation support
- **Robust error handling**: Specific exceptions with detailed error information
- **Logging integration**: Uses `Microsoft.Extensions.Logging` for diagnostics
- **Modern .NET**: Targets .NET 6.0+ with nullable reference types

## Installation

### NuGet Package (Recommended)
```
dotnet add package LibClipboardSharp
```

### Manual Installation
1. Clone this repository
2. Build the library: `dotnet build`
3. Include the native `libclipboard` binaries in your application's runtime directory

## Native Library Requirements

This wrapper requires the native `libclipboard` shared library to be available:
- **Windows**: `libclipboard.dll`
- **Linux**: `libclipboard.so` or `liblibclipboard.so`
- **macOS**: `libclipboard.dylib` or `liblibclipboard.dylib`

You will need to build libclipboard locally and then place the appropriate binary in your application's output directory or ensure it's in the system's library search path.

## Usage Examples

### Basic Text Operations

```csharp
using LibClipboard.Core;

// Create a clipboard instance
using var clipboard = new Clipboard();

// Set text to clipboard
clipboard.SetText("Hello, LibClipboardSharp!");

// Get text from clipboard
if (clipboard.TryGetText(out string text))
{
    Console.WriteLine($"Clipboard contains: {text}");
}

// Check if clipboard has text
if (clipboard.HasText)
{
    string content = clipboard.GetText();
    Console.WriteLine($"Text: {content}");
}
```

### Image Operations

```csharp
using LibClipboard.Core;

using var clipboard = new Clipboard();

// Read image from file and set to clipboard
byte[] imageData = File.ReadAllBytes("image.png");
clipboard.SetImage(imageData);

// Get image from clipboard
if (clipboard.TryGetImage(out byte[] clipboardImage))
{
    File.WriteAllBytes("clipboard_image.png", clipboardImage);
    Console.WriteLine("Image saved from clipboard");
}
```

### Clipboard Change Monitoring

```csharp
using LibClipboard.Core;

var options = new ClipboardOptions
{
    PollingInterval = TimeSpan.FromMilliseconds(500),
    EnableChangeDetection = true
};

using var clipboard = new Clipboard(options);

// Subscribe to change events
clipboard.OnClipboardChanged += (sender, e) =>
{
    Console.WriteLine($"Clipboard changed at {e.Timestamp}");
    Console.WriteLine($"Has text: {e.HasText}, Has image: {e.HasImage}");
};

// Start monitoring with cancellation support
using var cts = new CancellationTokenSource();
var pollingTask = clipboard.StartPollingAsync(cancellationToken: cts.Token);

// Do other work...
await Task.Delay(10000);

// Stop monitoring
cts.Cancel();
await pollingTask;
```

### Configuration Options

```csharp
using LibClipboard.Core;
using Microsoft.Extensions.Logging;

// Create logger
using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
var logger = loggerFactory.CreateLogger<Clipboard>();

// Configure options
var options = new ClipboardOptions
{
    MaxDataSize = 5 * 1024 * 1024, // 5MB limit
    TrimWhitespace = true,          // Auto-trim text
    PollingInterval = TimeSpan.FromMilliseconds(250)
};

using var clipboard = new Clipboard(options, logger);
```

### Error Handling

```csharp
using LibClipboard.Core;

try
{
    using var clipboard = new Clipboard();
    clipboard.SetText("Sample text");
}
catch (ClipboardInitializationException ex)
{
    Console.WriteLine($"Failed to initialize clipboard: {ex.Message}");
    // Handle missing native library or initialization failure
}
catch (ClipboardAccessException ex)
{
    Console.WriteLine($"Clipboard access failed: {ex.Message}");
    // Handle clipboard access issues (permissions, lock, etc.)
}
```

## Threading Considerations

⚠️ **Important**: This library does not automatically marshal calls to a specific thread. On Windows, clipboard operations often require the Single-Threaded Apartment (STA) model, typically the UI thread in desktop applications.

**For UI Applications:**
- WPF: Use `Dispatcher.Invoke()` to call clipboard methods from the UI thread
- WinForms: Use `Control.Invoke()` to ensure proper thread context
- Console apps: Generally work without special threading considerations

**Example for WPF:**
```csharp
private async void Button_Click(object sender, RoutedEventArgs e)
{
    await Dispatcher.InvokeAsync(() =>
    {
        using var clipboard = new Clipboard();
        clipboard.SetText("Hello from UI thread!");
    });
}
```

## API Reference

### Clipboard Class

#### Properties
- `bool HasText` - Checks if clipboard contains text
- `bool HasImage` - Checks if clipboard contains an image  
- `bool HasOwnership` - Checks if this application owns the clipboard

#### Methods
- `void SetText(string text)` - Sets clipboard text content
- `string GetText()` - Gets clipboard text content
- `bool TryGetText(out string text)` - Safely attempts to get text
- `void SetImage(byte[] imageBytes)` - Sets clipboard image content
- `byte[] GetImage()` - Gets clipboard image content
- `bool TryGetImage(out byte[] imageBytes)` - Safely attempts to get image
- `void Clear()` - Clears clipboard content
- `Task StartPollingAsync(TimeSpan? interval, CancellationToken token)` - Starts change monitoring

#### Events
- `EventHandler<ClipboardChangedEventArgs> OnClipboardChanged` - Fired when clipboard content changes

### ClipboardOptions Class

#### Properties
- `TimeSpan PollingInterval` - Interval for change detection (default: 100ms)
- `bool EnableChangeDetection` - Enable/disable polling (default: true)
- `long MaxDataSize` - Maximum data size in bytes (default: 10MB)
- `bool TrimWhitespace` - Auto-trim text content (default: false)

### Exception Types

- `ClipboardInitializationException` - Native library initialization failed
- `ClipboardAccessException` - Clipboard operation failed
- `UnsupportedClipboardFormatException` - Unsupported data format

## Building from Source

```bash
# Clone the repository
git clone https://github.com/yourusername/LibClipboardSharp.git
cd LibClipboardSharp

# Restore dependencies
dotnet restore

# Build the library
dotnet build

# Run tests (if available)
dotnet test

# Create NuGet package
dotnet pack
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Dependencies

- **Microsoft.Extensions.Logging.Abstractions** 8.0.0
- **Native libclipboard library** (runtime dependency)

## Supported Platforms

- **Windows** (x64, x86, ARM64)
- **Linux** (x64, ARM64) 
- **macOS** (x64, ARM64)

The library automatically detects the platform and loads the appropriate native library.
