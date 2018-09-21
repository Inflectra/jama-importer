using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.HelperClasses
{
    public class WcfUtils
    {
        /// <summary>
        /// Configure the SOAP connection for HTTP or HTTPS depending on what was specified
        /// </summary>
        /// <param name="httpBinding"></param>
        /// <param name="uri"></param>
        /// <remarks>Allows self-signed certs to be used</remarks>
        public static void ConfigureBinding(BasicHttpBinding httpBinding, Uri uri)
        {
            //Handle SSL if necessary
            if (uri.Scheme == "https")
            {
                httpBinding.Security.Mode = BasicHttpSecurityMode.Transport;
                httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

                //Allow self-signed certificates
                PermissiveCertificatePolicy.Enact("");
            }
            else
            {
                httpBinding.Security.Mode = BasicHttpSecurityMode.None;
            }
        }
    }
}
