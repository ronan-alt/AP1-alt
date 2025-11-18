using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AP1.Services
{
    public class Apis : IDisposable
    {
        #region Attributs
        // Un seul HttpClient pour toute la classe
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _jsonSettings;
        #endregion

        #region Ctor
        public Apis()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(Constantes.BaseApiAddress, UriKind.Absolute),
                Timeout = TimeSpan.FromSeconds(30)
            };

            _jsonSettings = new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.DateTime,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Converters = { new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" } }
            };
        }

        public void Dispose() => _httpClient?.Dispose();
        #endregion

        #region Méthodes publiques (signatures conservées)

        // GET -> Collection observable (pour compat UI)
        public async Task<ObservableCollection<T>> GetAllAsync<T>(string url, CancellationToken ct = default)
        {
            try
            {
                using var resp = await _httpClient.GetAsync(url, ct);
                await EnsureSuccess(resp, url);

                var json = await resp.Content.ReadAsStringAsync(ct);
                var list = JsonConvert.DeserializeObject<List<T>>(json, _jsonSettings) ?? new List<T>();

                // NB: idéalement, renvoyer List<T> ici, et construire l'ObservableCollection dans l'UI
                return new ObservableCollection<T>(list);
            }
            catch (Exception ex)
            {
                // Log minimal (à remplacer par ton logger)
                System.Diagnostics.Debug.WriteLine($"[GetAllAsync] {url} -> {ex.Message}");
                throw; // on propage pour que l'appelant décide
            }
        }

        // POST -> bool (succès/échec)
        public async Task<bool> PostOneAsync<T>(string endpoint, T requestDataObj, CancellationToken ct = default)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(requestDataObj, _jsonSettings);
                using var content = new StringContent(payload, Encoding.UTF8, "application/json");

                using var resp = await _httpClient.PostAsync(endpoint, content, ct);
                // si tu veux remonter l’erreur, remplace par: await EnsureSuccess(resp, endpoint, payload);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PostOneAsync] {endpoint} -> {ex.Message}");
                return false; // tu avais ce contrat, on le conserve
            }
        }

        // POST -> objet (mais signature ambiguë : T = requête ET réponse)
        public async Task<T?> GetOneAsync<T>(string endpoint, T requestDataObj, CancellationToken ct = default)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(requestDataObj, _jsonSettings);
                using var content = new StringContent(payload, Encoding.UTF8, "application/json");

                using var resp = await _httpClient.PostAsync(endpoint, content, ct);
                if (!resp.IsSuccessStatusCode) return default;

                var json = await resp.Content.ReadAsStringAsync(ct);
                var result = JsonConvert.DeserializeObject<T>(json, _jsonSettings);
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetOneAsync] {endpoint} -> {ex.Message}");
                return default;
            }
        }

        #endregion

        #region Helpers privés

        private static async Task EnsureSuccess(HttpResponseMessage response, string path, string payload = "")
        {
            if (response.IsSuccessStatusCode) return;

            var body = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
            var msg = $"API error {(int)response.StatusCode} {response.ReasonPhrase} on '{path}'. " +
                      (payload == null ? "" : $"Payload: {Trim(payload)} ") +
                      (string.IsNullOrWhiteSpace(body) ? "" : $"Body: {Trim(body)}");
            throw new HttpRequestException(msg);
        }

        private static string Trim(string s, int max = 600)
            => s.Length <= max ? s : s.Substring(0, max) + "…";

        #endregion
    }
}


