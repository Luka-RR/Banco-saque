using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Banco_ASP_2 // Altere para o namespace do seu projeto
{
    public static class SessionExtensions
    {
        // Método para armazenar um objeto na sessão como JSON
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            var json = JsonConvert.SerializeObject(value); // Serializa o objeto para JSON
            session.SetString(key, json); // Armazena o JSON na sessão
        }

        // Método para obter um objeto da sessão, a partir de um JSON
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var json = session.GetString(key); // Obtém o JSON da sessão
            if (string.IsNullOrEmpty(json)) // Se não encontrar o JSON
                return default(T); // Retorna o valor padrão para o tipo T

            return JsonConvert.DeserializeObject<T>(json); // Desserializa o JSON de volta para o tipo T
        }
    }
}


