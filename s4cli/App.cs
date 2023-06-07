using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// App  
//
// All methods of application
//
// Copyright 2023 Jorge Alberto Ponce Turrubiates
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
namespace s4cli
{
    /// <summary>
    /// App class
    /// </summary>
    class App
    {
        /// <summary>
        /// AWS Key
        /// </summary>
        public String AWSKey { get; set; }

        /// <summary>
        /// AWS Secret
        /// </summary>
        public String AWSSecret { get; set; }

        /// <summary>
        /// AWS S3 bucket name
        /// </summary>
        public String Bucket { get; set; }

        /// <summary>
        /// AWS Region like: us-east-1
        /// </summary>
        public String AWSRegion { get; set; }

        /// <summary>
        /// Local directory to upload
        /// </summary>
        public String LocalDir { get; set; }

        /// <summary>
        /// Shows usage mode
        /// </summary>
        public void ShowUsage()
        {
            Console.WriteLine("How to use:\n");
            Console.WriteLine("For upload a file:");
            Console.WriteLine("s4cli.exe upload C:\\path\\file_upload.ext s3/upload/file_upload.ext AWS_KEY AWS_SECRET S3_BUCKET AWS_REGION\n");
            Console.WriteLine("For upload a directory:");
            Console.WriteLine("s4cli.exe sync C:\\path\\directory s3/upload_dir/ AWS_KEY AWS_SECRET S3_BUCKET AWS_REGION\n");
            Console.WriteLine("For download a file:");
            Console.WriteLine("s4cli.exe download C:\\path\\file_download.ext s3/download/file_download.ext AWS_KEY AWS_SECRET S3_BUCKET AWS_REGION");
        }

        /// <summary>
        /// Upload file to AWS S3
        /// </summary>
        /// <param name="FileName">Full name of file like: C:\myfile.ext</param>
        /// <param name="S3Prefix">bucket prefix like: uploads/myfile.ext</param>
        /// <returns>bool</returns>
        public bool Upload(string FileName, string S3Prefix)
        {
            if (File.Exists(FileName))
            {
                UploadFile(FileName, S3Prefix).Wait();
            }
            else
            {
                ShowMsg("ERROR", FileName + " does not exists");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Upload directory content
        /// </summary>
        /// <param name="DirectoryName">Full name of directory like: C:\dir_to_upload</param>
        /// <param name="S3Prefix">bucket prefix like: uploads/mydir/</param>
        /// <returns>bool</returns>
        public bool Sync(string DirectoryName, string S3Prefix)
        {
            if (!S3Prefix.EndsWith("/"))
                S3Prefix = S3Prefix + "/";

            if (Directory.Exists(DirectoryName))
            {
                ScanDir(DirectoryName, S3Prefix);
            }
            else
            {
                ShowMsg("ERROR", DirectoryName + " does not exists");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Download AWS S3 object as file
        /// </summary>
        /// <param name="FileName">Full name of file like: C:\my_downloaded_file.ext</param>
        /// <param name="S3Prefix">bucket prefix like: uploads/myfile.ext</param>
        /// <returns>bool</returns>
        public bool Download(string FileName, string S3Prefix)
        {
            if (File.Exists(FileName))
            {
                ShowMsg("ERROR", FileName + " already exists");

                return false;
            }
            else
            {
                DownloadFile(FileName, S3Prefix).Wait();
            }

            return true;
        }

        /// <summary>
        /// Scan directory and upload files
        /// NOTE: If file name contains white spaces will replace with the character _
        /// </summary>
        /// <param name="Path">Full path of directory like: C:\dir_to_sync</param>
        /// <param name="S3Prefix">bucket prefix like: uploaded_dir/</param>
        public void ScanDir(String Path, string S3Prefix)
        {
            String S3Path = "";

            if (Directory.Exists(Path))
            {
                foreach (var item in Directory.GetFiles(Path))
                {
                    S3Path = item.ToString().Replace(this.LocalDir, "");
                    //S3Path = S3Path.Replace(" ", "_");
                    S3Path = S3Path.Replace("\\", "/");

                    if (S3Path.StartsWith("/"))
                        S3Path = S3Path.Remove(0, 1);

                    S3Path = S3Prefix + S3Path;

                    Upload(item, S3Path);
                }

                foreach (var item in Directory.GetDirectories(Path))
                {
                    ScanDir(item, S3Prefix);
                }
            }
        }

        /// <summary>
        /// Prepare task for upload file to AWS S3
        /// </summary>
        /// <param name="FileName">Full name of file like: C:\myfile.ext</param>
        /// <param name="S3Prefix">bucket prefix like: uploads/myfile.ext</param>
        /// <returns>Task</returns>
        private async Task UploadFile(string FileName, string S3Prefix)
        {
            AmazonS3Client s3Client = new AmazonS3Client(this.AWSKey, this.AWSSecret, RegionEndpoint.GetBySystemName(this.AWSRegion));

            try
            {
                ShowMsg("INFO", "Uploading " + FileName + " -> " + S3Prefix);

                var fileTransferUtility = new TransferUtility(s3Client);

                await fileTransferUtility.UploadAsync(FileName, Bucket, S3Prefix);
            }
            catch (AmazonS3Exception e)
            {
                ShowMsg("ERROR", e.Message);
            }
            catch (Exception e)
            {
                ShowMsg("ERROR", e.Message);
            }

            s3Client = null;
        }

        /// <summary>
        /// Prepare task for download file to AWS S3
        /// </summary>
        /// <param name="FileName">Full name of file like: C:\my_downloaded_file.ext</param>
        /// <param name="S3Prefix">bucket prefix like: uploads/myfile.ext</param>
        /// <returns>Task</returns>
        private async Task DownloadFile(string FileName, string S3Prefix)
        {
            AmazonS3Client s3Client = new AmazonS3Client(this.AWSKey, this.AWSSecret, RegionEndpoint.GetBySystemName(this.AWSRegion));

            try
            {
                ShowMsg("INFO", "Downloading " + S3Prefix + " -> " + FileName);

                var fileTransferUtility = new TransferUtility(s3Client);

                await fileTransferUtility.DownloadAsync(FileName, Bucket, S3Prefix);
            }
            catch (AmazonS3Exception e)
            {
                ShowMsg("ERROR", e.Message);
            }
            catch (Exception e)
            {
                ShowMsg("ERROR", e.Message);
            }

            s3Client = null;
        }

        /// <summary>
        /// Show message in console
        /// </summary>
        /// <param name="Type">Message type like: ERROR, INFO, etc</param>
        /// <param name="Msg">Message to show</param>
        public void ShowMsg(string Type, string Msg)
        {
            String Message = Type + " " + Msg;

            Console.WriteLine(Message);
        }
    }
}
