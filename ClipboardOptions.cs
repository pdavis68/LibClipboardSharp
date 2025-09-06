using System;

namespace LibClipboard.Core
{
    /// <summary>
    /// Configuration options for the native clipboard instance.
    /// </summary>
    public class ClipboardOptions
    {
        /// <summary>
        /// Gets or sets the polling interval for clipboard change detection.
        /// Default is 100 milliseconds.
        /// </summary>
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Gets or sets whether to enable clipboard change detection.
        /// Default is true.
        /// </summary>
        public bool EnableChangeDetection { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum size for clipboard data operations in bytes.
        /// Default is 10MB. Set to 0 for no limit.
        /// </summary>
        public long MaxDataSize { get; set; } = 10 * 1024 * 1024; // 10MB

        /// <summary>
        /// Gets or sets whether to automatically trim whitespace from text operations.
        /// Default is false.
        /// </summary>
        public bool TrimWhitespace { get; set; } = false;
    }
}
