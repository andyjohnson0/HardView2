using System;
using System.Collections.Generic;
using System.Linq;

using Android.Content;
using Android.Provider;


namespace uk.andyjohnson.HardView2
{
    /// <summary>
    /// Implements a collection of content resolver document IDs and methods to resolve them to Uris on-demand.
    /// This is much more time efficient than DocumentFile.ListFiles() - see https://stackoverflow.com/a/66296597/67316.
    /// </summary>
    public class DocumentCollection
    {
        /// <summary>
        /// Index value for "no position"
        /// </summary>
        public const int NoPosition = -1;


        /// <summary>
        /// Create a DocumentCollection object.
        /// </summary>
        /// <param name="context">Context used to access content resolver.</param>
        /// <param name="dirUri">Uri for the directory containing the image file documents.</param>
        /// <param name="fileTypes">Optional fille types (e.g. ".jpg" etc)</param>
        /// <returns>DocumentCollection object.</returns>
        public static DocumentCollection Create(
            Context context,
            Android.Net.Uri dirUri,
            IEnumerable<string> fileTypes = null)
        {
            return CreateImpl(context, dirUri, fileTypes);
        }


        /// <summary>
        /// Get url of directory containing the image document files. This is the url passed to Create().
        /// </summary>
        public Android.Net.Uri DirectoryUri
        {
            get { return this.dirUri; }
        }


        /// <summary>
        /// Number of image document files in the collection.
        /// </summary>
        public int Length
        {
            get { return documentFileIds.Length; }
        }


        /// <summary>
        /// Get the content url of the image file document at the specified position
        /// </summary>
        /// <param name="context">Context used to access content resolver</param>
        /// <param name="position">Zero-based position</param>
        /// <returns>Url of the image file document</returns>
        /// <exception cref="ArgumentNullException">A required argument was null</exception>
        /// <exception cref="ArgumentException">An argument value was invalid</exception>
        public Android.Net.Uri UriAt(
            Context context,
            int position)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (position < 0 || position >= documentFileIds.Length)
                throw new ArgumentException("Index out of range", nameof(position));

            return this.GetOrCreateUriAt(context, position);
        }


        /// <summary>
        /// Get the position of the next image file document after the supplied position.
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Position of the next image file document, or -1 if there is no next image file document</returns>
        public int NextPosition(int position)
        {
            return position >= 0 && position < this.Length - 1 ? position + 1 : DocumentCollection.NoPosition;
        }


        /// <summary>
        /// Get the position of the previous image file document before the supplied position.
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Position of the previous image file document, or -1 if there is no previous image file document</returns>
        public int PreviousPosition(int position)
        {
            return position >= 1 && position < this.Length ? position - 1 : DocumentCollection.NoPosition;
        }


        /// <summary>
        /// Get the position of the first image file document.
        /// </summary>
        /// <returns>Position of the first image file document, or -1 if there is no first image file document</returns>
        public int FirstPosition()
        {
            return this.Length > 0 ? 0 : DocumentCollection.NoPosition;
        }


        /// <summary>
        /// Get the position of the last image file document.
        /// </summary>
        /// <returns>Position of the last image file document, or -1 if there is no last image file document</returns>
        public int LastPosition()
        {
            return this.Length > 0 ? this.Length - 1 : DocumentCollection.NoPosition;
        }


        /// <summary>
        /// Get the position of a randomly-selected image file document.
        /// </summary>
        /// <param name="excludePosition">
        /// If specified, does not return this position.
        /// Use to prevent a current position from being returned.
        /// </param>
        /// <returns>Position of a randomly-selected image file document, or DocumentCollection.NoPosition if one could not be generated.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public int RandomPosition(
            int? excludePosition = null)
        {
            if (this.Length == 0)
                return DocumentCollection.NoPosition;

            var randomPosition = new System.Random().Next(0, this.Length - 1);
            if (excludePosition.HasValue && randomPosition >= excludePosition.Value)
                randomPosition += 1;  // Make sure we don't generate the excluded
            if (randomPosition == excludePosition)
                throw new InvalidOperationException();
            return randomPosition;
        }


        #region Implementaton

        private DocumentCollection(
            Android.Net.Uri dirUri,
            string[] documentIds)
        {
            if (dirUri == null)
                throw new ArgumentNullException(nameof(dirUri));
            if (documentIds == null)
                throw new ArgumentNullException(nameof(documentIds));

            this.dirUri = dirUri;
            this.documentFileIds = documentIds;
            this.documentFileUris = new Android.Net.Uri[documentIds.Length];  // Initially all nulls
        }

        private readonly Android.Net.Uri dirUri;

        private readonly string[] documentFileIds;
        private readonly Android.Net.Uri[] documentFileUris;


        private static DocumentCollection CreateImpl(
            Context context,
            Android.Net.Uri dirUri,
            IEnumerable<string> fileTypes = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (dirUri == null)
                throw new ArgumentNullException(nameof(dirUri));

            // Build a uri that refers to the _contents of_ our target dir.
            var dirChildrenUri = DocumentsContract.BuildChildDocumentsUriUsingTree(dirUri, DocumentsContract.GetTreeDocumentId(dirUri));

            // Query the contents of the target dir via the contents url. We want the document IDs to cache and use later.
            // Note that the file system provider that is used by our ContentResolver's Query() method seems† to ignore any selection
            // and order clauses that we pass to it, so we have to get everything and do filtering/ordering ourselves.
            // † By experiment, and also see https://stackoverflow.com/a/61214849/67316
            var projection = new string[]
            { 
                DocumentsContract.Document.ColumnMimeType,
                DocumentsContract.Document.ColumnDocumentId,
                DocumentsContract.Document.ColumnDisplayName
            };
            using (var cursor = context.ContentResolver.Query(dirChildrenUri, projection, null, null, null))
            {
                var documents = new List<(string documentId, string displayName)>();
                while (cursor.MoveToNext())
                {
                    if (cursor.GetString(0) == DocumentsContract.Document.MimeTypeDir)
                        continue;  // Ignore directories.
                    if (cursor.GetString(2).StartsWith('.'))
                        continue;  // Ignore hidden files.
                    if (fileTypes != null && !fileTypes.Any(s => cursor.GetString(2).EndsWith(s)))
                        continue;  // File types is specified and display name doesn't end with any of them.

                    // Match
                    documents.Add( (cursor.GetString(1), cursor.GetString(2)) );
                }

                // Sort our list of (document id, display name) tuples by display name so that the collection has
                // an expected ordering.
                // We use a case-insensitive sort by display name. Use StringComparer.Ordinal for case-sensitive.
                documents = documents.OrderBy(d => d.displayName, StringComparer.OrdinalIgnoreCase).ToList();

                // Instantiate a DocumentFileCollection object with document id values from the tuple list.
                // At this point we throw away the display names.
                return new DocumentCollection(dirUri, documents.Select(t => t.documentId).ToArray());
            }
        }


        private Android.Net.Uri GetOrCreateUriAt(
            Context context,
            int position)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (position < 0 || position >= documentFileUris.Length)
                throw new ArgumentException("Index out of range", nameof(position));

            // Instantiate the url of the image file document who's document id is at the supplied position.
            if (documentFileUris[position] == null)
            {
                var fileUri = DocumentsContract.BuildDocumentUriUsingTree(dirUri, documentFileIds[position]);
                documentFileUris[position] = fileUri;
            }

            return documentFileUris[position];
        }

        #endregion Implementaton
    }
}