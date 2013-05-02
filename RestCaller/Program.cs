using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

using Newtonsoft.Json.Linq;

namespace RestCaller
{
    class Program
    {
        private static string oauthToken = string.Empty;
        private static string serviceUrl = string.Empty;

        static void Main(string[] args)
        {
            //print message to console
            Console.WriteLine(" :: DeveloperForce Demo :: ");

            //login to the REST endpoint
            AuthenticateSfdcRestUser();

            Console.ReadLine();

            //query Accounts via REST API
            QueryRecord();

            //create a new Account via REST API
            //CreateRecord();

            Console.ReadLine();
        }


        /// <summary>
        /// Authenticate the user and get a token
        /// </summary>
        private static async void AuthenticateSfdcRestUser()
        {
            //print message to console
            Console.WriteLine("Authenticating against the OAuth endpoint ...");

            HttpClient authClient = new HttpClient();
            
            //defined remote access app - develop --> remote access --> new


            //set OAuth key and secret variables
            string sfdcConsumerKey = "[secret key]";
            string sfdcConsumerSecret = "[secret]";

            //set to Force.com user account that has API access enabled
            string sfdcUserName = "[username]";
            string sfdcPassword = "[password]";
            string sfdcToken = "[token]";

            //create login password value
            string loginPassword = sfdcPassword + sfdcToken;

           HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"grant_type","password"},
                    {"client_id",sfdcConsumerKey},
                    {"client_secret",sfdcConsumerSecret},
                    {"username",sfdcUserName},
                    {"password",loginPassword}
                }
                );

            HttpResponseMessage message = await authClient.PostAsync("https://login.salesforce.com/services/oauth2/token", content);

            string responseString = await message.Content.ReadAsStringAsync();

            JObject obj = JObject.Parse(responseString);
            oauthToken = (string)obj["access_token"];
            serviceUrl = (string)obj["instance_url"];

            //print response values
            Console.WriteLine(string.Format("The token value is {0}", responseString));
            Console.WriteLine("");
            Console.WriteLine("Press [Enter] to continue ...");
            Console.ReadLine();
        }

        /// <summary>
        /// Query for an account record
        /// </summary>
        private static async void QueryRecord()
        {
            Console.WriteLine("Querying account records with the REST API ...");

            HttpClient queryClient = new HttpClient();

            //QUERY: Retrieve records of type "account"
            //string restQuery = serviceUrl + "/services/data/v25.0/sobjects/Account";
            //QUERY: retrieve a specific account
            //string restQuery = serviceUrl + "/services/data/v25.0/sobjects/Account/001E000000N1H1O";
            //QUERY: Perform a SELECT operation
            string restQuery = serviceUrl + "/services/data/v25.0/query?q=SELECT+name+from+Account";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, restQuery);

            //add token to header
            request.Headers.Add("Authorization", "Bearer " + oauthToken);

            //return XML to the caller
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            //return JSON to the caller
            //request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //call endpoint async
            HttpResponseMessage response = await queryClient.SendAsync(request);

            string result = await response.Content.ReadAsStringAsync();

            Console.WriteLine(result);
            Console.WriteLine("");
            Console.WriteLine("Query complete.");
            Console.ReadLine();

        }

        /// <summary>
        /// Create a new account record
        /// </summary>
        private static async void CreateRecord()
        {
            Console.WriteLine("Creating account records with the REST API ...");

            HttpClient createClient = new HttpClient();

            //string requestMessage = "{\"Name\":\"DevForce20\", \"AccountNumber\":\"1005555\"}";
            //HttpContent content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

            string requestMessage = "<root><Name>DevForce21</Name><AccountNumber>8994432</AccountNumber></root>";
            HttpContent content = new StringContent(requestMessage, Encoding.UTF8, "application/xml");

            string uri = serviceUrl + "/services/data/v25.0/sobjects/Account";

            //create request message associated with POST verb
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);

            //add token to header
            request.Headers.Add("Authorization", "Bearer " + oauthToken);
            //return xml to the caller
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Content = content;

            HttpResponseMessage response = await createClient.SendAsync(request);

            string result = await response.Content.ReadAsStringAsync();

            Console.WriteLine(result);
            Console.WriteLine("");
            Console.WriteLine("Create complete.");
            Console.ReadLine();
        }
    }
}
