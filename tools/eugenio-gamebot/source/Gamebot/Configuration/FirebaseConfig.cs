namespace Gamebot.Configuration
{
    internal class FirebaseConfig
    {
        public static string PRIVATE_KEY { get; set; } = string.Empty;

        public static string PROJECT_ID { get; set; } = string.Empty;

        public static string ENVIRONMENT_GOOGLE_APPLICATION_CREDENTIALS { get; set; } = string.Empty;

        public static void SetConfig()
        {
            PRIVATE_KEY = "{\r\n              \"type\": \"service_account\",\r\n              \"project_id\": \"eugeniob-53da2\",\r\n              \"private_key_id\": \"dc33d38b989c0508bada73ab5cfa53d4dab5ef55\",\r\n              \"private_key\": \"-----BEGIN PRIVATE KEY-----\\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCd8NwIZgJCpJpQ\\nrneFfi63Yjmva6+8mx3+mDbZ3vp02AgF7WIzuHr9dqPoH7k6PcrwaIt0Y9XUyk8m\\nmB8sjpbUi2RbpHvbi68i6f8as23C/egLini2YjU0UhoIXqxHe8TnaJ0/h0emXrxH\\nBJxA7w/+lAacoNiD52bwIzT/g2Lk3YC58eDHvXK9m7w3rFt9oiZnRwenR2EK3NmP\\nYedF2V+jZhlw+9UkmwNp3YKf5MmBKrdX33KUAZdWtIE/Fm6I5BPtxuHKf3XOM3S+\\ngzDDexg2guUb/ta17y7enXl8lxVuJXekOlvi3+YR7Bj5JoZahYVIdjLr8bbhU4zO\\nZ0bftyUZAgMBAAECggEAB5r7aszIC2p+uHzg6i+MfgMqW51NY0ElnXFkpB19z6uU\\nHf5I2Mxni8CvSDRi3dWaP0phe2jP+Vpb8QlT6E2Iy30CMlGGL1PCIBBxhYPMuMje\\nPX6yV7dR8DjgjQpLXyPWyafleT4r6pcsQiU22v+dXw5vZRJvv/ugHtcrLFSo1J7B\\n6khh/YgG055GtJx6G65oZ8BleWiWZ3Vv1rBZLlV6TvapwY6i6oZzRo/wgBRtRn1m\\nUuIOjCkJH8zcVVf0zmycvbup9+PhcaxwbXQeE1EQNNYRBw5VTtm7zGaMcI0MvWUl\\nvgKIZwNE04SObxogoOB/pkal99u4ZC/aiu22rpd76wKBgQDMkLlprsN4QqQhpdac\\nhf1mjiHSz+29QDcZsDkDhFPZ/zjQaLzm0OdwVL4D2cUmCUiVF5LXUUiC7dbRqnT3\\nJcAht5G8S9Nnsk1u3bGuM1ZzJ4dsgrc0148hDsjh8O2o/8eklSKJ1JwiCWbO9gCE\\ntkrbIiVwE7F/O3S9bTYFsMsa0wKBgQDFpw2xaoAUqB2OFhzbR16L//axaoza4a9P\\nnncWBZTh9dAFPSh2hvdkp26OAmNKYPB3bk3C5aGnVKC6Jm7qqv3xEznH7rXlxky9\\nPojyN05Ps6j4T8iAeRgPzUmet3Cyhd+Qa4Gs29JK3xP7gLCKPyFaafHwg7jxlOV/\\nUZi9ova04wKBgBCT3eK5ne+d/u0XImg21407EXSjExIWJSD4zfYCAEWXUERbaIuP\\n/GF3DJsFR9vIPtCOQ4EdfjHWEmWVWbs5nPNei/nlbzFlGq9UWfTTBjt4xZm+khg/\\nK8MzDoZ43tmmOMiWqWDgEPMVLfaxquMWXsSZZM1LW0QDTneXZRxa8mqvAoGBAIY0\\nRrzndm4QNHkslI1jaVQgCY4vrdJVGSczjuJ6Cbxfns5f12CbhdFnSk7MWkS/i6dc\\nUxp5twiaEsQyom9eiSyuhDY35HRO7/4zPuNQIDFH6PwpmBd7oZXhKxLEtZXSBpnW\\nFWecnq5NyBCmMbi84tW0SB2qIuKN2kJX0iE5TNBHAoGALbc7CU5h8vdQDB77FKPg\\njlEa1aIeCE+BC/qwLL97PL+QRCZvHd/nLj/uQQ38ZjeMLGe3lDin2WjQX6Y0vMVC\\nuwzy3i+vdEGtNIWcQ4gNZvhbmKb5SadtD6MfiOVvrwrDN/Omj5uedhKzBgQ73F/I\\nFxhNXmOe1aT12wYPSmVrjE0=\\n-----END PRIVATE KEY-----\\n\",\r\n              \"client_email\": \"firebase-adminsdk-fbsvc@eugeniob-53da2.iam.gserviceaccount.com\",\r\n              \"client_id\": \"100217728081218318102\",\r\n              \"auth_uri\": \"https://accounts.google.com/o/oauth2/auth\",\r\n              \"token_uri\": \"https://oauth2.googleapis.com/token\",\r\n              \"auth_provider_x509_cert_url\": \"https://www.googleapis.com/oauth2/v1/certs\",\r\n              \"client_x509_cert_url\": \"https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-fbsvc%40eugeniob-53da2.iam.gserviceaccount.com\",\r\n              \"universe_domain\": \"googleapis.com\"\r\n            }";
            PROJECT_ID = "eugeniob-53da2";
            ENVIRONMENT_GOOGLE_APPLICATION_CREDENTIALS = "GOOGLE_APPLICATION_CREDENTIALS";
        }
    }
}
