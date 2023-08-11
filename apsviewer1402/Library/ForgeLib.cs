using MS.WindowsAPICodePack.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

        // Получение токена
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


        // Создание удвление бакетов

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


        // Выгрузка файлов

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
                //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
                byte[] bytes = System.IO.File.ReadAllBytes(filePath);

                //using (var content = new ByteArrayContent(bytes))
                //{
                //    content.Headers.ContentType = new MediaTypeHeaderValue("*/*");
                //    HttpClient client = new HttpClient();
                //    var response = await client.SendAsync(request);
                //    LoggerShow("Выгрузка файла");
                //    LoggerShow($"{response.StatusCode}");
                //    response.EnsureSuccessStatusCode();
                //    Stream responseStream = await response.Content.ReadAsStreamAsync();
                //    StreamReader reader = new StreamReader(responseStream);
                //    string rtend = reader.ReadToEnd();
                //    LoggerShow("read to end: <" + rtend + ">");
                //}

                var wc = new WebClient();
                var response = wc.UploadData(uploadUrl, "PUT", bytes);
                LoggerShow("---- uploading file response ------");
                var str = System.Text.Encoding.UTF8.GetString(response);
                LoggerShow(str);
                LoggerShow("---- end of uploading file response ------");
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

        // Список файлов в бакете
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


        // Конверт файлов в бакете

        public static async Task<TranslationJobResult> StartTranslationJob(string token, string objectId)
        {
            //string[] strings = filePath.Split('\\');
            //string fileName = strings.LastOrDefault();
            TranslationJobResult jobResult = new TranslationJobResult();
            string urn = SafeBase64Encode(objectId);
            try
            {
                LoggerShow("Translation:");
                LoggerShow("urn: ");
                LoggerShow($"{urn}");
                // https://developer.api.autodesk.com/modelderivative/v2/designdata/job
                // https://developer.api.autodesk.com/modelderivative/v2/designdata/job
                string url = $@"https:{""}//developer.api.autodesk.com/modelderivative/v2/designdata/job";
                string authorizationstring = $"Bearer {token}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", authorizationstring);
                //request.Headers.Add("x-ads-region", "US");
                request.Headers.Add("x-ads-force", "true");
                TranslationJob_Body bodyContent = new TranslationJob_Body()
                {
                    input = new TranslationJob_Body_Input()
                    {
                        urn = urn,
                        rootFilename = null,
                        compressedUrn = null
                    },
                    output = new TranslationJob_Body_Output()
                    {
                        destination = new TranslationJob_Body_Output_Destination()
                        {
                            region = "us"
                        },
                        formats = new List<TranslationJob_Body_Output_Format>()
                        {
                            new TranslationJob_Body_Output_Format()
                            {
                                type = "svf",
                                views = new List<string>()
                                {
                                    "2d", "3d"
                                }
                            }
                        }
                    }
                };
                string bodyContentString = JsonConvert.SerializeObject(bodyContent, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                LoggerShow(bodyContentString);
                request.Content = new System.Net.Http.StringContent(bodyContentString, Encoding.UTF8, "application/json");
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpClient client = new HttpClient();
                var response = await client.SendAsync(request);
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
                jobResult = JsonConvert.DeserializeObject<TranslationJobResult>(output);
                LoggerShow("принят в работу?: " + jobResult.result);
                string str = "";
                if (jobResult != null && jobResult.acceptedJobs != null && jobResult.acceptedJobs.output != null && jobResult.acceptedJobs.output.formats != null && jobResult.acceptedJobs.output.formats.Count() > 0)
                {
                    foreach (var t in jobResult.acceptedJobs.output.formats)
                    {
                        str += t.type + ", ";
                    }
                }
                else
                {
                    str = "ошибка";
                }
                str = str.Trim().Trim(',');
                LoggerShow("в формат: " + str + "\n");
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
            return jobResult;
        }

        public static async Task<GetManifestResultOutput> GetJobManifest(string token, string urn)
        {
            GetManifestResultOutput manifestResultOutput = new GetManifestResultOutput();
            //string urn = SafeBase64Encode(objectId);
            try
            {
                LoggerShow("Job Manifest:");
                // https://developer.api.autodesk.com/modelderivative/v2/designdata/<URL_SAFE_URN_OF_SOURCE_FILE>/manifest
                string url = $@"https:{""}//developer.api.autodesk.com/modelderivative/v2/designdata/{urn}/manifest";
                string authorizationstring = $"Bearer {token}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", authorizationstring);
                HttpClient client = new HttpClient();
                var response = await client.SendAsync(request);
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
                manifestResultOutput = JsonConvert.DeserializeObject<GetManifestResultOutput>(output);
                LoggerShow("прогресс: " + manifestResultOutput.progress);
                if (manifestResultOutput.progress.Equals("complete"))
                {
                    if (manifestResultOutput.derivatives != null)
                    {
                        foreach (var derivative in manifestResultOutput.derivatives)
                        {
                            LoggerShow($">{derivative.name}");
                            if (derivative.children != null)
                            {
                                foreach (var child in derivative.children)
                                {
                                    LoggerShow($"-->{child.name} : {child.mime} : {child.urn}");
                                    if (child.children != null)
                                    {
                                        foreach (var child2 in child.children)
                                        {
                                            LoggerShow($"---->{child2.name} : {child2.mime} : {child2.urn}");
                                        }
                                    }
                                }
                            }

                        }
                    }

                }

            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
            return manifestResultOutput;
        }

        public static async Task<DownloadLink> ObtainSignedCookieAndDownloadLink(string token, string urn, string urn2)
        {
            DownloadSVFLink downloadSVFLink = new DownloadSVFLink();
            var cookies = new List<string>();
            List<CloudFrontCookie> CFCookies = new List<CloudFrontCookie>();
            DownloadLink downloadLink = new DownloadLink();

            try
            {
                LoggerShow("Get Signed Cookie: \n");
                // https://developer.api.autodesk.com/modelderivative/v2/designdata/<URL_SAFE_URN_OF_SOURCE_FILE>/manifest/<URN_OF_OBJ_FILE>/signedcookies
                string url = $@"https:{""}//developer.api.autodesk.com/modelderivative/v2/designdata/{urn}/manifest/{urn2}/signedcookies";
                string authorizationstring = $"Bearer {token}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", authorizationstring);
                HttpClient client = new HttpClient();
                var response = await client.SendAsync(request);
                LoggerShow($"{response.StatusCode}");
                foreach (var h in response.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                    if (h.Key.ToString().Equals("Set-Cookie"))
                    {
                        foreach (string s in h.Value)
                        {
                            cookies.Add(s);
                        }

                    }
                }
                foreach (var h in response.Content.Headers)
                {
                    LoggerShow(h.Key.ToString() + ": " + string.Join(", ", h.Value));
                    if (h.Key.ToString().Equals("Set-Cookie"))
                    {
                        foreach (string s in h.Value)
                        {
                            cookies.Add(s);
                        }
                    }
                }
                string output = response.Content.ReadAsStringAsync().Result;
                LoggerShow(output);
                downloadSVFLink = JsonConvert.DeserializeObject<DownloadSVFLink>(output);
                LoggerShow("url:\n" + downloadSVFLink.url);

                downloadLink.Cookies = GetCloudFrontCookie(cookies);
                downloadLink.Link = downloadSVFLink;

                LoggerShow("--- cookies start ---");
                foreach (var c in CFCookies)
                {
                    LoggerShow(c.Name + ": " + c.Value);
                }
                LoggerShow("--- cookies end ---");
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
            return downloadLink;
        }

        private static List<CloudFrontCookie> GetCloudFrontCookie(List<string> cookies)
        {
            var output = new List<CloudFrontCookie>();
            foreach (var coo in cookies)
            {
                var cookie = new CloudFrontCookie();
                cookie.Name = coo.Split(';').FirstOrDefault().Split('=').FirstOrDefault();
                cookie.Value = coo.Split(';').FirstOrDefault().Split('=').LastOrDefault();
                output.Add(cookie);
            }
            return output;
        }

        public static async Task DownloadSVF(string urn, List<CloudFrontCookie> cookies)
        {
            try
            {
                var clientHandler = new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    UseCookies = true,
                    CookieContainer = new CookieContainer()
                };
                foreach (var coo in cookies)
                {
                    clientHandler.CookieContainer.Add(new Cookie() { Name = coo.Name, Value = coo.Value, Path = "/", Domain = "cdn.derivative.autodesk.com", HttpOnly = true });
                }
                using (var httpClient = new HttpClient(clientHandler))
                {
                    var fileResponse = httpClient.GetAsync(urn);

                    string fileName = urn.Split('/').LastOrDefault();

                    if (fileResponse.Result.IsSuccessStatusCode)
                    {
                        HttpContent content = fileResponse.Result.Content;
                        var contentStream = await content.ReadAsStreamAsync();
                        string dirPath = $@"C:\Users\{Environment.UserName}\Downloads\SvfTest";
                        if (!Directory.Exists(dirPath))
                            Directory.CreateDirectory(dirPath);
                        using (var fileStream = File.Create($@"{dirPath}\{fileName}"))
                        {
                            contentStream.CopyTo(fileStream);
                        }
                    }
                }
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
        }

        public static ManifestFiles GetNanifestFiles(string pathFile)
        {
            ManifestFiles manifestFiles = new ManifestFiles();
            try
            {
                string manifestJson = File.ReadAllText(pathFile);
                manifestFiles = JsonConvert.DeserializeObject<ManifestFiles>(manifestJson);
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
            return manifestFiles;
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

    public class TranslationJob_Body
    {
        public TranslationJob_Body_Input input { get; set; }
        public TranslationJob_Body_Output output { get; set; }
    }

    public class TranslationJob_Body_Output_Destination
    {
        public string region { get; set; }
    }

    public class TranslationJob_Body_Output_Format
    {
        public string type { get; set; }
        public List<string> views { get; set; }
    }

    public class TranslationJob_Body_Input
    {
        public string urn { get; set; }
        public string rootFilename { get; set; }
        public bool? compressedUrn { get; set; }
    }

    public class TranslationJob_Body_Output
    {
        public TranslationJob_Body_Output_Destination destination { get; set; }
        public List<TranslationJob_Body_Output_Format> formats { get; set; }
    }


    public class TranslationJobResult
    {
        public string result { get; set; }
        public string urn { get; set; }
        public TranslationJobResult_Acceptedjobs acceptedJobs { get; set; }
    }

    public class TranslationJobResult_Acceptedjobs
    {
        public TranslationJobResult_Acceptedjobs_Output output { get; set; }
    }

    public class TranslationJobResult_Acceptedjobs_Output
    {
        public TranslationJobResult_Acceptedjobs_Output_Destination destination { get; set; }
        public TranslationJobResult_Acceptedjobs_Output_Format[] formats { get; set; }
    }

    public class TranslationJobResult_Acceptedjobs_Output_Destination
    {
        public string region { get; set; }
    }

    public class TranslationJobResult_Acceptedjobs_Output_Format
    {
        public string type { get; set; }
        public List<string> views { get; set; }
    }


    ///////////
    ///

    public class GetManifestResultOutput
    {
        public string urn { get; set; }
        public Manifest_Derivative[] derivatives { get; set; }
        public string hasThumbnail { get; set; }
        public string progress { get; set; }
        public string type { get; set; }
        public string region { get; set; }
        public string version { get; set; }
        public string status { get; set; }
    }

    public class Manifest_Derivative
    {
        public string hasThumbnail { get; set; }
        public Manifest_Derivative_Child[] children { get; set; }
        public string name { get; set; }
        public string progress { get; set; }
        public string outputType { get; set; }
        //public Manifest_Derivative_Properties properties { get; set; }
        public string status { get; set; }
    }

    public class Manifest_Derivative_Properties
    {
        public Manifest_Derivative_Properties_DocumentInformation DocumentInformation { get; set; }
    }

    public class Manifest_Derivative_Properties_DocumentInformation
    {
        public string NavisworksFileCreator { get; set; }
        public int[] nwModelToWorldTransform { get; set; }
    }

    public class Manifest_Derivative_Child
    {
        public string guid { get; set; }
        public string type { get; set; }
        public string role { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string viewableID { get; set; } //
        public string hasThumbnail { get; set; }
        public float[] camera { get; set; } // from 2
        public string progress { get; set; }
        public bool useAsDefault { get; set; }
        public Manifest_Derivative_Child[] children { get; set; }
        public string urn { get; set; }
        public string mime { get; set; }
    }

    public class Manifest_Derivative_Child_Child
    {
        public string guid { get; set; }
        public string type { get; set; }
        public string role { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string viewableID { get; set; } // from 1
        public string hasThumbnail { get; set; }
        public float[] camera { get; set; } // 
        public bool useAsDefault { get; set; }
        public Manifest_Derivative_Child_Child_Child[] children { get; set; }
        public string progress { get; set; }
        public string urn { get; set; }
        public string mime { get; set; }
    }

    public class Manifest_Derivative_Child_Child_Child
    {
        public string urn { get; set; }
        public string role { get; set; }
        public string mime { get; set; }
        public string guid { get; set; }
        public string type { get; set; }
        public int[] resolution { get; set; }
        public string name { get; set; }
        public float[] camera { get; set; }
        public string status { get; set; }
    }


    public class DownloadSVFLink
    {
        public string etag { get; set; }
        public int size { get; set; }
        public string url { get; set; }
        public string contenttype { get; set; }
        public long expiration { get; set; }
    }

    public class CloudFrontCookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class DownloadLink
    {
        public List<CloudFrontCookie> Cookies { get; set; }
        public DownloadSVFLink Link { get; set; }
    }


    public class ManifestFiles
    {
        public string name { get; set; }
        public string toolkitversion { get; set; }
        public int manifestversion { get; set; }
        public Adskid adskID { get; set; }
        public Asset[] assets { get; set; }
        public Typeset[] typesets { get; set; }
    }

    public class Adskid
    {
        public string sourceSystem { get; set; }
        public string type { get; set; }
        public string id { get; set; }
        public string version { get; set; }
    }

    public class Asset
    {
        public string id { get; set; }
        public string type { get; set; }
        public string URI { get; set; }
        public int size { get; set; }
        public int usize { get; set; }
        public string typeset { get; set; }
    }

    public class Typeset
    {
        public string id { get; set; }
        public Type[] types { get; set; }
    }

    public class Type
    {
        public string _class { get; set; }
        public string type { get; set; }
        public int version { get; set; }
    }


}
