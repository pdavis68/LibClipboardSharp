# Native Library Placeholder

This directory should contain the native libclipboard library for Linux x64.

**Required file:** `libclipboard.so` or `liblibclipboard.so`

## Building from Source

To build the native library from source on Linux:

1. Install dependencies:
   ```bash
   # Ubuntu/Debian
   sudo apt-get install build-essential cmake libx11-dev

   # CentOS/RHEL/Fedora  
   sudo yum install gcc cmake libX11-devel
   # or
   sudo dnf install gcc cmake libX11-devel
   ```

2. Clone and build:
   ```bash
   git clone https://github.com/jtanx/libclipboard.git
   cd libclipboard
   mkdir build && cd build
   cmake ..
   make
   ```

3. Copy the resulting shared library to this directory

## Package Managers

You may also be able to install via system package managers:
```bash
# Check if available in your distribution
apt search libclipboard
yum search libclipboard
```

**Important:** Ensure the binary is compatible with your target architecture (x64) and linked against appropriate system libraries.
