using Gamebot.Configuration;
using Google.Cloud.Firestore;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Gamebot.Communication.Firebase
{

    public static class FirestoreHelpers
    {
        private static string filepath = "";
        public static FirestoreDb Database { get; private set; }

        public static void SetEnvironmentVariable()
        {
            FirebaseConfig.SetConfig();
            FirestoreHelpers.filepath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName())) + ".json";
            File.WriteAllText(FirestoreHelpers.filepath, FirebaseConfig.PRIVATE_KEY);
            File.SetAttributes(FirestoreHelpers.filepath, FileAttributes.Hidden);
            Environment.SetEnvironmentVariable(FirebaseConfig.ENVIRONMENT_GOOGLE_APPLICATION_CREDENTIALS, FirestoreHelpers.filepath);
            FirestoreHelpers.Database = FirestoreDb.Create(FirebaseConfig.PROJECT_ID, null);
            File.Delete(FirestoreHelpers.filepath);
        }

        public static async Task<bool> CheckConnectionEnabled()
        {
            string nameDocument = FirebaseStructureDBSettings.DocumentConfigurationDevelopment;
            nameDocument = FirebaseStructureDBSettings.DocumentConfigurationProduction;
            DocumentSnapshot snap = await FirestoreHelpers.Database.Collection(FirebaseStructureDBSettings.CollectionName).Document(nameDocument).GetSnapshotAsync(default(CancellationToken));
            bool flag;
            if (snap.Exists)
            {
                flag = snap.ConvertTo<FirebaseStructureDBSettings>().enable_bot;
            }
            else
            {
                flag = false;
            }
            return flag;
        }
    }
}
