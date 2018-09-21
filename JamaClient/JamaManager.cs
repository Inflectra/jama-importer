using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.JamaClient
{
    /// <summary>
    /// The Jama API manager class
    /// </summary>
    public class JamaManager
    {
        //We're currently using v1 of the Jama API
        public const string API_PATH = "/rest/v1/";

        private string baseUrl;
        private string username;
        private string password;

        //Logging functions
        private bool traceLogging = false;

        /// <summary>
        /// Should we use default credentials
        /// </summary>
        public bool UseDefaultCredentials
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseUrl">The base JIRA URL</param>
        /// <param name="username">The JIRA login</param>
        /// <param name="password">The JIRA password</param>
        /// <param name="eventLog">The Windows Event Log</param>
        public JamaManager(string baseUrl, string username, string password, bool traceLogging = false)
        {
            this.username = username;
            this.password = password;
            this.baseUrl = baseUrl;
            this.UseDefaultCredentials = false;
            this.traceLogging = traceLogging;
            
            //Allow self-signed SSL certificates
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(Certificates.ValidateRemoteCertificate);
        }

        /// <summary>
        /// Returns list of all projects
        /// </summary>
        public List<JamaProject> GetProjects()
        {
            //First try TLS 1.2
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string json = RunQuery("projects", method: "GET");
                JObject data = JObject.Parse(json);
                return data["data"].ToObject<List<JamaProject>>();
            }
            catch (WebException)
            {
                try
                {
                    //Then try TLS 1.1
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                    string json = RunQuery("projects", method: "GET");
                    JObject data = JObject.Parse(json);
                    return data["data"].ToObject<List<JamaProject>>();
                }
                catch (WebException)
                {
                    try
                    {
                        //Then try TLS 1.0
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                        string json = RunQuery("projects", method: "GET");
                        JObject data = JObject.Parse(json);
                        return data["data"].ToObject<List<JamaProject>>();
                    }
                    catch (WebException)
                    {
                        //Then try SSL 3.0, let this exception throw
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                        string json = RunQuery("projects", method: "GET");
                        JObject data = JObject.Parse(json);
                        return data["data"].ToObject<List<JamaProject>>();
                    }
                }
            }
        }

        /// <summary>
        /// Returns list of all projects
        /// </summary>
        public JamaProject GetProject(int projectId)
        {
            string json = RunQuery("projects", argument: "/" + projectId, method: "GET");
            JObject data = JObject.Parse(json);
            return data["data"].ToObject<JamaProject>();
        }

        public List<JamaRelease> GetReleases(int projectId)
        {
            string json = RunQuery("releases", argument: "?project=" + projectId, method: "GET");
            JObject data = JObject.Parse(json);
            return data["data"].ToObject<List<JamaRelease>>();
        }

        public JamaItem GetItem(int id)
        {
            string json = RunQuery("items", argument: "/" + id, method: "GET");
            JObject data = JObject.Parse(json);
            return data["data"].ToObject<JamaItem>();
        }

        public List<JamaItem> GetItemsForProject(int projectId, int startItem, int maxResults = 20)
        {
            string json = RunQuery("items", argument: "?project=" + projectId + "&startAt=" + startItem + "&maxResults=" + maxResults, method: "GET");
            JObject data = JObject.Parse(json);
            return data["data"].ToObject<List<JamaItem>>();
        }

        public JamaItemType GetItemType(int id)
        {
            string json = RunQuery("itemtypes", argument: "/" + id, method: "GET");
            JObject data = JObject.Parse(json);
            return data["data"].ToObject<JamaItemType>();
        }

        /// <summary>
        /// Tests if you can connect to Jama
        /// </summary>
        /// <returns></returns>
        public bool TestConnection()
        {
            try
            {
                //First try TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string json = RunQuery("projects", method: "GET");
                if (string.IsNullOrEmpty(json))
                {
                    return false;
                }
                return true;
            }
            catch (WebException)
            {
                try
                {
                    //Then try TLS 1.1
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                    string json = RunQuery("projects", method: "GET");
                    if (string.IsNullOrEmpty(json))
                    {
                        return false;
                    }
                    return true;
                }
                catch (WebException)
                {
                    try
                    {
                        //Then try TLS 1.0
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                        string json = RunQuery("projects", method: "GET");
                        if (string.IsNullOrEmpty(json))
                        {
                            return false;
                        }
                        return true;
                    }
                    catch (WebException)
                    {
                        //Finally try SSL 3.0
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                        string json = RunQuery("projects", method: "GET");
                        if (string.IsNullOrEmpty(json))
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Runs a generic Jama REST query
        /// </summary>
        /// <param name="resource">The resource we're accessing</param>
        /// <param name="argument">The URL querystring arguments</param>
        /// <param name="data">The POST data</param>
        /// <param name="method">The HTTP method</param>
        protected string RunQuery(string resource, string argument = null, string data = null, string method = "GET")
        {
            try
            {
                string url = string.Format("{0}{1}{2}", baseUrl, API_PATH, resource.ToString());

                if (argument != null)
                {
                    url = string.Format("{0}{1}", url, argument);
                }

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.ContentType = "application/json";
                request.Method = method;
                request.UseDefaultCredentials = this.UseDefaultCredentials;

                if (data != null)
                {
                    using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                    {
                        writer.Write(data);
                    }
                }

                string base64Credentials = GetEncodedCredentials();
                request.Headers.Add("Authorization", "Basic " + base64Credentials);

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                if (response == null)
                {
                    throw new Exception("Null Response received from Jama API");
                }

                string result = string.Empty;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }

                return result;
            }
            catch (WebException webException)
            {
                //See if we have a Response
                if (webException.Response != null)
                {
                    //Log the message with response and rethrow
                    HttpWebResponse errorResponse = webException.Response as HttpWebResponse;
                    string details = string.Empty;
                    using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream()))
                    {
                        details = reader.ReadToEnd();
                    }
                    throw new ApplicationException("Web Exception Error calling Jama REST API: '" + webException.Message + "' Details: " + details);
                }

                //Log the basic message and rethrow
                throw new ApplicationException("Web Exception Error calling Jama REST API: " + webException.Message);
            }
            catch (Exception exception)
            {
                //Log the message and rethrow
                throw exception;
            }
        }

        /// <summary>
        /// Gets the base64-encoded login/password
        /// </summary>
        /// <returns></returns>
        private string GetEncodedCredentials()
        {
            string mergedCredentials = string.Format("{0}:{1}", this.username, this.password);
            byte[] byteCredentials = UTF8Encoding.UTF8.GetBytes(mergedCredentials);
            return Convert.ToBase64String(byteCredentials);
        }
    }
}
