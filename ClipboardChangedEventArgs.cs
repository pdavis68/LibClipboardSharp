using System;

namespace LibClipboard.Core
{
    /// <summary>
    /// Provides data for clipboard change events.
    /// </summary>
    public class ClipboardChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the timestamp when the clipboard change was detected.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets a value indicating whether the clipboard contains text.
        /// </summary>
        public bool HasText { get; }

        /// <summary>
        /// Gets a value indicating whether the clipboard contains an image.
        /// </summary>
        public bool HasImage { get; }

        /// <summary>
        /// Gets a value indicating whether this application has ownership of the clipboard.
        /// </summary>
        public bool HasOwnership { get; }

        /// <summary>
        /// Initializes a new instance of the ClipboardChangedEventArgs class.
        /// </summary>
        /// <param name="hasText">Whether the clipboard contains text.</param>
        /// <param name="hasImage">Whether the clipboard contains an image.</param>
        /// <param name="hasOwnership">Whether this application has clipboard ownership.</param>
        public ClipboardChangedEventArgs(bool hasText, bool hasImage, bool hasOwnership)
        {
            Timestamp = DateTime.UtcNow;
            HasText = hasText;
            HasImage = hasImage;
            HasOwnership = hasOwnership;
        }
    }
}
