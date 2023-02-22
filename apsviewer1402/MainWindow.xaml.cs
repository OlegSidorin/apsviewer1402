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
using Autodesk.Forge;
using Autodesk.Forge.Client;
using Autodesk.Forge.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Microsoft.WindowsAPICodePack.Dialogs;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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

        public MainWindow()
        {
            InitializeComponent();

            //TextBoxObjectKey.Text = FileObjectKey;
            TextBoxPath.Text = FilePath;    

            ButtonTask01.Click += ButtonTaskAuth_Click;
            ButtonTask02.Click += ButtonTaskGetBuckets_Click;
            ButtonTask03.Click += ButtonTaskCreateBucket_Click;
            ButtonTask04.Click += ButtonTaskDeleteBuckets_Click;
            ButtonTask05.Click += ButtonTaskGetBucketUrl_Click;
            ButtonTask051.Click += ButtonTask051_Click;
            ButtonTask06.Click += ButtonTaskUpload_Click;
        }

        private void ButtonTask051_Click(object sender, RoutedEventArgs e)
        {
            string folder = @"C:\Users\" + Environment.UserName + @"\Downloads";

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = folder;
            dialog.IsFolderPicker = false;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FilePath = dialog.FileName;
                string[] strings = dialog.FileName.Split('\\');
                ButtonTask051.Content = strings.LastOrDefault();
            }
        }

        private async void ButtonTaskAuth_Click(object sender, RoutedEventArgs e)
        {
            TextBoxBottomInfo.Text = "Привет!\n";

            try
            {
                TokenClientCredentials token = await ForgeLib.GetTokenClientCredentials(CLIENT_ID, CLIENT_SECRET);
                TextBoxBottomInfo.Text += "Токен ";
                if (!string.IsNullOrEmpty(token.access_token)) { Token = token.access_token; TextBoxBottomInfo.Text += $"получен\n"; }
                else TextBoxBottomInfo.Text += $"не получен\n";
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
        }

        private async void ButtonTaskGetBuckets_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxBottomInfo.Text += "Получить бакеты:\n";
                BucketList bucketList = await ForgeLib.BucketsGet(Token);
                if (bucketList != null && bucketList.items != null && bucketList.items.Count() > 0)
                {
                    foreach (BucketInList b in bucketList.items)
                    {
                        TextBoxBottomInfo.Text += b.bucketKey + "\n";
                    }
                    
                }

            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }

        }

        private async void ButtonTaskCreateBucket_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxBottomInfo.Text += "Создать бакет:\n";
                string bucketName = "mybucket_" + DateTime.Now.Day + DateTime.Now.Month + "_" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second;
                CreatedBucket bucket = await ForgeLib.BucketCreate(Token, bucketName);
                TextBoxBottomInfo.Text += bucket.bucketKey + "\n";
                Bucket = bucket;
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }
        }

        private async void ButtonTaskDeleteBuckets_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxBottomInfo.Text += "Удалить все бакеты:\n";
                BucketList bucketList = await ForgeLib.BucketsGet(Token);
                if (bucketList != null && bucketList.items != null && bucketList.items.Count() > 0)
                {
                    foreach (BucketInList b in bucketList.items)
                    {
                        await ForgeLib.BucketDelete(Token, b.bucketKey);
                    }

                }

            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }

        }

        private async void ButtonTaskGetBucketUrl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxBottomInfo.Text += "Выгрузка файла:\n";
                UploadKeyResponse upKey =  await ForgeLib.BucketGetSignedUrl(Token, Bucket.bucketKey, FilePath);
                await ForgeLib.BucketUploadFile(upKey.urls[0], FilePath);
                UploadedFileInBucket upFile = await ForgeLib.BucketFinaliizeUpload(Token, Bucket.bucketKey, upKey.uploadKey, FilePath);
                TextBoxBottomInfo.Text += $"Выгружен файл: {upFile.objectId}\n";
            }
            catch (Exception ex) { LoggerShow(MethodName()); LoggerShow(ex.ToString()); }


        }

        private async void ButtonTaskUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObjectList obList = await ForgeLib.ObjectsGetInBucket(Token, Bucket.bucketKey);
                if (obList != null && obList.items != null && obList.items.Count() > 0) 
                {
                    TextBoxBottomInfo.Text += "Файлы в бакете: " + "\n";
                    foreach (ObjectInList o in obList.items)
                    {
                        TextBoxBottomInfo.Text += o.objectKey + "\n";
                    }
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



        


        private async Task<bool> Translate2Svf(string urn, JobPayloadDestination.RegionEnum targetRegion = JobPayloadDestination.RegionEnum.US)
        {
            try
            {
                Console.WriteLine("**** Requesting SVF translation for: " + urn);
                JobPayloadInput jobInput = new JobPayloadInput(urn);
                JobPayloadOutput jobOutput = new JobPayloadOutput(
                    new List<JobPayloadItem>(
                        new JobPayloadItem[] {
                                new JobPayloadItem (
                                    JobPayloadItem.TypeEnum.Svf,
                                    new List<JobPayloadItem.ViewsEnum> (
                                        new JobPayloadItem.ViewsEnum [] {
                                            JobPayloadItem.ViewsEnum._2d, JobPayloadItem.ViewsEnum._3d
                                        }
                                    ),
                                    null
                                )
                        }
                    ),
                    new JobPayloadDestination(targetRegion)
                );
                JobPayload job = new JobPayload(jobInput, jobOutput);
                bool bForce = true;
                //ApiResponse<dynamic> response = await DerivativesAPI.TranslateAsyncWithHttpInfo(job, bForce);
                //httpErrorHandler(response, "Failed to register file for SVF translation");
                return (true);
            }
            catch (Exception)
            {
                Console.WriteLine("**** Failed to register file for SVF translation");
                return (false);
            }
        }

        private async Task<bool> Translate2Svf2(string urn, JobPayloadDestination.RegionEnum targetRegion = JobPayloadDestination.RegionEnum.US)
        {
            try
            {
                Console.WriteLine("**** Requesting SVF2 translation for: " + urn);
                JobPayloadInput jobInput = new JobPayloadInput(urn);
                JobPayloadOutput jobOutput = new JobPayloadOutput(
                    new List<JobPayloadItem>(
                        new JobPayloadItem[] {
                                new JobPayloadItem (
                                    JobPayloadItem.TypeEnum.Svf2,
                                    new List<JobPayloadItem.ViewsEnum> (
                                        new JobPayloadItem.ViewsEnum [] {
                                            JobPayloadItem.ViewsEnum._2d, JobPayloadItem.ViewsEnum._3d
                                        }
                                    ),
                                    null
                                )
                        }
                    ),
                    new JobPayloadDestination(targetRegion)
                );
                JobPayload job = new JobPayload(jobInput, jobOutput);
                bool bForce = true;
                //ApiResponse<dynamic> response = await DerivativesAPI.TranslateAsyncWithHttpInfo(job, bForce);
                //httpErrorHandler(response, "Failed to register file for SVF2 translation");
                return (true);
            }
            catch (Exception)
            {
                Console.WriteLine("**** Failed to register file for SVF2 translation");
                return (false);
            }
        }

        private async Task<bool> Translate2Obj(string urn, string guid, JobObjOutputPayloadAdvanced.UnitEnum unit = JobObjOutputPayloadAdvanced.UnitEnum.Meter, JobPayloadDestination.RegionEnum targetRegion = JobPayloadDestination.RegionEnum.US)
        {
            try
            {
                Console.WriteLine("**** Requesting OBJ translation for: " + urn);
                JobPayloadInput jobInput = new JobPayloadInput(urn);
                JobPayloadOutput jobOutput = new JobPayloadOutput(
                    new List<JobPayloadItem>(
                        new JobPayloadItem[] {
                                new JobPayloadItem (
                                    JobPayloadItem.TypeEnum.Obj,
                                    null,
									//new JobObjOutputPayloadAdvanced (null, guid, new List<int> () { -1 }, unit) // all
									new JobObjOutputPayloadAdvanced (null, guid, new List<int> () { 1526, 1527 }, unit)
                                )
                        }
                    ),
                    new JobPayloadDestination(targetRegion)
                );
                JobPayload job = new JobPayload(jobInput, jobOutput);
                bool bForce = true;
                //ApiResponse<dynamic> response = await DerivativesAPI.TranslateAsyncWithHttpInfo(job, bForce);
                //httpErrorHandler(response, "Failed to register file for OBJ translation");
                return (true);
            }
            catch (Exception)
            {
                Console.WriteLine("**** Failed to register file for OBJ translation");
                return (false);
            }
        }

        private async Task<bool> Translate2Stl(string urn, string guid, JobObjOutputPayloadAdvanced.UnitEnum unit = JobObjOutputPayloadAdvanced.UnitEnum.Meter, JobPayloadDestination.RegionEnum targetRegion = JobPayloadDestination.RegionEnum.US)
        {
            try
            {
                Console.WriteLine("**** Requesting STL translation for: " + urn);
                JobPayloadInput jobInput = new JobPayloadInput(urn);
                JobPayloadOutput jobOutput = new JobPayloadOutput(
                    new List<JobPayloadItem>(
                        new JobPayloadItem[] {
                                new JobPayloadItem (
                                    JobPayloadItem.TypeEnum.Stl,
                                    null,
                                    new JobStlOutputPayloadAdvanced (JobStlOutputPayloadAdvanced.FormatEnum.Ascii, true, JobStlOutputPayloadAdvanced.ExportFileStructureEnum.Single)
                                )
                        }
                    ),
                    new JobPayloadDestination(targetRegion)
                );
                JobPayload job = new JobPayload(jobInput, jobOutput);
                bool bForce = true;
                //ApiResponse<dynamic> response = await DerivativesAPI.TranslateAsyncWithHttpInfo(job, bForce);
                //httpErrorHandler(response, "Failed to register file for STL translation");
                return (true);
            }
            catch (Exception)
            {
                Console.WriteLine("**** Failed to register file for STL translation");
                return (false);
            }
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
