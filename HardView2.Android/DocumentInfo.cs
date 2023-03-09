using System;
using System.Collections.Generic;
using System.Linq;

using Android.Content;
using Android.Provider;
using static Android.Graphics.Path;

namespace uk.andyjohnson.HardView2
{
    /// <summary>
    /// Access information about a document file given its uri
    /// </summary>
    public class DocumentInfo
    {
        /// <summary>
        /// Create a DocumentInfo object given a document iri.
        /// Uses the SAF content resolver.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="fileUri">Document file uri</param>
        /// <returns>DocumentInfo object, or null if the document file referenced by the uri does not exist.</returns>
        /// <exception cref="ArgumentNullException">Argument cannot be null</exception>
        public static DocumentInfo Create(
            Context context,
            Android.Net.Uri fileUri)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (fileUri == null)
                throw new ArgumentNullException(nameof(fileUri));

            var projection = new string[]
            {
                DocumentsContract.Document.ColumnDisplayName,
                DocumentsContract.Document.ColumnSize
            };
            using (var cursor = context.ContentResolver.Query(fileUri, projection, null, null, null))
            {
                if (cursor.MoveToNext())
                {
                    // Found.
                    return new DocumentInfo()
                    {
                        FileUri = fileUri,
                        DisplayName = cursor.GetString(0),
                        Length = cursor.GetLong(1)
                    };
                }
                else
                {
                    // Not found.
                    return null;
                }
            }
        }


        /// <summary>
        /// Document file uri. This is the value passed to the Create() method.
        /// </summary>
        public Android.Net.Uri FileUri { get; private set; }

        /// <summary>
        /// Display name of the file (e.g."image123.png").
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Length of the document file in bytes.
        /// </summary>
        public long Length { get; private set; }

        /// <summary>
        /// Display length of the document file (e.g. "127KB", "1.2MB", etc)
        /// </summary>
        public string DisplayLength
        {
            get
            {
                const long kb = 1024L;
                const long mb = 1024L * 1024L;

                if (this.Length >= mb)
                    return String.Format("{0:0.0}MB", (float)this.Length / (float)mb);
                else
                    return String.Format("{0:0.0}KB", (float)this.Length / (float)kb);
            }
        }
    }
}