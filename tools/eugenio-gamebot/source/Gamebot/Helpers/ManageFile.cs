using Gamebot.Models;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Gamebot.Helpers
{
    internal class ManageFile
    {
        public static string SaveFile(string saveDir, string text, string fileName = "", bool newName = false, bool usingNameFile = false)
        {
            string folderToSaveFile = Path.Combine(Constants.PathProject(), saveDir);
            string extension = "txt";
            ManageFile.CreateDirectory(folderToSaveFile);
            if (!usingNameFile)
            {
                fileName = ((!newName) ? ManageFile.GetUniqueFilePath(Path.Combine(folderToSaveFile, fileName), true) : (Guid.NewGuid().ToString() + "." + extension));
            }
            File.WriteAllText(Path.Combine(folderToSaveFile, fileName), text);
            return fileName;
        }

        public static string SaveLogFile(string saveDir, string text, string fileName = "", bool newName = false, bool usingNameFile = false)
        {
            string text2;
            try
            {
                string folderToSaveFile = Path.Combine(Constants.PathProject(), saveDir);
                string extension = "txt";
                ManageFile.CreateDirectory(folderToSaveFile);
                if (!usingNameFile)
                {
                    fileName = ((!newName) ? ManageFile.GetUniqueFilePath(Path.Combine(folderToSaveFile, fileName), true) : (Guid.NewGuid().ToString() + "." + extension));
                }
                File.AppendAllText(Path.Combine(folderToSaveFile, fileName), text);
                text2 = fileName;
            }
            catch (Exception)
            {
                text2 = "";
            }
            return text2;
        }

        public static string ReadFile(string readDir, string fileName)
        {
            string text;
            try
            {
                text = File.ReadAllText(Path.Combine(Path.Combine(Constants.PathProject(), readDir), fileName));
            }
            catch (Exception)
            {
                Log.PrintInfo("READ FILE ERROR | NO FILE EXISTS");
                text = string.Empty;
            }
            return text;
        }

        public static string ReadFile(string fileName)
        {
            string text;
            try
            {
                text = File.ReadAllText(fileName);
            }
            catch (Exception)
            {
                Log.PrintInfo("READ FILE ERROR | NO FILE EXISTS");
                text = string.Empty;
            }
            return text;
        }

        private static string GetUniqueFilePath(string completeFilePath, bool getOnlyFilename = false)
        {
            if (File.Exists(completeFilePath))
            {
                string folder = Path.GetDirectoryName(completeFilePath);
                string filename = Path.GetFileNameWithoutExtension(completeFilePath);
                string extension = Path.GetExtension(completeFilePath);
                int number = 0;
                Match regex = Regex.Match(completeFilePath, "(.+)_\\((\\d+)\\)\\.\\w+");
                if (regex.Success)
                {
                    filename = regex.Groups[1].Value;
                    number = int.Parse(regex.Groups[2].Value);
                }
                do
                {
                    number++;
                    completeFilePath = Path.Combine(folder, string.Format("{0}_({1}){2}", filename, number, extension));
                }
                while (File.Exists(completeFilePath));
            }
            if (!getOnlyFilename)
            {
                return completeFilePath;
            }
            return Path.GetFileName(completeFilePath);
        }

        private static void CreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
