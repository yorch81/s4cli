using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// s4cli  
//
// Simple AWS S3 console client
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
    /// Main class
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main method of application
        /// 
        /// How to use:
        /// 
        /// For upload a file:
        /// s4cli.exe upload C:\path\file_upload.ext s3/upload/file_upload.ext AWS_KEY AWS_SECRET S3_BUCKET AWS_REGION
        /// 
        /// For upload a directory:
        /// s4cli.exe sync C:\path\directory s3/upload_dir/ AWS_KEY AWS_SECRET S3_BUCKET AWS_REGION
        /// 
        /// For download a file:
        /// s4cli.exe download C:\path\file_download.ext s3/download/file_download.ext AWS_KEY AWS_SECRET S3_BUCKET AWS_REGION
        /// </summary>
        /// <param name="args">Arguments of application</param>
        static void Main(string[] args)
        {
            string action = "";
            string file = "";
            string prefix = "";
            string awskey = "";
            string awssecret = "";
            string bucket = "";
            string region = "us-east-1";

            Console.WriteLine("---------------------------- Simple AWS S3 Client ----------------------------\n");

            App app = new App();

            if (args.Length == 0)
            {
                app.ShowMsg("ERROR", "Incorrect parameters number");
                app.ShowUsage();
            }
            else
            {
                if (args.Length > 7)
                {
                    app.ShowMsg("ERROR", "Incorrect parameters number");
                    app.ShowUsage();
                }
                else
                {
                    action = args[0];
                    file = args[1];
                    prefix = args[2];
                    awskey = args[3];
                    awssecret = args[4];
                    bucket = args[5];

                    if (args.Length == 7)
                        region = args[6];

                    app.AWSKey = awskey;
                    app.AWSSecret = awssecret;
                    app.AWSRegion = region;
                    app.Bucket = bucket;

                    switch (action)
                    {
                        case "upload":
                            app.Upload(file, prefix);
                            break;
                        case "sync":
                            app.LocalDir = file;
                            app.Sync(file, prefix);
                            break;
                        case "download":
                            app.Download(file, prefix);
                            break;
                        default:
                            app.ShowMsg("ERROR", "Not supported action");
                            app.ShowUsage();
                            break;
                    }
                }
            }
        }
    }
}
