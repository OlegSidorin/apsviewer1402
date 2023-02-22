using Autodesk.Forge.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace apsviewer1402.Library
{
    public static class ForgeLib
    {
        public static string Token { get; set; }

        private static readonly char[] padding = { '=' };
        public static string SafeBase64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return (System.Convert.ToBase64String(plainTextBytes)
                .TrimEnd(padding).Replace('+', '-').Replace('/', '_')
            );
        }
        public static async Task<TokenClientCredentials> GetTokenClientCredentials(string client_id, string client_secret)
        {
            TokenClientCredentials tokenClientCredentials = new TokenClientCredentials();
            try
            {
                string url = $@"https:{""}//developer.api.autodesk.com/authentication/v2/token";
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{client_id}:{client_secret}");
                string authorizationstring = $"Basic {System.Convert.ToBase64String(plainTextBytes)}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", authorizationstring);
                request.Content = new FormUrlEncodedContent(new[]
                        {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("scope", "data:read data:write data:create data:search bucket:create bucket:read bucket:update bucket:delete")
                    });
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                HttpClient client = new HttpClient();
                var response = await client.SendAsync(request);
                LoggerShow("Аутентификация");
                LoggerShow($"{response.StatusCode}");
                foreach (var h in response.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                }
                foreach (var h in response.Content.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                }
                string output = response.Content.ReadAsStringAsync().Result;
                LoggerShow(output);
                tokenClientCredentials = JsonConvert.DeserializeObject<TokenClientCredentials>(output);

            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }

            return tokenClientCredentials;
        }

        public static async Task<CreatedBucket> BucketCreate(string token, string bucketName)
        {
            CreatedBucket bucket = new CreatedBucket();
            try
            {
                string url = $@"https:{""}//developer.api.autodesk.com/oss/v2/buckets";
                string authorizationstring = $"Bearer {token}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", authorizationstring);
                request.Headers.Add("x-ads-region", "US");
                BodyRequestCreateBucket bodyContent = new BodyRequestCreateBucket() { bucketKey = bucketName };
                request.Content = new System.Net.Http.StringContent(JsonConvert.SerializeObject(bodyContent), Encoding.UTF8, "application/json");
                //request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpClient client = new HttpClient();
                var response = await client.SendAsync(request);
                LoggerShow("Создать новый бакет");
                LoggerShow($"{response.StatusCode}");
                foreach (var h in response.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                }
                foreach (var h in response.Content.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                }
                string output = response.Content.ReadAsStringAsync().Result;
                LoggerShow(output);
                bucket = JsonConvert.DeserializeObject<CreatedBucket>(output);
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
            return bucket;
        }

        public static async Task<BucketList> BucketsGet(string token)
        {
            BucketList bucketList = new BucketList();
            //List<BucketInList> buckets = new List<BucketInList>();
            try
            {
                string url = $@"https:{""}//developer.api.autodesk.com/oss/v2/buckets";
                string authorizationstring = $"Bearer {token}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", authorizationstring);
                HttpClient client = new HttpClient();
                var response = await client.SendAsync(request);
                LoggerShow("Список бакетов");
                LoggerShow($"{response.StatusCode}");
                foreach (var h in response.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                }
                foreach (var h in response.Content.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                }
                string output = response.Content.ReadAsStringAsync().Result;
                LoggerShow(output);
                bucketList = JsonConvert.DeserializeObject<BucketList>(output);

            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
            return bucketList;
        }

        public static async Task BucketDelete(string token, string bucketKey)
        {
            try
            {
                string url = $@"https:{""}//developer.api.autodesk.com/oss/v2/buckets/{bucketKey}";
                string authorizationstring = $"Bearer {token}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
                request.Headers.Add("Authorization", authorizationstring);
                HttpClient client = new HttpClient();
                var response = await client.SendAsync(request);
                LoggerShow("Удаление бакета");
                LoggerShow($"{response.StatusCode}");
                foreach (var h in response.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                }
                foreach (var h in response.Content.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                }
                string output = response.Content.ReadAsStringAsync().Result;
                LoggerShow(output);

            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
        }

        public static async Task<UploadKeyResponse> BucketGetSignedUrl(string token, string bucketKey, string filePath)
        {
            UploadKeyResponse upKey = new UploadKeyResponse();
            try
            {
                string[] strings = filePath.Split('\\');
                string objectKey = strings.LastOrDefault();
                string url = $@"https:{""}//developer.api.autodesk.com/oss/v2/buckets/{bucketKey}/objects/{objectKey}/signeds3upload?minutesExpiration=10";
                string authorizationstring = $"Bearer {token}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", authorizationstring);
                //request.Headers.Add("x-ads-region", "US");
                //BodyRequestSignedUrl bodyContent = new BodyRequestSignedUrl() { ossbucketKey = bucketKey, ossSourceFileObjectKey = objectKey };
                //request.Content = new System.Net.Http.StringContent(JsonConvert.SerializeObject(bodyContent), Encoding.UTF8, "application/json");
                //request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpClient client = new HttpClient();
                var response = await client.SendAsync(request);
                LoggerShow("Получить Signed Url");
                LoggerShow($"{response.StatusCode}");
                foreach (var h in response.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                }
                foreach (var h in response.Content.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                }
                string output = response.Content.ReadAsStringAsync().Result;
                LoggerShow(output);
                upKey = JsonConvert.DeserializeObject<UploadKeyResponse>(output);
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
            return upKey;
        }

        public static async Task BucketUploadFile(string uploadUrl, string filePath)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
                byte[] bytes = System.IO.File.ReadAllBytes(filePath);

                using (var content = new ByteArrayContent(bytes))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("*/*");
                    HttpClient client = new HttpClient();
                    var response = await client.SendAsync(request);
                    LoggerShow("Выгрузка файла");
                    LoggerShow($"{response.StatusCode}");
                    response.EnsureSuccessStatusCode();
                    Stream responseStream = await response.Content.ReadAsStreamAsync();
                    StreamReader reader = new StreamReader(responseStream);
                    string rtend = reader.ReadToEnd();
                    LoggerShow("read to end: <" + rtend + ">");
                }
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
        }

        public static async Task<UploadedFileInBucket> BucketFinaliizeUpload(string token, string bucketKey, string uploadKey, string filePath)
        {
            UploadedFileInBucket upFile = new UploadedFileInBucket();
            try
            {
                string[] strings = filePath.Split('\\');
                string objectKey = strings.LastOrDefault();
                // https://developer.api.autodesk.com/oss/v2/buckets/<YOUR_BUCKET_KEY>/objects/<YOUR_OBJECT_KEY>/signeds3upload
                string url = $@"https:{""}//developer.api.autodesk.com/oss/v2/buckets/{bucketKey}/objects/{objectKey}/signeds3upload";
                string authorizationstring = $"Bearer {token}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", authorizationstring);
                //request.Headers.Add("x-ads-region", "US");
                BodyRequestFinalizeUpload bodyContent = new BodyRequestFinalizeUpload() { ossbucketKey = bucketKey, ossSourceFileObjectKey = objectKey, uploadKey = uploadKey };
                request.Content = new System.Net.Http.StringContent(JsonConvert.SerializeObject(bodyContent), Encoding.UTF8, "application/json");
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpClient client = new HttpClient();
                var response = await client.SendAsync(request);
                LoggerShow("Завершение выгрузки");
                LoggerShow($"{response.StatusCode}");
                foreach (var h in response.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                }
                foreach (var h in response.Content.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                }
                string output = response.Content.ReadAsStringAsync().Result;
                LoggerShow(output);
                upFile = JsonConvert.DeserializeObject<UploadedFileInBucket>(output);
                LoggerShow("выгружен: " + upFile.objectKey);
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
            return upFile;
        }

        public static async Task<ObjectList> ObjectsGetInBucket(string token, string bucketKey)
        {
            ObjectList objectList = new ObjectList();
            //List<BucketInList> buckets = new List<BucketInList>();
            try
            {
                string url = $@"https:{""}//developer.api.autodesk.com/oss/v2/buckets/{bucketKey}/objects";
                string authorizationstring = $"Bearer {token}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", authorizationstring);
                HttpClient client = new HttpClient();
                var response = await client.SendAsync(request);
                LoggerShow("Список файлов");
                LoggerShow($"{response.StatusCode}");
                foreach (var h in response.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                }
                foreach (var h in response.Content.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                }
                string output = response.Content.ReadAsStringAsync().Result;
                LoggerShow(output);
                objectList = JsonConvert.DeserializeObject<ObjectList>(output);

            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
            return objectList;
        }



        static void LoggerShow(string str) => Logger.Create.AndShow(str);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string MethodName()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }
    }

    public class TokenClientCredentials
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }
    public class CreatedBucket
    {
        public string bucketKey { get; set; }
        public string bucketOwner { get; set; }
        public long createdDate { get; set; }
        public BucketPermission[] permissions { get; set; }
        public string policyKey { get; set; }
    }
    public class BucketPermission
    {
        public string authId { get; set; }
        public string access { get; set; }
    }
    public class BodyRequestCreateBucket
    {
        public string bucketKey { get; set; }
        public string access { get; set; } = "full";
        public string policyKey { get; set; } = "transient";
    }
    public class BucketList
    {
        public BucketInList[] items { get; set; }
    }
    public class BucketInList
    {
        public string bucketKey { get; set; }
        public long createdDate { get; set; }
        public string policyKey { get; set; }
    }


    public class UploadKeyResponse
    {
        public string uploadKey { get; set; }
        public DateTime uploadExpiration { get; set; }
        public DateTime urlExpiration { get; set; }
        public string[] urls { get; set; }
    }


    public class BodyRequestSignedUrl
    {
        public string ossbucketKey { get; set; }
        public string ossSourceFileObjectKey { get; set; }
        public string access { get; set; } = "full";
        public string policyKey { get; set; } = "transient";
    }


    public class BodyRequestFinalizeUpload
    {
        public string ossbucketKey { get; set; }
        public string ossSourceFileObjectKey { get; set; }
        public string access { get; set; } = "full";
        public string uploadKey { get; set; }
    }


    public class UploadedFileInBucket
    {
        public string bucketKey { get; set; }
        public string objectId { get; set; }
        public string objectKey { get; set; }
        public int size { get; set; }
        public string contentType { get; set; }
        public string location { get; set; }
    }

    public class ObjectList
    {
        public ObjectInList[] items { get; set; }
    }
    public class ObjectInList
    {
        public string bucketKey { get; set; }
        public string objectKey { get; set; }
        public string objectId { get; set; }
        public string sha1 { get; set; }
        public int size { get; set; }
        public string location { get; set; }
    }

}
