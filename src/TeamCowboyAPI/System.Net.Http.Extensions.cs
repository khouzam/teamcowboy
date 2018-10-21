/*************************************************************************************************
 * Copyright (c) 2018 MagikInfo
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software withou
 * restriction, including without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
/*************************************************************************************************/

namespace MagikInfo.TeamCowboy.Extensions
{
    using System.IO;
    using System.IO.Compression;

    public static class HttpNetExtensions
    {
        public static Stream GetResponseStream(this System.Net.Http.HttpResponseMessage response)
        {
            var task = response.Content.ReadAsStreamAsync();
            task.Wait();
            if (task.IsFaulted)
            {
                throw task.Exception;
            }
            else
            {
                Stream responseStream = task.Result;
                var encodings = response.Content.Headers.ContentEncoding;
                if (encodings != null)
                {
                    if (encodings.Contains("gzip"))
                    {
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                    }
                    else if (encodings.Contains("deflate"))
                    {
                        responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
                    }
                }
                return responseStream;
            }
        }
    }
}
