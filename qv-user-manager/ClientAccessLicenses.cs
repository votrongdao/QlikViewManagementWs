﻿/*
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
using System.Linq;
using System.Text;
using qv_user_manager.QMSBackendService;

namespace qv_user_manager
{
    public class ClientAccessLicenses
    {
        /// <summary>
        /// ADD CALs
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="users"></param>
        public static string Add(ICollection<string> documents, ICollection<string> users)
        {
            StringBuilder response = new StringBuilder();
            try
            {
                // Initiate backend client
                var backendClient = new QMSBackendClient();

                // Get a time limited service key
                ServiceKeyClientMessageInspector.ServiceKey = backendClient.GetTimeLimitedServiceKey();

                // Get available QlikView Servers
                var serviceList = backendClient.GetServices(ServiceTypes.QlikViewServer);

                if (documents.Count == 0)
                // NAMED CALS
                {
                    // Loop through available servers
                    foreach (var server in serviceList)
                    {
                        // Get Named CALs
                        var config = backendClient.GetCALConfiguration(server.ID, CALConfigurationScope.NamedCALs);

                        // Get number of users BEFORE modifications
                        var numberOfCals = config.NamedCALs.AssignedCALs.Count;

                        // Add CAL's (already existing CAL's seems unaffected, but how to check for them? Is it necessary?)
                        foreach (var user in users.Select(u => new AssignedNamedCAL
                        {
                            UserName = u.ToUpper(),
                        }))
                        {
                            config.NamedCALs.AssignedCALs.Add(user);
                        }

                        // Save changes
                        backendClient.SaveCALConfiguration(config);

                        // Get number of users AFTER modifications
                        var addedCals = config.NamedCALs.AssignedCALs.Count - numberOfCals;

                        if (addedCals <= 0)
                            response.AppendLine(String.Format("Nothing to add on {0}", server.Name));
                        else
                            response.AppendLine(String.Format("Added {0} CALs on {1}", addedCals, server.Name));

                        var inLicense = config.NamedCALs.InLicense;
                        var assigned = config.NamedCALs.Assigned;

                        // Warn if not enough available CAL's
                        if (addedCals >= inLicense)
                            response.AppendLine(String.Format("WARNING: Attempted to assign {0} CALs on {1} but the license only allows {2} CALs.", addedCals, server.Name, inLicense));
                        else if (assigned >= inLicense)
                            response.AppendLine("WARNING: All available CALs in the license have been assigned.");
                    }
                }
                // DOCUMENT CALS
                else
                {
                    // Loop through available servers
                    foreach (var server in serviceList)
                    {
                        // Get documents on each server
                        var userDocuments = backendClient.GetUserDocuments(server.ID);

                        // Loop through available documents
                        foreach (var docNode in userDocuments)
                        {
                            // Continue if no matching documents
                            if (!documents.Contains(docNode.Name.ToLower())) continue;

                            var metaData = backendClient.GetDocumentMetaData(docNode, DocumentMetaDataScope.Licensing);

                            // Get allocated CAL's for document
                            var allocatedCals = metaData.Licensing.CALsAllocated;

                            // Allocate more CAL's if necessary
                            if (users.Count > allocatedCals)
                                metaData.Licensing.CALsAllocated = users.Count();

                            // Add document CAL's
                            foreach (var user in users.Select(u => new AssignedNamedCAL
                            {
                                UserName = u
                            }))
                            {
                                metaData.Licensing.AssignedCALs.Add(user);
                            }

                            // Save changes
                            backendClient.SaveDocumentMetaData(metaData);

                            response.AppendLine(String.Format("Added {0} Document CALs to '{1}' on {2}", users.Count(), docNode.Name, server.Name));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.AppendLine(ex.Message);
            }
            return response.ToString();
        }

        /// <summary>
        /// List CALs
        /// </summary>
        public static string List()
        {
            StringBuilder response = new StringBuilder();
            try
            {
                // Initiate backend client
                var backendClient = new QMSBackendClient();

                // Get a time limited service key
                ServiceKeyClientMessageInspector.ServiceKey = backendClient.GetTimeLimitedServiceKey();

                // Get available QlikView Servers
                var serviceList = backendClient.GetServices(ServiceTypes.QlikViewServer);

                response.AppendLine("UserName;LastUsed;QuarantinedUntil;Document;Server");

                // Loop through available servers
                foreach (var server in serviceList)
                {
                    // Get Named CALs
                    var config = backendClient.GetCALConfiguration(server.ID, CALConfigurationScope.NamedCALs);

                    foreach (var c in config.NamedCALs.AssignedCALs)
                        response.AppendLine(String.Format("{0};{1};{2};{3};{4}", c.UserName, c.LastUsed.Year > 0001 ? c.LastUsed.ToString() : "", c.QuarantinedUntil.Year > 0001 ? c.QuarantinedUntil.ToString() : "", "", server.Name));

                    // Get Document CAL's
                    var userDocuments = backendClient.GetUserDocuments(server.ID);

                    foreach (var docNode in userDocuments)
                    {
                        var metaData = backendClient.GetDocumentMetaData(docNode, DocumentMetaDataScope.Licensing);

                        foreach (var c in metaData.Licensing.AssignedCALs)
                            response.AppendLine(String.Format("{0};{1};{2};{3};{4}", c.UserName, c.LastUsed.Year > 0001 ? c.LastUsed.ToString() : "", c.QuarantinedUntil.Year > 0001 ? c.QuarantinedUntil.ToString() : "", docNode.Name, server.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                response.AppendLine(ex.Message);
            }
            return response.ToString();
        }

        /// <summary>
        /// Remove CALs
        /// </summary>
        public static string Remove()
        {
            StringBuilder response = new StringBuilder();
            try
            {
                // Initiate backend client
                var backendClient = new QMSBackendClient();

                // Get a time limited service key
                ServiceKeyClientMessageInspector.ServiceKey = backendClient.GetTimeLimitedServiceKey();

                // Get available QlikView Servers
                var serviceList = backendClient.GetServices(ServiceTypes.QlikViewServer);

                // Number of inactive days
                const int days = -30;

                // Loop through available servers
                foreach (var server in serviceList)
                {
                    /**********************
                     *     NAMED CALS
                     **********************/

                    // Get CAL configuration
                    var config = backendClient.GetCALConfiguration(server.ID, CALConfigurationScope.NamedCALs);

                    // Get number of users BEFORE modifications
                    var numberOfUsers = config.NamedCALs.AssignedCALs.Count;

                    // Iterate through all CAL's and remove the inactive ones
                    foreach (var c in config.NamedCALs.AssignedCALs.ToList().Where(u => u.LastUsed.Year > 0001 && u.LastUsed.CompareTo(DateTime.UtcNow.AddDays(days)) == -1))
                        config.NamedCALs.AssignedCALs.Remove(c);

                    // Save changes
                    backendClient.SaveCALConfiguration(config);

                    // Get number of users BEFORE modifications
                    var removedUsers = numberOfUsers - config.NamedCALs.AssignedCALs.Count;

                    if (removedUsers <= 0)
                        response.AppendLine(String.Format("No CALs to remove on {0}", server.Name));
                    else
                        response.AppendLine(String.Format("Removed {0} CALs on {1}", removedUsers, server.Name));

                    /**********************
                     *   DOCUMENT CALS
                     **********************/

                    // Get Document CAL's
                    var userDocuments = backendClient.GetUserDocuments(server.ID);

                    foreach (var docNode in userDocuments)
                    {
                        // Get licensing meta data
                        var metaData = backendClient.GetDocumentMetaData(docNode, DocumentMetaDataScope.Licensing);

                        // Get number of users BEFORE modifications
                        numberOfUsers = metaData.Licensing.AssignedCALs.Count;

                        // Iterate through all CAL's and remove the inactive ones
                        foreach (var c in metaData.Licensing.AssignedCALs.ToList().Where(u => u.LastUsed.Year > 0001 && u.LastUsed.CompareTo(DateTime.UtcNow.AddDays(days)) == -1))
                            metaData.Licensing.AssignedCALs.Remove(c);

                        // Save changes
                        backendClient.SaveDocumentMetaData(metaData);

                        // Get number of users AFTER modifications
                        removedUsers = numberOfUsers - metaData.Licensing.AssignedCALs.Count;

                        if (removedUsers <= 0)
                            response.AppendLine(String.Format("No Document CALs to remove from '{0}' on {1}", docNode.Name, server.Name));
                        else
                            response.AppendLine(String.Format("Removed {0} Document CALs from '{1}' on {2}", removedUsers, docNode.Name, server.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                response.AppendLine(ex.Message);
            }
            return response.ToString();
        }

        /// <summary>
        /// List information about CALs
        /// </summary>
        public static string CalInfo()
        {
            StringBuilder response = new StringBuilder();
            try
            {
                // Initiate backend client
                var backendClient = new QMSBackendClient();

                // Get a time limited service key
                ServiceKeyClientMessageInspector.ServiceKey = backendClient.GetTimeLimitedServiceKey();

                // Get available QlikView Servers
                var serviceList = backendClient.GetServices(ServiceTypes.QlikViewServer);

                response.AppendLine("Server;NamedAssigned;NamedInLicense;NamedLeased;NamedAllowDynamic;NamedAllowLease;DocAssigned;DocInLicense;SessionAvailable;SessionInLicense;UsageAvailable;UsageInLicense");

                // Loop through available servers
                foreach (var server in serviceList)
                {
                    // Get Named CALs
                    var config = backendClient.GetCALConfiguration(server.ID, CALConfigurationScope.All);

                    response.Append(server.Name + ";");
                    response.Append(config.NamedCALs.AssignedCALs.Count + ";");
                    response.Append(config.NamedCALs.InLicense + ";");
                    response.Append(config.NamedCALs.LeasedCALs.Count + ";");
                    response.Append(config.NamedCALs.AllowDynamicAssignment + ";");
                    response.Append(config.NamedCALs.AllowLicenseLease + ";");
                    response.Append(config.DocumentCALs.Assigned + ";");
                    response.Append(config.DocumentCALs.InLicense + ";");
                    response.Append(config.SessionCALs.Available + ";");
                    response.Append(config.SessionCALs.InLicense + ";");
                    response.Append(config.UsageCALs.Available + ";");
                    response.AppendLine(config.UsageCALs.InLicense.ToString());
                }
            }
            catch (Exception ex)
            {
                response.AppendLine(ex.Message);
            }
            return response.ToString();
        }
    }
}
