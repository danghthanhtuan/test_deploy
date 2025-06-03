using System.Security.Cryptography;
using System.Text;
using System.Net;

namespace WebApp.Helpers
{
    public class VnPayLibrary
    {
        public const string VERSION = "2.1.0";
        private Dictionary<string, string> _requestData = new Dictionary<string, string>();
        private Dictionary<string, string> _responseData = new Dictionary<string, string>();

        public void AddRequestData(string key, string value)
        {
            _requestData[key] = value;
        }

        public void AddResponseData(string key, string value)
        {
            _responseData[key] = value;
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder queryString = new StringBuilder();
            StringBuilder rawData = new StringBuilder();

            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    // ✅ URL encode key and value for queryString
                    queryString.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");

                    // ✅ Only encode key, leave value as-is for rawData
                    rawData.Append(WebUtility.UrlEncode(kv.Key) + "=" + kv.Value + "&");
                }
            }

            if (queryString.Length > 0) queryString.Length -= 1; // remove last '&'
            if (rawData.Length > 0) rawData.Length -= 1;         // remove last '&'

            string signData = rawData.ToString();
            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);

            return baseUrl + "?" + queryString.ToString() + "&vnp_SecureHash=" + vnp_SecureHash;
        }


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

        private static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }

        public string GetRequestRaw()
        {
            var sortedData = _requestData
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .OrderBy(kv => kv.Key);

            return string.Join("&", sortedData.Select(kv =>
                WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value)));
        }

    }
}
