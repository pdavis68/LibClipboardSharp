# Native Library Placeholder

This directory should contain the native libclipboard library for macOS x64.

**Required file:** `libclipboard.dylib` or `liblibclipboard.dylib`

## Building from Source

To build the native library from source on macOS:

1. Install Xcode Command Line Tools:
   ```bash
   xcode-select --install
   ```

2. Install CMake (via Homebrew):
   ```bash
   brew install cmake
   ```

3. Clone and build:
   ```bash
   git clone https://github.com/jtanx/libclipboard.git
   cd libclipboard
   mkdir build && cd build
   cmake ..
   make
   ```

4. Copy the resulting dylib to this directory

## Homebrew

You may also be able to install via Homebrew if available:
```bash
brew search libclipboard
```

**Important:** Ensure the binary is compatible with your target architecture (x64) and macOS version requirements.
