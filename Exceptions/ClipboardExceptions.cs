using System;

namespace LibClipboard.Core
{
    /// <summary>
    /// Base exception for all clipboard-related errors.
    /// </summary>
    public abstract class ClipboardException : Exception
    {
        protected ClipboardException() { }
        protected ClipboardException(string message) : base(message) { }
        protected ClipboardException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when the native clipboard instance fails to initialize.
    /// </summary>
    public class ClipboardInitializationException : ClipboardException
    {
        public ClipboardInitializationException() { }
        public ClipboardInitializationException(string message) : base(message) { }
        public ClipboardInitializationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when clipboard operations fail due to access issues.
    /// </summary>
    public class ClipboardAccessException : ClipboardException
    {
        public ClipboardAccessException() { }
        public ClipboardAccessException(string message) : base(message) { }
        public ClipboardAccessException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when attempting to work with unsupported clipboard data formats.
    /// </summary>
    public class UnsupportedClipboardFormatException : ClipboardException
    {
        public UnsupportedClipboardFormatException() { }
        public UnsupportedClipboardFormatException(string message) : base(message) { }
        public UnsupportedClipboardFormatException(string message, Exception innerException) : base(message, innerException) { }
    }
}
