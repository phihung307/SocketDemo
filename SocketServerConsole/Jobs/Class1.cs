//public static void GetData()
//{
//    if (string.IsNullOrEmpty(bearerToken))
//    {
//        bearerToken = GetAccessToken();
//    }
//    const string url = "https://vapi.vnappmob.com/api/v2/exchange_rate/vcb";
//    HttpClient client = new HttpClient();
//    client.BaseAddress = new Uri(url);
//    client.DefaultRequestHeaders.Accept.Clear();
//    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
//    var responseMessage = client.GetAsync(url).Result;
//    if (responseMessage.IsSuccessStatusCode)
//    {
//        var data = responseMessage.Content.ReadAsStringAsync().Result;
//        var responseJOject = JsonConvert.DeserializeObject<JObject>(data);
//    }
//    else
//    {
//        if (responseMessage.StatusCode == HttpStatusCode.Unauthorized
//            || responseMessage.StatusCode == HttpStatusCode.Forbidden)
//        {
//            bearerToken = string.Empty;
//            GetData();
//        }
//    }

//}
//public static string GetAccessToken()
//{
//    var token = string.Empty;
//    try
//    {
//        using (var client = new HttpClient())
//        {
//            var responseMessage = client.GetAsync("https://vapi.vnappmob.com/api/request_api_key?scope=exchange_rate").Result;
//            if (responseMessage.IsSuccessStatusCode)
//            {
//                var data = responseMessage.Content.ReadAsStringAsync().Result;
//                var responseJOject = JsonConvert.DeserializeObject<JObject>(data);
//                token = (string)responseJOject["results"];
//            }
//        }
//    }
//    catch (Exception)
//    {
//    }
//    return token;
//}