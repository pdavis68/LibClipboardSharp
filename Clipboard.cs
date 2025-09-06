using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using LibClipboard.Core.Internal;

namespace LibClipboard.Core
{
    /// <summary>
    /// Provides a safe, idiomatic C# interface to the native libclipboard library.
    /// This class must be used from an appropriate thread for your application type
    /// (e.g., the UI thread in WPF or WinForms applications on Windows).
    /// </summary>
    public class Clipboard : IDisposable
    {
        private readonly ClipboardHandle _handle;
        private readonly ClipboardOptions _options;
        private readonly ILogger? _logger;
        private bool _disposed;
        private CancellationTokenSource? _pollingCancellationTokenSource;
        private Task? _pollingTask;

        /// <summary>
        /// Occurs when the clipboard content changes during polling.
        /// </summary>
        public event EventHandler<ClipboardChangedEventArgs>? OnClipboardChanged;

        /// <summary>
        /// Gets a value indicating whether the clipboard currently contains text.
        /// </summary>
        public bool HasText
        {
            get
            {
                ThrowIfDisposed();
                return NativeMethods.clipboard_has_text(_handle) != 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the clipboard currently contains an image.
        /// </summary>
        public bool HasImage
        {
            get
            {
                ThrowIfDisposed();
                return NativeMethods.clipboard_has_image(_handle) != 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this application has clipboard ownership.
        /// </summary>
        public bool HasOwnership
        {
            get
            {
                ThrowIfDisposed();
                return NativeMethods.clipboard_has_ownership(_handle) != 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Clipboard class.
        /// </summary>
        /// <param name="options">Optional configuration options for the clipboard instance.</param>
        /// <param name="logger">Optional logger for diagnostic output.</param>
        /// <exception cref="ClipboardInitializationException">Thrown when the native clipboard instance fails to initialize.</exception>
        public Clipboard(ClipboardOptions? options = null, ILogger? logger = null)
        {
            _options = options ?? new ClipboardOptions();
            _logger = logger;

            try
            {
                // For now, we pass IntPtr.Zero as libclipboard doesn't expose options in the current API
                _handle = NativeMethods.clipboard_new(IntPtr.Zero);
                if (_handle.IsInvalid)
                {
                    throw new ClipboardInitializationException("Failed to initialize native clipboard instance. The native library may not be available or compatible.");
                }

                _logger?.LogDebug("Clipboard instance initialized successfully");
            }
            catch (DllNotFoundException ex)
            {
                throw new ClipboardInitializationException("Native libclipboard library not found. Ensure the library is available in the application's runtime path.", ex);
            }
            catch (Exception ex) when (!(ex is ClipboardInitializationException))
            {
                throw new ClipboardInitializationException("Failed to initialize clipboard due to an unexpected error.", ex);
            }
        }

        /// <summary>
        /// Sets the clipboard text content.
        /// </summary>
        /// <param name="text">The text to set. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when text is null.</exception>
        /// <exception cref="ClipboardAccessException">Thrown when the clipboard operation fails.</exception>
        public void SetText(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            ThrowIfDisposed();

            try
            {
                var processedText = _options.TrimWhitespace ? text.Trim() : text;
                var utf8Bytes = Encoding.UTF8.GetBytes(processedText + '\0'); // Null-terminated

                if (_options.MaxDataSize > 0 && utf8Bytes.Length > _options.MaxDataSize)
                {
                    throw new ClipboardAccessException($"Text data size ({utf8Bytes.Length} bytes) exceeds maximum allowed size ({_options.MaxDataSize} bytes).");
                }

                var result = NativeMethods.clipboard_set_text(_handle, utf8Bytes);
                if (result != 0)
                {
                    throw new ClipboardAccessException($"Failed to set clipboard text. Native error code: {result}");
                }

                _logger?.LogDebug("Successfully set clipboard text ({Length} bytes)", utf8Bytes.Length);
            }
            catch (Exception ex) when (!(ex is ArgumentNullException || ex is ClipboardAccessException || ex is ObjectDisposedException))
            {
                _logger?.LogError(ex, "Unexpected error setting clipboard text");
                throw new ClipboardAccessException("An unexpected error occurred while setting clipboard text.", ex);
            }
        }

        /// <summary>
        /// Gets the current text content from the clipboard.
        /// </summary>
        /// <returns>The clipboard text content, or null if no text is available.</returns>
        /// <exception cref="ClipboardAccessException">Thrown when the clipboard operation fails.</exception>
        public string? GetText()
        {
            ThrowIfDisposed();

            IntPtr textPtr = IntPtr.Zero;
            try
            {
                textPtr = NativeMethods.clipboard_text(_handle);
                if (textPtr == IntPtr.Zero)
                {
                    _logger?.LogDebug("No text available in clipboard");
                    return null;
                }

                var text = Marshal.PtrToStringUTF8(textPtr);
                if (text != null && _options.TrimWhitespace)
                {
                    text = text.Trim();
                }

                _logger?.LogDebug("Successfully retrieved clipboard text ({Length} characters)", text?.Length ?? 0);
                return text;
            }
            catch (Exception ex) when (!(ex is ObjectDisposedException))
            {
                _logger?.LogError(ex, "Error retrieving clipboard text");
                throw new ClipboardAccessException("Failed to retrieve clipboard text.", ex);
            }
            finally
            {
                if (textPtr != IntPtr.Zero)
                {
                    try
                    {
                        NativeMethods.clipboard_text_free(_handle, textPtr);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to free native text memory");
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to get the current text content from the clipboard without throwing exceptions.
        /// </summary>
        /// <param name="text">When this method returns, contains the clipboard text if successful; otherwise, null.</param>
        /// <returns>true if text was successfully retrieved; otherwise, false.</returns>
        public bool TryGetText(out string? text)
        {
            text = null;
            try
            {
                text = GetText();
                return text != null;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "TryGetText failed");
                return false;
            }
        }

        /// <summary>
        /// Sets the clipboard image content.
        /// </summary>
        /// <param name="imageBytes">The image data to set. Should be in PNG format for best cross-platform compatibility.</param>
        /// <exception cref="ArgumentNullException">Thrown when imageBytes is null.</exception>
        /// <exception cref="ClipboardAccessException">Thrown when the clipboard operation fails.</exception>
        public void SetImage(byte[] imageBytes)
        {
            if (imageBytes == null)
                throw new ArgumentNullException(nameof(imageBytes));

            ThrowIfDisposed();

            try
            {
                if (_options.MaxDataSize > 0 && imageBytes.Length > _options.MaxDataSize)
                {
                    throw new ClipboardAccessException($"Image data size ({imageBytes.Length} bytes) exceeds maximum allowed size ({_options.MaxDataSize} bytes).");
                }

                var result = NativeMethods.clipboard_set_image(_handle, imageBytes, imageBytes.Length);
                if (result != 0)
                {
                    throw new ClipboardAccessException($"Failed to set clipboard image. Native error code: {result}");
                }

                _logger?.LogDebug("Successfully set clipboard image ({Length} bytes)", imageBytes.Length);
            }
            catch (Exception ex) when (!(ex is ArgumentNullException || ex is ClipboardAccessException || ex is ObjectDisposedException))
            {
                _logger?.LogError(ex, "Unexpected error setting clipboard image");
                throw new ClipboardAccessException("An unexpected error occurred while setting clipboard image.", ex);
            }
        }

        /// <summary>
        /// Gets the current image content from the clipboard.
        /// </summary>
        /// <returns>The clipboard image data, or null if no image is available.</returns>
        /// <exception cref="ClipboardAccessException">Thrown when the clipboard operation fails.</exception>
        public byte[]? GetImage()
        {
            ThrowIfDisposed();

            IntPtr imagePtr = IntPtr.Zero;
            try
            {
                imagePtr = NativeMethods.clipboard_image(_handle, out int length);
                if (imagePtr == IntPtr.Zero || length <= 0)
                {
                    _logger?.LogDebug("No image available in clipboard");
                    return null;
                }

                if (_options.MaxDataSize > 0 && length > _options.MaxDataSize)
                {
                    throw new ClipboardAccessException($"Image data size ({length} bytes) exceeds maximum allowed size ({_options.MaxDataSize} bytes).");
                }

                var imageBytes = new byte[length];
                Marshal.Copy(imagePtr, imageBytes, 0, length);

                _logger?.LogDebug("Successfully retrieved clipboard image ({Length} bytes)", length);
                return imageBytes;
            }
            catch (Exception ex) when (!(ex is ClipboardAccessException || ex is ObjectDisposedException))
            {
                _logger?.LogError(ex, "Error retrieving clipboard image");
                throw new ClipboardAccessException("Failed to retrieve clipboard image.", ex);
            }
            finally
            {
                if (imagePtr != IntPtr.Zero)
                {
                    try
                    {
                        NativeMethods.clipboard_image_free(_handle, imagePtr);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to free native image memory");
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to get the current image content from the clipboard without throwing exceptions.
        /// </summary>
        /// <param name="imageBytes">When this method returns, contains the clipboard image data if successful; otherwise, null.</param>
        /// <returns>true if image data was successfully retrieved; otherwise, false.</returns>
        public bool TryGetImage(out byte[]? imageBytes)
        {
            imageBytes = null;
            try
            {
                imageBytes = GetImage();
                return imageBytes != null;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "TryGetImage failed");
                return false;
            }
        }

        /// <summary>
        /// Clears the clipboard content.
        /// </summary>
        /// <exception cref="ClipboardAccessException">Thrown when the clipboard operation fails.</exception>
        public void Clear()
        {
            ThrowIfDisposed();

            try
            {
                var result = NativeMethods.clipboard_clear(_handle);
                if (result != 0)
                {
                    throw new ClipboardAccessException($"Failed to clear clipboard. Native error code: {result}");
                }

                _logger?.LogDebug("Successfully cleared clipboard");
            }
            catch (Exception ex) when (!(ex is ClipboardAccessException || ex is ObjectDisposedException))
            {
                _logger?.LogError(ex, "Unexpected error clearing clipboard");
                throw new ClipboardAccessException("An unexpected error occurred while clearing clipboard.", ex);
            }
        }

        /// <summary>
        /// Starts polling for clipboard changes asynchronously.
        /// </summary>
        /// <param name="interval">The polling interval. If not specified, uses the interval from ClipboardOptions.</param>
        /// <param name="cancellationToken">Token to cancel the polling operation.</param>
        /// <returns>A task representing the polling operation.</returns>
        public Task StartPollingAsync(TimeSpan? interval = null, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (!_options.EnableChangeDetection)
            {
                _logger?.LogWarning("Polling requested but change detection is disabled in options");
                return Task.CompletedTask;
            }

            // Cancel any existing polling
            _pollingCancellationTokenSource?.Cancel();

            var pollingInterval = interval ?? _options.PollingInterval;
            _pollingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _pollingTask = Task.Run(async () =>
            {
                _logger?.LogDebug("Starting clipboard polling with interval {Interval}", pollingInterval);

                var lastHasText = HasText;
                var lastHasImage = HasImage;
                var lastHasOwnership = HasOwnership;

                try
                {
                    while (!_pollingCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await Task.Delay(pollingInterval, _pollingCancellationTokenSource.Token);

                        if (_disposed)
                            break;

                        try
                        {
                            var pollResult = NativeMethods.clipboard_poll(_handle);
                            if (pollResult != 0) // Change detected
                            {
                                var currentHasText = HasText;
                                var currentHasImage = HasImage;
                                var currentHasOwnership = HasOwnership;

                                if (currentHasText != lastHasText || 
                                    currentHasImage != lastHasImage || 
                                    currentHasOwnership != lastHasOwnership)
                                {
                                    var eventArgs = new ClipboardChangedEventArgs(
                                        currentHasText, 
                                        currentHasImage, 
                                        currentHasOwnership);

                                    OnClipboardChanged?.Invoke(this, eventArgs);

                                    lastHasText = currentHasText;
                                    lastHasImage = currentHasImage;
                                    lastHasOwnership = currentHasOwnership;

                                    _logger?.LogDebug("Clipboard change detected and event fired");
                                }
                            }
                        }
                        catch (Exception ex) when (!_pollingCancellationTokenSource.Token.IsCancellationRequested)
                        {
                            _logger?.LogWarning(ex, "Error during clipboard polling");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger?.LogDebug("Clipboard polling cancelled");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Unexpected error in clipboard polling");
                }
            }, _pollingCancellationTokenSource.Token);

            return _pollingTask;
        }

        /// <summary>
        /// Stops the current polling operation if one is running.
        /// </summary>
        [Obsolete("Use the CancellationToken parameter in StartPollingAsync instead.")]
        public void StopPolling()
        {
            _pollingCancellationTokenSource?.Cancel();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Clipboard));
        }

        /// <summary>
        /// Releases all resources used by the Clipboard instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the Clipboard and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _pollingCancellationTokenSource?.Cancel();
                    try
                    {
                        _pollingTask?.Wait(TimeSpan.FromSeconds(1));
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Error waiting for polling task to complete during disposal");
                    }

                    _pollingCancellationTokenSource?.Dispose();
                    _handle?.Dispose();
                }

                _disposed = true;
                _logger?.LogDebug("Clipboard instance disposed");
            }
        }

        /// <summary>
        /// Finalizer for the Clipboard class.
        /// </summary>
        ~Clipboard()
        {
            Dispose(false);
        }
    }
}
