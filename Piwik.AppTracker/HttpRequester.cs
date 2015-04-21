using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;

namespace Piwik.AppTracker
{
    /// <summary>
    /// Internal class used to send HTTP request
    /// </summary>
    class HttpRequester
    {
        public static string UserAgent = "HttpRequester";
        public static string AcceptLanguage = null;

        /// <summary>
        /// Send a HTTP request, and return the response as byte[]
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeout"></param>
        /// <param name="method"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] request(string url, int timeout = 30*1000, string method = "GET", string data = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.UserAgent = HttpRequester.UserAgent;

            if (!String.IsNullOrEmpty(HttpRequester.AcceptLanguage))
                request.Headers.Add("Accept-Language", HttpRequester.AcceptLanguage);

            request.Timeout = timeout;

            if (!string.IsNullOrEmpty(data))
            {
                request.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }
            }

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();

                return ParseResponse(response);
            }
            catch (WebException exception)
            {
                response = (HttpWebResponse)exception.Response;
                if (response == null)
                    throw;

                return ParseResponse(response);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        private static byte[] ParseResponse(HttpWebResponse response)
        {
            var statusCode = (int)response.StatusCode;
            if (!(200 <= statusCode && statusCode <= 299))
                throw new Exception(string.Format("Server return response code:{0}", statusCode));

            return ReadResponseContent(response);
        }

        /// <summary>
        /// Read the response content stream
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private static byte[] ReadResponseContent(HttpWebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                BinaryReader reader = null;
                try
                {
                    reader = new System.IO.BinaryReader(stream);
                    if ((int)response.ContentLength >= 0)
                    {
                        return reader.ReadBytes((int)response.ContentLength);
                    }
                    else
                    {
                        var mem = new MemoryStream();
                        var buf = new byte[4096];
                        int nCount;
                        do
                        {
                            nCount = reader.Read(buf, 0, buf.Length);
                            if (nCount > 0)
                                mem.Write(buf, 0, nCount);
                        } while (nCount > 0);

                        mem.Seek(0, SeekOrigin.Begin);
                        return mem.ToArray();
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
        }
    }
}
