﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using Ionic.Zip;
#if DEBUG
using System.Windows.Forms;
#endif
namespace ZibomodInstaller
{
    class DriveAPI //API parser
    {
        public Dictionary<string,dynamic> GetDriveFolderList(string DriveFolderID)
        {
            Dictionary<string, dynamic> folderContentData = null; //Define data storage for parsed JSON content
            JavaScriptSerializer jsonParser = new JavaScriptSerializer(); 
            using (WebClient DriveClient = new WebClient()) //WebClient can be scrapped after the file has been downloaded into memory.
            {
                string jsonData = DriveClient.DownloadString("https://www.googleapis.com/drive/v2beta/files?openDrive=true&reason=102&syncType=0&errorRecovery=false&q=trashed = false and '" + DriveFolderID + "' in parents&fields=kind,nextPageToken,items(kind,title,mimeType,createdDate,modifiedDate,modifiedByMeDate,lastViewedByMeDate,fileSize,owners(kind,permissionId,displayName,picture),lastModifyingUser(kind,permissionId,displayName,picture),hasThumbnail,thumbnailVersion,iconLink,id,shared,sharedWithMeDate,userPermission(role),explicitlyTrashed,quotaBytesUsed,shareable,copyable,fileExtension,sharingUser(kind,permissionId,displayName,picture),spaces,editable,version,teamDriveId,hasAugmentedPermissions,trashingUser(kind,permissionId,displayName,picture),trashedDate,parents(id),labels(starred,hidden,trashed,restricted,viewed),capabilities(canCopy,canDownload,canEdit,canAddChildren,canDelete,canRemoveChildren,canShare,canTrash,canRename,canReadTeamDrive,canMoveTeamDriveItem)),incompleteSearch&appDataFilter=NO_APP_DATA&spaces=drive&maxResults=50&orderBy=folder,title_natural asc&key=AIzaSyCTF8x5DeVXllRTPMrtIwnY5DaBjbjKts8"); //Key at the end
                folderContentData = jsonParser.Deserialize<Dictionary<string, dynamic>>(jsonData); //Convert downloaded JSON data to data which is readable by C#
            }
            return folderContentData;
        }
        public void DownloadFile(string ID, string DownloadLocation)
        {
            using (WebClient DriveClient = new WebClient())
            {
                DriveClient.DownloadFile("https://drive.google.com/uc?id=" + ID + "&authuser=0&export=download",DownloadLocation);
            }
        }
    }
    class InstallActions
    {
        public static void ZiboPrepareDir(string xplaneDir)
        {
            DirectoryCopy(xplaneDir + @"Aircraft\Laminar Research\Boeing B737-800", xplaneDir + @"Aircraft\B737-800X", true);

        }
        public static void ZiboDownload()
        {
            DriveAPI ZiboDrive = new DriveAPI(); //Import the API parser
            Dictionary<string,dynamic> folderContentData = ZiboDrive.GetDriveFolderList("0B-tdl3VvPeOOYm12Wm80V04wdDQ"); //Get list of items in folder
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
                     potentialFiles.Add(i); //Add to list of zip files as ID, so that we don't have to look up later when we're downloading it
                }
            }
            for (int i = 0; i < potentialFiles.Count; i++)
            {
                potentialFilesAddedDate.Add(folderItemAddedDate[potentialFiles[i]]); //Add potentialfiles' last modified date so that they get the same ID.
            }
            string DownloadID = "";
            for (int i = 0; i < potentialFiles.Count; i++)
            {
                int NewestFile = potentialFiles[potentialFilesAddedDate.IndexOf(potentialFilesAddedDate.Max())]; //Find which file ID is the newest file.
                if (potentialFiles[i] == NewestFile) //If the current file is the newest one, then do:
                {
                    int selectedDownload;
                    selectedDownload = potentialFiles[i];
                    DownloadID = folderItemDriveID[selectedDownload]; //Select DriveID for downloading the file
                }
            }
            ZiboDrive.DownloadFile(DownloadID, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BoeingDL.zip"); //Downloads file to %Appdata%
        }
        public static void ZiboExtract(string xplaneDir)
        {
            using (ZipFile BoeingDL = ZipFile.Read(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BoeingDL.zip"))
            {
                BoeingDL.ExtractAll(xplaneDir + @"Aircraft\B737-800X", ExtractExistingFileAction.OverwriteSilently);
            }
        }
        public static void CleanUp()
        {
            File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BoeingDL.zip");
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
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
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
