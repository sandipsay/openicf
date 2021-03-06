/*
 * ====================
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS HEADER.
 * 
 * Copyright 2008-2009 Sun Microsystems, Inc. All rights reserved.     
 * 
 * The contents of this file are subject to the terms of the Common Development 
 * and Distribution License("CDDL") (the "License").  You may not use this file 
 * except in compliance with the License.
 * 
 * You can obtain a copy of the License at 
 * http://IdentityConnectors.dev.java.net/legal/license.txt
 * See the License for the specific language governing permissions and limitations 
 * under the License. 
 * 
 * When distributing the Covered Code, include this CDDL Header Notice in each file
 * and include the License file at identityconnectors/legal/license.txt.
 * If applicable, add the following below this CDDL Header, with the fields 
 * enclosed by brackets [] replaced by your own identifying information: 
 * "Portions Copyrighted [year] [name of copyright owner]"
 * ====================
 * Portions Copyrighted 2012-2014 ForgeRock AS.
 */
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Org.IdentityConnectors.Framework.Common.Objects;
using System.DirectoryServices;
using DS = System.DirectoryServices;
using Org.IdentityConnectors.Framework.Common.Exceptions;
using System.Diagnostics;
using System.Security;
using ActiveDs;
using Org.IdentityConnectors.Common.Security;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Org.IdentityConnectors.ActiveDirectory
{
    /// <summary>
    /// Collection of Active directory utilities.  Some are static methods,
    /// other require configuration, so they are instance methods.
    /// </summary>
    public class ActiveDirectoryUtils
    {
        // tracing (uses ActiveDirectoryConnector name!)
        internal static TraceSource LOGGER = new TraceSource(TraceNames.DEFAULT);
        private const int CAT_DEFAULT = 1;      // default tracing event category

        ActiveDirectoryConfiguration _configuration = null;
        private CustomAttributeHandlers _customHandlers = null;
        private ICollection<String> _knownObjectClasses = new HashSet<String>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">
        /// Configuration object for the connector.
        /// </param>
        public ActiveDirectoryUtils(ActiveDirectoryConfiguration configuration)
        {
            _configuration = configuration;
            _customHandlers = new CustomAttributeHandlers(_configuration);
        }

        // e.g. <GUID=64A09EFC1C50EB40B1692D8FBA4BBB64> --> fc9ea064-501c-40eb-b169-2d8fba4bbb64
        public static string ConvertADGUIDtoObjectGUID(string uid)
        {
            if (uid == null)
            {
                return null;
            }

            uid = uid.ToLower();
            if (!uid.StartsWith("<guid=") || !uid.EndsWith(">") || uid.Length != 39)
            {
                return uid;
            }
            uid = uid.Substring(6, 32);
            StringBuilder rv = new StringBuilder();
            rv.Append(uid.Substring(6, 2));
            rv.Append(uid.Substring(4, 2));
            rv.Append(uid.Substring(2, 2));
            rv.Append(uid.Substring(0, 2));
            rv.Append("-");
            rv.Append(uid.Substring(10, 2));
            rv.Append(uid.Substring(8, 2));
            rv.Append("-");
            rv.Append(uid.Substring(14, 2));
            rv.Append(uid.Substring(12, 2));
            rv.Append("-");
            rv.Append(uid.Substring(16, 4));
            rv.Append("-");
            rv.Append(uid.Substring(20, 12));
            return rv.ToString();
        }


        public static String ConvertUIDToSearchString(Uid uid)
        {
            // e.g. <GUID=8184d4af97d9ed4c949c21665768881b>
            string uidValue = uid.GetUidValue().ToLower();
            if (!uidValue.StartsWith("<guid=") || !uidValue.EndsWith(">") || uidValue.Length != 39)
            {
                throw new ArgumentException("Unsupported UID format: " + uidValue);
            }
            string raw = uidValue.Substring(6, 32);
            StringBuilder rv = new StringBuilder();
            for (int i = 0; i < raw.Length; i += 2)
            {
                rv.Append("\\").Append(raw.Substring(i, 2));
            }
            return rv.ToString();
        }

        /// <summary>
        /// Converts a guid in byte array form to a string suitable
        /// for ldap search.
        /// </summary>
        /// <param name="guidBytes"></param>
        /// <returns></returns>
        public static String ConvertUIDBytesToSearchString(Byte[] guidBytes)
        {
            String searchGuid = "";

            for (int i = 0; i < guidBytes.Length; i++)
            {
                searchGuid += String.Format("\\{0:x2}", guidBytes[i]);
            }

            return searchGuid;
        }

        /// <summary>
        /// Converts a guid in byte array form to a string with the format
        /// &gt;GUID = xxxxxxxxxxxxxxxxxxxxxxxxxxxxx&lt; where the x's represent
        /// uppercase hexadecimal digits
        /// </summary>
        /// <param name="guidBytes"></param>
        /// <returns></returns>
        internal static String ConvertUIDBytesToGUIDString(Byte[] guidBytes)
        {
            return ConvertBytesToADSpecialString("GUID", guidBytes);
        }

        internal static String ConvertSIDBytesToGUIDString(Byte[] sidBytes)
        {
            return ConvertBytesToADSpecialString("SID", sidBytes);
        }

        internal static String ConvertBytesToADSpecialString(string attribute, Byte[] bytes)
        {
            String guidString = "<" + attribute + "=";

            for (int i = 0; i < bytes.Length; i++)
            {
                guidString += String.Format("{0:x2}", bytes[i]);
            }
            guidString += ">";

            return guidString;
        }

        /// <summary>
        /// Returns an ldap path in the form of: 
        /// LDAP://servernameIfSpecified/path
        /// </summary>
        /// <param name="serverName">Servername can be null</param>
        /// <param name="path">Path should not be null</param>
        /// <returns></returns>
        internal static String GetLDAPPath(string serverName, string path)
        {
            return GetFullPath("LDAP", serverName, path);
        }

        /// <summary>
        /// Returns a path string in the format:
        /// GC://servernameIfSpecified/path
        /// </summary>
        /// <param name="serverName">Servername is optional</param>
        /// <param name="path">Path should be specified</param>
        /// <returns></returns>
        internal static String GetGCPath(string serverName, string path)
        {
            return GetFullPath("GC", serverName, path);
        }

        /// <summary>
        /// Returns a path string in the format:
        /// provider://servernameIfSpecified/path
        /// </summary>
        /// <param name="provider">provider (such as ldap or gc)</param>
        /// <param name="serverName">servername - optional</param>
        /// <param name="path">path to resource</param>
        /// <returns></returns>
        internal static String GetFullPath(string provider, string serverName, string path)
        {
            IADsPathname pathName = getADSPathname(provider, serverName, path);
            return pathName.Retrieve((int)ADS_FORMAT_ENUM.ADS_FORMAT_X500);
        }

        /// <summary>
        /// uses iadspathname to create paths in a standard way
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="serverName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static IADsPathname getADSPathname(string provider, string serverName, string path)
        {
            IADsPathname pathName = new PathnameClass();
            if ((provider != null) && (provider.Length != 0))
            {
                pathName.Set(provider, (int)ADS_SETTYPE_ENUM.ADS_SETTYPE_PROVIDER);
            }

            if ((serverName != null) && (serverName.Length != 0))
            {
                pathName.Set(serverName, (int)ADS_SETTYPE_ENUM.ADS_SETTYPE_SERVER);
            }

            if ((path != null) && (path.Length != 0))
            {
                // must supply a path
                pathName.Set(path, (int)ADS_SETTYPE_ENUM.ADS_SETTYPE_DN);
            }
            return pathName;
        }

        /// <summary>
        /// Gets the dn of the parent object of the object specified by childDn
        /// </summary>
        /// <param name="childDn">distinguished name of an object to retrieve the parent of</param>
        /// <returns>distinguished name of the parent of 'childDn' or null</returns>
        internal static string GetParentDn(string childDn)
        {
            IADsPathname pathName = getADSPathname(null, null, childDn);
            return pathName.Retrieve((int)ADS_FORMAT_ENUM.ADS_FORMAT_X500_PARENT);
        }

        /// <summary>
        /// Updates an AD object (also called by create after object is created)
        /// </summary>
        /// <param name="oclass"></param>
        /// <param name="directoryEntry"></param>
        /// <param name="attributes"></param>
        /// <param name="type"></param>
        /// <param name="config"></param>
        internal void UpdateADObject(ObjectClass oclass, 
            DirectoryEntry directoryEntry, ICollection<ConnectorAttribute> attributes,
            UpdateType type, ActiveDirectoryConfiguration config) 
        {
            if(oclass.Equals(ObjectClass.ACCOUNT)) 
            {
                // translate attribute passed in
                foreach (ConnectorAttribute attribute in attributes)
                {
                    // encountered problems when processing change password at the same time 
                    // as setting expired.  It would be set to expired, but the change would 
                    // clear that.  So we must ensure that expired comes last.
                    if (OperationalAttributes.PASSWORD_EXPIRED_NAME.Equals(attribute.Name))
                    {
                        continue;
                    }

                    AddConnectorAttributeToADProperties(oclass,
                        directoryEntry, attribute, type);

                    //  Uncommenting the next line is very helpful in
                    //  finding mysterious errors.                    
                    // Trace.TraceInformation("Committing after setting attribute {0} to {1}", attribute.Name, attribute.Value);
                    // directoryEntry.CommitChanges();
                }

                directoryEntry.CommitChanges();

                // now do the password change.  This is handled separately, because
                // it might be a user changing his own password, or it might be an
                // administrative change.

                GuardedString gsNewPassword = ConnectorAttributeUtil.GetPasswordValue(attributes);
                if (gsNewPassword != null)
                {
                    GuardedString gsCurrentPassword = ConnectorAttributeUtil.GetCurrentPasswordValue(attributes);
                    PasswordChangeHandler changeHandler = new PasswordChangeHandler(_configuration);
                    if (gsCurrentPassword == null)
                    {
                        // just a normal password change
                        changeHandler.changePassword(directoryEntry, gsNewPassword);
                    }
                    else
                    {
                        changeHandler.changePassword(directoryEntry,
                            gsCurrentPassword, gsNewPassword);
                    }


                UserAccountControl.Set(directoryEntry.Properties[ActiveDirectoryConnector.ATT_USER_ACOUNT_CONTROL], 
                    UserAccountControl.PASSWD_NOTREQD, false);
                    directoryEntry.CommitChanges();
                }

                // see note in loop above for explaination of this
                ConnectorAttribute expirePasswordAttribute = ConnectorAttributeUtil.Find(
                    OperationalAttributes.PASSWORD_EXPIRED_NAME, attributes);

                if (expirePasswordAttribute != null)
                {
                    AddConnectorAttributeToADProperties(oclass,
                        directoryEntry, expirePasswordAttribute, type);
                    directoryEntry.CommitChanges();
                }
                /*
                UserAccountControl.Set(directoryEntry.Properties[ActiveDirectoryConnector.ATT_USER_ACOUNT_CONTROL],
                    UserAccountControl.PASSWD_NOTREQD, false);
                */
                directoryEntry.CommitChanges();

                HandleNameAndContainerChange(type, directoryEntry, attributes, config);
            }
            else if (oclass.Equals(ActiveDirectoryConnector.groupObjectClass))
            {
                // translate attribute passed in
                foreach (ConnectorAttribute attribute in attributes)
                {
                    // Temporary
                    // Trace.TraceInformation(String.Format("Setting attribute {0} to {1}",
                    //    attribute.Name, attribute.Value));
                    AddConnectorAttributeToADProperties(oclass,
                        directoryEntry, attribute, type);
                    //                  Uncommenting the next line is very helpful in
                    //                  finding mysterious errors.
                    //                 directoryEntry.CommitChanges();
                }

                directoryEntry.CommitChanges();
                HandleNameAndContainerChange(type, directoryEntry, attributes, config);
            }
            else if (oclass.Equals(ActiveDirectoryConnector.ouObjectClass))
            {
                // translate attribute passed in
                foreach (ConnectorAttribute attribute in attributes)
                {
                    // Temporary
                    // Trace.TraceInformation(String.Format("Setting attribute {0} to {1}",
                    //    attribute.Name, attribute.Value));
                    AddConnectorAttributeToADProperties(oclass,
                        directoryEntry, attribute, type);
                    //                  Uncommenting the next line is very helpful in
                    //                  finding mysterious errors.
                    // directoryEntry.CommitChanges();
                }

                directoryEntry.CommitChanges();
                HandleNameAndContainerChange(type, directoryEntry, attributes, config);
            }
            else
            {
                String objectClassName = GetADObjectClass(oclass);
                // translate attribute passed in
                foreach (ConnectorAttribute attribute in attributes)
                {
                    // Temporary
                    // Trace.TraceInformation(String.Format("Setting attribute {0} to {1}",
                    //    attribute.Name, attribute.Value));
                    AddConnectorAttributeToADProperties(oclass,
                        directoryEntry, attribute, type);
                    //                  Uncommenting the next line is very helpful in
                    //                  finding mysterious errors.
                    // directoryEntry.CommitChanges();
                }

                directoryEntry.CommitChanges();
                HandleNameAndContainerChange(type, directoryEntry, attributes, config);
            }
        }

        // entry may be null, needs to be get fresh in that case
        internal ConnectorAttribute GetConnectorAttributeFromADEntry(ObjectClass oclass,
            String attributeName, DS.SearchResult searchResult, DirectoryEntry entry)
        {
        	
        	Boolean ourEntry = false;
            // Boolean translated = false;
            if (searchResult == null)
            {
                throw new ConnectorException(_configuration.ConnectorMessages.Format(
                    "ex_AttributeNull",
                    "Could not add connector attribute to <null> search result"));
            }

            if (entry == null) 
            {
            	ourEntry = true;
            	entry = searchResult.GetDirectoryEntry();
            }
            try 
            {
            	return _customHandlers.GetCaFromDe(oclass, 
                	attributeName, searchResult, entry);
            }
            finally
            {
            	if (ourEntry && entry != null)
            	{
            		entry.Dispose();
            	}
            }

        }

        internal void AddConnectorAttributeToADProperties(ObjectClass oclass,
            DirectoryEntry directoryEntry, ConnectorAttribute attribute, 
            UpdateType type)
        {
            // Boolean translated = false;
            if (directoryEntry == null)
            {
                throw new ConnectorException(_configuration.ConnectorMessages.Format(
                    "ex_CouldNotAddNullAttributeToDe",
                    "Could not add connector attribute to <null> directory entry"));
            }

            _customHandlers.UpdateDeFromCa(oclass, type,
                directoryEntry, attribute);

        }

        /*
        /// <summary>
        /// creates and returns a connector attribute or null.  the attribute
        /// has the name 'name' and the values associated with 'name' in the
        /// directory entry
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pvc"></param>
        /// <returns></returns>
        private static ConnectorAttribute CreateConnectorAttribute(String name, 
            PropertyValueCollection pvc)
        {
            ConnectorAttributeBuilder attributeBuilder = new ConnectorAttributeBuilder();

            if (name == null)
            {
                return null;
            }

            attributeBuilder.Name = name;

            if (pvc == null)
            {
                attributeBuilder.AddValue(null);
            }
            else
            {
                for (int i = 0; i < pvc.Count; i++)
                {
                    Object valueObject = pvc[i];
                    if ((pvc[i] == null) ||
                        (FrameworkUtil.IsSupportedAttributeType(valueObject.GetType())))
                    {
                        attributeBuilder.AddValue(pvc[i]);
                    }
                    else
                    {
                        Trace.TraceWarning(
                            "Unsupported attribute type ... calling ToString (Name: \'{0}\'({1}) Type: \'{2}\' String Value: \'{3}\'",
                            name, i, pvc[i].GetType(), pvc[i].ToString());
                        attributeBuilder.AddValue(pvc[i].ToString());
                    }
                }
            }

            return attributeBuilder.Build();
        }
        
        private static void AddConnectorAttributeToADProperties_general(
            PropertyCollection properties,
            ConnectorAttribute attribute, UpdateType type)
        {
            // null out the values if we are deleting
            // or replacing attributes.
            if (type.Equals(UpdateType.DELETE) ||
                type.Equals(UpdateType.REPLACE))
            {
                properties[attribute.Name].Value = null;
            }

            // if we are updating or adding, put the
            // new values in.
            if (type.Equals(UpdateType.ADD) ||
                type.Equals(UpdateType.REPLACE))
            {
                foreach (Object valueObject in attribute.Value)
                {
                    properties[attribute.Name].Add(valueObject);
                }
            }
        }
        */
        
        /// <summary>
        /// Gets a single value from a propertyvaluecollection  
        /// for a particular property name.  Its an error if the
        /// property contains multiple values.
        /// </summary>
        /// <param name="pvc"></param>
        /// <returns></returns>
        internal Object GetSingleValue(PropertyValueCollection pvc)
        {
            if((pvc == null) || (pvc.Count == 0))
            {
                return null;
            }

            if (pvc.Count > 1)
            {
                String msg = _configuration.ConnectorMessages.Format(
                    "ex_ExpectingSingleValue",
                    "Expecting single value, but found multiple values for attribute {0}",
                    pvc.PropertyName);
                throw new ConnectorException(msg);
            }

            return pvc[0];
        }

        /// <summary>
        /// Finds a DirectoryEntry by it's uid
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="uid"></param>
        /// <param name="adminUserName"></param>
        /// <param name="adminPassword"></param>
        /// <returns></returns>
        internal static DirectoryEntry GetDirectoryEntryFromUid(String serverName,
            Uid uid, string adminUserName, string adminPassword)
        {
            DirectoryEntry foundDirectoryEntry = new DirectoryEntry(
                ActiveDirectoryUtils.GetLDAPPath(serverName, uid.GetUidValue()),
                adminUserName, adminPassword);
            string dn = (string)foundDirectoryEntry.Properties["distinguishedName"][0];
            foundDirectoryEntry.Dispose();
            foundDirectoryEntry = new DirectoryEntry(
                ActiveDirectoryUtils.GetLDAPPath(serverName, dn),
                adminUserName, adminPassword);
            return foundDirectoryEntry;
        }

        /// <summary>
        /// Returns the AD ObjectClass associated with a particular
        /// Connector ObjectClass
        /// </summary>
        /// <param name="oclass"></param>
        /// <returns></returns>
        internal String GetADObjectClass(ObjectClass oclass)
        {

            if (oclass.Equals(ObjectClass.ACCOUNT))
            {
                return _configuration.ObjectClass;
            }
            else if (ActiveDirectoryConnector.groupObjectClass.Equals(oclass))
            {
                return "Group";
            }
            else if (ActiveDirectoryConnector.ouObjectClass.Equals(oclass))
            {
                return "organizationalUnit";
            }
            else
            {
                // It's not something I know about, so I'll consult the AD schema.
                // if it's there, fine, but if not throw an exception.

                //first check to see if we have seen it before.
                String objectClassName = oclass.GetObjectClassValue();
                if(_knownObjectClasses.Contains(objectClassName))
                {
                    return objectClassName;
                }

                // if we havent seen it before, consult AD's schema
                ActiveDirectorySchema ADSchema = GetADSchema();
                ActiveDirectorySchemaClass ADSchemaClass = null;
                try
                {
                    ADSchemaClass = ADSchema.FindClass(objectClassName);
                    _knownObjectClasses.Add(objectClassName);
                    return objectClassName;
                }
                catch (ActiveDirectoryObjectNotFoundException exception)
                {
                    String msg = _configuration.ConnectorMessages.Format(
                        "ex_ObjectClassInvalidForConnector",
                        "ObjectClass \'{0}\' is not valid for this connector",
                        objectClassName);
                    throw new ConnectorException(msg);
                }

            }
        }

        /// <summary>
        /// Puts an ldap string into a normalilzed format
        /// </summary>
        /// <param name="ldapString"></param>
        /// <returns></returns>
        public static String NormalizeLdapString(String ldapString)
        {
            /// (?<!\\)    Matches if the preceding character is not a backslash
            /// (?:\\\\)*  Matches any number of occurrences of two backslashes
            /// ,          Matches a comma
            var regexObj = new Regex(@"(?<!\\)(?:\\\\)*,");
            String[] parts = regexObj.Split(ldapString).ToArray();

            StringBuilder normalPath = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                normalPath.Append(parts[i].Trim());
                // append a comma after each part (except the last one)
                if (i < (parts.Length - 1))
                {
                    normalPath.Append(",");
                }
            }
            return normalPath.ToString();
        }

        public static String GetRelativeName(Name name)
        {
            return GetNameAsCN(name.GetNameValue());
        }

        /// <summary>
        /// Returns the leaf value of a distinguished name
        /// </summary>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        internal static String GetNameAsCN(String nameValue)
        {
            IADsPathname pathName = getADSPathname(null, null, nameValue);            
            return pathName.Retrieve((int)ADS_FORMAT_ENUM.ADS_FORMAT_LEAF);
        }

        public static string RandomStr()
        {
            return System.IO.Path.GetRandomFileName().Replace(".", "-");
        }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="directoryEntry"></param>
        /// <param name="attributes"></param>
        /// <param name="config"></param>
        private static void HandleNameAndContainerChange(UpdateType type, 
            DirectoryEntry directoryEntry, ICollection<ConnectorAttribute> attributes, 
            ActiveDirectoryConfiguration config)
        {
            Name nameAttribute = ConnectorAttributeUtil.GetNameFromAttributes(attributes);
            if(nameAttribute == null)
            {
                // no name, so must not be a container change
                return;
            }

            if (!type.Equals(UpdateType.REPLACE))
            {
                // this only make sense for replace.  you can't
                // add a name or delete a name
                return;
            }

            String oldName = directoryEntry.Name;
            String newName = GetRelativeName(nameAttribute);
            bool nameChanged = !NormalizeLdapString(oldName).Equals(NormalizeLdapString(newName), StringComparison.OrdinalIgnoreCase);

            String oldContainer = GetParentDn(directoryEntry.Path);
            String newContainer = GetParentDn(nameAttribute.GetNameValue());
            bool containerChanged = !NormalizeLdapString(oldContainer).Equals(NormalizeLdapString(newContainer), StringComparison.OrdinalIgnoreCase);

            if (!nameChanged && !containerChanged)
            {
                return;
            }

            if (nameChanged && !containerChanged)           // rename without moving
            {
                LOGGER.TraceEvent(TraceEventType.Verbose, CAT_DEFAULT, "Renaming {0} to {1}", oldName, newName);
                directoryEntry.Rename(newName);
                LOGGER.TraceEvent(TraceEventType.Verbose, CAT_DEFAULT, "Rename OK");
                return;
            }

            // so this is move with or without rename
            
            // step 1: if WITH rename, we have to rename the entry to a temporary name first

            String temporaryName = null;
            if (nameChanged)
            {
                temporaryName = oldName + "-" + RandomStr();
                LOGGER.TraceEvent(TraceEventType.Verbose, CAT_DEFAULT, "Renaming {0} to a temporary name of {1}", oldName, temporaryName);
                directoryEntry.Rename(temporaryName);
                LOGGER.TraceEvent(TraceEventType.Verbose, CAT_DEFAULT, "Rename OK");
            }

            // step 2: do the move

            try
            {
                String newContainerLdapPath = ActiveDirectoryUtils.GetLDAPPath(config.LDAPHostName, newContainer);
                DirectoryEntry newContainerDe = new DirectoryEntry(newContainerLdapPath, config.DirectoryAdminName, config.DirectoryAdminPassword);
                LOGGER.TraceEvent(TraceEventType.Verbose, CAT_DEFAULT, "Moving from {0} to {1} ({2})", oldContainer, newContainer, newContainerLdapPath);
                directoryEntry.MoveTo(newContainerDe);
                LOGGER.TraceEvent(TraceEventType.Verbose, CAT_DEFAULT, "Move OK");
                newContainerDe.Dispose();
            }
            catch (Exception e)
            {
                LOGGER.TraceEvent(TraceEventType.Information, CAT_DEFAULT, "Exception caught when moving: {0}", e);
                if (nameChanged)
                {
                    LOGGER.TraceEvent(TraceEventType.Information, CAT_DEFAULT, "Renaming back from temporary name of {0} to {1}", temporaryName, oldName);
                    directoryEntry.Rename(oldName);
                    LOGGER.TraceEvent(TraceEventType.Verbose, CAT_DEFAULT, "Rename OK");
                }
                throw e;
            }

            // step 3: if WITH rename, then rename from temporary name to the new name
            if (nameChanged)
            {
                LOGGER.TraceEvent(TraceEventType.Verbose, CAT_DEFAULT, "Renaming from temporary name of {0} to a new name of {1}", temporaryName, newName);
                directoryEntry.Rename(newName);
                LOGGER.TraceEvent(TraceEventType.Verbose, CAT_DEFAULT, "Rename OK");
            }
        }

        public static SecureString GetSecureString(String stringToSecure)
        {
            SecureString secure = new SecureString();

            foreach (char nextChar in stringToSecure)
            {
                secure.AppendChar(nextChar);
            }

            return secure;
        } 

        internal static string GetDnFromPath(string fullPath)
        {
            IADsPathname pathName = new PathnameClass();
            pathName.Set(fullPath, (int)ADS_SETTYPE_ENUM.ADS_SETTYPE_FULL);
            return pathName.Retrieve((int)ADS_FORMAT_ENUM.ADS_FORMAT_X500_DN);
        }

        internal static DomainController GetDomainController(ActiveDirectoryConfiguration configuration)
        {
            String serverName = configuration.LDAPHostName;
            DomainController controller = null;

            if ((serverName == null) || (serverName.Length == 0))
            {
                // get the active directory schema
                DirectoryContext context = new DirectoryContext(
                        DirectoryContextType.Domain,
                        configuration.DomainName,
                        configuration.DirectoryAdminName,
                        configuration.DirectoryAdminPassword);
                controller = DomainController.FindOne(context);
            }
            else
            {
                DirectoryContext context = new DirectoryContext(
                        DirectoryContextType.DirectoryServer,
                        configuration.LDAPHostName,
                        configuration.DirectoryAdminName,
                        configuration.DirectoryAdminPassword);
                controller = DomainController.GetDomainController(context);
            }

            return controller;
        }

        public static string GetDomainControllerName(ActiveDirectoryConfiguration configuration)
        {
            string serverName = configuration.LDAPHostName;
            if (string.IsNullOrEmpty(serverName))
            {
                // serverless
                using (DirectoryEntry rootDe = new DirectoryEntry("LDAP://RootDSE",
                    configuration.DirectoryAdminName, configuration.DirectoryAdminPassword))
                {
                    serverName = rootDe.Properties["dnsHostName"].Value as string;
                }
            }

            return serverName;
        }

        internal ActiveDirectorySchema GetADSchema()
        {
            String serverName = _configuration.LDAPHostName;
            Forest forest = null;

            if ((serverName == null) || (serverName.Length == 0))
            {
                // get the active directory schema
                LOGGER.TraceEvent(TraceEventType.Verbose, CAT_DEFAULT, "Trying to lookup Domain controller for domain {0}", 
                    _configuration.DomainName);

                DirectoryContext context = new DirectoryContext(
                        DirectoryContextType.Domain,
                        _configuration.DomainName,
                        _configuration.DirectoryAdminName,
                        _configuration.DirectoryAdminPassword);
                
                DomainController dc = DomainController.FindOne(context);
                LOGGER.TraceEvent(TraceEventType.Verbose, CAT_DEFAULT, "Found Domain controller named {0} with ipAddress {1} for domain {2}",
                    dc.Name, dc.IPAddress, _configuration.DomainName);
                forest = dc.Forest;
                LOGGER.TraceEvent(TraceEventType.Verbose, CAT_DEFAULT, "Found forest");
            }
            else
            {
                DirectoryContext context = new DirectoryContext(
                        DirectoryContextType.DirectoryServer,
                        _configuration.LDAPHostName,
                        _configuration.DirectoryAdminName,
                        _configuration.DirectoryAdminPassword);
                forest = Forest.GetForest(context);
            }

            LOGGER.TraceEvent(TraceEventType.Verbose, CAT_DEFAULT, "Getting schema from AD");
            ActiveDirectorySchema ADSchema = forest.Schema;
            LOGGER.TraceEvent(TraceEventType.Verbose, CAT_DEFAULT, "Got schema from AD");

            return ADSchema;
        }

        // gets a long from a LargeInteger (COM object)
        static internal ulong GetLongFromLargeInteger(LargeInteger largeInteger)
        {
            ulong lHigh = 0;
            ulong lLow = 0;
            unchecked
            {
                lHigh = (uint)largeInteger.HighPart;
                lLow = (uint)largeInteger.LowPart;
            }

            ulong retVal = (ulong) lHigh;
            retVal = (retVal << 32);
            retVal = retVal + (ulong)lLow;
            return retVal;
        }

        // sets a LargeInteger (COM object) from a long
        static internal LargeInteger GetLargeIntegerFromLong(Int64 int64Value)
        {
            LargeInteger largeInteger = new LargeIntegerClass();
            largeInteger.HighPart = (int)(int64Value >> 32); ;
            largeInteger.LowPart = unchecked((int)(int64Value & 0xFFFFFFFF));
            return largeInteger;
        }

        /// <summary>
        /// Determines whether <paramref name="dn"/> is a valid distinguished name.
        /// </summary>
        /// <param name="dn">The string representation of the distinguished name to validate.</param>
        /// <returns>
        /// 	<c>true</c> if <paramref name="dn"/> is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>A DN is valid if it can be processed by the AD API. This method does not test RFC 2253 compliance,
        /// but only basic syntactical check.</remarks>
        internal static bool IsValidDn(string dn)
        {
            var result = false;
            try
            {
                if (getADSPathname( null, null, dn ) != null)
                {
                    result = true;
                }
            }
            catch (COMException comex)
            {
                if (comex.ErrorCode == -2147463168) //E_ADS_BAD_PATHNAME
                {
                    result = false;
                }
                else
                {
                    throw;
                }
            }
            return result;
        }

        /// <summary>
        /// Converts a DirectoryServicesCOMException into more meaningful ICF exception (e.g. AlreadyExistsException).
        /// </summary>
        /// 
        /// Actually, it is questionable if the exception mapping can be done in a universal way like this,
        /// or whether it has to be specific for individual operations (search, create, update, ...). We
        /// will see.
        public static Exception ComToIcfException(DirectoryServicesCOMException originalException, String message)
        {
            LOGGER.TraceEvent(TraceEventType.Information, CAT_DEFAULT, "ErrorCode = {0}, ExtendedError = {1}, ExtendedErrorMessage = {2}",
                originalException.ErrorCode, originalException.ExtendedError, originalException.ExtendedErrorMessage);

            if (originalException.ErrorCode == -2147463168 ||     // ADS_BAD_PATHNAME
                originalException.ErrorCode == -2147016656)       // LDAP_NO_SUCH_OBJECT
            {
                return new NoSuchAdObjectException(originalException.Message + ": " + message, originalException);                       
                        // not sure if the exception is related to the object as a whole, or to some of its attributes
                        // therefore we don't return UnknownUidException directly
            }
            else if (originalException.ErrorCode == -2147217911)  // ADO_PERMISSION_DENIED
            {
                return new PermissionDeniedException(originalException.Message + ": " + message, originalException);
            }
            else if (originalException.ErrorCode == -2147024891)    // ADS_INSUFFICIENT_RIGHTS
            {
                return new PermissionDeniedException(originalException.Message + ": " + message, originalException);
            }
            else if (originalException.ErrorCode == -2147023570)    // LDAP_INVALID_CREDENTIALS
            {
                return new InvalidCredentialException(originalException.Message + ": " + message, originalException);
            }
            else if (originalException.ErrorCode == -2147019886)    // LDAP_ALREADY_EXISTS
            {
                return new AlreadyExistsException(originalException.Message + ": " + message, originalException);
            }
            else if (originalException.ErrorCode == -2147016691)    // LDAP_ATTRIBUTE_OR_VALUE_EXISTS This error occurs primarily when you try to add members to groups that have been members of this group beforehand.
            {
                return originalException;       // if we returned AlreadyExistsException here the caller might be confused WHAT does 'already exist'
            }
            else if (originalException.ErrorCode == -2147016657)    // LDAP_CONSTRAINT_VIOLATION
            {
                return originalException;       // here will be something like SchemaException when it will be available
            }
            else
            {
                return originalException;
            }
        }

        /// <summary>
        /// Converts a System.Runtime.InteropServices.COMException into more meaningful ICF exception (e.g. AlreadyExistsException).
        /// </summary>
        /// 
        /// Actually, it is questionable if the exception mapping can be done in a universal way like this,
        /// or whether it has to be specific for individual operations (search, create, update, ...). We
        /// will see.
        public static Exception OtherComToIcfException(System.Runtime.InteropServices.COMException originalException, String message)
        {
            LOGGER.TraceEvent(TraceEventType.Information, CAT_DEFAULT, "ErrorCode = {0}", originalException.ErrorCode);

            if (originalException.ErrorCode == -2147022651)    // password too weak
            {
                return new ArgumentException(originalException.Message + ": " + message, originalException);       /* see also https://groups.google.com/d/msg/connid-dev/i4-N22CARZ8/S-1Yv-iqWBUJ */
            }
            else
            {
                return originalException;
            }
        }
    }

    public class NoSuchAdObjectException : ConnectorException {
        public NoSuchAdObjectException()
            : base() {
        }

        public NoSuchAdObjectException(string message)
            : base(message) {
        }

        public NoSuchAdObjectException(Exception ex)
            : base(ex) {
        }

        public NoSuchAdObjectException(string message, Exception ex)
            : base(message, ex) {
        }
    }


}
