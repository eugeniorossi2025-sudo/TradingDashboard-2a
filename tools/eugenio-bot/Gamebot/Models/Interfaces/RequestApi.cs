using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Gamebot.Models.Interfaces
{
    public class RequestApi : IRequestApi
    {
        public async Task<ExternalResponse<Tout>> PostAsync<Tin, Tout>(string uri, Tin objectData, Dictionary<string, string> attribute = null, string token = "") where Tin : class where Tout : class
        {
            _ = 1;
            try
            {
                HttpClient client = new HttpClient();
                try
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    _SetApiKeyHeader(client);
                    StringContent serialized = new StringContent(JsonSerializer.Serialize(objectData), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(uri, (HttpContent)(object)serialized);
                    try
                    {
                        return JsonSerializer.Deserialize<ExternalResponse<Tout>>(await response.Content.ReadAsStringAsync());
                    }
                    finally
                    {
                        ((IDisposable)response)?.Dispose();
                    }
                }
                finally
                {
                    ((IDisposable)client)?.Dispose();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> GetAsync<Tout>(string baseUrl, Dictionary<string, string> queryParameters = null, string token = "")  
        {
            // *** ⚠️ NOTA: Qui si ripresenta il problema della creazione di un nuovo HttpClient ad ogni chiamata. ***
            using HttpClient client = new HttpClient();

            // 1. Costruisci l'URL completo con i parametri della Query String
            string urlCompleto = baseUrl;
            if (queryParameters != null && queryParameters.Count > 0)
            {
                var query = new StringBuilder("?");
                foreach (var param in queryParameters)
                {
                    // Aggiungi il parametro e la codifica (importante!)
                    query.Append($"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}&");
                }
                urlCompleto += query.ToString().TrimEnd('&');
            }

            try
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _SetApiKeyHeader(client);

                // 2. Invia la richiesta GET e attendi
                HttpResponseMessage response = await client.GetAsync(urlCompleto);
                response.EnsureSuccessStatusCode(); // Lancia un'eccezione se non è 2xx

                // 3. Leggi la risposta e deserializza l'esito
                //return  await response.Content.ReadAsStringAsync();

                string content = await response.Content.ReadAsStringAsync(); // Ottieni l'HTML
                
                if (int.TryParse(content.Trim(), out int numero))
                    return numero.ToString();
                
                return ExtractTextFromHtml(content); // Restituisci il testo pulito
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void _SetApiKeyHeader(HttpClient httpClient)
        {
        }

        private string ExtractTextFromHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }

            // 1. Carica la stringa HTML in un oggetto documento di HtmlAgilityPack
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            // 2. Estrai il testo. Puoi usare l'XPath "//body" per focalizzarti sul corpo,
            // o semplicemente selezionare la radice del documento.
            var bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");

            // Se il body esiste, usa GetInnerText() per estrarre tutto il testo
            // e Normalizza il risultato (rimuovendo spazi multipli e ritorni a capo)
            if (bodyNode != null)
            {
                string rawText = bodyNode.InnerText;
                // Rimuovi spazi extra e ritorni a capo per normalizzare il testo
                return System.Text.RegularExpressions.Regex.Replace(rawText, @"\s+", " ").Trim();
            }

            // Fallback, se non trova il body, usa l'intero documento
            return System.Text.RegularExpressions.Regex.Replace(htmlDoc.DocumentNode.InnerText, @"\s+", " ").Trim();
        }
    }
}
