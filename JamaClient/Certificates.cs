using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.JamaClient
{
    /// <summary>
    /// Allow self-signed certificates when connecting to Jama
    /// </summary>
    class Certificates
    {
        public static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            //All certificates are OK
            return true;
        }
    }
}
