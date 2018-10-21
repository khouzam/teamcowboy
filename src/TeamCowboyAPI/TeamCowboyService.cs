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

namespace MagikInfo.TeamCowboy
{
    using MagikInfo.TeamCowboy.Extensions;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;

    public partial class TeamCowboyService
    {
        /// <summary>
        /// The public API key to include in the requests
        /// </summary>
        private readonly string public_api_key;

        /// <summary>
        /// The private API key that is used to generate the signature
        /// </summary>
        private readonly string private_api_key;

        /// <summary>
        /// The Jan-1-1970 Date to use to convert to Unix Time
        /// </summary>
        private static readonly DateTime c_epoch = new DateTime(1970, 1, 1);

        /// <summary>
        /// A SHA generation class
        /// </summary>
        private readonly SHA1 sha1 = new SHA1Managed();

        /// <summary>
        /// The cached UserID once login has succeeded.
        /// </summary>
        private string _userID;

        /// <summary>
        /// The HttpClient used to issue the network requests
        /// </summary>
        private HttpClient _httpClient;

        /// <summary>
        /// An event and delegate that clients can listen to to know that
        /// operations are in progress.
        /// </summary>
        /// <param name="addOperation">
        /// boolean indicating if an opration is
        /// being added (started) or removed (completed)
        /// </param>
        public delegate void PendingOperationHandler(bool addOperation);
        public event PendingOperationHandler PendingOperationChanged;

        /// <summary>
        /// Create the TeamCowboy Service that will communicate with the
        /// TeamCowboy backend
        /// </summary>
        /// <param name="publicKey">The API Public Key</param>
        /// <param name="privateKey">The API Private Key</param>
        public TeamCowboyService(string publicKey, string privateKey)
        {
            public_api_key = publicKey;
            private_api_key = privateKey;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Invoked when a new network operation is in progress.
        /// </summary>
        internal void AddPendingOp()
        {
            PendingOperationChanged?.Invoke(true);
        }

        /// <summary>
        /// Invoked when a network operation has completed.
        /// </summary>
        internal void RemovePendingOp()
        {
            PendingOperationChanged?.Invoke(false);
        }

        /// <summary>
        /// Given a method, verb and collection of ValuePairs, genereate a QueryString
        /// for the request to TeamCowboy
        /// </summary>
        /// <param name="method">The Method that we need to call</param>
        /// <param name="verb">The verb that we need to call</param>
        /// <param name="values">
        /// An array of ValuePairs indicating the parameters passed to the request
        /// </param>
        /// <returns></returns>
        private string GenerateRequest(string method, HttpMethod verb, ValuePair[] values)
        {
            TimeSpan time = (DateTime.Now.ToUniversalTime() - c_epoch);
            string timeValue = Math.Floor(time.TotalSeconds).ToString();
            string nonce = Math.Floor(time.TotalMilliseconds).ToString();
            StringBuilder sigInput = new StringBuilder($"{private_api_key}|{verb}|{method}|{timeValue}|{nonce}|");
            StringBuilder requestString = new StringBuilder();

            // Get our value pairs
            var parameters = new List<ValuePair>()
            {
                new ValuePair("api_key", public_api_key),
                new ValuePair("method", method),
                new ValuePair("timestamp", timeValue),
                new ValuePair("nonce", nonce),
                new ValuePair("response_type", "json")
            };

            foreach (var pair in values)
            {
                parameters.Add(pair);
            }

            var sortedParams = new List<ValuePair>();
            foreach (var pair in parameters)
            {
                // Create a new list with all the parameters, lowercased and the values encoded
                var name = pair.Name.ToLower();
                var encodedValue = HttpUtility.UrlEncode(pair.Value.ToString()).Replace("+", "%20").Replace("!", "%21");
                var value = encodedValue.ToLower();
                sortedParams.Add(new ValuePair(name, value));

                // Add the actual ValuePair to the request string
                requestString.Append($"{pair.Name}={encodedValue}&");
            }

            // Sort the list
            sortedParams.Sort();

            // Add the value pairs to the signature generation string
            foreach (ValuePair item in sortedParams)
            {
                sigInput.Append($"{item.Name}={item.Value}&");
            }

            // Remove the last & from the string
            sigInput.Remove(sigInput.Length - 1, 1);

            // Compute the SHA of the signature generation string
            byte[] sha = sha1.ComputeHash(Encoding.UTF8.GetBytes(sigInput.ToString()));

            // Convert the SHA into a hex string
            StringBuilder shaSig = new StringBuilder(40);
            foreach (byte b in sha)
            {
                shaSig.Append(b.ToString("x2"));
            }

            // Add the signature to the request string
            requestString.Append($"sig={shaSig}");

            return requestString.ToString();
        }

        /// <summary>
        /// Common method to issue a TeamCowboy Request
        /// </summary>
        /// <param name="method">The method (API) to invoke</param>
        /// <param name="verb">The request verb (GET/POST)</param>
        /// <param name="parameters">
        /// An array of ValuePair that represents the parameters to the method
        /// </param>
        /// <param name="secure">boolean indicating if the request needs to be over SSL</param>
        /// <returns>A JObject as the response from the API call</returns>
        private async Task<JObject> TeamCowboyAPIAsync(
            string method,
            HttpMethod verb,
            ValuePair[] parameters,
            bool secure = false)
        {
            // Generate the proper data for the request
            string data = GenerateRequest(method, verb, parameters);

            Debug.WriteLine($"{method}, {data}");

            string uri = secure ? c_SecureServer : c_Server;

            // For GET requests, put the data as the query string
            if (verb != HttpMethod.Post)
            {
                uri += "?" + data;
            }

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(uri),
                Method = verb
            };

            // For post requests, put the data as the content
            if (verb == HttpMethod.Post && !string.IsNullOrEmpty(data))
            {
                request.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
            }

            // Send the request and get the response
            var response = (await _httpClient.SendAsync(request));//.EnsureSuccessStatusCode();

            // Convert the response to a JObject.
            return JObject.Parse(new StreamReader(response.GetResponseStream()).ReadToEnd());
        }
    }
}
