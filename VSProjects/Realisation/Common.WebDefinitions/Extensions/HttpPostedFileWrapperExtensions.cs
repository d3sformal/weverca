using System.Web;

namespace Common.WebDefinitions.Extensions
{
    public static class HttpPostedFileWrapperExtensions
    {
        /// <summary>
        /// Return an array of bytes representing the file
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public static byte[] ToArray(this HttpPostedFileWrapper file)
        {
            byte[] buffer = new byte[file.InputStream.Length];
            file.InputStream.Read(buffer, 0, buffer.Length);

            return buffer;
        }
    }
}
