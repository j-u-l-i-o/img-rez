using System;
using System.Drawing;

namespace ApplicationResizer.Logic
{
    public class ResizeImageFromPath : ResizeImage
    {
        public string OriginalPath { get; set; }

        /// <summary>
        /// Initializes a new instance of the ResizeImageFromPath class.
        /// </summary>
        public ResizeImageFromPath(string originalPath)
        {
            OriginalPath = originalPath;
            OriginalImage = System.Drawing.Image.FromFile(OriginalPath);
        }
    }
}
