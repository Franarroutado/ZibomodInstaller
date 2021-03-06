﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using Ionic.Zip;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
//using SevenZip;

namespace ZibomodInstaller
{
    class GDriveAPI //API parser
    {
        public string accessToken = null;
        public WebClient DriveClient = new WebClient();
        //DriveAPIWindow driveAPI = new DriveAPIWindow();
        //public void GetPrivateDriveAccessToken()
        //{
        //    accessToken = null;
        //    driveAPI.FormClosed += DriveAPI_FormClosed;
        //    driveAPI.Show();
        //}

        //private void DriveAPI_FormClosed(object sender, FormClosedEventArgs e)
        //{
        //    accessToken = Regex.Match(Convert.ToString(driveAPI.webBrowser1.Url), "code=(.*?)#").Groups[1].Value;
        //    string client_id = "483328243011-p256f3f7p8tlbfsl5reqj2b22rqvk2e5.apps.googleusercontent.com";
        //    if (accessToken == null)
        //    {

        //    } else
        //    {
        //        Uri uri = new Uri("https://www.googleapis.com/oauth2/v4/token?code=" + accessToken + "&client_id=483328243011-p07ia8t70iec45qm818sh5ucjkkg98m7.apps.googleusercontent.com&client_secret=NZag9WKbGjvZled9YX8kRKPi&redirect_uri=https://oauth2.example.com&scope=&grant_type=authorization_code");
        //        string getAccessToken = DriveClient.UploadString(uri, "");
        //        accessToken = Regex.Match(getAccessToken, "\"access_token\": \"(.*?)\",").Groups[1].Value;
        //        CopyToDrive("1qiNHMsbIj8-kMoPCf_JaVyglvPTaP8rQ", accessToken, GetOwnDriveID(accessToken));
        //    }
        //}

        public Dictionary<string,dynamic> GetDriveFolderList(string DriveFolderID)
        {
            Dictionary<string, dynamic> folderContentData = null; //Define data storage for parsed JSON content
            JavaScriptSerializer jsonParser = new JavaScriptSerializer(); 
            string jsonData = DriveClient.DownloadString("https://www.googleapis.com/drive/v2beta/files?openDrive=true&reason=102&syncType=0&errorRecovery=false&q=trashed = false and '" + DriveFolderID + "' in parents&fields=kind,nextPageToken,items(kind,title,mimeType,createdDate,modifiedDate,modifiedByMeDate,lastViewedByMeDate,fileSize,owners(kind,permissionId,displayName,picture),lastModifyingUser(kind,permissionId,displayName,picture),hasThumbnail,thumbnailVersion,iconLink,id,shared,sharedWithMeDate,userPermission(role),explicitlyTrashed,quotaBytesUsed,shareable,copyable,fileExtension,sharingUser(kind,permissionId,displayName,picture),spaces,editable,version,teamDriveId,hasAugmentedPermissions,trashingUser(kind,permissionId,displayName,picture),trashedDate,parents(id),labels(starred,hidden,trashed,restricted,viewed),capabilities(canCopy,canDownload,canEdit,canAddChildren,canDelete,canRemoveChildren,canShare,canTrash,canRename,canReadTeamDrive,canMoveTeamDriveItem)),incompleteSearch&appDataFilter=NO_APP_DATA&spaces=drive&maxResults=50&orderBy=folder,title_natural asc&key=AIzaSyCTF8x5DeVXllRTPMrtIwnY5DaBjbjKts8"); //Key at the end
            folderContentData = jsonParser.Deserialize<Dictionary<string, dynamic>>(jsonData); //Convert downloaded JSON data to data which is readable by C#
            return folderContentData;
        }
        public void DownloadFile(string ID, string DownloadLocation)
        {
            System.Uri Uri = new System.Uri("https://www.googleapis.com/drive/v3/files/" + ID + "?alt=media&key=AIzaSyCTF8x5DeVXllRTPMrtIwnY5DaBjbjKts8");
            DriveClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Downloader_DownloadProgressChanged);
            DriveClient.DownloadFileAsync(Uri, DownloadLocation);
        }
        public int downloadProgress;
        public void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            downloadProgress = e.ProgressPercentage;
        }
        public void DownloadFileWithAuth(string ID, string DownloadLocation, string AuthID)
        {
            System.Uri Uri = new System.Uri("https://www.googleapis.com/drive/v3/files/" + ID + "?alt=media&access_token=" + AuthID + "&key=AIzaSyCTF8x5DeVXllRTPMrtIwnY5DaBjbjKts8");
            DriveClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Downloader_DownloadProgressChanged);
            DriveClient.DownloadFile(Uri, DownloadLocation);
        }
        public void CopyToDrive(string ID, string AuthID, string folderID)
        {
            System.Uri Uri = new System.Uri("https://www.googleapis.com/drive/v2/files/" + ID + "/copy?access_token=" + AuthID + "&key=AIzaSyCTF8x5DeVXllRTPMrtIwnY5DaBjbjKts8");
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            Dictionary<string, dynamic> fileData = new Dictionary<string, dynamic>();
            fileData.Add("title", "B738-InstallerCopy.zip");
            fileData.Add("parents", new Dictionary<string,dynamic>());
            string[] ownDriveID = new string[1] { folderID };
            (fileData["parents"]).Add("id", ownDriveID);
            string jsondata = javaScriptSerializer.Serialize(fileData);
            string copy = DriveClient.UploadString(Uri, "{\"title\":\"ZiboModInstaller-quotabypass.zip\",\"parents\":[{\"id\":\"0AMUcLKx7YvgfUk9PVA\"}]}");
        }
        public string GetOwnDriveID(string AuthID)
        {
            Uri Uri = new Uri("https://www.googleapis.com/drive/v2/about?access_token=" + AuthID + "&key=AIzaSyCTF8x5DeVXllRTPMrtIwnY5DaBjbjKts8");
            string response = DriveClient.DownloadString(Uri);
            Dictionary<string, dynamic> jsonResponse = null; //Define data storage for parsed JSON content
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();
            jsonResponse = jsonParser.Deserialize<Dictionary<string, dynamic>>(response); //Convert downloaded JSON data to data which is readable by C#
            string DriveID = jsonResponse["rootFolderId"];
            return DriveID;
        }
    }
    //class OneDriveAPI
    //{
    //    public WebClient OneDriveClient = new WebClient();
    //    public Dictionary<string, dynamic> GetOneDriveDriveFolderList(string DriveFolderID)
    //    {
    //        string jsonData = OneDriveClient.DownloadString("https://skyapi.onedrive.live.com/API/2/GetItems?caller=&sb=0&ps=100&sd=0&gb=0&d=1&m=nb-NO&iabch=1&pi=5&path=1&lct=1&rset=odweb&v=0.8132011512416184&si=0&authKey=!AGh1XRe67b3h-u0&id=" + DriveFolderID);
    //        Dictionary<string, dynamic> FolderContentData = null;
    //        return FolderContentData;
    //        //TODO: Fix function
    //    }
    //}
    class LogFile
    {
        public static string LogLoc = InstallActions.AppData + "\\log.txt";
        public static void Write(string text)
        {
            File.AppendAllText(LogLoc, text);
        }
        public static void DeleteLog()
        {
            if (File.Exists(LogFile.LogLoc))
            {
                File.Delete(LogFile.LogLoc);
            }
        }
    }
    class InstallActions
    {
        public static string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ZiboModInstaller";

        //public static void GetZiboModChangelog () //TODO: This function should return string
        //{
        //    WebClient ZiboForum = new WebClient();
        //    string ZiboModChangelog = ZiboForum.DownloadString(@"https://forums.x-plane.org/index.php?/forums/topic/138974-b737-800x-zibo-mod-info-installation-download-links/");
        //    MatchCollection regex = Regex.(ZiboModChangelog, @"[^>]+(?![^<]*\>)");
        //    Console.WriteLine(Convert.ToString(regex));
        //}

        public static void InitConfig()
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-us");
            if (!Directory.Exists(AppData))
            {
                Directory.CreateDirectory(AppData);
            }
            Thread.Sleep(50);
            if (!File.Exists(AppData + "\\data.xml"))
            {
                File.WriteAllBytes(AppData + "\\data.xml", Properties.Resources.defaultconfig);
            }
            if (!File.Exists(AppData + "\\7z.dll"))
            {
                File.WriteAllBytes(AppData + "\\7z.dll", Properties.Resources._7z);
            }
            //SevenZipBase.SetLibraryPath(AppData + "\\7z.dll");
            if (File.Exists(AppData + "\\log.txt"))
            {
                File.Delete(AppData + "\\log.txt");
            }
        }
        public static void ResetConfig()
        {
            Directory.Delete(AppData, true);
            InitConfig();
        }
        public static void SaveConfig()
        {
            System.Xml.XmlDocument xmlConfigDoc = new System.Xml.XmlDocument();
            xmlConfigDoc.PreserveWhitespace = true;
            xmlConfigDoc.Load(AppData + "\\data.xml");
            xmlConfigDoc.SelectSingleNode("installer/configuration/xplanePath").InnerText = InstallOptionsPage._InstallOptionsPage.xplaneDirTextBox.Text;
            //xmlConfigDoc.SelectSingleNode("installer/configuration/audiobirdxp").InnerText = Convert.ToString(InstallOptionsPage._InstallOptionsPage.audioBirdCheck.Checked);
            //xmlConfigDoc.SelectSingleNode("installer/configuration/texturemod").InnerText = Convert.ToString(InstallOptionsPage._InstallOptionsPage.RGModCheckbox.Checked);
            xmlConfigDoc.SelectSingleNode("installer/data/ziboVer").InnerText = Convert.ToString(InstallOptionsPage.installedZibo);
            //xmlConfigDoc.SelectSingleNode("installer/data/fmodVer").InnerText = Convert.ToString(InstallOptionsPage.installedAudioB);
            xmlConfigDoc.SelectSingleNode("installer/configuration/textureres").InnerText = Convert.ToString(InstallOptionsPage._InstallOptionsPage.dropdownbox.SelectedIndex);
            //xmlConfigDoc.SelectSingleNode("installer/data/texturemodinstalled").InnerText = Convert.ToString(InstallOptionsPage.texturemodInstalled);
            xmlConfigDoc.Save(AppData + "\\data.xml");
        }
        public static void UpdateUserStatus(string text)
        {
            LogFile.Write("\n[UserInfo] " + DateTime.Now.TimeOfDay + " | " + text);
            InstallPage._InstallPage.CurrentAction.Text = text;
        }
        public static void AppendLogText(string text)
        {
            LogFile.Write("\n[DebugMsg] " + DateTime.Now.TimeOfDay + " | " + text);
        }

        public static void ZiboPrepareDir(string xplaneDir)
        {
            InstallOptionsPage.installedZibo = "";
            InstallOptionsPage.installedAudioB = "";
            DirectoryCopy(xplaneDir + @"Aircraft\Laminar Research\Boeing B737-800", xplaneDir + @"Aircraft\B737-800X", true);
        }
        public static void ZiboCleanDir(string xplaneDir)
        {
            if (Directory.Exists(xplaneDir + @"Aircraft\B737-800X\airfoils"))
            {
                Directory.Delete(xplaneDir + @"Aircraft\B737-800X\airfoils", true);
            }
            if (Directory.Exists(xplaneDir + @"Aircraft\B737-800X\cockpit"))
            {
                Directory.Delete(xplaneDir + @"Aircraft\B737-800X\cockpit", true);
            }
            if (Directory.Exists(xplaneDir + @"Aircraft\B737-800X\cockpit_3d"))
            {
                Directory.Delete(xplaneDir + @"Aircraft\B737-800X\cockpit_3d", true);
            }
            if (Directory.Exists(xplaneDir + @"Aircraft\B737-800X\objects"))
            {
                Directory.Delete(xplaneDir + @"Aircraft\B737-800X\objects", true);
            }
            if (Directory.Exists(xplaneDir + @"Aircraft\B737-800X\plugins"))
            {
                Directory.Delete(xplaneDir + @"Aircraft\B737-800X\plugins", true);
            }
            if (Directory.Exists(xplaneDir + @"Aircraft\B737-800X\sounds"))
            {
                Directory.Delete(xplaneDir + @"Aircraft\B737-800X\sounds", true);
            }
            if (File.Exists(xplaneDir + @"Aircraft\B737-800X\b738.acf"))
            {
                File.Delete(xplaneDir + @"Aircraft\B737-800X\b738.acf");
            }
            if (File.Exists(xplaneDir + @"Aircraft\B737-800X\B738X_apt.dat"))
            {
                File.Delete(xplaneDir + @"Aircraft\B737-800X\B738X_apt.dat");
            }
            if (File.Exists(xplaneDir + @"Aircraft\B737-800X\B738X_rnw.dat"))
            {
                File.Delete(xplaneDir + @"Aircraft\B737-800X\B738X_rnw.dat");
            }
            if (File.Exists(xplaneDir + @"Aircraft\B737-800X\B738X_rnw.dat"))
            {
                File.Delete(xplaneDir + @"Aircraft\B737-800X\B738X_rnw.dat");
            }
            if (File.Exists(xplaneDir + @"Aircraft\B737-800X\b738_cockpit.obj"))
            {
                File.Delete(xplaneDir + @"Aircraft\B737-800X\b738_cockpit.obj");
            }
        }
        public static string FindLatestGDriveFile(string FolderID, bool SearchZiboOnly)
        {
            string DownloadID = "";
            GDriveAPI ZiboDrive = new GDriveAPI(); //Import the API parser
            Dictionary<string,dynamic> folderContentData = ZiboDrive.GetDriveFolderList(FolderID); //Get list of items in folder
            List<string> folderItemName = new List<string>(); //Define lists for item properties
            List<double> folderItemAddedDate = new List<double>();
            List<string> folderItemDriveID = new List<string>();
            List<int> potentialFiles = new List<int>(); //Define list of zip candidates for installation
            List<double> potentialFilesAddedDate = new List<double>(); // Define list of zip candidates with date to find newest of the few.

            for (int i = 0; i < folderContentData["items"].Count; i++) //Add a global index of files and directories in drive folder, so that we can search in it.
            {
                folderItemName.Add(folderContentData["items"][i]["title"]); //Add title to list
                folderItemAddedDate.Add(Convert.ToDouble(Convert.ToDateTime(folderContentData["items"][i]["createdDate"]).Ticks)); //Add the file added date to list
                folderItemDriveID.Add(folderContentData["items"][i]["id"]);

                if (folderItemName[i].Contains(".zip"))
                {
                    if (!folderItemName[i].Contains("B738") &&!folderItemName[i].Contains("B737") && !folderItemName[i].Contains("Boeing") && SearchZiboOnly) //In case there are other files in zibo's Google Drive
                    {
                        
                    } else
                    {
                        potentialFiles.Add(i); //Add to list of zip files as ID, so that we don't have to look up later when we're downloading it
                    }
                }
            }
            for (int i = 0; i < potentialFiles.Count; i++)
            {
                potentialFilesAddedDate.Add(folderItemAddedDate[potentialFiles[i]]); //Add potentialfiles' last modified date so that they get the same ID.
            }
            int NewestFile = potentialFiles[potentialFilesAddedDate.IndexOf(potentialFilesAddedDate.Max())]; //Find which file ID is the newest file.
            DownloadID = folderItemDriveID[NewestFile];//Select DriveID for downloading the file
            return DownloadID;
        }
        //ZiboMod
        public static void ZiboDownload(string DownloadID)
        {
            GDriveAPI ZiboDrive = new GDriveAPI();
            ZiboDrive.DownloadFile(DownloadID, AppData + "\\BoeingDL.zip"); //Downloads file to %Appdata%. Operation is async!
            while (ZiboDrive.DriveClient.IsBusy)
            {
                InstallPage._InstallPage.UpdateProgressbar(ZiboDrive.downloadProgress);
                Thread.Sleep(2);
            }

        }
        public static void ZiboExtract(string xplaneDir)
        {
            try
            {
                using (Ionic.Zip.ZipFile BoeingDL = Ionic.Zip.ZipFile.Read(AppData + "\\BoeingDL.zip"))
                {
                    ZiboCleanDir(InstallPage.xplaneDir);
                    BoeingDL.ExtractAll(AppData + @"\ZiboDL", ExtractExistingFileAction.OverwriteSilently);
                }
                
            } catch (Exception ex)
            {
                UpdateUserStatus("Download quota for the file is exceeded.");
                AppendLogText("Download quota for the file is exceeded or another issue has caused Google to return a non-zip file.");
                Thread.Sleep(2000);
                throw ex;
            }

        }
        public static void ZiboInstall(string xplaneDir)
        {
            string acDirectory = FindACDir(AppData + @"\ZiboDL");
            if (acDirectory == null) //If FindACDir couldn't find the b737-800x directory, the files are probably in the root
            {
                acDirectory = AppData + @"\ZiboDL";
            }
            DirectoryCopy(acDirectory, xplaneDir + @"Aircraft\B737-800X\", true);
            if(InstallOptionsPage._InstallOptionsPage.dropdownbox.SelectedIndex == 0)
            {
                File.Copy(acDirectory + @"\ACF_2k_4k\b738.acf.4k", xplaneDir + @"Aircraft\B737-800X\b738.acf", true);
            } else
            {
                File.Copy(acDirectory + @"\ACF_2k_4k\b738.acf.2k", xplaneDir + @"Aircraft\B737-800X\b738.acf", true);
            }
        }
        ////AudioBird
        //public static void AudioDownload(string DownloadID)
        //{
        //    GDriveAPI AudioDrive = new GDriveAPI();
        //    AudioDrive.DownloadFile(DownloadID, AppData + "\\AXP-Immersion.zip"); //Downloads file to %Appdata%
        //    while (AudioDrive.DriveClient.IsBusy)
        //    {
        //        InstallPage._InstallPage.UpdateProgressbar(AudioDrive.downloadProgress);
        //        Thread.Sleep(2);
        //    }
        //}
        //public static void AudioExtract()
        //{
        //    //using (ZipFile AudioDL = ZipFile.Read(AppData + "\\AXP-Immersion.zip"))
        //    //{
        //    //    AudioDL.ExtractAll(AppData + @"\AudioDL", ExtractExistingFileAction.OverwriteSilently);
        //    //}
        //    using (SevenZipExtractor AudioDL = new SevenZipExtractor(AppData + "\\AXP-Immersion.zip"))
        //    {
        //        AudioDL.ExtractArchive(AppData + @"\AudioDL");
        //    }

        //}
        //public static void AudioInstall(string xplaneDir)
        //{
        //    if (!Directory.Exists(xplaneDir + @"Aircraft\B737-800X\fmod"))
        //    {
        //        Directory.CreateDirectory(xplaneDir + @"Aircraft\B737-800X\fmod");
        //    }
        //    string fmodDirectory = FindFMODDir(AppData + @"\AudioDL");

        //    DirectoryCopy(fmodDirectory, xplaneDir + @"Aircraft\B737-800X\fmod", true);
        //}
        //private static string FindFMODDir(string DirectoryToLookIn)
        //{
        //    string fmodDirectory = null;
        //    DirectoryInfo dir = new DirectoryInfo(DirectoryToLookIn);
        //    DirectoryInfo[] dirs = dir.GetDirectories();
        //    foreach (DirectoryInfo subdir in dirs)
        //    {
        //        if (subdir.FullName.Contains("fmod"))
        //        {
        //            fmodDirectory = subdir.FullName;
        //            break;
        //        }
        //        else
        //        {
        //            fmodDirectory = FindFMODDir(subdir.FullName);
        //        }
        //    }
        //    return fmodDirectory;
        //}
        private static string FindACDir(string DirectoryToLookIn)
        {
            string ACDir = null;
            DirectoryInfo dir = new DirectoryInfo(DirectoryToLookIn);
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                if (subdir.FullName.Contains("B737-800X"))
                {
                    ACDir = subdir.FullName;
                    break;
                }
                else
                {
                    ACDir = FindACDir(subdir.FullName);
                }
            }
            return ACDir;
        }
        //
        //Jamalje's improved textures
        //public static string FindLatestRG() //Attempt to find the newest RG Mod to extract Jamalje's textures from, however as RG mod is now payware, this function is mainly deprecated. Automatic fallback to the newest known free RG Mod
        //{
        //    using (WebClient VK = new WebClient())
        //    {
        //        string DownloadID = "";
        //        VK.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1");
        //        string VKGroup = VK.DownloadString(@"https://vk.com/xplane11rgmod");
        //        MatchCollection posts = Regex.Matches(VKGroup, "div id=\"post-.*?_(.*?)\" class=\"_post[\\s\\S\\n]*?title=\"https:\\/\\/drive\\.google\\.com\\/file\\/d\\/(.*?)\\/view");
        //        try
        //        {
        //            DownloadID = posts[1].Groups[2].Value;
        //        } catch (ArgumentOutOfRangeException)
        //        {
        //            DownloadID = "1aZPQMD4tI51XFbdmlgT-bPh6RqlgKwlF"; //Fallback to newest known free version.
        //        }
        //        return DownloadID;
        //    }
        //}
        //public static void TextureDownload(string ID)
        //{
        //    DriveAPI RGDrive = new DriveAPI();
        //    RGDrive.DownloadFile(ID, AppData + "\\RG-Mod.zip");
        //    while (RGDrive.DriveClient.IsBusy)
        //    {
        //        InstallPage._InstallPage.UpdateProgressbar(RGDrive.downloadProgress);
        //        Thread.Sleep(2);
        //    }
        //}
        //public static void TextureExtract(bool isTextureOnly, string xPlanePath)
        //{
        //    if (!isTextureOnly)
        //    {
        //        using (ZipFile RGMod = ZipFile.Read(AppData + "\\RG-Mod.zip"))
        //        {
        //            RGMod.ExtractAll(xPlanePath + "\\Aircraft\\B737-800X", ExtractExistingFileAction.OverwriteSilently);
        //        }
        //    } else
        //    {
        //        using (ZipFile RGMod = ZipFile.Read(AppData + "\\RG-Mod.zip"))
        //        {
        //            RGMod.ExtractSelectedEntries("name = *.dds", "objects", xPlanePath+"\\Aircraft\\B737-800X", ExtractExistingFileAction.OverwriteSilently);
        //        }
        //    }

        //}
        public static void CleanUp()
        {
            if(File.Exists(AppData + "\\BoeingDL.zip"))
            {
                File.Delete(AppData + "\\BoeingDL.zip");
            }
            if (File.Exists(AppData + "\\AXP-Immersion.zip"))
            {
                File.Delete(AppData + "\\AXP-Immersion.zip");
            }
            if (File.Exists(AppData + "\\RG-Mod.zip"))
            {
                File.Delete(AppData + "\\RG-Mod.zip");
            }
            if (Directory.Exists(AppData + "\\AudioDL"))
            {
                Directory.Delete(AppData + "\\AudioDL", true);
            }
            if (Directory.Exists(AppData + "\\ZiboDL"))
            {
                Directory.Delete(AppData + "\\ZiboDL", true);
            }
        }
        //
        //
        //
        //DirectoryCopy
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException();
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
