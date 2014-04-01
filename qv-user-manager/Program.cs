/*
The MIT License (MIT)

Copyright (c) 2011 Rikard Braathen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;
using NDesk.Options;

namespace qv_user_manager
{
    public class Program
    {
        public enum ExitCode
        {
            Success = 0,
            Error = 1
        }

        static void Main(string[] args)
        {
            Run(args);
        }

        public static string Run(string[] args)
        {
            var response = new StringBuilder();
            var responseWriter = new StringWriter(response);


            if (!VerifyServerConfig(responseWriter))
            {
                Environment.ExitCode = (int)ExitCode.Error;
                return "Error: Verify Server Config Failed";
            }

            var list = "";
            var add = "";
            var remove = "";
            var docs = "";
            var prefix = "";
            var version = false;
            var help = false;

            try
            {
                var p = new OptionSet {
                    { "l|list=", "List CALs, users or documents [{CAL|DMS|DOCS}]", v => list = v.ToLower() },
                    { "a|add=", "Add users or assign CALs [{CAL|DMS}]", v => add = v.ToLower() },
                    { "r|remove=", "Remove specified users or inactive CALs [{CAL|DMS}]", v => remove = v.ToLower() },
                    { "d|document=", "QlikView document(s) to perform actions on", v => docs = v.ToLower() },
                    { "p|prefix=", "Use specified prefix for all users and CALs", v => prefix = v },
                    { "V|version", "Show version information", v => version = v != null },
                    { "?|h|help", "Show usage information", v => help = v != null },
                };

                p.Parse(args);

                if (help || args.Length == 0)
                {
                    response.AppendLine("Usage: qv-user-manager [options]");
                    response.AppendLine("Handles QlikView CALs and DMS user authorizations.");
                    response.AppendLine();
                    response.AppendLine("Options:");
                    p.WriteOptionDescriptions(responseWriter);
                    response.AppendLine();
                    response.AppendLine("Options can be in the form -option, /option or --long-option");
                    return response.ToString();
                }

                if (version)
                {
                    response.AppendLine("qv-user-manager 20111028\n");
                    response.AppendLine("This program comes with ABSOLUTELY NO WARRANTY.");
                    response.AppendLine("This is free software, and you are welcome to redistribute it");
                    response.AppendLine("under certain conditions.\n");
                    response.AppendLine("Code: git clone git://github.com/braathen/qv-user-manager.git");
                    response.AppendLine("Home: <https://github.com/braathen/qv-user-manager>");
                    response.AppendLine("Bugs: <https://github.com/braathen/qv-user-manager/issues>");
                    return response.ToString();
                }
            }
            catch (Exception ex)
            {
                response.AppendLine(ex.Message);
                return response.ToString();
            }

            // Split list of multiple documents
            var documents = new List<string>(docs.Split(new[] { ';', ',', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

            // Remove possible duplicate documents
            documents = documents.Distinct().ToList();

            var users = new List<string>();

            // Look for console redirection or piped data
            try
            {
                var isKeyAvailable = Console.KeyAvailable;
            }
            catch (InvalidOperationException)
            {
                string s;
                while ((s = Console.ReadLine()) != null)
                {
                    users.Add(prefix + s.Trim());
                }
            }

            // Remove possible duplicate users
            users = users.Distinct().ToList();

            switch (remove)
            {
                case "dms":
                    response.AppendLine(DocumentMetadataService.Remove(documents, users));
                    break;
                case "cal":
                    response.AppendLine(ClientAccessLicenses.Remove());
                    break;
            }

            switch (add)
            {
                case "dms":
                    response.AppendLine(DocumentMetadataService.Add(documents, users));
                    break;
                case "cal":
                    response.AppendLine(ClientAccessLicenses.Add(documents, users));
                    break;
            }

            switch (list)
            {
                case "dms":
                    response.AppendLine(DocumentMetadataService.List(documents));
                    break;
                case "cal":
                    response.AppendLine(ClientAccessLicenses.List());
                    break;
                case "docs":
                    response.AppendLine(DocumentMetadataService.DocInfo(documents));
                    break;
                case "calinfo":
                    response.AppendLine(ClientAccessLicenses.CalInfo());
                    break;
            }

            Environment.ExitCode = (int)ExitCode.Success;
            return response.ToString();
        }

        /// <summary>
        /// Verify that the user has changed the server settings
        /// </summary>
        /// <returns></returns>
        public static bool VerifyServerConfig(TextWriter o)
        {
            var sbresponse = new StringBuilder();
            try
            {
                var clientSection = ConfigurationManager.GetSection("system.serviceModel/client") as ClientSection;

                var propertyInformation = clientSection.ElementInformation.Properties[string.Empty];

                var endpointCollection = propertyInformation.Value as ChannelEndpointElementCollection;

                var address = endpointCollection[0].Address.ToString();

                if (address.Contains("your-server-address"))
                {
                    o.WriteLine("The server address needs to be configured in the qv-user-manager.config file." + System.Environment.NewLine);

                    o.WriteLine("Current value: " + address);
                    
                    return false;
                }
            }
            catch (Exception e)
            {
                o.WriteLine(e.Message);
                return false;
            }
            return true;
        }
    }
}
