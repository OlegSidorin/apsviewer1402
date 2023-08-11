using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using apsviewer1402.Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Microsoft.WindowsAPICodePack.Dialogs;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Xml.Linq;

namespace apsviewer1402
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string FilePath { get; set; } = @"C:\Users\o.sidorin\Downloads\box.ipt";
        //string FileObjectKey { get; set; } = @"box.ipt";

        private static string CLIENT_ID { get; set; } = "wrOO1INOPHqNdkiYiZEmtHafXpXfbnLc";
        private static string CLIENT_SECRET { get; set; } = "o7qQnsMkSSXR9A6t";
        private string Token { get; set; } = string.Empty;

        private static CreatedBucket Bucket { get; set; }
        private static ObjectInList ObjectInList { get; set; }
        private static UploadedFileInBucket UploadedFile { get; set; }
        private static TranslationJobResult JobResult { get; set; }
        private static GetManifestResultOutput ManifestResult { get; set; }
        public static List<Manifest_Derivative_Child> Files { get; set; }
        public static string DownloadFolderUrn { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            //TextBoxObjectKey.Text = FileObjectKey;
            TextBoxPath.Text = FilePath;    

            ButtonTask01.Click += ButtonTaskAuth_Click;
            ButtonTask02.Click += ButtonTaskGetBuckets_Click;
            ButtonTask03.Click += ButtonTaskCreateBucket_Click;
            ButtonTask04.Click += ButtonTaskDeleteBuckets_Click;
            ButtonFileName.Click += ButtonTaskOpenFileClick;
            ButtonTask06.Click += ButtonTaskGetFiles_Click;
            ButtonTaskTranslate.Click += ButtonTaskTranslateFile_Click;
            ButtonTask08.Click += ButtonTaskJobManifest_Click;
            ButtonTaskDownload.Click += ButtonTaskDownloadSVF_Click;
            ButtonTaskGetAssets.Click += ButtonTaskGetAssets_Click;

            ContentRendered += (sender, args) =>
            {
                TextBoxInfo.CaretIndex = TextBoxInfo.Text.Length;
                TextBoxInfo.ScrollToEnd(); // not necessary for single line texts
                TextBoxInfo.Focus();
            };
        }

        private async void ButtonTaskGetAssets_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxInfo.Text += $"URNFolder {DownloadFolderUrn}\n";
                string filePath = $@"C:\Users\o.sidorin\Downloads\SvfTest\manifest.json";
                ManifestFiles manifestFiles = ForgeLib.GetNanifestFiles(filePath);
                foreach (var asset in manifestFiles.assets)
                {
                    string fileName = asset.URI;
                    if (!fileName.StartsWith("embed:"))
                    {
                        TextBoxInfo.Text += $"Скачивание файла {asset.URI}\n";
                        DownloadLink dLink = await ForgeLib.ObtainSignedCookieAndDownloadLink(Token, ManifestResult.urn, DownloadFolderUrn + fileName);
                        await ForgeLib.DownloadSVF(dLink.Link.url, dLink.Cookies);
                    }
                }
                TextBoxInfo.Text += "Файлы загружены в папку Загрузки/SvfTest\n";
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
        }

        private async void ButtonTaskDownloadSVF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var file in Files)
                {
                    TextBoxInfo.Text += "Скачивание файла " + file.urn.Split('/').LastOrDefault() + "\n";
                    if (file.urn.Split('/').LastOrDefault().EndsWith(".svf") )
                        DownloadFolderUrn = file.urn.Replace(file.urn.Split('/').LastOrDefault(), "");
                    DownloadLink dLink = await ForgeLib.ObtainSignedCookieAndDownloadLink(Token, ManifestResult.urn, file.urn); 
                    await ForgeLib.DownloadSVF(dLink.Link.url, dLink.Cookies);
                }
                TextBoxInfo.Text += "Файлы загружены в папку Загрузки/SvfTest\n";
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
        }

        private async void ButtonTaskJobManifest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxInfo.Text += "Проверить статус:\n";
                GetManifestResultOutput result = await ForgeLib.GetJobManifest(Token, JobResult.urn);
                ManifestResult = result;
                TextBoxInfo.Text += ManifestResult.progress + "\n";
                if (result.progress.Equals("complete"))
                {
                    Files = new List<Manifest_Derivative_Child>();
                    if (result.derivatives != null)
                    {
                        foreach (var derivative in result.derivatives)
                        {
                            if (derivative.children != null)
                            {
                                foreach (var child in derivative.children)
                                {
                                    if (child.mime != null)
                                    {
                                        if (child.mime.Equals("application/autodesk-svf") || child.mime.Equals("application/autodesk-db"))
                                        {
                                            Files.Add(child);
                                        }
                                    }
                                    if (child.children != null)
                                    {
                                        foreach (var child2 in child.children)
                                        {
                                            if (child2.mime != null)
                                            {
                                                if (child2.mime.Equals("application/autodesk-svf") || child2.mime.Equals("application/autodesk-db"))
                                                {
                                                    Files.Add(child2);
                                                }
                                            }
                                            
                                        }
                                    }
                                }
                            }
                        }
                    }
                    TextBoxInfo.Text += "Файлы для скачивания: \n";
                    foreach (var f in Files)
                    {
                        string name = f.urn.Split('/').LastOrDefault();
                        TextBoxInfo.Text += name + "\n";
                    }

                }
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
        }

        private async void ButtonTaskTranslateFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxInfo.Text += "Преобразование в svf:\n";
                TranslationJobResult jobResult = await ForgeLib.StartTranslationJob(Token, UploadedFile.objectId);
                TextBoxInfo.Text += "принята в работу: " + jobResult.result;
                string str = "";
                if (jobResult.acceptedJobs != null && jobResult.acceptedJobs.output.formats != null && jobResult.acceptedJobs.output.formats.Count() > 0)
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
                TextBoxInfo.Text += " в формат: " + str + "\n";
                JobResult = jobResult;
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
        }

        private async void ButtonTaskOpenFileClick(object sender, RoutedEventArgs e)
        {
            string folder = @"C:\Users\" + Environment.UserName + @"\Downloads";

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = folder;
            dialog.IsFolderPicker = false;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FilePath = dialog.FileName;
                string[] strings = dialog.FileName.Split('\\');
                ButtonFileName.Content = strings.LastOrDefault();

                try
                {
                    TextBoxInfo.Text += "Выгрузка файла:\n";
                    UploadKeyResponse upKey = await ForgeLib.BucketGetSignedUrl(Token, Bucket.bucketKey, FilePath);
                    await ForgeLib.BucketUploadFile(upKey.urls[0], FilePath);
                    UploadedFileInBucket upFile = await ForgeLib.BucketFinaliizeUpload(Token, Bucket.bucketKey, upKey.uploadKey, FilePath);
                    UploadedFile = upFile;
                    TextBoxInfo.Text += $"Выгружен файл: {UploadedFile.objectId}\n";
                }
                catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }

            }

        }

        private async void ButtonTaskAuth_Click(object sender, RoutedEventArgs e)
        {
            TextBoxInfo.Text = "Привет!\n";

            try
            {
                TokenClientCredentials token = await ForgeLib.GetTokenClientCredentials(CLIENT_ID, CLIENT_SECRET);
                TextBoxInfo.Text += "Токен ";
                if (!string.IsNullOrEmpty(token.access_token)) { Token = token.access_token; TextBoxInfo.Text += $"получен\n"; }
                else TextBoxInfo.Text += $"не получен\n";
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
        }

        private async void ButtonTaskGetBuckets_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxInfo.Text += "Получить бакеты:\n";
                BucketList bucketList = await ForgeLib.BucketsGet(Token);
                if (bucketList != null && bucketList.items != null && bucketList.items.Count() > 0)
                {
                    foreach (BucketInList b in bucketList.items)
                    {
                        TextBoxInfo.Text += b.bucketKey + "\n";
                    }
                    
                }
                else
                {
                    TextBoxInfo.Text += "бакетов нет\n";
                }

            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }

        }

        private async void ButtonTaskCreateBucket_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxInfo.Text += "Создать бакет:\n";
                string bucketName = "mybucket_" + DateTime.Now.Day + DateTime.Now.Month + "_" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second;
                CreatedBucket bucket = await ForgeLib.BucketCreate(Token, bucketName);
                TextBoxInfo.Text += bucket.bucketKey + "\n";
                Bucket = bucket;
                TextBoxBucket.Text = Bucket.bucketKey;
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
        }

        private async void ButtonTaskDeleteBuckets_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxInfo.Text += "Удалить все бакеты:\n";
                BucketList bucketList = await ForgeLib.BucketsGet(Token);
                if (bucketList != null && bucketList.items != null && bucketList.items.Count() > 0)
                {
                    foreach (BucketInList b in bucketList.items)
                    {
                        await ForgeLib.BucketDelete(Token, b.bucketKey);
                    }

                }
                else
                {
                    TextBoxInfo.Text += "все бакеты удалены\n";
                }

            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }

        }


        private async void ButtonTaskGetFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string bucketName = Bucket.bucketKey; // TextBoxBucket.Text;
                ObjectList obList = await ForgeLib.ObjectsGetInBucket(Token, bucketName);
                if (obList != null && obList.items != null && obList.items.Count() > 0) 
                {
                    TextBoxInfo.Text += $"Файлы в бакете ({bucketName}): " + "\n";
                    foreach (ObjectInList o in obList.items)
                    {
                        TextBoxInfo.Text += o.objectKey + "\n";
                    }
                    ObjectInList = obList.items.FirstOrDefault();
                    //TextBoxObjectName.Text = obList.items.FirstOrDefault().objectKey;
                    //TextBoxObjectId.Text = obList.items.FirstOrDefault().objectId;
                }
                else
                {
                    TextBoxInfo.Text += $"В бакете ({bucketName}) пусто " + "\n";
                }

            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }


        }

        private readonly char[] padding = { '=' };
        public string SafeBase64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return (System.Convert.ToBase64String(plainTextBytes)
                .TrimEnd(padding).Replace('+', '-').Replace('/', '_')
            );
        }

        public string SafeBase64Decode(string base64EncodedData)
        {
            string st = base64EncodedData.Replace('_', '/').Replace('-', '+');
            switch (base64EncodedData.Length % 4)
            {
                case 2:
                    st += "==";
                    break;
                case 3:
                    st += "=";
                    break;
            }
            var base64EncodedBytes = System.Convert.FromBase64String(st);
            return (System.Text.Encoding.UTF8.GetString(base64EncodedBytes));
        }

        public string BuildURN(string bucketKey, string objectKey)
        {
            return (SafeBase64Encode($"urn:adsk.objects:os.object:{bucketKey}/{objectKey}"));
        }


        void LoggerShow(string str) => Logger.Create.AndShow(str);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string MethodName()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }
    }
}
