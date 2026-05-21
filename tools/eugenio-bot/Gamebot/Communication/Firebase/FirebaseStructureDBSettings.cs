using Google.Cloud.Firestore;

namespace Gamebot.Communication.Firebase
{
    [FirestoreData]
    public class FirebaseStructureDBSettings
    {
        public static string CollectionName = "settings";

        public static string DocumentConfigurationDevelopment = "config_dev";

        public static string DocumentConfigurationProduction = "config_prod";

        [FirestoreProperty]
        public bool enable_bot { get; set; }
    }
}
