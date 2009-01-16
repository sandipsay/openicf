/*
 * ====================
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS HEADER.
 * 
 * Copyright 2007-2008 Sun Microsystems, Inc. All rights reserved.     
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
 */
using System;
using System.Security;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Org.IdentityConnectors.Common;
using Org.IdentityConnectors.Common.Security;

using Org.IdentityConnectors.Framework.Spi;
using Org.IdentityConnectors.Framework.Spi.Operations;
using Org.IdentityConnectors.Framework.Api.Operations;
using Org.IdentityConnectors.Framework.Common.Serializer;
using Org.IdentityConnectors.Framework.Common.Objects.Filters;
namespace Org.IdentityConnectors.Framework.Common.Objects
{
    #region ConnectorAttributeUtil
    public static class ConnectorAttributeUtil {

        /**
         * Gets the string value from the single value attribute.
         * 
         * @param attr
         *            ConnectorAttribute to retrieve the string value from.
         * @return null if the value is null otherwise the string value for the
         *         attribute.
         * @throws ClassCastException
         *             iff the object in the attribute is not an string.
         * @throws IllegalArgumentException
         *             iff the attribute is a multi valued instead of single valued.
         */
        public static string GetStringValue(ConnectorAttribute attr) {
            return (string)GetSingleValue(attr);
        }
    
        /**
         * Gets the string value from the single value attribute.
         * 
         * @param attr
         *            ConnectorAttribute to retrieve the string value from.
         * @return null if the value is null otherwise the string value for the
         *         attribute.
         * @throws IllegalArgumentException
         *             iff the attribute is a multi valued instead of single valued.
         */
        public static string GetAsStringValue(ConnectorAttribute attr) {
            object obj = GetSingleValue(attr);
            return obj != null ? obj.ToString() : null;
        }

        public static GuardedString GetGuardedStringValue(ConnectorAttribute attr) {
            object obj = GetSingleValue(attr);
            return obj != null ? (GuardedString)obj : null;
        }
        /**
         * Gets the integer value from the single value attribute.
         * 
         * @param attr
         *            ConnectorAttribute to retrieve the integer value from.
         * @return null if the value is null otherwise the integer value for the
         *         attribute.
         * @throws ClassCastException
         *             iff the object in the attribute is not an integer.
         * @throws IllegalArgumentException
         *             iff the attribute is a multi valued instead of single valued.
         */
        public static int? GetIntegerValue(ConnectorAttribute attr) {
            object obj = GetSingleValue(attr);
            return obj != null ? (int?) obj : null;
        }
    
        /**
         * Gets the long value from the single value attribute.
         * 
         * @param attr
         *            ConnectorAttribute to retrieve the long value from.
         * @return null if the value is null otherwise the long value for the
         *         attribute.
         * @throws ClassCastException
         *             iff the object in the attribute is not an long.
         * @throws IllegalArgumentException
         *             iff the attribute is a multi valued instead of single valued.
         */
        public static long? GetLongValue(ConnectorAttribute attr) {
            Object obj = GetSingleValue(attr);
            return obj != null ? (long?) obj : null;
        }
 
        /**
         * Gets the date value from the single value attribute.
         * 
         * @param attr
         *            ConnectorAttribute to retrieve the date value from.
         * @return null if the value is null otherwise the date value for the
         *         attribute.
         * @throws ClassCastException
         *             iff the object in the attribute is not an long.
         * @throws IllegalArgumentException
         *             iff the attribute is a multi valued instead of single valued.
         */
        public static DateTime? GetDateTimeValue(ConnectorAttribute attr) {
            long? val = GetLongValue(attr);
            if (val != null) {
                return DateTimeUtil.GetDateTimeFromUtcMillis(val.Value);
            }
            return null;
        }

        /**
         * Gets the integer value from the single value attribute.
         * 
         * @param attr
         *            ConnectorAttribute to retrieve the integer value from.
         * @return null if the value is null otherwise the integer value for the
         *         attribute.
         * @throws ClassCastException
         *             iff the object in the attribute is not an integer.
         * @throws IllegalArgumentException
         *             iff the attribute is a multi valued instead of single valued.
         */
        public static double? GetDoubleValue(ConnectorAttribute attr) {
            Object obj = GetSingleValue(attr);
            return obj != null ? (double?) obj : null;
        }
        
        public static bool? GetBooleanValue(ConnectorAttribute attr) {
            object obj = GetSingleValue(attr);
            return obj != null ? (bool?) obj : null;
        }

        /**
         * Get the single value from the ConnectorAttribute.
         */
        public static object GetSingleValue(ConnectorAttribute attr) {
            Object ret = null;
            IList<Object> val = attr.Value;
            if (val != null) {
                // make sure this only called for single value..
                if (val.Count > 1) {
                    const string MSG = "The method is only for single value attributes.";
                    throw new ArgumentException(MSG);
                }
                ret = val[0];
            }
            return ret;
        }
        
        /// <summary>
        /// Transform a <code>Collection</code> of <code></code>ConnectorAttribute} instances into a {@link Map}.
        /// The key to each element in the map is the <i>name</i> of an <code>ConnectorAttribute</code>.
        /// The value of each element in the map is the <code>ConnectorAttribute</code> instance with that name.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static IDictionary<string, ConnectorAttribute> ToMap(
            ICollection<ConnectorAttribute> attributes) {
            IDictionary<string, ConnectorAttribute> ret = 
                new Dictionary<string, ConnectorAttribute>(
                    StringComparer.OrdinalIgnoreCase);
            foreach (ConnectorAttribute attr in attributes) {
                ret[attr.Name] = attr;
            }
            return ret;
        }
    
        /**
         * Get the {@link Uid} from the attribute set.
         * 
         * @param attrs
         *            set of {@link ConnectorAttribute}s that may contain a {@link Uid}.
         * @return null if the set does not contain a {@link Uid} object the first
         *         one found.
         */
        public static Uid GetUidAttribute(ICollection<ConnectorAttribute> attrs) {
            return (Uid)Find(Uid.NAME, attrs);
        }
    
        /**
         * Filters out all special attributes from the set. These special attributes
         * include {@link Password}, {@link Uid} etc..
         * 
         * @param attrs
         *            set of {@link ConnectorAttribute}s to filter out the operational and
         *            default attributes.
         * @return a set that only contains plain attributes or empty.
         */
        public static ICollection<ConnectorAttribute> GetBasicAttributes(ICollection<ConnectorAttribute> attrs) {
            ICollection<ConnectorAttribute> ret = new HashSet<ConnectorAttribute>();
            foreach (ConnectorAttribute attr in attrs) {
                // note this is dangerous because we need to be consistent
                // in the naming of special attributes.
                if (!IsSpecial(attr)) {
                    ret.Add(attr);
                }
            }
            return ret;
        }
        /**
         * Filter out any basic attributes from the specified set, leaving only
         * special attributes. Special attributes include {@link Name}, {@link Uid},
         * and {@link OperationalAttributes}.
         * 
         * @param attrs
         *            set of {@link Attribute}s to filter out the basic attributes
         * @return a set that only contains special attributes or an empty set if
         *         there are none.
         */
        public static ICollection<ConnectorAttribute> GetSpecialAttributes(ICollection<ConnectorAttribute> attrs) {
            ICollection<ConnectorAttribute> ret = new HashSet<ConnectorAttribute>();
            foreach (ConnectorAttribute attr in attrs) {
                if (IsSpecial(attr)) {
                    ret.Add(attr);
                }
            }
            return ret;
        }
        
        /**
         * Returns a mutable copy of the original set with the uid attribute removed.
         * @param attrs The original set. Must not be null.
         * @return A mutable copy of the original set with the uid attribute removed.
         */
        public static ICollection<ConnectorAttribute> FilterUid(ICollection<ConnectorAttribute> attrs) {
            Assertions.NullCheck(attrs, "attrs");
            HashSet<ConnectorAttribute> ret = new HashSet<ConnectorAttribute>();
            foreach (ConnectorAttribute attr in attrs) {
                if (!(attr is Uid)) {
                    ret.Add(attr);
                }
            }
            return ret;        
        }
        
        /**
         * Returns a mutable copy of the original set with the uid attribute added.
         * @param attrs The original set. Must not be null.
         * @param uid The uid. Must not be null.
         * @return A mutable copy of the original set with the uid attribute added.
         */
        public static ICollection<ConnectorAttribute> AddUid(ICollection<ConnectorAttribute> attrs, Uid uid) {
            Assertions.NullCheck(attrs, "attrs");
            Assertions.NullCheck(uid, "uid");
            HashSet<ConnectorAttribute> ret = new HashSet<ConnectorAttribute>(attrs);
            ret.Add(uid);
            return ret;
        }
    
        /**
         * Determines if this attribute is a special attribute.
         * 
         * @param attr
         *            {@link ConnectorAttribute} to test for against.
         * @return true iff the attribute value is a {@link Uid},
         *         {@link ObjectClass}, {@link Password}, or
         *         {@link OperationalAttributes}.
         * @throws NullPointerException
         *             iff the attribute parameter is null.
         */
        public static bool IsSpecial(ConnectorAttribute attr) {
            // note this is dangerous because we need to be consistent
            // in the naming of special attributes.
            String name = attr.Name;
            return IsSpecialName(name);
        }
        
        /**
         * Determines if this attribute is a special attribute.
         * 
         * @param attr
         *            {@link ConnectorAttribute} to test for against.
         * @return true iff the attribute value is a {@link Uid},
         *         {@link ObjectClass}, {@link Password}, or
         *         {@link OperationalAttributes}.
         * @throws NullPointerException
         *             iff the attribute parameter is null.
         */
        public static bool IsSpecial(ConnectorAttributeInfo attr) {
            // note this is dangerous because we need to be consistent
            // in the naming of special attributes.
            String name = attr.Name;
            return IsSpecialName(name);
        }
        
        private static bool IsSpecialName(String name) {
            // note this is dangerous because we need to be consistent
            // in the naming of special attributes.
            return (name.StartsWith("@@") && name.EndsWith("@@"));        
        }
        
        /// <summary>
        /// Creates the special naming for operational type attributes.
        /// </summary>
        /// <param name="name">string to make special</param>
        /// <returns>name constructed for use as an operational attribute.</returns>
    	public static string CreateSpecialName(string name) {
        	if (StringUtil.IsBlank(name)) {
            	const string ERR = "Name parameter must not be blank!";
            	throw new ArgumentException(ERR);
        	}
        	return "@@" + name + "@@";
    	}
    
	    /// <summary>
	    /// Gets the 'Name' attribute from a set of ConnectorAttributes.
	    /// </summary>
	    /// <param name="attrs">set of attributes to search against.</param>
	    /// <returns>the 'Name' attribute it if exsist otherwise<code>null</code></returns>
	    public static Name GetNameFromAttributes(ICollection<ConnectorAttribute> attrs) {
	        return (Name)Find(Name.NAME, attrs);
	    }
	    
        
        /**
         * Find the {@link ConnectorAttribute} of the given name in the {@link Set}.
         * 
         * @param name
         *            {@link ConnectorAttribute}'s name to search for.
         * @param attrs
         *            {@link Set} of attribute to search.
         * @return {@link ConnectorAttribute} with the specified otherwise <code>null</code>.
         */
        public static ConnectorAttribute Find(string name, ICollection<ConnectorAttribute> attrs) {
            Assertions.NullCheck(name, "name");
            ICollection<ConnectorAttribute> attributes = CollectionUtil.NullAsEmpty(attrs);
            foreach (ConnectorAttribute attr in attributes) {
                if (attr.Is(name)) {
                    return attr;
                }
            }
            return null;
        }
        /**
         * Get the password value from the provided set of {@link ConnectorAttribute}s.
         */
        public static GuardedString GetPasswordValue(ICollection<ConnectorAttribute> attrs) {
            ConnectorAttribute pwd = Find(OperationalAttributes.PASSWORD_NAME, attrs);
            return (pwd == null) ? null : GetGuardedStringValue(pwd);
        }
    
        /**
         * Get the current password value from the provided set of {@link Attribute}s.
         * 
         * @param attrs
         *            Set of {@link Attribute}s that may contain the current password
         *            {@link OperationalAttributes#CURRENT_PASSWORD_NAME}
         *            {@link Attribute}.
         * @return <code>null</code> if it does not exist in the {@link Set} else
         *         the value.
         */
        public static GuardedString GetCurrentPasswordValue(ICollection<ConnectorAttribute> attrs) {
            ConnectorAttribute pwd = Find(OperationalAttributes.CURRENT_PASSWORD_NAME, attrs);
            return (pwd == null) ? null : GetGuardedStringValue(pwd);
        }    
        /**
         * Determine if the {@link ConnectorObject} is locked out. By getting the
         * value of the {@link OperationalAttributes#LOCK_OUT_NAME}.
         * 
         * @param obj
         *            {@link ConnectorObject} object to inspect.
         * @throws NullPointerException
         *             iff the parameter 'obj' is <code>null</code>.
         * @return <code>null</code> if the attribute does not exist otherwise to
         *         value of the {@link ConnectorAttribute}.
         */
        public static bool? IsLockedOut(ConnectorObject obj) {
            ConnectorAttribute attr = obj.GetAttributeByName(OperationalAttributes.LOCK_OUT_NAME);
            return (attr == null) ?  null : GetBooleanValue(attr);
        }
    
        /**
         * Determine if the {@link ConnectorObject} is enable. By getting the value
         * of the {@link OperationalAttributes#ENABLE_NAME}.
         * 
         * @param obj
         *            {@link ConnectorObject} object to inspect.
         * @throws IllegalStateException
         *             if the object does not contain attribute in question.
         * @throws NullPointerException
         *             iff the parameter 'obj' is <code>null</code>.
         * @return <code>null</code> if the attribute does not exist otherwise to
         *         value of the {@link ConnectorAttribute}.
         */
        public static bool? IsEnabled(ConnectorObject obj) {
            ConnectorAttribute attr = obj.GetAttributeByName(OperationalAttributes.ENABLE_NAME);
            return (attr == null) ?  null : GetBooleanValue(attr);
        }
    
        /**
         * Retrieve the password expiration date from the {@link ConnectorObject}.
         * 
         * @param obj
         *            {@link ConnectorObject} object to inspect.
         * @throws IllegalStateException
         *             if the object does not contain attribute in question.
         * @throws NullPointerException
         *             iff the parameter 'obj' is <code>null</code>.
         * @return <code>null</code> if the {@link ConnectorAttribute} does not exist
         *         otherwise the value of the {@link ConnectorAttribute}.
         */
        public static DateTime? GetPasswordExpirationDate(ConnectorObject obj) {
            DateTime? ret = null;
            ConnectorAttribute attr = obj.GetAttributeByName(OperationalAttributes.PASSWORD_EXPIRATION_DATE_NAME);
            if (attr != null) {
                long? date = GetLongValue(attr);
                if (date != null) {
                    ret = DateTime.FromFileTimeUtc(date.Value);
                }
            }
            return ret;
        }
        /**
         * Get the password expired attribute from a {@link Collection} of
         * {@link Attribute}s.
         * 
         * @param attrs
         *            set of attribute to find the expired password
         *            {@link Attribute}.
         * @return <code>null</code> if the attribute does not exist and the value
         *         of the {@link Attribute} if it does.
         */
        public static bool? GetPasswordExpired(ICollection<ConnectorAttribute> attrs) {
            ConnectorAttribute pwd = Find(OperationalAttributes.PASSWORD_EXPIRED_NAME, attrs);
            return (pwd == null) ? null : GetBooleanValue(pwd);
        }
        
        /**
         * Determine if the password is expired for this object.
         * 
         * @param obj
         *            {@link ConnectorObject} that should contain a password expired
         *            attribute.
         * @return <code>null</code> if the attribute does not exist and the value
         *         of the {@link Attribute} if it does.
         */
        public static bool? IsPasswordExpired(ConnectorObject obj) {
            ConnectorAttribute pwd = obj.GetAttributeByName(OperationalAttributes.PASSWORD_EXPIRED_NAME);
            return (pwd == null) ? null : GetBooleanValue(pwd);
        }

        /**
         * Get the enable date from the set of attributes.
         * 
         * @param attrs
         *            set of attribute to find the enable date
         *            {@link Attribute}.
         * @return <code>null</code> if the attribute does not exist and the value
         *         of the {@link Attribute} if it does.
         */
        public static DateTime? GetEnableDate(ICollection<ConnectorAttribute> attrs) {
            ConnectorAttribute attr = Find(OperationalAttributes.ENABLE_DATE_NAME, attrs);
            return (attr == null) ? null : GetDateTimeValue(attr);
        }
    }
    #endregion    
    
    #region ConnectorAttributeInfoUtil
    public static class ConnectorAttributeInfoUtil {
        /**
         * Transform a <code>Collection</code> of {@link AttributeInfo} instances into
         * a {@link Map}. The key to each element in the map is the <i>name</i> of
         * an <code>AttributeInfo</code>. The value of each element in the map is the
         * <code>AttributeInfo</code> instance with that name.
         * 
         * @param attributes
         *            set of AttributeInfo to transform to a map.
         * @return a map of string and AttributeInfo.
         * @throws NullPointerException
         *             iff the parameter <strong>attributes</strong> is
         *             <strong>null</strong>.
         */
        public static IDictionary<string, ConnectorAttributeInfo> ToMap(
                ICollection<ConnectorAttributeInfo> attributes) {
            IDictionary<string, ConnectorAttributeInfo> 
                ret = new Dictionary<string, ConnectorAttributeInfo>(
                    StringComparer.OrdinalIgnoreCase);
            foreach (ConnectorAttributeInfo attr in attributes) {
                ret[attr.Name] = attr;
            }
            return ret;
        }
    
        /**
         * Find the {@link AttributeInfo} of the given name in the {@link Set}.
         * 
         * @param name
         *            {@link AttributeInfo}'s name to search for.
         * @param attrs
         *            {@link Set} of AttributeInfo to search.
         * @return {@link AttributeInfo} with the specified otherwise <code>null</code>.
         */
        public static ConnectorAttributeInfo Find(string name, ICollection<ConnectorAttributeInfo> attrs) {
            Assertions.NullCheck(name, "name");
            ICollection<ConnectorAttributeInfo> attributes = CollectionUtil.NullAsEmpty(attrs);
            foreach (ConnectorAttributeInfo attr in attributes) {
                if (attr.Is(name)) {
                    return attr;
                }
            }
            return null;
        }
    }
    #endregion

    #region BigDecimal
    /// <summary>
    /// Placeholder since C# doesn't have a BigInteger
    /// </summary>
    public sealed class BigDecimal {
        private BigInteger _unscaledVal;
        private int _scale;
        public BigDecimal(BigInteger unscaledVal,
                         int scale) {
            if ( unscaledVal == null ) {
                throw new ArgumentNullException();
            }
            _unscaledVal = unscaledVal;
            _scale = scale;
        }
        public BigInteger UnscaledValue {
            get {
                return _unscaledVal;
            }
        }
        public int Scale {
            get {
                return _scale;
            }
        }
        public override bool Equals(object o) {
            BigDecimal other = o as BigDecimal;
            if ( other != null ) {
                return UnscaledValue.Equals(other.UnscaledValue) &&
                    Scale == other.Scale;
            }
            return false;
        }
        public override int GetHashCode() {
            return _unscaledVal.GetHashCode();
        }
        
        public override string ToString()
        {
            return UnscaledValue.ToString();
        }
    }
    #endregion
    
    #region BigInteger
    /// <summary>
    /// Placeholder since C# doesn't have a BigInteger
    /// </summary>
    public sealed class BigInteger {
        private string _value;
        public BigInteger(string val) {
            if ( val == null ) {
                throw new ArgumentNullException();
            }
            _value = val;
        }
        public string Value {
            get {
                return _value;
            }
        }
        public override bool Equals(object o) {
            BigInteger other = o as BigInteger;
            if ( other != null ) {
                return Value.Equals(other.Value);
            }
            return false;
        }
        public override int GetHashCode() {
            return _value.GetHashCode();
        }
        public override string ToString() {
            return _value;
        }
    }
    #endregion    
    
    #region ConnectorAttribute
    /// <summary>
    /// Represents a named collection of values within a resource object, 
    /// although the simplest case is a name-value pair (e.g., email, 
    /// employeeID).  Values can be empty, null, or set with various types.
    /// Empty and null are supported because it makes a difference on some 
    /// resources (in particular database resources). The developer of a 
    /// Connector will use an builder to construct an instance of 
    /// ConnectorAttribute.
    /// </summary>
    public class ConnectorAttribute {
        private readonly string _name;
        private readonly IList<object> _value;

        internal ConnectorAttribute(string name, IList<object> val) {
            if (StringUtil.IsBlank(name)) {
                throw new ArgumentException("Name must not be blank!");
            }
            if (OperationalAttributes.PASSWORD_NAME.Equals(name) ||
                OperationalAttributes.CURRENT_PASSWORD_NAME.Equals(name)) {
                // check the value..
                if (val == null || val.Count != 1) {
                    String MSG = "Must be a single value.";
                    throw new ArgumentException(MSG);
                }
                if (!(val[0] is GuardedString)) {
                    const string MSG = "Password value must be an instance of GuardedString.";
                    throw new ArgumentException(MSG);
                }
            }            
            _name = name;
            // copy to prevent corruption preserve null
            _value = (val == null) ? null : CollectionUtil.NewReadOnlyList<object>(val);
        }

        public string Name {
            get {
                return _name;
            }
        }

        public IList<object> Value {
            get {
                return _value;
            }
        }
        
        public bool Is(string name) {
            return Name.ToUpper().Equals(name.ToUpper());
        }
        
        public sealed override bool Equals(Object obj) {
            // test identity
            if (this == obj) {
                return true;
            }
            // test for null..
            if (obj == null) {
                return false;
            }
            // test that the exact class matches
            if (!(GetType().Equals(obj.GetType()))) {
                return false;
            }
            // test name field..
            ConnectorAttribute other = (ConnectorAttribute) obj;
            if (!_name.ToUpper().Equals(other._name.ToUpper())) {
                return false;
            }
            
            if (!CollectionUtil.Equals(_value,other._value)) {
                return false;
            }
            return true;
        }
        
        public sealed override int GetHashCode() {
            return _name.ToUpper().GetHashCode();
        }
        
        
        public override string ToString() {
            // poor man's consistent toString impl..
            StringBuilder bld = new StringBuilder();
            bld.Append("ConnectorAttribute: ");
            IDictionary<string, object> map = new Dictionary<string, object>();
            map["Name"] = Name;
            map["Value"] = Value;
            bld.Append(map.ToString());
            return bld.ToString();
        }        
    }
    #endregion

    #region ConnectorAttributeBuilder
    public sealed class ConnectorAttributeBuilder {
        private const String NAME_ERROR = "Name must not be blank!";

        private string _name;
        private IList<Object> _value;
        
        public ConnectorAttributeBuilder() {
        }
        public static ConnectorAttribute Build(String name) {
            return new ConnectorAttributeBuilder(){ Name=name }.Build();
        }
        public static ConnectorAttribute Build(String name,
                                               params Object [] args) {
            ConnectorAttributeBuilder bld = new ConnectorAttributeBuilder();
            bld.Name = name;
            bld.AddValue(args);
            return bld.Build();
        }
        public static ConnectorAttribute Build(String name,
                                               ICollection<object> val) {
            ConnectorAttributeBuilder bld = new ConnectorAttributeBuilder();
            bld.Name = name;
            bld.AddValue(val);
            return bld.Build();
        }
        
        public string Name {
            get {
                return _name;
            }
            set {
                if (StringUtil.IsBlank(value)) {
                    throw new ArgumentException(NAME_ERROR);
                }
                _name = value;
            }
        }
        
        public IList<Object> Value {
            get {
                return _value == null ? null : CollectionUtil.AsReadOnlyList(_value);
            }
        }
        
        public ConnectorAttributeBuilder AddValue(params Object [] args) {
            AddValuesInternal(args);
            return this;
        }
        public ConnectorAttributeBuilder AddValue(ICollection<Object> values) {
            AddValuesInternal(values);
            return this;
        }
        
        public ConnectorAttribute Build() {
            if (StringUtil.IsBlank(Name)) {
                throw new ArgumentException(NAME_ERROR);
            }
            if (Uid.NAME.Equals(_name)) {
                return new Uid(GetSingleStringValue());
            } else if (Org.IdentityConnectors.Framework.Common.Objects.Name.NAME.Equals(_name)) {
                return new Name(GetSingleStringValue());
            } 
            return new ConnectorAttribute(Name, _value);
        }
        private void CheckSingleValue() {
            if (_value == null || _value.Count != 1) {
                const String MSG = "Must be a single value.";
                throw new ArgumentException(MSG);
            }
        }
        private String GetSingleStringValue() {
            CheckSingleValue();
            if (!(_value[0] is String)) {
                const String MSG = "Must be single string value.";
                throw new ArgumentException(MSG);
            }
            return (String) _value[0];
        }
        private void AddValuesInternal(IEnumerable<Object> values) {
            if (values != null) {
                // make sure the list is ready to receive values.
                if (_value == null) {
                    _value = new List<object>();
                }
                // add each value checking to make sure its correct
                foreach (Object v in values) {
                    FrameworkUtil.CheckAttributeValue(v);
                    _value.Add(v);
                }
            }
        }
        
        // =======================================================================
        // Operational Attributes
        // =======================================================================
        /**
         * Builds an password expiration date {@link ConnectorAttribute}. This
         * {@link ConnectorAttribute} represents the date/time a password will expire on a
         * resource.
         * 
         * @param dateTime
         *            UTC time in milliseconds.
         * @return an {@link ConnectorAttribute} built with the pre-defined name for password
         *         expiration date.
         */
        public static ConnectorAttribute BuildPasswordExpirationDate(DateTime dateTime) {
            return BuildPasswordExpirationDate(DateTimeUtil.GetUtcTimeMillis(dateTime));
        }
    
        /**
         * Builds an password expiration date {@link ConnectorAttribute}. This
         * {@link ConnectorAttribute} represents the date/time a password will expire on a
         * resource.
         * 
         * @param dateTime
         *            UTC time in milliseconds.
         * @return an {@link ConnectorAttribute} built with the pre-defined name for password
         *         expiration date.
         */
        public static ConnectorAttribute BuildPasswordExpirationDate(long dateTime) {
            return Build(OperationalAttributes.PASSWORD_EXPIRATION_DATE_NAME,
                    dateTime);
        }
    
        /**
         * Builds the operational attribute password.
         * 
         * @param password
         *            the string that represents a password.
         * @return an attribute that represents a password.
         */
        public static ConnectorAttribute BuildPassword(GuardedString password) {
            return Build(OperationalAttributes.PASSWORD_NAME, password);
        }
    
        /**
         * Builds the operational attribute current password. The current password
         * indicates this a password change by the account owner and not an
         * administrator. The use case is that an administrator password change may
         * not keep history or validate against policy.
         * 
         * @param password
         *            the string that represents a password.
         * @return an attribute that represents a password.
         */
        public static ConnectorAttribute BuildCurrentPassword(GuardedString password) {
            return Build(OperationalAttributes.CURRENT_PASSWORD_NAME, password);
        }
        
        public static ConnectorAttribute BuildPassword(SecureString password) {
            return Build(OperationalAttributes.PASSWORD_NAME, new GuardedString(password));
        }
        public static ConnectorAttribute BuildCurrentPassword(SecureString password) {
            return Build(OperationalAttributes.CURRENT_PASSWORD_NAME, new GuardedString(password));
        }
        /**
         * Builds ant operational attribute that either represents the object is
         * enabled or sets in disabled depending on where its used for instance on
         * {@link CreateApiOp} it could be used to create a disabled account. In
         * {@link SearchApiOp} it would show the object is enabled or disabled.
         * 
         * @param value
         *            true indicates the object is enabled otherwise false.
         * @return {@link ConnectorAttribute} that determines the enable/disable state of an
         *         object.
         */
        public static ConnectorAttribute BuildEnabled(bool val) {
            return Build(OperationalAttributes.ENABLE_NAME, val);
        }
    
        /**
         * Builds out an operational {@link ConnectorAttribute} that determines the enable
         * date for an object.
         * 
         * @param date
         *            The date and time to enable a particular object, or the date
         *            time an object will be enabled.
         * @return {@link ConnectorAttribute}
         */
        public static ConnectorAttribute BuildEnableDate(DateTime date) {
            return BuildEnableDate(DateTimeUtil.GetUtcTimeMillis(date));
        }
    
        /**
         * Builds out an operational {@link ConnectorAttribute} that determines the enable
         * date for an object. The time parameter is UTC in milliseconds.
         * 
         * @param date
         *            The date and time to enable a particular object, or the date
         *            time an object will be enabled.
         * @return {@link ConnectorAttribute}
         */
        public static ConnectorAttribute BuildEnableDate(long date) {
            return Build(OperationalAttributes.ENABLE_DATE_NAME, date);
        }
    
        /**
         * Builds out an operational {@link ConnectorAttribute} that determines the disable
         * date for an object.
         * 
         * @param date
         *            The date and time to enable a particular object, or the date
         *            time an object will be enabled.
         * @return {@link ConnectorAttribute}
         */
        public static ConnectorAttribute BuildDisableDate(DateTime date) {
            return BuildDisableDate(DateTimeUtil.GetUtcTimeMillis(date));
        }
    
        /**
         * Builds out an operational {@link ConnectorAttribute} that determines the disable
         * date for an object. The time parameter is UTC in milliseconds.
         * 
         * @param date
         *            The date and time to enable a particular object, or the date
         *            time an object will be enabled.
         * @return {@link ConnectorAttribute}
         */
        public static ConnectorAttribute BuildDisableDate(long date) {
            return Build(OperationalAttributes.DISABLE_DATE_NAME, date);
        }
    
        /**
         * Builds the lock attribute that determines if an object is locked out.
         * 
         * @param lock
         *            true if the object is locked otherwise false.
         * @return {@link ConnectorAttribute} that represents the lock state of an object.
         */
        public static ConnectorAttribute BuildLockOut(bool lck) {
            return Build(OperationalAttributes.LOCK_OUT_NAME, lck);
        }
        
        /**
         * Builds out an operational {@link Attribute} that determines if a password
         * is expired or expires a password.
         * 
         * @param value
         *            from the API true expires and from the SPI its shows its
         *            either expired or not.
         * @return {@link Attribute}
         */
        public static ConnectorAttribute BuildPasswordExpired(bool expired) {
            return Build(OperationalAttributes.PASSWORD_EXPIRED_NAME, expired);
        }
    
        // =======================================================================
        // Pre-defined Attributes
        // =======================================================================
    
        /**
         * Builds out a pre-defined {@link ConnectorAttribute} that determines the last login
         * date for an object.
         * 
         * @param date
         *            The date and time of the last login.
         * @return {@link ConnectorAttribute}
         */
        public static ConnectorAttribute BuildLastLoginDate(DateTime date) {
            return BuildLastLoginDate(DateTimeUtil.GetUtcTimeMillis(date));
        }
    
        /**
         * Builds out a pre-defined {@link ConnectorAttribute} that determines the last login
         * date for an object. The time parameter is UTC in milliseconds.
         * 
         * @param date
         *            The date and time of the last login.
         * @return {@link ConnectorAttribute}
         */
        public static ConnectorAttribute BuildLastLoginDate(long date) {
            return Build(PredefinedAttributes.LAST_LOGIN_DATE_NAME, date);
        }
    
        /**
         * Builds out a pre-defined {@link ConnectorAttribute} that determines the last
         * password change date for an object.
         * 
         * @param date
         *            The date and time the password was changed.
         * @return {@link ConnectorAttribute}
         */
        public static ConnectorAttribute BuildLastPasswordChangeDate(DateTime date) {
            return BuildLastPasswordChangeDate(DateTimeUtil.GetUtcTimeMillis(date));
        }
    
        /**
         * Builds out a pre-defined {@link ConnectorAttribute} that determines the last
         * password change date for an object.
         * 
         * @param date
         *            The date and time the password was changed.
         * @return {@link ConnectorAttribute}
         */
        public static ConnectorAttribute BuildLastPasswordChangeDate(long date) {
            return Build(PredefinedAttributes.LAST_PASSWORD_CHANGE_DATE_NAME, date);
        } 
    
        /**
         * Common password policy attribute where the password must be changed every
         * so often. The value for this attribute is milliseconds since its the
         * lowest common denominator.
         */
        public static ConnectorAttribute BuildPasswordChangeInterval(long val) {
            return Build(PredefinedAttributes.PASSWORD_CHANGE_INTERVAL_NAME, val);
        }
    }
    #endregion

    #region ConnectorMessages
    /**
     * Message catalog for a given connector.
     */
    public interface ConnectorMessages {
        /**
         * Formats the given message key in the current UI culture.
         * @param key The message key to format. 
         * @param dflt The default message if key is not found. If null, defaults
         * to key.
         * @param args Parameters with which to format the message.
         * @return The formatted string.
         */
        String Format(String key, String dflt, params object [] args);
    }
    #endregion
    
    #region ConnectorObject
    public sealed class ConnectorObject {
        private readonly ObjectClass _objectClass;
        private readonly IDictionary<string, ConnectorAttribute> _attrs;
        public ConnectorObject(ObjectClass objectClass, ICollection<ConnectorAttribute> attrs) {
            if (objectClass == null) {
                throw new ArgumentException("ObjectClass may not be null");
            }            
            if ( attrs == null || attrs.Count == 0 ) {
                throw new ArgumentException("attrs cannot be empty or null.");
            }
            _objectClass = objectClass;
            _attrs = 
            CollectionUtil.NewReadOnlyDictionary(attrs,
                                         value => { return value.Name;});
            if (!_attrs.ContainsKey(Uid.NAME)) {
                const String MSG = "The ConnectorAttribute set must contain a Uid.";
                throw new ArgumentException(MSG);
            }
            if (!_attrs.ContainsKey(Name.NAME)) {
            	const string MSG = "The ConnectorAttribute set must contain a Name.";
            	throw new ArgumentException(MSG);
            }
        } 
        public ICollection<ConnectorAttribute> GetAttributes() {
            return _attrs.Values;
        }
        public ConnectorAttribute GetAttributeByName(string name) {
            return CollectionUtil.GetValue(_attrs,name,null);
        }
        public Uid Uid {
            get {
                return (Uid)GetAttributeByName(Uid.NAME);
            }
        }
        public Name Name {
            get {
                return (Name)GetAttributeByName(Name.NAME);
            }
        }
        public ObjectClass ObjectClass {
            get {
                return _objectClass;
            }
        }
        public override int GetHashCode() {
            return CollectionUtil.GetHashCode(_attrs);
        }
        public override bool Equals(Object o) {
            ConnectorObject other = o as ConnectorObject;
            if ( other != null ) {
                if (!_objectClass.Equals(other.ObjectClass)) {
                    return false;
                }
                return CollectionUtil.Equals(_attrs,other._attrs);
            }
            return false;
        }
    }
    #endregion
    
    #region ConnectorObjectBuilder
    public sealed class ConnectorObjectBuilder {
        private IDictionary<string, ConnectorAttribute> _attributes;
        public ConnectorObjectBuilder() {
            _attributes = new Dictionary<string, ConnectorAttribute>();
            // default always add the account object class..
            ObjectClass = ObjectClass.ACCOUNT;
        }
        
        public void SetUid(string uid) {
            AddAttribute(new Uid(uid));
        }
    
        public void SetUid(Uid uid) {
            AddAttribute(uid);
        }
        
        public void SetName(string name) {
        	AddAttribute(new Name(name));
        }
        
        public void SetName(Name name) {
        	AddAttribute(name);
        }
        
        public ObjectClass ObjectClass { get; set; }
        
        // =======================================================================
        // Clone basically..
        // =======================================================================
        /**
         * Takes all the attribute from a {@link ConnectorObject} and add/overwrite
         * the current attributes.
         */
        public ConnectorObjectBuilder Add(ConnectorObject obj) {
            // simply add all the attributes it will include (Uid, ObjectClass..)
            foreach (ConnectorAttribute attr in obj.GetAttributes()) {
               AddAttribute(attr);
            }
            ObjectClass = obj.ObjectClass;
            return this;
        }
        
        public ConnectorObjectBuilder AddAttribute(params ConnectorAttribute [] attrs) {
            ValidateParameter(attrs, "attrs");
            foreach (ConnectorAttribute a in attrs) {
                //DONT use Add - it throws exceptions if already there
                _attributes[a.Name] = a;
            }
            return this;
        }
        public ConnectorObjectBuilder AddAttributes(ICollection<ConnectorAttribute> attrs) {
            ValidateParameter(attrs, "attrs");
            foreach (ConnectorAttribute a in attrs) {
                _attributes[a.Name] = a;
            }
            return this;
        }
        /**
         * Adds values to the attribute.
         */
        public ConnectorObjectBuilder AddAttribute(String name, params object [] objs) {
            AddAttribute(ConnectorAttributeBuilder.Build(name, objs));
            return this;
        }

        /**
         * Adds each object in the collection.
         */
        public ConnectorObjectBuilder AddAttribute(String name, ICollection<object> obj) {
            AddAttribute(ConnectorAttributeBuilder.Build(name, obj));
            return this;
        }
        public ConnectorObject Build() {
            // check that there are attributes to return..
            if (_attributes.Count == 0) {
                throw new InvalidOperationException("No attributes set!");
            }
            return new ConnectorObject(ObjectClass,_attributes.Values);
        }
        private static void ValidateParameter(Object param, String paramName) {
            if (param == null) {
                String FORMAT = "Parameter "+param+" must not be null!";
                throw new NullReferenceException(FORMAT);
            }
        }
    }
    #endregion

    #region ConnectorAttributeInfo
    public sealed class ConnectorAttributeInfo {
        private readonly string _name;
        private readonly Type _type;
        private readonly Flags _flags;
        
    	/**
    	 * Enum of modifier flags to use for attributes. Note that
    	 * this enum is designed for configuration by exception such that
    	 * an empty set of flags are the defaults:
    	 * <ul>
         *     <li>updateable</li>
         *     <li>creatable</li>
         *     <li>returned by default</li>
         *     <li>readable</li>
         *     <li>single-valued</li>
         *     <li>optional</li>
         * </ul>
    	 */
    	[FlagsAttribute]
    	public enum Flags {
    	    NONE = 0,
    	    REQUIRED = 1,
    	    MULTIVALUED = 2,
    	    NOT_CREATABLE = 4,
    	    NOT_UPDATEABLE = 8,
    	    NOT_READABLE = 16,
    	    NOT_RETURNED_BY_DEFAULT = 32
    	}

        internal ConnectorAttributeInfo(string name, Type type,
                Flags flags) {
            if (StringUtil.IsBlank(name)) {
                throw new ArgumentException("Name must not be blank!");
            }
            if ((OperationalAttributes.PASSWORD_NAME.Equals(name) ||
                    OperationalAttributes.CURRENT_PASSWORD_NAME.Equals(name)) &&
                    !typeof(GuardedString).Equals(type)) {
                String MSG = "Password based attributes must be of type GuardedString.";
                throw new ArgumentException(MSG);
            }
            // check the type..
            FrameworkUtil.CheckAttributeType(type);
            _name = name;
            _type = type;
            _flags = flags;
    		if (!IsReadable && IsReturnedByDefault) {
    		    throw new ArgumentException("Attribute "+name+" is flagged as not-readable, so it should also be as not-returned-by-default.");
    		}
        }

        
        /**
         * The native name of the attribute.
         * 
         * @return the native name of the attribute its describing.
         */
        public string Name {
            get {
                return _name;
            }
        }

        /**
         * The basic type associated with this attribute. All primitives are
         * supported.
         * 
         * @return the native type if uses.
         */
        public Type ValueType {
            get {
                return _type;
            }
        }
        
    	/**
    	 * Returns the set of flags associated with the attribute.
    	 * @return the set of flags associated with the attribute
    	 */
    	public Flags InfoFlags {
    	    get {
        	    return _flags;
    	    }
    	}
        

        public bool Is(string name) {
            return Name.ToUpper().Equals(name.ToUpper());
        }

        /**
         * Determines if the attribute is readable.
         * 
         * @return true if the attribute is readable else false.
         */
        public bool IsReadable {
            get {
                return (_flags & Flags.NOT_READABLE) == 0;
            }
        }

        /**
         * Determines if the attribute is writable on create.
         * 
         * @return true if the attribute is writable on create else false.
         */
        public bool IsCreatable {
            get {
                return (_flags & Flags.NOT_CREATABLE) == 0;
            }
        }
        
    	/**
    	 * Determines if the attribute is writable on update.
    	 * 
    	 * @return true if the attribute is writable on update else false.
    	 */
    	public bool IsUpdateable {
    	    get {
    	        return (_flags & Flags.NOT_UPDATEABLE) == 0;
    	    }
    	}

        /**
         * Determines whether this attribute is required for creates.
         * 
         * @return true if the attribute is required for an object else false.
         */
        public bool IsRequired {
            get {
                return (_flags & Flags.REQUIRED) != 0;
            }
        }

        /**
         * Determines if this attribute can handle multiple values. There is a
         * special case with byte[] since in most instances this denotes a single
         * object.
         * 
         * @return true if the attribute is multi-value otherwise false.
         */
        public bool IsMultiValued {
            get {
                return (_flags & Flags.MULTIVALUED) != 0;
            }
        }
        
       /**
        * Determines if the attribute is returned by default. Indicates if an
        * {@link ConnectorAttribute} will be returned during {@link SearchApiOp} or
        * {@link GetApiOp} inside a {@link ConnectorObject} by default. The default
        * value is <code>true</code>.
        * 
        * @return false iff the attribute should not be returned by default.
        */
        public bool IsReturnedByDefault {
            get {
                return (_flags & Flags.NOT_RETURNED_BY_DEFAULT) == 0;
            }
        }
        
        public override bool Equals(Object o) {
            ConnectorAttributeInfo other = o as ConnectorAttributeInfo;
            if ( other != null ) {
                if (!Name.ToUpper().Equals(other.Name.ToUpper())) {
                    return false;
                }
                if (!ValueType.Equals(other.ValueType)) {
                    return false;
                }
                if (_flags != other._flags) {
                    return false;
                }
                return true;
            }
            return false;
        }
        
        public override int GetHashCode() {
            return _name.ToUpper().GetHashCode();
        }
       
        public override string ToString() {
           return SerializerUtil.SerializeXmlObject(this,false);
        }
    }
    #endregion

    #region ConnectorAttributeInfoBuilder
    /**
     * Simplifies the process of building 'AttributeInfo' objects. This class is
     * responsible for providing a default implementation of {@link AttributeInfo}.
     * 
     * <code>
     * AttributeInfoBuilder bld = new AttributeInfoBuilder("email");
     * bld.setRequired(true);
     * AttributeInfo info = bld.build();
     * </code>
     * 
     * @author Will Droste
     * @version $Revision: 1.9 $
     * @since 1.0
     */
    public sealed class ConnectorAttributeInfoBuilder {
    
    	private String _name;
    	private Type _type;
    	private ConnectorAttributeInfo.Flags _flags;
    
        /**
         * Creates an builder with all the defaults set. The name must be set before
         * the 'build' method is called otherwise an {@link IllegalStateException}
         * is thrown.
         * 
         * <pre>
         * Name: &lt;not set&gt;
         * Readable: true
         * Writeable: true
         * Required: false
         * Type: string
         * MultiValue: false
         * </pre>
         */
        public ConnectorAttributeInfoBuilder() {
            ValueType=(typeof(String));
            _flags = ConnectorAttributeInfo.Flags.NONE;
        }
        
        /**
         * Creates an builder with all the defaults set. The name must be set before
         * the 'build' method is called otherwise an {@link IllegalStateException}
         * is thrown.
         * 
         * <pre>
         * Name: &lt;not set&gt;
         * Readable: true
         * Writeable: true
         * Required: false
         * Type: string
         * MultiValue: false
         * </pre>
         */
        public ConnectorAttributeInfoBuilder(String name) : this(name,typeof(String)){
        }
        
    	/**
    	 * Creates an builder with all the defaults set. The name must be set before
    	 * the 'build' method is called otherwise an {@link IllegalStateException}
    	 * is thrown.
    	 * 
    	 * <pre>
    	 * Name: &lt;not set&gt;
    	 * Readable: true
    	 * Writeable: true
    	 * Required: false
    	 * Type: string
    	 * MultiValue: false
    	 * </pre>
    	 */
    	public ConnectorAttributeInfoBuilder(String name, Type type) {
    		Name=(name);
    		ValueType=(type);
    		//noneOf means the defaults
    		_flags = ConnectorAttributeInfo.Flags.NONE;
    	}
    
    	/**
    	 * Builds an {@link AttributeInfo} object based on the properties set.
    	 * 
    	 * @return {@link AttributeInfo} based on the properties set.
    	 */
    	public ConnectorAttributeInfo Build() {
    		return new ConnectorAttributeInfo(_name, _type, _flags);
    	}
    
    	/**
    	 * Sets the unique name of the {@link AttributeInfo} object.
    	 * 
    	 * @param name
    	 *            unique name of the {@link AttributeInfo} object.
    	 */
    	public String Name {
    	    set {
        		if (StringUtil.IsBlank(value)) {
        			throw new ArgumentException("Argument must not be blank.");
        		}
        		_name = value;
    	    }
    	}
    
    	/**
    	 * Please see {@link FrameworkUtil#checkAttributeType(Class)} for the
    	 * definitive list of supported types.
    	 * 
    	 * @param value
    	 *            type for an {@link Attribute}'s value.
    	 * @throws IllegalArgumentException
    	 *             if the Class is not a supported type.
    	 */
    	public Type ValueType {
    	    set {
        		FrameworkUtil.CheckAttributeType(value);
        		_type = value;
    	    }
    	}
    
    	/**
    	 * Determines if the attribute is readable.
    	 */
    	public bool Readable {
    	    set {
        	    SetFlag(ConnectorAttributeInfo.Flags.NOT_READABLE,!value);
    	    }
    	}
    
    	/**
    	 * Determines if the attribute is writable.
    	 */
    	public bool Creatable {
    	    set {
                SetFlag(ConnectorAttributeInfo.Flags.NOT_CREATABLE,!value);
    	    }
    	}
    
    	/**
    	 * Determines if this attribute is required.
    	 */
    	public bool Required {
    	    set {
                SetFlag(ConnectorAttributeInfo.Flags.REQUIRED,value);
    	    }
    	}
    
    	/**
    	 * Determines if this attribute supports multivalue.
    	 */
    	public bool MultiValued {
    	    set {
                SetFlag(ConnectorAttributeInfo.Flags.MULTIVALUED,value);
    	    }
    	}
    
    	/**
    	 * Determines if this attribute writable during update.
    	 */
    	public bool Updateable {
    	    set {
                SetFlag(ConnectorAttributeInfo.Flags.NOT_UPDATEABLE,!value);
    	    }
    	}
    	
    	public bool ReturnedByDefault {
    	    set {
                SetFlag(ConnectorAttributeInfo.Flags.NOT_RETURNED_BY_DEFAULT,!value);
    	    }
    	}
    		
    	/**
    	 * Sets all of the flags for this builder.
    	 * @param flags The set of attribute info flags. Null means clear all flags.
    	 * <p>
    	 * NOTE: EnumSet.noneOf(AttributeInfo.Flags.class) results in
    	 * an attribute with the default behavior:
    	 * <ul>
    	 *     <li>updateable</li>
    	 *     <li>creatable</li>
         *     <li>returned by default</li>
         *     <li>readable</li>
         *     <li>single-valued</li>
         *     <li>optional</li>
    	 * </ul>
    	 */
    	public ConnectorAttributeInfo.Flags InfoFlags {
    	    set {
        	    _flags = value;
    	    }
    	}
    	
        private void SetFlag(ConnectorAttributeInfo.Flags flag, bool value) {
            if (value) {
                _flags = _flags | flag;
            }
            else {
                _flags = _flags & ~flag;
            }
        }
    	
    	/**
    	 * Convenience method to create an AttributeInfo. Equivalent to
    	 * <code>
    	 * new AttributeInfoBuilder(name,type).setFlags(flags).build()
    	 * </code>
    	 * @param name The name of the attribute
    	 * @param type The type of the attribute
    	 * @param flags The flags for the attribute. Null means clear all flags
    	 * @return The attribute info 
    	 */
    	public static ConnectorAttributeInfo Build(String name, Type type,
    	        ConnectorAttributeInfo.Flags flags) {
    	    return new ConnectorAttributeInfoBuilder(name,type){InfoFlags=flags}.Build();
    	}
        /**
         * Convenience method to create an AttributeInfo. Equivalent to
         * <code>
         * AttributeInfoBuilder.build(name,type,null)
         * </code>
         * @param name The name of the attribute
         * @param type The type of the attribute
         * @param flags The flags for the attribute
         * @return The attribute info 
         */
        public static ConnectorAttributeInfo Build(String name, Type type) {
            return Build(name,type,ConnectorAttributeInfo.Flags.NONE);
        }
    
        /**
         * Convenience method to create an AttributeInfo. Equivalent to
         * <code>
         * AttributeInfoBuilder.build(name,type)
         * </code>
         * @param name The name of the attribute
         * @return The attribute info 
         */
        public static ConnectorAttributeInfo Build(String name) {
            return Build(name,typeof(String));
        }
    }
    #endregion
    
    #region FileName
    /// <summary>
    /// Placeholder for java.io.File since C#'s
    /// FileInfo class throws exceptions if the
    /// file doesn't exist.
    /// </summary>
    public sealed class FileName {
        private string _path;
        public FileName(string path) {
            if ( path == null ) {
                throw new ArgumentNullException();
            }
            _path = path;
        }
        public string Path {
            get {
                return _path;
            }
        }
        public override bool Equals(object o) {
            FileName other = o as FileName;
            if ( other != null ) {
                return Path.Equals(other.Path);
            }
            return false;
        }
        public override int GetHashCode() {
            return _path.GetHashCode();
        }
        public override string ToString() {
            return _path;
        }
    }
    #endregion
    
    #region Name
    public sealed class Name : ConnectorAttribute {
        
        public readonly static string NAME = ConnectorAttributeUtil.CreateSpecialName("NAME");
        public readonly static ConnectorAttributeInfo INFO =
            new ConnectorAttributeInfoBuilder(NAME) {Required=true}.Build();
        
        public Name(String value) : base(NAME, CollectionUtil.NewReadOnlyList<object>(value)) {
        }
        
        /**
         * The single value of the attribute that is the unique id of an object.
         * 
         * @return value that identifies an object.
         */
        public String GetNameValue() {
            return ConnectorAttributeUtil.GetStringValue(this);
        }
    }
    #endregion
    
    #region ObjectClass
    public sealed class ObjectClass {
        public const String ACCOUNT_NAME = "account";
        public const String PERSON_NAME = "person";
        public const String GROUP_NAME = "group";
        public const String ORGANIZATION_NAME = "organization";
        /**
         * Denotes an account based object.
         */
        public static readonly ObjectClass ACCOUNT = new ObjectClass(ACCOUNT_NAME); 
        /**
         * Denotes a person based object.
         */
        public static readonly ObjectClass PERSON = new ObjectClass(PERSON_NAME);
        /**
         * Denotes a group based object.
         */
        public static readonly ObjectClass GROUP = new ObjectClass(GROUP_NAME); 
        /**
         * Denotes a organization based object.
         */
        public static readonly ObjectClass ORGANIZATION = new ObjectClass(ORGANIZATION_NAME); 
        
        private readonly String _type;
        
        public ObjectClass(String type) {
            if ( type == null ) {
                throw new ArgumentException("Type cannot be null.");
            }
            _type = type;
        }
        public String GetObjectClassValue() {
            return _type;
        }
        
        /**
         * Convenience method to build the display name key for
         * an object class.  
         * 
         * @return The display name key.
         */
        public String GetDisplayNameKey() {
            return "MESSAGE_OBJECT_CLASS_"+_type.ToUpper();
        }

        /**
         * Determines if the 'name' matches this {@link ObjectClass}.
         * 
         * @param name
         *            case-insensitive string representation of the ObjectClass's
         *            type.
         * @return <code>true</code> if the case-insensitive name is equal to
         *         that of the one in this {@link ObjectClass}.
         */
        public bool Is(String name) {
            return this._type.equalsIgnoreCase(name);    
        }    
        
        public override int GetHashCode() {
            return _type.ToUpper().GetHashCode();
        }
    
        public override bool Equals(object obj) {
            // test identity
            if(this == obj)
            {
                return true;
            }

            // test for null..
            if(obj == null)
            {
                return false;
            }

            // test that the exact class matches
            if(!(GetType().Equals(obj.GetType())))
            {
                return false;
            }

            ObjectClass other = (ObjectClass) obj;

            if(!Is(other._type)) {
                return false;
            }

            return true;
        }
        
        public override string ToString()
        {
            return "ObjectClass: " + _type;
        }
    }
    #endregion
    
    #region ObjectClassInfo
    public sealed class ObjectClassInfo {
        
        private readonly String _type;
        private readonly ICollection<ConnectorAttributeInfo> _info;
        private readonly bool _isContainer;

        public ObjectClassInfo(String type, 
                               ICollection<ConnectorAttributeInfo> attrInfo,
                               bool isContainer) {
            Assertions.NullCheck(type, "type");
            _type = type;
            _info = CollectionUtil.NewReadOnlySet(attrInfo);
            _isContainer = isContainer;
            // check to make sure name exists
            IDictionary<string, ConnectorAttributeInfo> dict 
                = ConnectorAttributeInfoUtil.ToMap(attrInfo);
            if (!dict.ContainsKey(Name.NAME)) {
                const string MSG = "Missing 'Name' connector attribute info.";
                throw new ArgumentException(MSG);
            }
        }

        public ICollection<ConnectorAttributeInfo> ConnectorAttributeInfos {
            get {
                return this._info;
            }
        }
        
        public String ObjectType {
            get {
                return this._type;
            }
        }

        /**
        * Determines if the 'name' matches this {@link ObjectClassInfo}.
        * 
        * @param name
        *            case-insensitive string representation of the ObjectClassInfo's
        *            type.
        * @return <code>true</code> if the case insensitive type is equal to
        *         that of the one in this {@link ObjectClassInfo}.
        */
        public bool Is(String name) {
            return this._type.Equals(name, StringComparison.CurrentCultureIgnoreCase);
        }
        
        public bool IsContainer {
            get {
                return this._isContainer;
            }
        }
        
        public override int GetHashCode() {
            return _type.ToUpper().GetHashCode();
        }
        
        public override bool Equals(Object obj) {
            // test identity
            if (this == obj) {
                return true;
            }

            // test for null..
            if(obj == null)
            {
                return false;
            }

            if (obj.GetType().Equals(this.GetType()))
            {
                return false;
            }

            ObjectClassInfo other = obj as ObjectClassInfo;

            if (!ObjectType.Equals(other.ObjectType, StringComparison.CurrentCultureIgnoreCase)) {
                return false;
            }

            if (!CollectionUtil.Equals(ConnectorAttributeInfos,
                                       other.ConnectorAttributeInfos)) {
                return false;
            }

            if (_isContainer != other._isContainer) {
                return false;
            }

            return true;
        }
        
        public override string ToString()
        {
            return SerializerUtil.SerializeXmlObject(this,false);
        }
    }
    #endregion
        
    #region ObjectClassInfoBuilder
    /**
     * Used to help facilitate the building of {@link ObjectClassInfo} objects.
     */
    public sealed class ObjectClassInfoBuilder {
        private bool _isContainer;
        private IDictionary<string, ConnectorAttributeInfo> _info;
    
        public ObjectClassInfoBuilder() {
            _info = new Dictionary<string, ConnectorAttributeInfo>();
            ObjectType = ObjectClass.ACCOUNT_NAME; 
        }
        
        public string ObjectType { get; set; }
    
        /**
         * Add each {@link AttributeInfo} object to the {@link ObjectClassInfo}.
         */
        public ObjectClassInfoBuilder AddAttributeInfo(ConnectorAttributeInfo info) {
            if (_info.ContainsKey(info.Name)) {
                const string MSG = "ConnectorAttributeInfo of name ''{0}'' already exists!";
                throw new ArgumentException(String.Format(MSG, info.Name));
            }
            _info[info.Name] = info;
            return this;
        }
    
        public ObjectClassInfoBuilder AddAllAttributeInfo(ICollection<ConnectorAttributeInfo> info) {
            foreach (ConnectorAttributeInfo cainfo in info) {
                AddAttributeInfo(cainfo);
            }
        	return this;
        }
        
        public bool IsContainer {
            get {
                return _isContainer;
            }
            set {
                _isContainer = value;
            }
        }
    
        public ObjectClassInfo Build() {
            // determine if name is missing and add it by default
            if (!_info.ContainsKey(Name.NAME)) {
                _info[Name.NAME] = Name.INFO;
            }
            return new ObjectClassInfo(ObjectType, _info.Values, _isContainer);
        }
    }
    #endregion

    #region OperationalAttributeInfos
    /**
     * {@link AttributeInfo} for each operational attribute.
     */
    public static class OperationalAttributeInfos {
        /**
         * Gets/sets the enable status of an object.
         */
        public static readonly ConnectorAttributeInfo ENABLE = 
            ConnectorAttributeInfoBuilder.Build(
                OperationalAttributes.ENABLE_NAME, typeof(bool));
    
        /**
         * Gets/sets the enable date for an object.
         */
        public static readonly ConnectorAttributeInfo ENABLE_DATE = 
            ConnectorAttributeInfoBuilder.Build(
                OperationalAttributes.ENABLE_DATE_NAME, typeof(long));
    
        /**
         * Gets/sets the disable date for an object.
         */
        public static readonly ConnectorAttributeInfo DISABLE_DATE = 
            ConnectorAttributeInfoBuilder.Build(
                OperationalAttributes.DISABLE_DATE_NAME, typeof(long));
    
        /**
         * Gets/sets the lock out attribute for an object.
         */
        public static readonly ConnectorAttributeInfo LOCK_OUT = 
            ConnectorAttributeInfoBuilder.Build(
                OperationalAttributes.LOCK_OUT_NAME, typeof(bool));
    
        /**
         * Gets/sets the password expiration date for an object.
         */
        public static readonly ConnectorAttributeInfo PASSWORD_EXPIRATION_DATE = 
            ConnectorAttributeInfoBuilder.Build(
                OperationalAttributes.PASSWORD_EXPIRATION_DATE_NAME, typeof(long));
    
        /**
         * Normally this is a write-only attribute. Sets the password for an object.
         */
        public static readonly ConnectorAttributeInfo PASSWORD = 
            ConnectorAttributeInfoBuilder.Build(
                OperationalAttributes.PASSWORD_NAME, typeof(GuardedString), 
                ConnectorAttributeInfo.Flags.NOT_READABLE |
                ConnectorAttributeInfo.Flags.NOT_RETURNED_BY_DEFAULT);
    
        /**
         * Used in conjunction with password to do an account level password change.
         * This is for a non-administrator change of the password and therefore
         * requires the current password.
         */
        public static readonly ConnectorAttributeInfo CURRENT_PASSWORD = 
            ConnectorAttributeInfoBuilder.Build(
                OperationalAttributes.CURRENT_PASSWORD_NAME, typeof(GuardedString),                 
                ConnectorAttributeInfo.Flags.NOT_READABLE |
                ConnectorAttributeInfo.Flags.NOT_RETURNED_BY_DEFAULT);
            
        /**
         * Used to determine if a password is expired or to expire a password.
         */
        public static readonly ConnectorAttributeInfo PASSWORD_EXPIRED = 
            ConnectorAttributeInfoBuilder.Build(
                OperationalAttributes.PASSWORD_EXPIRED_NAME, typeof(bool));
    
    }
    #endregion
    
    #region OperationalAttributes
    /**
     * Operational attributes have special meaning and cannot be represented by pure
     * operations. For instance some administrators would like to create an account
     * in the disabled state. The do not want this to be a two operation process
     * since this can leave the door open to abuse. Therefore special attributes
     * that can perform operations were introduced. The
     * {@link OperationalAttributes#DISABLED} attribute could be added to the set of
     * attribute sent to a Connector for the {@link CreateOp} operation. To tell the
     * {@link Connector} to create the account with it in the disabled state whether
     * the target resource itself has an attribute or an additional method must be
     * called.
     */
    public static class OperationalAttributes {
        /**
         * Gets/sets the enable status of an object.
         */
        public static readonly string ENABLE_NAME = ConnectorAttributeUtil.CreateSpecialName("ENABLE");
        /**
         * Gets/sets the enable date for an object.
         */
        public static readonly string ENABLE_DATE_NAME = ConnectorAttributeUtil.CreateSpecialName("ENABLE_DATE");
        /**
         * Gets/sets the disable date for an object.
         */
        public static readonly string DISABLE_DATE_NAME = ConnectorAttributeUtil.CreateSpecialName("DISABLE_DATE");
        /**
         * Gets/sets the lock out attribute for an object.
         */
        public static readonly string LOCK_OUT_NAME = ConnectorAttributeUtil.CreateSpecialName("LOCK_OUT");
        /**
         * Gets/sets the password expiration date for an object.
         */
        public static readonly string PASSWORD_EXPIRATION_DATE_NAME = ConnectorAttributeUtil.CreateSpecialName("PASSWORD_EXPIRATION_DATE");
        /**
         * Gets/sets the password expired for an object.
         */
        public static readonly string PASSWORD_EXPIRED_NAME = ConnectorAttributeUtil.CreateSpecialName("PASSWORD_EXPIRED");
        /**
         * Normally this is a write-only attribute. Sets the password for an object.
         */
        public static readonly string PASSWORD_NAME = ConnectorAttributeUtil.CreateSpecialName("PASSWORD");
        /**
         * Used in conjunction with password to do an account level password change.
         * This is for a non-administrator change of the password and therefore
         * requires the current password.
         */
        public static readonly string CURRENT_PASSWORD_NAME = ConnectorAttributeUtil.CreateSpecialName("CURRENT_PASSWORD");

	    // =======================================================================
	    // Helper Methods..
	    // =======================================================================
	    public static readonly ICollection<string> OPERATIONAL_ATTRIBUTE_NAMES =
	    	CollectionUtil.NewReadOnlySet<string> (
                LOCK_OUT_NAME,
                ENABLE_NAME,
                ENABLE_DATE_NAME,
                DISABLE_DATE_NAME,
                PASSWORD_EXPIRATION_DATE_NAME,
                PASSWORD_NAME,
                CURRENT_PASSWORD_NAME,
                PASSWORD_EXPIRED_NAME
            );
	
	    public static ICollection<string> GetOperationalAttributeNames() {
	        return CollectionUtil.NewReadOnlySet<string>(OPERATIONAL_ATTRIBUTE_NAMES);
	    }
	    public static bool IsOperationalAttribute(ConnectorAttribute attr) {
	        string name = (attr != null) ? attr.Name : null;
	        return OPERATIONAL_ATTRIBUTE_NAMES.Contains(name);
	    }
    }
    #endregion

    #region PredefinedAttributes
    /**
     * List of well known or pre-defined attributes. Common attributes that most
     * resources have that are not operational in nature.
     */
    public static class PredefinedAttributes {
        /**
         * Attribute that should hold a reasonable value to
         * display for the value of an object.  If this is not present, then the
         * application will have to use the NAME to show the value.
         */
        public static readonly String SHORT_NAME = ConnectorAttributeUtil.CreateSpecialName("SHORT_NAME");
        
        /**
         * Attribute that should hold the value of the object's description,
         * if one is available.
         */
        public static readonly String DESCRIPTION = ConnectorAttributeUtil.CreateSpecialName("DESCRIPTION");
        
        /**
         * Read-only attribute that shows the last date/time the password was
         * changed.
         */
        public static readonly string LAST_PASSWORD_CHANGE_DATE_NAME = ConnectorAttributeUtil.CreateSpecialName("LAST_PASSWORD_CHANGE_DATE");
    
        /**
         * Common password policy attribute where the password must be changed every
         * so often. The value for this attribute is milliseconds since its the
         * lowest common denominator.
         */
        public static readonly string PASSWORD_CHANGE_INTERVAL_NAME = ConnectorAttributeUtil.CreateSpecialName("PASSWORD_CHANGE_INTERVAL");
        
        /**
         * Last login date for an account.  This is usually used to determine inactivity.
         */
        public static readonly string LAST_LOGIN_DATE_NAME = ConnectorAttributeUtil.CreateSpecialName("LAST_LOGIN_DATE");
        
        /**
         * Groups an account object belongs to.
         */
        public static readonly string GROUPS_NAME = ConnectorAttributeUtil.CreateSpecialName("GROUPS");
        
        /**
         * Accounts that belong to a group or organization.
         */
        public static readonly string ACCOUNTS_NAME = ConnectorAttributeUtil.CreateSpecialName("ACCOUNTS");
    
        /**
         * An organization that that an account/person belongs to.
         */
        public static readonly string ORGANIZATION_NAME = ConnectorAttributeUtil.CreateSpecialName("ORGANIZATION");
    }
    #endregion

    #region PredefinedAttributeInfos
    public static class PredefinedAttributeInfos {
        /**
         * Attribute that should hold a reasonable value to
         * display for the value of an object.  If this is not present, then the
         * application will have to use the NAME to show the value.
         */
        public static readonly ConnectorAttributeInfo SHORT_NAME = 
            ConnectorAttributeInfoBuilder.Build(PredefinedAttributes.SHORT_NAME);
        
        /**
         * Attribute that should hold the value of the object's description,
         * if one is available.
         */
        public static readonly ConnectorAttributeInfo DESCRIPTION = 
            ConnectorAttributeInfoBuilder.Build(PredefinedAttributes.DESCRIPTION);
        /**
         * Read-only attribute that shows the last date/time the password was
         * changed.
         */
        public static readonly ConnectorAttributeInfo LAST_PASSWORD_CHANGE_DATE = 
            ConnectorAttributeInfoBuilder.Build(
                PredefinedAttributes.LAST_PASSWORD_CHANGE_DATE_NAME, 
                typeof(long), 
                ConnectorAttributeInfo.Flags.NOT_CREATABLE |
                ConnectorAttributeInfo.Flags.NOT_UPDATEABLE);
    
        /**
         * Common password policy attribute where the password must be changed every
         * so often. The value for this attribute is milliseconds since its the
         * lowest common denominator.
         */
        public static readonly ConnectorAttributeInfo PASSWORD_CHANGE_INTERVAL = 
            ConnectorAttributeInfoBuilder.Build(
                PredefinedAttributes.PASSWORD_CHANGE_INTERVAL_NAME, typeof(long));
    
        /**
         * Last login date for an account. This is usually used to determine
         * inactivity.
         */
        public static readonly ConnectorAttributeInfo LAST_LOGIN_DATE = 
            ConnectorAttributeInfoBuilder.Build(
                PredefinedAttributes.LAST_LOGIN_DATE_NAME, 
                typeof(long), 
                ConnectorAttributeInfo.Flags.NOT_CREATABLE |
                ConnectorAttributeInfo.Flags.NOT_UPDATEABLE);
            
        /**
         * Groups that an account or person belong to. The Attribute values are the
         * UID value of each group that an account has membership in.
         */
        public static readonly ConnectorAttributeInfo GROUPS = 
            ConnectorAttributeInfoBuilder.Build(PredefinedAttributes.GROUPS_NAME,
                    typeof(String),
                    ConnectorAttributeInfo.Flags.MULTIVALUED |
                    ConnectorAttributeInfo.Flags.NOT_RETURNED_BY_DEFAULT);
            
    
        /**
         * Accounts that are members of a group or organization. The Attribute
         * values are the UID value of each account the has a group or organization
         * membership.
         */
        public static readonly ConnectorAttributeInfo ACCOUNTS =
            ConnectorAttributeInfoBuilder.Build(PredefinedAttributes.ACCOUNTS_NAME,
                    typeof(String),
                    ConnectorAttributeInfo.Flags.MULTIVALUED|
                    ConnectorAttributeInfo.Flags.NOT_RETURNED_BY_DEFAULT);
            
    
        /**
         * Organizations that an account or person is a member of. The Attribute
         * values are the UID value of each organization that an account or person is
         * a member of.
         */
        public static readonly ConnectorAttributeInfo ORGANIZATION =
            ConnectorAttributeInfoBuilder.Build(PredefinedAttributes.ORGANIZATION_NAME,
                                       typeof(String),
                    ConnectorAttributeInfo.Flags.NOT_RETURNED_BY_DEFAULT);
    }
    #endregion

    #region OperationOptions
    /**
     * Arbitrary options to be passed into various operations. This serves
     * as a catch-all for extra options.
     */
    public sealed class OperationOptions {
    
        /**
         * An option to use with {@link SearchApiOp} that specified the scope
         * under which to perform the search. To be used in conjunction with
         * {@link #OP_CONTAINER}. Must be one of the following values
         * <ol>
         *    <li>{@link #SCOPE_OBJECT}</li>
         *    <li>{@link #SCOPE_ONE_LEVEL}</li>
         *    <li>{@link #SCOPE_SUBTREE}</li>
         * </ol>
         */
        public const String OP_SCOPE = "SCOPE";
        public const String SCOPE_OBJECT = "object";
        public const String SCOPE_ONE_LEVEL = "onelevel";
        public const String SCOPE_SUBTREE = "subtree";
        
        /**
         * An option to use with {@link SearchApiOp} that specified the container
         * under which to perform the search. Must be of type {@link QualifiedUid}.
         * Should be implemented for those object classes whose {@link ObjectClassInfo#isContainer()}
         * returns true.
         */
        public const String OP_CONTAINER = "CONTAINER";
        
        /**
         * An option to use with {@link ScriptOnResourceApiOp} and possibly others
         * that specifies an account under which to execute the script/operation.
         * The specified account will appear to have performed any action that the
         * script/operation performs.
         * <p>
         * Check the javadoc for a particular connector to see whether that
         * connector supports this option.
         */
        public static readonly string OP_RUN_AS_USER = "RUN_AS_USER";
    
        /**
         * An option to use with {@link ScriptOnResourceApiOp} and possibly others
         * that specifies a password under which to execute the script/operation.
         */
        public static readonly string OP_RUN_WITH_PASSWORD = "RUN_WITH_PASSWORD";
    
        /**
         * Determines the attributes to retrieve during {@link SearchApiOp} and
         * {@link SyncApiOp}.
         */
        public static readonly string OP_ATTRIBUTES_TO_GET = "ATTRS_TO_GET";
    
        private readonly IDictionary<String,Object> _operationOptions;
    
        /**
         * Public only for serialization; please use {@link OperationOptionsBuilder}.
         * @param operationOptions The options.
         */
        public OperationOptions(IDictionary<String,Object> operationOptions) {
            foreach (Object val in operationOptions.Values) {
                FrameworkUtil.CheckOperationOptionValue(val);
            }            
            //clone options to do a deep copy in case anything
            //is an array
            IDictionary<Object,Object> operationOptionsClone = (IDictionary<Object,Object>)SerializerUtil.CloneObject(operationOptions);
            _operationOptions = CollectionUtil.NewReadOnlyDictionary<Object,Object,String,Object>(operationOptionsClone);
        }
        
        /**
         * Returns a map of options. Each value in the map
         * must be of a type that the framework can serialize.  
         * See {@link ObjectSerializerFactory} for a list of supported types.
         * 
         * @return A map of options.
         */
        public IDictionary<String,Object> Options {
            get {
                return _operationOptions;
            }
        }
        
        /**
         * Convenience method that returns {@link #OP_SCOPE}.
         * @return The value for {@link #OP_SCOPE}.
         */
        public String Scope {
            get {
                return (String) CollectionUtil.GetValue(_operationOptions,OP_SCOPE,null);
            }
        }
        
        /**
         * Convenience method that returns {@link #OP_CONTAINER}.
         * @return The value for {@link #OP_CONTAINER}.
         */
        public QualifiedUid getContainer {
            get {
                return (QualifiedUid) CollectionUtil.GetValue(_operationOptions,OP_CONTAINER,null);
            }
        }
        
        /**
         * Get the string array of attribute names to return in the object.
         */
        public string[] AttributesToGet {
            get {
                return (string[]) CollectionUtil.GetValue(
                    _operationOptions, OP_ATTRIBUTES_TO_GET, null);
            }
        }
    
        /**
         * Get the account to run the operation as..
         */
        public string RunAsUser {
            get {
                return (string) CollectionUtil.GetValue(
                    _operationOptions, OP_RUN_AS_USER, null);
            }
        }
        
        /**
         * Get the password to run the operation as..
         */
        public GuardedString RunWithPassword {
            get {
                return (GuardedString) CollectionUtil.GetValue(
                    _operationOptions, OP_RUN_WITH_PASSWORD, null);
            }
        }
        
        public override string ToString()
        {
            StringBuilder bld = new StringBuilder();
            bld.Append("OperationOptions: ").Append(Options);
            return bld.ToString();
        }
    }
    #endregion
    
    #region OperationOptionsBuilder
    /**
     * Builder for {@link OperationOptions}.
     */
    public sealed class OperationOptionsBuilder {
        private readonly IDictionary<String,Object> _options; 
    
        /**
         * Create a builder with an empty set of options.
         */
        public OperationOptionsBuilder() {
            _options = new Dictionary<String, Object>();            
        }
        
        /**
         * Create a builder from an existing set of options.
         * @param options The existing set of options. Must not be null.
         */
        public OperationOptionsBuilder(OperationOptions options) {
            Assertions.NullCheck(options, "options");
            // clone options to do a deep copy in case anything
            // is an array
            IDictionary<Object, Object> operationOptionsClone = (IDictionary<Object, Object>) SerializerUtil
                    .CloneObject(options.Options);
            _options = CollectionUtil.NewDictionary<object,object,string,object>(operationOptionsClone);
        }
    
        /**
         * Sets a given option and a value for that option.
         * @param name The name of the option
         * @param value The value of the option. Must be one of the types that
         * we can serialize.  
         * See {@link ObjectSerializerFactory} for a list of supported types.
         */
        public void SetOption(String name, Object value) {
            if ( name == null ) {
                throw new ArgumentException("Argument 'value' cannot be null.");
            }
            //don't validate value here - we do that implicitly when
            //we clone in the constructor of OperationOptions
            _options[name] = value;
        }
            
        /**
         * Returns a mutable reference of the options map.
         * @return A mutable reference of the options map.
         */
        public IDictionary<String,Object> Options {
            get {
                //might as well be mutable since it's the builder and
                //we don't want to deep copy anyway
                return _options;
            }
        }
        
        /**
         * Creates the <code>OperationOptions</code>.
         * @return The newly-created <code>OperationOptions</code>
         */
        public OperationOptions Build() {
            return new OperationOptions(_options);
        }
        
        /**
         * Sets the {@link OperationOptions#OP_ATTRIBUTES_TO_GET} option.
         * 
         * @param attrNames
         *            list of {@link Attribute} names.
         */
        public string[] AttributesToGet {
            set {
                Assertions.NullCheck(value, "AttributesToGet");
                // don't validate value here - we do that in
                // the constructor of OperationOptions - that's
                // really the only place we can truly enforce this
                _options[OperationOptions.OP_ATTRIBUTES_TO_GET] = value;
            }
        }
    
        /**
         * Set the run with password option.
         */
        public GuardedString RunWithPassword {
            set {
                Assertions.NullCheck(value, "RunWithPassword");
                _options[OperationOptions.OP_RUN_WITH_PASSWORD] = value;
            }
        }
        
        /**
         * Set the run as user option.
         */
        public string RunAsUser {
            set {
                Assertions.NullCheck(value, "RunAsUser");
                _options[OperationOptions.OP_RUN_AS_USER] = value;
            }
        }
        /**
         * Convenience method to set {@link OperationOptions#OP_SCOPE}
         * @param scope The scope. May not be null.
         * @return A this reference to allow chaining
         */
        public string Scope {
            set {
                Assertions.NullCheck(value, "scope");
                _options[OperationOptions.OP_SCOPE] = value;
            }
        }
        
        /**
         * Convenience method to set {@link OperationOptions#OP_CONTAINER}
         * @param container The container. May not be null.
         * @return A this reference to allow chaining
         */
        public QualifiedUid Container {
            set {
                Assertions.NullCheck(value, "container");
                _options[OperationOptions.OP_CONTAINER] = value;
            }
        }

    }
    #endregion
    
    #region OperationOptionInfo
    public sealed class OperationOptionInfo {
        private String _name;
        private Type _type;
        
        public OperationOptionInfo(String name,
                Type type) {
            Assertions.NullCheck(name, "name");
            Assertions.NullCheck(type, "type");
            FrameworkUtil.CheckOperationOptionType(type);
            _name = name;
            _type = type;
        }
        
        public String Name {
            get {
                return _name;
            }
        }
        
        public Type OptionType {
            get {
                return _type;
            }
        }
        
        public override bool Equals(Object o) {
            if (o is OperationOptionInfo) {
                OperationOptionInfo other =
                    (OperationOptionInfo)o;
                if (!_name.Equals(other._name)) {
                    return false;
                }
                if (!_type.Equals(other._type)) {
                    return false;
                }
                return true;
            }
            return false;
        }
        
        public override int GetHashCode() {
            return _name.GetHashCode();
        }
        
        public override string ToString()
        {
            StringBuilder bld = new StringBuilder();
            bld.Append("OperationOptionInfo(");
            bld.Append(_name);
            bld.Append(_type.ToString());
            bld.Append(')');
            return bld.ToString();
        }
        
    }
    #endregion
    
    #region OperationOptionInfoBuilder
    public sealed class OperationOptionInfoBuilder {
        private String _name;
        private Type _type;
        
        public OperationOptionInfoBuilder() {
        }
           
        public OperationOptionInfoBuilder(String name,
                Type type) {
            _name = name;
            _type = type;
        }
        
        public String Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }
                
        public Type OptionType {
            get {
                return _type;
            }
            set {
                _type = value;
            }
        }
                
        public OperationOptionInfo Build() {
            return new OperationOptionInfo(_name,_type);
        }
        
        public static OperationOptionInfo Build(String name, Type type) {
            return new OperationOptionInfoBuilder(name,type).Build();
        }
        
        public static OperationOptionInfo Build(String name) {
            return Build(name, typeof(string));
        }
        
        public static OperationOptionInfo BuildAttributesToGet() {
            return Build(OperationOptions.OP_ATTRIBUTES_TO_GET, typeof(string[]));
        }
        
        public static OperationOptionInfo BuildRunWithPassword() {
            return Build(OperationOptions.OP_RUN_WITH_PASSWORD);
        }
        
        public static OperationOptionInfo BuildRunAsUser() {
            return Build(OperationOptions.OP_RUN_AS_USER);
        }
        public static OperationOptionInfo BuildScope() {
            return Build(OperationOptions.OP_SCOPE);
        }
        
        public static OperationOptionInfo BuildContainer() {
            return Build(OperationOptions.OP_CONTAINER,typeof(QualifiedUid));
        }
    }
    #endregion

    #region QualifiedUid
    /**
     * A fully-qualified uid. That is, a pair of {@link ObjectClass} and
     * {@link Uid}.
     */
    public sealed class QualifiedUid {
        private readonly ObjectClass _objectClass;
        private readonly Uid _uid;
        
        /**
         * Create a QualifiedUid.
         * @param objectClass The object class. May not be null.
         * @param uid The uid. May not be null.
         */
        public QualifiedUid(ObjectClass objectClass,
                Uid uid) {
            Assertions.NullCheck(objectClass,"objectClass");
            Assertions.NullCheck(uid,"uid");
            _objectClass = objectClass;
            _uid = uid;
        }
        
        /**
         * Returns the object class.
         * @return The object class.
         */
        public ObjectClass ObjectClass {
            get {
                return _objectClass;
            }
        }
        
        /**
         * Returns the uid.
         * @return The uid.
         */
        public Uid Uid {
            get {
                return _uid;
            }
        }
        
        /**
         * Returns true iff o is a QualifiedUid and the object class and uid match.
         */
        public override bool Equals(Object o) {
            if ( o is QualifiedUid ) {
                QualifiedUid other = (QualifiedUid)o;
                return ( _objectClass.Equals(other._objectClass) &&
                         _uid.Equals(other._uid) );
            }
            return false;
        }
        
        /**
         * Returns a hash code based on uid
         */
        public override int GetHashCode() {
            return _uid.GetHashCode();
        }
        
        /**
         * Returns a string representation acceptible for debugging.
         */
        public override String ToString() {
            return SerializerUtil.SerializeXmlObject(this, false);
        }
        
    }    
    #endregion
    
    #region ResultsHandler
    /**
    * Encapsulate the handling of each object returned by the search.
    */
    public delegate bool ResultsHandler(ConnectorObject obj); 
    #endregion
    
    #region Schema
    /**
     * Determines the objects supported by a
     * {@link com.sun.openconnectors.framework.spi.Connector}.
     * The {@link Schema} object is used to represent the basic objects that a
     * connector supports. This does not prevent a connector from supporting more.
     * Rather, this is informational for the caller of the connector to understand
     * a minimum support level.
     * The schema defines 4 primary data structures
     * <ol>
     *   <li>Declared ObjectClasses ({@link #getObjectClassInfo()}).</li>
     *   <li>Declared OperationOptionInfo ({@link #getOperationOptionInfo()}). </li>
     *   <li>Supported ObjectClasses by operation ({@link #getSupportedObjectClassesByOperation()}). </li>
     *   <li>Supported OperationOptionInfo by operation({@link #getSupportedOptionsByOperation()()}). </li>
     * </ol>
     * 
     * TODO: add more to describe and what is expected from this call and how it is
     * used.. based on OperationalAttribute etc..
     */
    public sealed class Schema {
        private readonly ICollection<ObjectClassInfo> _declaredObjectClasses;
        private readonly ICollection<OperationOptionInfo> _declaredOperationOptions;
        private readonly IDictionary<SafeType<APIOperation>,ICollection<ObjectClassInfo>>
        _supportedObjectClassesByOperation; 
        private readonly IDictionary<SafeType<APIOperation>,ICollection<OperationOptionInfo>>
        _supportedOptionsByOperation;     

        /**
         * Public only for serialization; please use
         * SchemaBuilder instead.
         * @param info
         * @param supportedObjectClassesByOperation
         */
        public Schema(ICollection<ObjectClassInfo> info,
                      ICollection<OperationOptionInfo> options,
                      IDictionary<SafeType<APIOperation>,ICollection<ObjectClassInfo>> supportedObjectClassesByOperation,
                      IDictionary<SafeType<APIOperation>,ICollection<OperationOptionInfo>> supportedOptionsByOperation) {
            _declaredObjectClasses = CollectionUtil.NewReadOnlySet<ObjectClassInfo>(info);
            _declaredOperationOptions = CollectionUtil.NewReadOnlySet(options);

            //make read-only
            {
                IDictionary<SafeType<APIOperation>,ICollection<ObjectClassInfo>> temp = 
                    new Dictionary<SafeType<APIOperation>,ICollection<ObjectClassInfo>>();
                foreach (KeyValuePair<SafeType<APIOperation>,ICollection<ObjectClassInfo>> entry in 
                    supportedObjectClassesByOperation) {
                    SafeType<APIOperation> op =
                        entry.Key;
                    ICollection<ObjectClassInfo> resolvedClasses =
                        CollectionUtil.NewReadOnlySet(entry.Value);
                    temp[op] = resolvedClasses;
                }
                _supportedObjectClassesByOperation = CollectionUtil.AsReadOnlyDictionary(temp);
            }
            //make read-only
            {
                IDictionary<SafeType<APIOperation>,ICollection<OperationOptionInfo>> temp = 
                    new Dictionary<SafeType<APIOperation>,ICollection<OperationOptionInfo>>();
                foreach (KeyValuePair<SafeType<APIOperation>,ICollection<OperationOptionInfo>> entry in 
                    supportedOptionsByOperation) {
                    SafeType<APIOperation> op =
                        entry.Key;
                    ICollection<OperationOptionInfo> resolvedClasses =
                        CollectionUtil.NewReadOnlySet(entry.Value);
                    temp[op] = resolvedClasses;
                }
                _supportedOptionsByOperation = CollectionUtil.AsReadOnlyDictionary(temp);
            }
        }
        
        /**
         * Returns the set of object classes that are defined in the schema, regardless
         * of which operations support them.
         */
        public ICollection<ObjectClassInfo> ObjectClassInfo {
            get {
                return _declaredObjectClasses;
            }
        }
        
        /**
         * Returns the ObjectClassInfo for the given type.
         * @param type The type to find.
         * @return the ObjectClassInfo for the given type or null if not found.
         */
        public ObjectClassInfo FindObjectClassInfo(String type) {
            foreach (ObjectClassInfo info in _declaredObjectClasses) {
                if ( info.Is(type) ) {
                    return info;
                }
            }
            return null;
        }
        
        /**
         * Returns the set of operation options that are defined in the schema, regardless
         * of which operations support them.
         * @return The options defined in this schema.
         */
        public ICollection<OperationOptionInfo> OperationOptionInfo {
            get {
                return _declaredOperationOptions;
            }
        }
        
        /**
         * Returns the OperationOptionInfo for the given name.
         * @param name The name to find.
         * @return the OperationOptionInfo for the given name or null if not found.
         */
        public OperationOptionInfo FindOperationOptionInfo(String name) {
            Assertions.NullCheck(name, "name");
            foreach (OperationOptionInfo info in _declaredOperationOptions) {
                if ( info.Name.Equals(name) ) {
                    return info;
                }
            }
            return null;
        }
        
        /**
         * Returns the supported object classes for the given operation.
         * @param apiop The operation.
         * @return the supported object classes for the given operation.
         */
        public ICollection<ObjectClassInfo> GetSupportedObjectClassesByOperation(SafeType<APIOperation> apiop) {
            ICollection<ObjectClassInfo> rv = 
                CollectionUtil.GetValue(_supportedObjectClassesByOperation,apiop,null);
            if ( rv == null ) {
                ICollection<ObjectClassInfo> empty = 
                    CollectionUtil.NewReadOnlySet<ObjectClassInfo>();
                        
                return empty;
            }
            else {
                return rv;
            }
        }
        
        /**
         * Returns the supported options for the given operation.
         * @param apiop The operation.
         * @return the supported options for the given operation.
         */
        public ICollection<OperationOptionInfo> GetSupportedOptionsByOperation(SafeType<APIOperation> apiop) {
            ICollection<OperationOptionInfo> rv = 
                CollectionUtil.GetValue(_supportedOptionsByOperation,apiop,null);
            if ( rv == null ) {
                ICollection<OperationOptionInfo> empty = 
                    CollectionUtil.NewReadOnlySet<OperationOptionInfo>();
                return empty;
            }
            else {
                return rv;
            }
        }
        
        /**
         * Returns the set of object classes that apply to a particular operation.
         * @return the set of object classes that apply to a particular operation.
         */
        public IDictionary<SafeType<APIOperation>,ICollection<ObjectClassInfo>> SupportedObjectClassesByOperation {
            get {
                return _supportedObjectClassesByOperation;
            }
        }
        /**
         * Returns the set of operation options that apply to a particular operation.
         * @return the set of operation options that apply to a particular operation.
         */
        public IDictionary<SafeType<APIOperation>,ICollection<OperationOptionInfo>> SupportedOptionsByOperation {
            get {
                return _supportedOptionsByOperation;
            }
        }

        
        
        public override int GetHashCode() {
            return CollectionUtil.GetHashCode(_declaredObjectClasses);
        }
        
        public override bool Equals(object o) {
            Schema other = o as Schema;
            if ( other != null ) {
                if (!CollectionUtil.Equals(ObjectClassInfo,other.ObjectClassInfo)) {
                    return false;
                }
                if (!CollectionUtil.Equals(OperationOptionInfo,other.OperationOptionInfo)) {
                    return false;
                }
                if (!CollectionUtil.Equals(_supportedObjectClassesByOperation,
                                              other._supportedObjectClassesByOperation)) {
                    return false;                    
                }
                if (!CollectionUtil.Equals(_supportedOptionsByOperation,
                                              other._supportedOptionsByOperation)) {
                    return false;                    
                }
                return true;
            }
            return false;
        }
        
        public override string ToString()
        {
            return SerializerUtil.SerializeXmlObject(this, false);
        }
    }
    #endregion
    
    #region SchemaBuilder
    /**
     * Simple builder class to help facilitate creating a {@link Schema} object.
     */
    public sealed class SchemaBuilder {
        private readonly SafeType<Connector> _connectorClass;
        private readonly ICollection<ObjectClassInfo> _declaredObjectClasses
        = new HashSet<ObjectClassInfo>();
        private readonly ICollection<OperationOptionInfo> _declaredOperationOptions
        = new HashSet<OperationOptionInfo>();
        
        private readonly IDictionary<SafeType<APIOperation>,ICollection<ObjectClassInfo>>
            _supportedObjectClassesByOperation = 
                new Dictionary<SafeType<APIOperation>,ICollection<ObjectClassInfo>>();
        private readonly IDictionary<SafeType<APIOperation>,ICollection<OperationOptionInfo>>
            _supportedOptionsByOperation = 
                new Dictionary<SafeType<APIOperation>,ICollection<OperationOptionInfo>>();
    
    
        /**
         * 
         */
        public SchemaBuilder(SafeType<Connector> connectorClass) {
            Assertions.NullCheck(connectorClass, "connectorClass");
            _connectorClass = connectorClass;
        }
    
        /**
         * Adds another ObjectClassInfo to the schema. Also, adds this
         * to the set of supported classes for every operation defined by
         * the Connector.
         * 
         * @param info
         * @throws IllegalStateException If already defined
         */
        public void DefineObjectClass(ObjectClassInfo info) {
            Assertions.NullCheck(info, "info");
            if (_declaredObjectClasses.Contains(info)) {
                throw new InvalidOperationException("ObjectClass already defined: "+
                        info.ObjectType);
            }
            _declaredObjectClasses.Add(info);
            foreach (SafeType<APIOperation> op in 
                FrameworkUtil.GetDefaultSupportedOperations(_connectorClass)) {
                ICollection<ObjectClassInfo> oclasses = 
                    CollectionUtil.GetValue(_supportedObjectClassesByOperation,op,null);
                if (oclasses == null) {
                    oclasses = new HashSet<ObjectClassInfo>();
                    _supportedObjectClassesByOperation[op] = oclasses;
                }
                oclasses.Add(info);
            }
        }
        /**
         * Adds another OperationOptionInfo to the schema. Also, adds this
         * to the set of supported options for every operation defined by
         * the Connector.
         */
        public void DefineOperationOption(OperationOptionInfo info) {
            Assertions.NullCheck(info, "info");
            if (_declaredOperationOptions.Contains(info)) {
                throw new InvalidOperationException("OperationOption already defined: "+
                        info.Name);
            }
            _declaredOperationOptions.Add(info);
            foreach (SafeType<APIOperation> op in 
                FrameworkUtil.GetDefaultSupportedOperations(_connectorClass)) {
                ICollection<OperationOptionInfo> oclasses = 
                    CollectionUtil.GetValue(_supportedOptionsByOperation,op,null);
                if (oclasses == null) {
                    oclasses = new HashSet<OperationOptionInfo>();
                    _supportedOptionsByOperation[op] = oclasses;
                }
                oclasses.Add(info);
            }
        }
    
        /**
         * Adds another ObjectClassInfo to the schema. Also, adds this
         * to the set of supported classes for every operation defined by
         * the Connector.
         * @throws IllegalStateException If already defined
         */
        public void DefineObjectClass(String type, ICollection<ConnectorAttributeInfo> attrInfo) {
            ObjectClassInfoBuilder bld = new ObjectClassInfoBuilder();
            bld.ObjectType = type;
            bld.AddAllAttributeInfo(attrInfo);
            ObjectClassInfo obj = bld.Build();
            DefineObjectClass(obj);
        }
        
        /**
         * Adds another OperationOptionInfo to the schema. Also, adds this
         * to the set of supported options for every operation defined by
         * the Connector.
         * @throws IllegalStateException If already defined
         */
        public void DefineOperationOption(String optionName, Type type) {
            OperationOptionInfoBuilder bld = new OperationOptionInfoBuilder();
            bld.Name=(optionName);
            bld.OptionType=(type);
            OperationOptionInfo info = bld.Build();
            DefineOperationOption(info);
        }
        
        /**
         * Adds the given ObjectClassInfo as a supported ObjectClass for
         * the given operation. 
         * @param op The SPI operation
         * @param def The ObjectClassInfo
         * @throws IllegalArgumentException If the given ObjectClassInfo was
         *  not already defined using {@link #defineObjectClass(ObjectClassInfo)}.
         */
        public void AddSupportedObjectClass(SafeType<SPIOperation> op,
                ObjectClassInfo def)
        {
            Assertions.NullCheck(op, "op");
            Assertions.NullCheck(def, "def");
            ICollection<SafeType<APIOperation>> apis =
                FrameworkUtil.Spi2Apis(op);
            if (!_declaredObjectClasses.Contains(def)) {
                throw new ArgumentException("ObjectClass "+def.ObjectType+
                        " not defined in schema.");
            }
            foreach (SafeType<APIOperation> api in apis) {
                ICollection<ObjectClassInfo> infos = 
                    CollectionUtil.GetValue(_supportedObjectClassesByOperation,api,null);
                if ( infos == null ) {
                    throw new ArgumentException("Operation "+op+
                            " not implement by connector.");                
                }
                if ( infos.Contains(def)) {
                    throw new ArgumentException("ObjectClass "+def.ObjectType+
                            " already supported for operation "+op);
                }
                infos.Add(def);
            }
        }
        
        /**
         * Removes the given ObjectClassInfo as a supported ObjectClass for
         * the given operation. 
         * @param op The SPI operation
         * @param def The ObjectClassInfo
         * @throws IllegalArgumentException If the given ObjectClassInfo was
         *  not already defined using {@link #defineObjectClass(ObjectClassInfo)}.
         */
        public void RemoveSupportedObjectClass(SafeType<SPIOperation> op,
                ObjectClassInfo def)
        {
            Assertions.NullCheck(op, "op");
            Assertions.NullCheck(def, "def");
            ICollection<SafeType<APIOperation>> apis =
                FrameworkUtil.Spi2Apis(op);
            if (!_declaredObjectClasses.Contains(def)) {
                throw new ArgumentException("ObjectClass "+def.ObjectType+
                        " not defined in schema.");
            }
            foreach (SafeType<APIOperation> api in apis) {
                ICollection<ObjectClassInfo> infos = 
                    CollectionUtil.GetValue(_supportedObjectClassesByOperation,api,null);
                if ( infos == null ) {
                    throw new ArgumentException("Operation "+op+
                            " not implement by connector.");                
                }
                if ( !infos.Contains(def)) {
                    throw new ArgumentException("ObjectClass "+def.ObjectType
                            +" already removed for operation "+op);
                }
                infos.Remove(def);
            }
        }
        /**
         * Adds the given OperationOptionInfo as a supported option for
         * the given operation. 
         * @param op The SPI operation
         * @param def The OperationOptionInfo
         * @throws IllegalArgumentException If the given OperationOptionInfo was
         *  not already defined using {@link #defineOperationOption(OperationOptionInfo)}.
         */
        public void AddSupportedOperationOption(SafeType<SPIOperation> op,
                OperationOptionInfo def) {
            Assertions.NullCheck(op, "op");
            Assertions.NullCheck(def, "def");
            ICollection<SafeType<APIOperation>> apis =
                FrameworkUtil.Spi2Apis(op);
            if (!_declaredOperationOptions.Contains(def)) {
                throw new ArgumentException("OperationOption "+def.Name+
                        " not defined in schema.");
            }
            foreach (SafeType<APIOperation> api in apis) {
                ICollection<OperationOptionInfo> infos =
                    CollectionUtil.GetValue(_supportedOptionsByOperation,api,null);
                if ( infos == null ) {
                    throw new ArgumentException("Operation "+op+
                            " not implement by connector.");                
                }
                if ( infos.Contains(def) ) {
                    throw new ArgumentException("OperationOption "+def.Name+
                            " already supported for operation "+op);
                }
                infos.Add(def);
            }
        }
        
        /**
         * Removes the given OperationOptionInfo as a supported option for
         * the given operation. 
         * @param op The SPI operation
         * @param def The OperationOptionInfo
         * @throws IllegalArgumentException If the given OperationOptionInfo was
         *  not already defined using {@link #defineOperationOption(OperationOptionInfo)}.
         */
        public void RemoveSupportedOperationOption(SafeType<SPIOperation> op,
                OperationOptionInfo def) {
            Assertions.NullCheck(op, "op");
            Assertions.NullCheck(def, "def");
            ICollection<SafeType<APIOperation>> apis =
                FrameworkUtil.Spi2Apis(op);
            if (!_declaredOperationOptions.Contains(def)) {
                throw new ArgumentException("OperationOption "+def.Name+
                        " not defined in schema.");
            }
            foreach (SafeType<APIOperation> api in apis) {
                ICollection<OperationOptionInfo> infos = 
                    CollectionUtil.GetValue(_supportedOptionsByOperation,api,null);
                if ( infos == null ) {
                    throw new ArgumentException("Operation "+op+
                            " not implement by connector.");                
                }
                if ( !infos.Contains(def) ) {
                    throw new ArgumentException("OperationOption "+def.Name+
                            " already removed for operation "+op);
                }
                infos.Remove(def);
            }
        }
            
        /**
         * Clears the operation-specific supported classes. Normally, when
         * you add an ObjectClass, using {@link #defineObjectClass(ObjectClassInfo)},
         * it is added to all operations. You may then remove those that you need
         * using {@link #removeSupportedObjectClass(Class, ObjectClassInfo)}. You
         * may wish, as an alternative to clear everything out and instead add using
         * {@link #addSupportedObjectClass(Class, ObjectClassInfo)}. 
         */
        public void ClearSupportedObjectClassesByOperation() {
            foreach (ICollection<ObjectClassInfo> values in 
                _supportedObjectClassesByOperation.Values)
            {
                values.Clear();
            }
        }
        /**
         * Clears the operation-specific supported options. Normally, when
         * you add an OperationOptionInfo, using {@link #defineOperationOption(OperationOptionInfo)(ObjectClassInfo)},
         * it is added to all operations. You may then remove those that you need
         * using {@link #removeSupportedOperationOption(Class, OperationOptionInfo)}. You
         * may wish, as an alternative to clear everything out and instead add using
         * {@link #addSupportedOperationOption(Class, OperationOptionInfo)}. 
         */
        public void ClearSupportedOptionsByOperation() {
            foreach (ICollection<OperationOptionInfo> values in 
                _supportedOptionsByOperation.Values)
            {
                values.Clear();
            }
        }
    
        /**
         * Builds the {@link Schema} object based on the {@link ObjectClassInfo}s
         * added so far.
         * 
         * @return new Schema object based on the info provided.
         */
        public Schema Build() {
            if (_declaredObjectClasses.Count == 0) {
                String ERR = "Must be at least one ObjectClassInfo object!";
                throw new InvalidOperationException(ERR);
            }
            return new Schema(_declaredObjectClasses,
                              _declaredOperationOptions,
                              _supportedObjectClassesByOperation,
                              _supportedOptionsByOperation);
        }
    }
    #endregion
    
    #region ScriptContext
    /**
     * Encapsulates a script and all of its parameters.
     * @see com.sun.openconnectors.framework.api.operations.ScriptOnResourceApiOp
     * @see com.sun.openconnectors.framework.api.operations.ScriptOnConnectorApiOp
     */
    public sealed class ScriptContext {
        
        private readonly String _scriptLanguage;
        private readonly String _scriptText;
        private readonly IDictionary<String,Object> _scriptArguments;
        
        /**
         * Public only for serialization; please use {@link ScriptContextBuilder}.
         * @param scriptLanguage The script language. Must not be null.
         * @param scriptText The script text. Must not be null.
         * @param scriptArguments The script arguments. May be null.
         */
        public ScriptContext(String scriptLanguage,
                String scriptText,
                IDictionary<String,Object> scriptArguments) {
            
            if (scriptLanguage == null) {
                throw new ArgumentException("Argument 'scriptLanguage' must be specified");
            }
            if (scriptText == null) {
                throw new ArgumentException("Argument 'scriptText' must be specified");
            }
            //clone script arguments and options - this serves two purposes
            //1)makes sure everthing is serializable
            //2)does a deep copy
            IDictionary<Object,Object> scriptArgumentsClone = (IDictionary<Object,Object>)SerializerUtil.CloneObject(scriptArguments);
            _scriptLanguage = scriptLanguage;
            _scriptText = scriptText;
            _scriptArguments = CollectionUtil.NewReadOnlyDictionary<object,object,string,object>(scriptArgumentsClone);
        }
        
        /**
         * Identifies the language in which the script is written 
         * (e.g., <code>bash</code>, <code>csh</code>, 
         * <code>Perl4</code> or <code>Python</code>).
         * @return The script language.
         */
        public String ScriptLanguage {
            get {
                return _scriptLanguage;
            }
        }
        
        /**
         * Returns the text (i.e., actual characters) of the script.
         * @return The text of the script.
         */
        public String ScriptText {
            get {
                return _scriptText;
            }
        }
        
        /**
         * Returns a map of arguments to be passed to the script.
         * Values must be types that the framework can serialize.
         * See {@link ObjectSerializerFactory} for a list of supported types.
         * @return A map of arguments to be passed to the script.
         */
        public IDictionary<String,Object> ScriptArguments {
            get {
                return _scriptArguments;
            }
        }
        
        public override string ToString()
        {
            StringBuilder bld = new StringBuilder();
            bld.Append("ScriptContext: ");
            // poor man's to string method.
            IDictionary<string, object> map = new Dictionary<string, object>();
            map["Language"] = ScriptLanguage;
            map["Text"] = ScriptText;
            map["Arguments"] = ScriptArguments;
            bld.Append(map.ToString());
            return bld.ToString();
        }
        
    }
    #endregion
    
    #region ScriptContextBuilder
    /**
     * Builds an {@link ScriptContext}.
     */
    public sealed class ScriptContextBuilder {
        private String _scriptLanguage;
        private String _scriptText;
        private readonly IDictionary<String,Object> _scriptArguments = new
        Dictionary<String,Object>();
    
        /**
         * Creates an empty builder.
         */
        public ScriptContextBuilder() {
            
        }
        
        /**
         * Creates a builder with the required parameters specified.
         * @param scriptLanguage a string that identifies the language
         * in which the script is written 
         * (e.g., <code>bash</code>, <code>csh</code>, 
         * <code>Perl4</code> or <code>Python</code>).
         * @param scriptText The text (i.e., actual characters) of the script.
         */
        public ScriptContextBuilder(String scriptLanguage,
                String scriptText) {
            _scriptLanguage = scriptLanguage;
            _scriptText = scriptText;
        }
        
        /**
         * Identifies the language in which the script is written 
         * (e.g., <code>bash</code>, <code>csh</code>, 
         * <code>Perl4</code> or <code>Python</code>).
         * @return The script language.
         */
        public String ScriptLanguage {
            get {
                return _scriptLanguage;
            }
            set {
                _scriptLanguage = value;
            }
        }
                
        /**
         * Returns the actual characters of the script.
         * @return the actual characters of the script.
         */
        public String ScriptText {
            get {
                return _scriptText;
            }
            set {
                _scriptText = value;
            }
        }
                
        /**
         * Adds or sets an argument to pass to the script.
         * @param name The name of the argument. Must not be null.
         * @param value The value of the argument. Must be one of
         * type types that the framework can serialize.
         * @see ObjectSerializerFactory for a list of supported types.
         */
        public ScriptContextBuilder AddScriptArgument(String name, Object value) {
            if ( name == null ) {
                throw new ArgumentException("Argument 'name' cannot be null.");
            }
            //don't validate value here - we do that implicitly when
            //we clone in the constructor of ScriptRequest
            _scriptArguments[name] = value;
            return this;
        }
        
        /**
         * Removes the given script argument.
         * @param name The name of the argument. Must not be null.
         */
        public ScriptContextBuilder RemoveScriptArgument(String name) {
            if ( name == null ) {
                throw new ArgumentException("Argument 'name' cannot be null.");
            }
            _scriptArguments.Remove(name);
            return this;
        }
        
        /**
         * Returns a mutable reference of the script arguments map.
         * @return A mutable reference of the script arguments map.
         */
        public IDictionary<String,Object> ScriptArguments {
            get {
                //might as well be mutable since it's the builder and
                //we don't want to deep copy anyway
                return _scriptArguments;
            }
        }
    
        /**
         * Creates a <code>ScriptContext</code>. 
         * The <code>scriptLanguage</code> and <code>scriptText</code>
         * must be set prior to calling this.
         * @return The <code>ScriptContext</code>.
         */
        public ScriptContext Build() {
            return new ScriptContext(_scriptLanguage,
                    _scriptText,
                    _scriptArguments);
        }
    }
    #endregion
    
    #region SyncDelta
    /**
     * Represents a change to an object in a resource.
     * 
     * @see SyncApiOp
     * @see SyncOp
     */
    public sealed class SyncDelta {
        private readonly SyncToken _token;
        private readonly SyncDeltaType _deltaType;
        private readonly Uid _uid;
        private readonly ConnectorObject _object;
    
        /**
         * Creates a SyncDelata
         * @param token
         *            The token. Must not be null.
         * @param deltaType
         *            The delta. Must not be null.
         * @param uid
         *            The uid. Must not be null.           
         * @param object 
         *            The object that has changed. May be null for delete.
         */
        internal SyncDelta(SyncToken token, SyncDeltaType deltaType,
                Uid uid,
                ConnectorObject obj) {
            Assertions.NullCheck(token, "token");
            Assertions.NullCheck(deltaType, "deltaType");
            Assertions.NullCheck(uid, "uid");
            
            //only allow null object for delete
            if ( obj == null && 
                 deltaType != SyncDeltaType.DELETE) {
                throw new ArgumentException("ConnectorObject must be specified for anything other than delete.");
            }
            
            //if object not null, make sure its Uid
            //matches
            if ( obj != null ) {
                if (!uid.Equals(obj.Uid)) {
                    throw new ArgumentException("Uid does not match that of the object.");                
                }
            }
    
            _token = token;
            _deltaType = deltaType;
            _uid    = uid;
            _object = obj;
    
        }
    
        /**
         * Returns the <code>Uid</code> of the object that changed.
         * 
         * @return the <code>Uid</code> of the object that changed.
         */
        public Uid Uid {
            get {
                return _uid;
            }
        }
        
        /**
         * Returns the connector object that changed. This
         * may be null in the case of delete.
         * @return The object or possibly null if this
         * represents a delete.
         */
        public ConnectorObject Object {
            get {
                return _object;
            }
        }
    
        /**
         * Returns the <code>SyncToken</code> of the object that changed.
         * 
         * @return the <code>SyncToken</code> of the object that changed.
         */
        public SyncToken Token {
            get {
                return _token;
            }
        }
    
        /**
         * Returns the type of the change the occured.
         * 
         * @return The type of change that occured.
         */
        public SyncDeltaType DeltaType {
            get {
                return _deltaType;
            }
        }
    
        
        public override String ToString() {
            IDictionary<String,Object> values = new Dictionary<String, Object>();
            values["Token"] = _token;
            values["DeltaType"] = _deltaType;
            values["Uid"] = _uid;
            values["Object"] = _object;
            return values.ToString();
        }
        
        public override int GetHashCode() {
            return _uid.GetHashCode();
        }
        
        public override bool Equals(Object o) {
            if ( o is SyncDelta ) {
                SyncDelta other = (SyncDelta)o;
                if (!_token.Equals(other._token)) {
                    return false;
                }
                if (!_deltaType.Equals(other._deltaType)) {
                    return false;
                }
                if (!_uid.Equals(other._uid)) {
                    return false;
                }
                if (_object == null) {
                    if ( other._object != null ) {
                        return false;
                    }
                }
                else if (!_object.Equals(other._object)) {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
    #endregion
    
    #region SyncDeltaBuilder
    /**
     * Builder for {@link SyncDelta}.
     */
    public sealed class SyncDeltaBuilder {
        private SyncToken _token;
        private SyncDeltaType _deltaType;
        private Uid _uid;
        private ConnectorObject _object;
    
        /**
         * Create a new <code>SyncDeltaBuilder</code>
         */
        public SyncDeltaBuilder() {
    
        }
        
        /**
         * Creates a new <code>SyncDeltaBuilder</code> whose
         * values are initialized to those of the delta.
         * @param delta The original delta.
         */
        public SyncDeltaBuilder(SyncDelta delta) {
            _token = delta.Token;
            _deltaType = delta.DeltaType;
            _object = delta.Object;
            _uid = delta.Uid;
        }
            
        /**
         * Returns the <code>SyncToken</code> of the object that changed.
         * 
         * @return the <code>SyncToken</code> of the object that changed.
         */
        public SyncToken Token {
            get {
                return _token;
            }
            set {
                _token = value;
            }
        }
        
        /**
         * Returns the type of the change that occurred.
         * 
         * @return The type of change that occurred.
         */
        public SyncDeltaType DeltaType {
            get {
                return _deltaType;
            }
            set {
                _deltaType = value;
            }
        }
        
        /**
         * Returns the <code>Uid</code> of the object that changed.
         * Note that this is implicitly set when you call
         * {@link #setObject(ConnectorObject)}.
         * 
         * @return the <code>Uid</code> of the object that changed.
         */
        public Uid Uid {
            get {
                return _uid;
            }
            set {
                _uid = value;
            }
        }
        
        /**
         * Returns the object that changed.
         * Sets the object that changed and implicitly
         * sets Uid if object is not null.
         * @return The object that changed. May be null for
         * deletes.
         */
        public ConnectorObject Object {
            get {
                return _object;
            }
            set {
                _object = value;
                if ( value != null ) {
                    _uid = value.Uid;
                }                
            }
        }
    
        /**
         * Creates a SyncDelta. Prior to calling the following must be specified:
         * <ol>
         * <li>{@link #setObject(ConnectorObject) Object} (for anything other than delete)</li>
         * <li>{@link #setUid(Uid) Uid} (this is implictly set when calling {@link #setObject(ConnectorObject)})</li>
         * <li>{@link #setToken(SyncToken) Token}</li>
         * <li>{@link #setDeltaType(SyncDeltaType) DeltaType}</li>
         * </ol>
         */
        public SyncDelta Build() {
            return new SyncDelta(_token, _deltaType, _uid, _object);
        }
    }
    #endregion
    
    #region SyncDeltaType
    /**
     * The type of change.
     */
    public enum SyncDeltaType {
        /**
         * The change represents either a create or an update in
         * the resource. These are combined into a single value because:
         * <ol>
         *    <li>Many resources will not be able to distinguish a create from an update.
         *    Those that have an audit log will be able to. However, many implementations
         *    will only have the current record and a modification timestamp.</li>
         *    <li>Regardless of whether or not the resource can distinguish the two cases,
         *    the application needs to distinguish. </li>
         * </ol>
         */
        CREATE_OR_UPDATE, 
        
        /**
         * The change represents a DELETE in the resource
         */
        DELETE
    }
    #endregion

    #region SyncResultsHandler
    /**
     * Called to handle a delta in the stream. Will be called multiple times,
     * once for each result. Although a callback, this is still invoked
     * synchronously. That is, it is guaranteed that following a call to
     * {@link SyncApiOp#sync(ObjectClass, SyncToken, SyncResultsHandler)} no
     * more invocations to {@link #handle(SyncDelta)} will be performed.
     * 
     * @param delta
     *            The change
     * @return True iff the application wants to continue processing more
     *         results.
     * @throws RuntimeException
     *             If the application encounters an exception. This will stop
     *             the interation and the exception will be propogated back to
     *             the application.
     */
    public delegate bool SyncResultsHandler(SyncDelta delta);
    #endregion
    
    #region SyncToken
    /**
     * Abstract "place-holder" for synchronization. The application must not make
     * any attempt to interpret the value of the token. From the standpoint of the
     * application the token is merely a black-box. The application may only persist
     * the value of the token for use on subsequent synchronization requests.
     * <p>
     * What this token represents is entirely connector-specific. On some connectors
     * this might be a last-modified value. On others, it might be a unique ID of a
     * log table entry. On others such as JMS, this might be a dummy value since
     * JMS itself keeps track of the state of the sync.
     */
    public sealed class SyncToken {
    
        private Object _value;
    
        /**
         * Creates a new
         * 
         * @param value
         *            May not be null. TODO: define set of allowed value types
         *            (currently same as set of allowed attribute values).
         */
        public SyncToken(Object value) {
            Assertions.NullCheck(value, "value");
            FrameworkUtil.CheckAttributeValue(value);
            _value = value;
        }
    
        /**
         * Returns the value for the token.
         * 
         * @return The value for the token.
         */
        public Object Value {
            get {
                return _value;
            }
        }
        
        public override String ToString() {
            return "SyncToken: " + _value.ToString();
        }
        
        public override int GetHashCode() {
            return CollectionUtil.GetHashCode(_value);
        }
        
        public override bool Equals(Object o) {
            if ( o is SyncToken ) {
                SyncToken other = (SyncToken)o;
                return CollectionUtil.Equals(_value, other._value);
            }
            return false;
        }
    
    
    }
    #endregion
    
    #region Uid
    public sealed class Uid : ConnectorAttribute {
        
        public static readonly string NAME = ConnectorAttributeUtil.CreateSpecialName("UID");
        
        public Uid(String val) : base(NAME, CollectionUtil.NewReadOnlyList<object>(Check(val))) {
        }
        private static String Check(String value) {
            if (StringUtil.IsBlank(value)) {
                String ERR = "Uid value must not be blank!";
                throw new ArgumentException(ERR);
            }
            return value;
        }
        /**
         * The single value of the attribute that is the unique id of an object.
         * 
         * @return value that identifies an object.
         */
        public String GetUidValue() {
            return ConnectorAttributeUtil.GetStringValue(this);
        }
    }
    #endregion

    #region ConnectorAttributesAccessor
    /**
	 * Attributes Accessor convenience methods for accessing attributes.
	 * 
	 * This class wraps a set of attributes to make lookup faster than the
	 * {@link AttributeUtil#find(String, Set)} method, since that method must
	 * re-create the map each time.
	 * 
	 * @author Warren Strange
	 */
	public class ConnectorAttributesAccessor {
	
	    ICollection<ConnectorAttribute> _attrs;
	    IDictionary<String, ConnectorAttribute> _attrMap;
	
	    public ConnectorAttributesAccessor(ICollection<ConnectorAttribute> attrs) {
	        _attrs = attrs;
	        _attrMap = ConnectorAttributeUtil.ToMap(attrs);
	    }
	
	    /**
	     * Find the named attribute
	     * 
	     * @param name -
	     *            the attribute name to search for
	     * @return the Attribute, or null if not found.
	     */
	    public ConnectorAttribute Find(String name) {
	        return CollectionUtil.GetValue(_attrMap, name, null);
	    }
	
	    /**
	     * Get the {@link Name} attribute from the set of attributes.
	     * 
	     * @return the {@link Name} attribute in the set.
	     */
	    public Name GetName() {
	        return (Name) Find(Name.NAME);
	    }
	
	    /**
	     * Return the enabled status of the account.
	     * 
	     * If the ENABLE operational attribute is present, it's value takes
	     * precedence over the current value. If it is missing, the currentlyEnabled
	     * status is returned instead.
	     * 
	     * @param dflt
	     *            the default state if enable is not found.
	     * @return true if the account is enabled, false otherwise
	     */
	    public bool GetEnabled(bool dflt) {
	        bool e = dflt;
	        ConnectorAttribute enable = Find(OperationalAttributes.ENABLE_NAME);
	        if (enable != null) {
	            e = ConnectorAttributeUtil.GetBooleanValue(enable).Value;
	        }
	        return e;
	    }
	
	    /**
	     * Get the password as a GuardeString
	     * 
	     * @return the password as a guarded String
	     */
	    public GuardedString GetPassword() {
	        ConnectorAttribute a = Find(OperationalAttributes.PASSWORD_NAME);
	        return a == null ? null : ConnectorAttributeUtil.GetGuardedStringValue(a);
	    }
	
	    /**
	     * Return a list of attributes
	     * 
	     * @param name -
	     *            name of attribute to search for.
	     * 
	     * @return The List (generic object) iff it exists otherwise null.
	     */
	    public IList<Object> FindList(String name) {
	        ConnectorAttribute a = Find(name);
	        return (a == null) ? null : a.Value;
	    }
	
	    /**
	     * Return the multivalued attribute as a list of strings. This will throw a
	     * ClassCastException if the underlying attribute list is not of type
	     * String.
	     * 
	     * @param name
	     *            the name of the attribute to search for
	     * @return a List of String values for the attribute
	     */
	    public IList<String> FindStringList(String name) {
	        IList<Object> l = FindList(name);
	        if (l != null) {
	            IList<String> ret = new List<String>(l.Count);
	            foreach (object o in l) {
	                ret.Add((String) o);
	            }
	            return ret;
	        }
	        return null;
	    }
	
	    /**
	     * Determines if the set as the attribute specified.
	     * 
	     * @param name
	     *            attribute name
	     * @return true if the named attribute exists, false otherwise
	     */
	    public bool HasAttribute(String name) {
	        return Find(name) != null;
	    }
	
	    /**
	     * Get the string value from the specified (single-valued) attribute.
	     * 
	     * @param name
	     *            Attribute from which to retrieve the long value.
	     * @return null if the value is null otherwise the long value for the
	     *         attribute.
	     * @throws ClassCastException
	     *             iff the object in the attribute is not an long.
	     * @throws IllegalArgumentException
	     *             iff the attribute is a multi-valued (rather than
	     *             single-valued).
	     */
	    public String FindString(String name) {
	        ConnectorAttribute a = Find(name);
	        return a == null ? null : ConnectorAttributeUtil.GetStringValue(a);
	    }
	
	    /**
	     * Get the integer value from the specified (single-valued) attribute.
	     * 
	     * @param name
	     *            Attribute from which to retrieve the long value.
	     * @return null if the value is null otherwise the long value for the
	     *         attribute.
	     * @throws ClassCastException
	     *             iff the object in the attribute is not an long.
	     * @throws IllegalArgumentException
	     *             iff the attribute is a multi-valued (rather than
	     *             single-valued).
	     */
	    public int? FindInteger(String name) {
	        ConnectorAttribute a = Find(name);
	        return (a == null) ? null : ConnectorAttributeUtil.GetIntegerValue(a);
	    }
	
	    /**
	     * Get the long value from the specified (single-valued) attribute.
	     * 
	     * @param name
	     *            Attribute from which to retrieve the long value.
	     * @return null if the value is null otherwise the long value for the
	     *         attribute.
	     * @throws ClassCastException
	     *             iff the object in the attribute is not an long.
	     * @throws IllegalArgumentException
	     *             iff the attribute is a multi-valued (rather than
	     *             single-valued).
	     */
	    public long? FindLong(String name) {
	        ConnectorAttribute a = Find(name);
	        return a == null ? null : ConnectorAttributeUtil.GetLongValue(a);
	    }
	
	    /**
	     * Get the date value from the specified (single-valued) attribute that
	     * contains a long.
	     * 
	     * @param name
	     *            Attribute from which to retrieve the date value.
	     * @return null if the value is null otherwise the date value for the
	     *         attribute.
	     * @throws ClassCastException
	     *             iff the object in the attribute is not an long.
	     * @throws IllegalArgumentException
	     *             iff the attribute is a multi-valued (rather than
	     *             single-valued).
	     */
	    public DateTime? FindDateTime(String name) {
	        ConnectorAttribute a = Find(name);
	        return a == null ? null : ConnectorAttributeUtil.GetDateTimeValue(a);
	    }
	
	    /**
	     * Get the integer value from the specified (single-valued) attribute.
	     * 
	     * @param name
	     *            Attribute from which to retrieve the integer value.
	     * @return null if the value is null otherwise the integer value for the
	     *         attribute.
	     * @throws ClassCastException
	     *             iff the object in the attribute is not an integer.
	     * @throws IllegalArgumentException
	     *             iff the attribute is a multi-valued (rather than
	     *             single-valued)..
	     */
	    public double? FindDouble(String name) {
	        ConnectorAttribute a = Find(name);
	        return a == null ? null : ConnectorAttributeUtil.GetDoubleValue(a);
	    }

	    /**
	     * Get the boolean value from the specified (single-valued) attribute.
	     * 
	     * @param name
	     *            Attribute from which to retrieve the boolean value.
	     * @return null if the value is null otherwise the boolean value for the
	     *         attribute.
	     * @throws ClassCastException
	     *             iff the object in the attribute is not an {@link Boolean}.
	     * @throws IllegalArgumentException
	     *             iff the attribute is a multi-valued (rather than
	     *             single-valued).
	     */
	    public bool? FindBoolean(String name) {
	        ConnectorAttribute a = Find(name);
	        return a == null ? null : ConnectorAttributeUtil.GetBooleanValue(a);
	    }
	}
	#endregion
}
