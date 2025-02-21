using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace AccountServer.Http;

public class AccountServerHttpClient
{
	public static AccountServerHttpClient Instance { get; private set; } = new AccountServerHttpClient();

	static HttpClient _Client = new HttpClient();
	static public string BaseUrl { get; private set; } = String.Format("http://localhost:{0}/", 7778);//TODO File로 빼서 관리.

	public static T? HttpSendObject<T>(object sendObject, HttpMethod method, string queryPath)
	{
		HttpRequestMessage message = new HttpRequestMessage(method, new Uri($"{BaseUrl}{queryPath}"));

		string jsonString = JsonConvert.SerializeObject(sendObject);
		//byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);

		message.Content = new StringContent(jsonString, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
		HttpResponseMessage result = _Client.Send(message);

		string resultText = "";
		using (Stream output = result.Content.ReadAsStream())
		{
			using (StreamReader reader = new StreamReader(output, Encoding.UTF8))
			{
				resultText = reader.ReadToEnd();
			}	
		}
		Console.WriteLine(resultText);
		T? obj = JsonConvert.DeserializeObject<T>(resultText);
		return obj;
	}

	public static async void HttpSendObjectAsync(object sendObject, HttpMethod method, string path = "")
	{
		HttpRequestMessage message = new HttpRequestMessage(method, new Uri($"{BaseUrl}{path}"));

		string jsonString = JsonConvert.SerializeObject(sendObject);
		//byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);

		message.Content = new StringContent(jsonString, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
		HttpResponseMessage result = await _Client.SendAsync(message);
		string content = await result.Content.ReadAsStringAsync();
		Console.WriteLine(content);
	}

	public static async void HttpSendTextAsync(string text, HttpMethod method, string path = "")
	{
		HttpRequestMessage message = new HttpRequestMessage(method, new Uri($"{BaseUrl}{path}"));

		message.Content = new StringContent(text, Encoding.UTF8);
		HttpResponseMessage result = await _Client.SendAsync(message);
		string content = await result.Content.ReadAsStringAsync();
		Console.WriteLine(content);
	}

}
