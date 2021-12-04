using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#pragma warning disable SYSLIB0014
namespace DiscDL
{
    public class WebRequestGetExample
    {
        private static string Token = "";
        private static string ChannelID = "429764371242024961";
        private static string LastMsgID = "";
        public static string Save_dir_name = @"/img";
        private static int nb = 0;
        private static WebResponse? response;
        private static JArray? jArray;

        public static async Task Main(string[] args)
        {
            response = Http($"https://discord.com/api/v9/channels/{ChannelID}/messages");
            await Data(response);
            while (true)
            {
                try
                {
                    response = Http($"https://discord.com/api/v9/channels/{ChannelID}/messages?before={LastMsgID}");
                    await Data(response);
                }
                catch { break; }
            }

                Console.Write($"\nFile saved: {nb}\nPRESS ENTER TO EXIT");
                Console.ReadKey();
                response.Close();
        }

        private static async Task Data(WebResponse response)
        {
            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                jArray = JArray.Parse(responseFromServer);
                File.WriteAllText(Environment.CurrentDirectory.ToString() + Save_dir_name + "/log.json", jArray.ToString());
                Console.WriteLine(jArray.Count());

                Root getID = JsonConvert.DeserializeObject<Root>(jArray[jArray.Count() - 1].ToString());
                LastMsgID = getID.id;
                await debutAsync(jArray, jArray.Count());
                if (jArray.Count() == 0) return;
            }
        }

        private static WebResponse Http(string uri)
        {
            WebRequest request = WebRequest.Create(uri);
            request.Headers.Add("authorization", Token);
            request.ContentType = "application/json";
            request.Credentials = CredentialCache.DefaultCredentials;

            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            return response;
        }

        private static async Task debutAsync(JArray jArray,int count)
        {
            for (int i = 0; i < count; i++)
            {
                Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(jArray[i].ToString());
                try
                {
                    foreach (Attachment url in myDeserializedClass.attachments)
                    {
                        Console.WriteLine($"\nAuthor: {myDeserializedClass.author.username}#{myDeserializedClass.author.discriminator} // MEDIA");
                        await DownloadImageAsync(Environment.CurrentDirectory.ToString() + Save_dir_name, RandomNumber(0, 5000).ToString(), new Uri(url.url));
                    }
                }
                catch{}


                try
                {
                    foreach (Embed url in myDeserializedClass.embeds)
                    {
                        Console.WriteLine($"\nAuthor: {myDeserializedClass.author.username}#{myDeserializedClass.author.discriminator} // EMBED");
                        await EmbedDLAsync(url);
                    }
                }
                catch{}
            }
        }

        private static async Task EmbedDLAsync(Embed url)
        {
            try
            {
                await DownloadImageAsync(Environment.CurrentDirectory.ToString() + Save_dir_name, RandomNumber(0, 5000).ToString(), new Uri(url.image.url.ToString()));
            }
            catch { }
            try
            {
                await DownloadImageAsync(Environment.CurrentDirectory.ToString() + Save_dir_name, RandomNumber(0, 5000).ToString(), new Uri(url.url.ToString()));
            }
            catch { }
            try
            {
                await DownloadImageAsync(Environment.CurrentDirectory.ToString() + Save_dir_name, RandomNumber(0, 5000).ToString(), new Uri(url.thumbnail.url.ToString()));
            }
            catch { }
            try
            {
                await DownloadImageAsync(Environment.CurrentDirectory.ToString() + Save_dir_name, RandomNumber(0, 5000).ToString(), new Uri(url.video.url.ToString()));
            }
            catch { }
        }

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }



        private static async Task DownloadImageAsync(string directoryPath, string fileName, Uri uri)
        {
            Console.WriteLine($" {nb+1} // {LastMsgID} // URL: {uri}");
            Console.WriteLine($"Download size: {SizeSuffix(uri.ToString(), 2)}");
            using var httpClient = new HttpClient();

            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
            var fileExtension = Path.GetExtension(uriWithoutQuery);

            var path = Path.Combine(directoryPath, $"{fileName}{fileExtension}");
            Directory.CreateDirectory(directoryPath);

            var imageBytes = await httpClient.GetByteArrayAsync(uri);
            await File.WriteAllBytesAsync(path, imageBytes);
            nb++;
        }

        private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB"};
        private static string SizeSuffix(string url, int decimalPlaces = 1)
        {

            long result = -1;

            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            req.Method = "HEAD";
            using (System.Net.WebResponse resp = req.GetResponse())
            {
                if (long.TryParse(resp.Headers.Get("Content-Length"), out long ContentLength))
                {
                    result = ContentLength;
                }
            }


            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (result == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            int mag = (int)Math.Log(result, 1024);

            decimal adjustedSize = (decimal)result / (1L << (mag * 10));

            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }

        private static readonly Random _random = new Random();
    
        public static int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        public class Author
        {
            public string? username { get; set; }
            public string? discriminator { get; set; }
        }

        public class Attachment
        {
            public string? url { get; set; }
        }

        public class Embed
        {
            public string? url { get; set; }
            public Image? image { get; set; }
            public vid? video { get; set; }
            public thumb? thumbnail { get; set; }
            public prov? provider { get; set; }

        }

        public class thumb
        {
            public string? url { get; set; }
            public string? proxy_url { get; set; }
        }

        public class prov
        {
            public string? name { get; set; }
            public string? url { get; set; }
        }

        public class Image
        {
            public string? url { get; set; }
            public string? proxy_url { get; set; }
        }

        public class vid
        {
            public string? url { get; set; }
            public string? proxy_url { get; set; }
        }

        public class Root
        {
            public string? id { get; set; }
            public Author? author { get; set; }
            public List<Attachment>? attachments { get; set; }
            public List<Embed> embeds { get; set; }
        }
    }
}