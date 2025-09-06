using LibClipboard.Core;
using Xunit;

namespace LibClipboardSharp.Tests
{
    public class ClipboardOptionsTests
    {
        [Fact]
        public void ClipboardOptions_DefaultValues_AreCorrect()
        {
            var options = new ClipboardOptions();
            
            Assert.Equal(TimeSpan.FromMilliseconds(100), options.PollingInterval);
            Assert.True(options.EnableChangeDetection);
            Assert.Equal(10 * 1024 * 1024, options.MaxDataSize);
            Assert.False(options.TrimWhitespace);
        }

        [Fact]
        public void ClipboardOptions_CanSetCustomValues()
        {
            var options = new ClipboardOptions
            {
                PollingInterval = TimeSpan.FromSeconds(1),
                EnableChangeDetection = false,
                MaxDataSize = 1024,
                TrimWhitespace = true
            };

            Assert.Equal(TimeSpan.FromSeconds(1), options.PollingInterval);
            Assert.False(options.EnableChangeDetection);
            Assert.Equal(1024, options.MaxDataSize);
            Assert.True(options.TrimWhitespace);
        }
    }

    public class ClipboardChangedEventArgsTests
    {
        [Fact]
        public void ClipboardChangedEventArgs_InitializesCorrectly()
        {
            var timestamp = DateTime.UtcNow;
            var eventArgs = new ClipboardChangedEventArgs(true, false, true);

            Assert.True(eventArgs.HasText);
            Assert.False(eventArgs.HasImage);
            Assert.True(eventArgs.HasOwnership);
            Assert.True(eventArgs.Timestamp >= timestamp);
            Assert.True(eventArgs.Timestamp <= DateTime.UtcNow);
        }
    }

    public class ClipboardExceptionTests
    {
        [Fact]
        public void ClipboardInitializationException_CanBeThrown()
        {
            var message = "Test message";
            var exception = new ClipboardInitializationException(message);

            Assert.Equal(message, exception.Message);
            Assert.IsType<ClipboardInitializationException>(exception);
            Assert.IsAssignableFrom<ClipboardException>(exception);
            Assert.IsAssignableFrom<Exception>(exception);
        }

        [Fact]
        public void ClipboardAccessException_CanBeThrown()
        {
            var message = "Access denied";
            var innerException = new InvalidOperationException("Inner");
            var exception = new ClipboardAccessException(message, innerException);

            Assert.Equal(message, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
            Assert.IsType<ClipboardAccessException>(exception);
        }

        [Fact]
        public void UnsupportedClipboardFormatException_CanBeThrown()
        {
            var exception = new UnsupportedClipboardFormatException();

            Assert.IsType<UnsupportedClipboardFormatException>(exception);
            Assert.IsAssignableFrom<ClipboardException>(exception);
        }
    }
}
