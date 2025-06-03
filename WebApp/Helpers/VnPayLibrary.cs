using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace WebApp.Helpers
{
    public class VnPayLibrary
    {
        public const string VERSION = "2.1.0";

        private readonly Dictionary<string, string> _requestData = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _responseData = new Dictionary<string, string>();

        #region Add Data Methods

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData[key] = value;
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData[key] = value;
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }

        #endregion

        #region Create Request URL

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            var queryString = new StringBuilder();
            var rawData = new StringBuilder();

            var sortedData = _requestData
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .OrderBy(kv => kv.Key);

            foreach (var kv in sortedData)
            {
                queryString.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                rawData.Append(kv.Key + "=" + kv.Value + "&");
            }

            // Xoá dấu & cuối
            if (queryString.Length > 0) queryString.Length -= 1;
            if (rawData.Length > 0) rawData.Length -= 1;

            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, rawData.ToString());

            // Thêm SHA512 type vào URL (VNPay yêu cầu)
            return $"{baseUrl}?{queryString}&vnp_SecureHashType=SHA512&vnp_SecureHash={vnp_SecureHash}";
        }

        #endregion

        #region Validate Signature

        public bool ValidateSignature(string inputHash, string hashSecret)
        {
            var sorted = _responseData
                .Where(kvp => kvp.Key != "vnp_SecureHash" && kvp.Key != "vnp_SecureHashType")
                .OrderBy(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var rawData = string.Join("&", sorted.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            var computedHash = HmacSHA512(hashSecret, rawData);

            return string.Equals(computedHash, inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion

        #region Utility Methods

        public string GetRequestRaw()
        {
            var sortedData = _requestData
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .OrderBy(kv => kv.Key);

            return string.Join("&", sortedData.Select(kv =>
                WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value)));
        }

        private static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(inputBytes);
                foreach (var b in hashBytes)
                {
                    hash.Append(b.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        #endregion
    }
}
