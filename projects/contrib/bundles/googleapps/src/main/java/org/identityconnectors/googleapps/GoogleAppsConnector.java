/*
 * Copyright 2008 Sun Microsystems, Inc. All rights reserved.
 * 
 * U.S. Government Rights - Commercial software. Government users 
 * are subject to the Sun Microsystems, Inc. standard license agreement
 * and applicable provisions of the FAR and its supplements.
 * 
 * Use is subject to license terms.
 * 
 * This distribution may include materials developed by third parties.
 * Sun, Sun Microsystems, the Sun logo, Java and Project Identity 
 * Connectors are trademarks or registered trademarks of Sun 
 * Microsystems, Inc. or its subsidiaries in the U.S. and other
 * countries.
 * 
 * UNIX is a registered trademark in the U.S. and other countries,
 * exclusively licensed through X/Open Company, Ltd. 
 * 
 * -----------
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS HEADER.
 * 
 * Copyright 2008 Sun Microsystems, Inc. All rights reserved. 
 * 
 * The contents of this file are subject to the terms of the Common Development
 * and Distribution License(CDDL) (the License).  You may not use this file
 * except in  compliance with the License. 
 * 
 * You can obtain a copy of the License at
 * http://identityconnectors.dev.java.net/CDDLv1.0.html
 * See the License for the specific language governing permissions and 
 * limitations under the License.  
 * 
 * When distributing the Covered Code, include this CDDL Header Notice in each
 * file and include the License file at identityconnectors/legal/license.txt.
 * If applicable, add the following below this CDDL Header, with the fields 
 * enclosed by brackets [] replaced by your own identifying information: 
 * "Portions Copyrighted [year] [name of copyright owner]"
 * -----------
 */
package org.identityconnectors.googleapps;


import org.identityconnectors.framework.common.objects.OperationOptions;
import org.identityconnectors.framework.common.objects.filter.FilterTranslator;


import org.identityconnectors.common.logging.Log;
import org.identityconnectors.framework.common.exceptions.ConnectorException;
import org.identityconnectors.framework.common.objects.Attribute;
import org.identityconnectors.framework.common.objects.AttributeInfo;
import org.identityconnectors.framework.common.objects.AttributeInfoBuilder;
import org.identityconnectors.framework.common.objects.AttributeUtil;
import org.identityconnectors.framework.common.objects.Name;
import org.identityconnectors.framework.common.objects.ObjectClass;
import org.identityconnectors.framework.common.objects.OperationalAttributeInfos;
import org.identityconnectors.framework.common.objects.ResultsHandler;
import org.identityconnectors.framework.common.objects.Schema;
import org.identityconnectors.framework.common.objects.SchemaBuilder;
import org.identityconnectors.framework.common.objects.Uid;
import org.identityconnectors.framework.spi.Configuration;
import org.identityconnectors.framework.spi.Connector;
import org.identityconnectors.framework.spi.ConnectorClass;
import org.identityconnectors.framework.spi.operations.CreateOp;
import org.identityconnectors.framework.spi.operations.DeleteOp;
import org.identityconnectors.framework.spi.operations.SchemaOp;
import org.identityconnectors.framework.spi.operations.SearchOp;
import java.util.HashSet;
import java.util.Set;
import org.identityconnectors.framework.common.objects.AttributesAccessor;
import org.identityconnectors.framework.spi.PoolableConnector;
import org.identityconnectors.framework.spi.operations.TestOp;
import org.identityconnectors.framework.spi.operations.UpdateOp;

/**
 * <p> A connector for Google Apps for your domain.
 * This connector can manage accounts on Google Apps. It also 
 * manages nicknames (email aliases) associated with users.
 * <p>
 *
 * Notes
 *
 * According to this thread: <br/>
 * http://groups.google.com/group/google-apps-apis/browse_thread/thread/d68b2458b4e84777
 * <br/>
 * The API does not support a rename operation on an account. You have to delete
 * and receate the account.
 * <br/>
 * Quota can be read - but it appears to ignore it on a create. This is the
 * same behavior as creating through the Google Admin Web GUI (it doesnt
 * even let you set it). I believe this is due to the type of google apps
 * account I have used for testing. The default is to allow the user to
 * have the maxium quota - which is what most organizations will want anyways.
 *
 * 
 * @author Warren Strange
 * @version $Revision $
 * @since 1.0
 */
@ConnectorClass(configurationClass = GoogleAppsConfiguration.class, displayNameKey = "googleapps.connector.display")
public class GoogleAppsConnector implements
        PoolableConnector, CreateOp, SearchOp<String>, DeleteOp, SchemaOp, UpdateOp, TestOp {

    public static final String ATTR_FAMILY_NAME = "familyName";
    public static final String ATTR_GIVEN_NAME = "givenName";
    public static final String ATTR_QUOTA = "quota";
    public static final String ATTR_NICKNAME_LIST = "nicknames";
     public static final String ATTR_GROUP_LIST = "groups";

    // Group Objects
    public static final String ATTR_MEMBER_LIST = "members";
    public static final String ATTR_OWNER_LIST = "owners";
    public static final String ATTR_GROUP_TEXT_NAME = "groupName";
    public static final String ATTR_GROUP_DESCRIPTION = "groupDescription";
    public static final String ATTR_GROUP_PERMISSIONS = "groupPermissions"; 
    /**
     * Setup logging for the {@link GoogleAppsConnector}.
     */
    Log log = Log.getLog(GoogleAppsConnector.class);
    /**
     * Place holder for the {@link Connection} passed into the callback
     * {@link ConnectionFactory#setConnection(Connection)}.
     */
    private GoogleAppsConnection gc;
    /**
     * Place holder for the {@link Configuration} passed into the callback
     * {@link GoogleAppsConnector#init(Configuration)}.
     */
    private GoogleAppsConfiguration config;


    private GoogleAppsUserOps   userOps;
    private GoogleAppsGroupOps groupOps;

    // =======================================================================
    // Initialize/dispose methods..
    // =======================================================================
    /**
     * Callback method to receive the {@link Configuration}.
     * 
     * @see org.identityconnectors.framework.spi.Connector#init(Configuration)
     */
    public void init(Configuration cfg) {
        this.config = (GoogleAppsConfiguration) cfg;
        this.gc = newConnection();
        this.userOps = new GoogleAppsUserOps(gc);
        this.groupOps = new GoogleAppsGroupOps(gc);
    }

    /**
     * Disposes of the {@link GoogleAppsConnector}'s resources.
     *
     * Nothing really to do here as the google apps adapter does not
     * consume client resources.
     * 
     * @see Connector#dispose()
     */
    public void dispose() {
    }

    /**
     * 
     * {@inheritDoc}
     * @see SchemaOp#schema()
     */
    public Schema schema() {
        if (config == null) {
            throw new IllegalStateException("Configuration object has not been set.");
        }
        SchemaBuilder schemaBuilder = new SchemaBuilder(getClass());

        Set<AttributeInfo> attributes = new HashSet<AttributeInfo>();

        // Required Attributes
        //
        AttributeInfoBuilder name = new AttributeInfoBuilder();
        name.setCreateable(true);
        name.setUpdateable(false);
        name.setName(Name.NAME);

        attributes.add(name.build());
        // regular attributes
        attributes.add(AttributeInfoBuilder.build(ATTR_FAMILY_NAME));
        attributes.add(AttributeInfoBuilder.build(ATTR_GIVEN_NAME));

        // quota can be set on create, but can not be updated after account creation
        AttributeInfoBuilder q = new AttributeInfoBuilder();
        q.setCreateable(true);
        q.setName(ATTR_QUOTA);
        q.setType(Integer.class);
        q.setUpdateable(false);
        q.setReadable(true);

        attributes.add(q.build());
        // Multi-valued attributes - group/nicknames - not required / not returned by default
        attributes.add(buildMultivaluedAttribute(ATTR_NICKNAME_LIST, String.class, false, false));
        attributes.add(buildMultivaluedAttribute(ATTR_MEMBER_LIST, String.class, false, false));


        // Operational Attributes - password and enable/disable status
        //
        attributes.add(OperationalAttributeInfos.PASSWORD);
        attributes.add(OperationalAttributeInfos.ENABLE);    
      
       
        schemaBuilder.defineObjectClass(ObjectClass.ACCOUNT_NAME, attributes);


        // Build Group Schema
        attributes = new HashSet<AttributeInfo>();
        attributes.add(name.build());
        attributes.add(AttributeInfoBuilder.build(ATTR_GROUP_TEXT_NAME));
        attributes.add(AttributeInfoBuilder.build(ATTR_GROUP_DESCRIPTION));
        attributes.add(AttributeInfoBuilder.build(ATTR_GROUP_PERMISSIONS));
        attributes.add(buildMultivaluedAttribute(ATTR_OWNER_LIST, String.class, false, false));
        attributes.add(buildMultivaluedAttribute(ATTR_MEMBER_LIST, String.class, false, false));
        schemaBuilder.defineObjectClass(ObjectClass.GROUP_NAME, attributes);
       

        return schemaBuilder.build();

    }
    // =======================================================================
    // Connection Pooling..
    // =======================================================================

    /**
     * Creates a new {@link Connection} based on the {@link Configuration}
     * passed into {@link GoogleAppsConnector#init(Configuration)}.
     * 
     * @see ConnectionFactory#newConnection()
     */
    public GoogleAppsConnection newConnection() {
        return new GoogleAppsConnection(config);
    }

    /**
     * Call-back to receive a tested {@link Connection} from the pool.
     * 
     * @see ConnectionFactory#setConnection(Connection)
     */
    public void setConnection(GoogleAppsConnection conn) {
        this.gc = conn;
    }

    /**
     * Creates a a google apps user 
     * 
     * @param attrs
     *            Various attributes of to a google apps user or group
     * @return the value that represents the google aps account id for the user. 
     */
    public Uid create(ObjectClass oclass, Set<Attribute> attrs, OperationOptions arg2) {
        Name name = AttributeUtil.getNameFromAttributes(attrs);

        log.info("Create {0}", name);
        AttributesAccessor a = new AttributesAccessor(attrs);

        if( name == null ) return null;

        if(ObjectClass.ACCOUNT.equals(oclass))
            return userOps.createUser(name, a);
        else if( ObjectClass.GROUP.equals(oclass))
            return groupOps.createGroup(name,a);
        else
            throw new IllegalArgumentException("Unsupported Object Class=" + oclass.getObjectClassValue());
    }

    /**
     * Delete a google apps account and all associated nicknames.
     *
     * Note that once an account is deleted, the account id can not be reused
     * for 5 days (according to google).
     * 
     * @see DeleteOp#delete(Uid)
     */
    public void delete(final ObjectClass objClass, final Uid uid, OperationOptions options) {
        String id = uid.getUidValue();
        log.info("Deleting {0}", id);
        if (ObjectClass.ACCOUNT.equals(objClass)) 
            userOps.delete(id);
        else if(ObjectClass.GROUP.equals(objClass) )
            groupOps.delete(id);
        else
          throw new IllegalArgumentException("Unsupported Object Class=" + objClass.getObjectClassValue()); 
    }

    public Configuration getConfiguration() {
        return config;
    }

    /**
     * Utility to build a multi valued attribute.
     * 
     * @param name
     * @param clazz
     * @param required
     * @return
     */
    private AttributeInfo buildMultivaluedAttribute(String name, Class<?> clazz, boolean required, boolean returnByDefault) {
        AttributeInfoBuilder builder = new AttributeInfoBuilder();
        builder.setName(name);
        builder.setType(clazz);
        builder.setRequired(required);
        builder.setMultiValued(true);
        builder.setReturnedByDefault(returnByDefault);
        return builder.build();
    }

    public FilterTranslator<String> createFilterTranslator(ObjectClass arg0, OperationOptions arg1) {
        return new GoogleAppsFilterTranslator();
    }

    /**
     * Execute the search operation.
     *
     * This adapter only supports searching by account name
     *
     *
     *
     * @param oclass Object class to search for. We only support ACCOUNT
     * @param query - Query string. Must be the account name or NULL for all accounts
     * @param handler - Results handler
     * @param ops - search options. By default, the nicknames for a user are not returned
     */
    public void executeQuery(ObjectClass oclass, String query, ResultsHandler handler, OperationOptions ops) {
        log.info("query string = {0}", query);
        if (ObjectClass.ACCOUNT.equals(oclass))
            userOps.query(query,handler,ops);
        else if ( ObjectClass.GROUP.equals(oclass))
            groupOps.query(query,handler,ops);
        else
            throw new IllegalArgumentException("Unsupported objectclass '" + oclass + "'");
    }


    /**
     * Update a googleapps user account
     *
     * Note that we do not support account rename operations
     * Only the name (given,family), nicknames,
     * and the password can be updated
     * Quota can not be changed after a create
     *
     *
     * 
     * Nickname list updates are rather inefficent - but the number
     * of nickname for a user should be small.
     *
     * 
     * @param objclass - Object class - we only support ACCOUNT
     * @param replaceAttrs - attribute set to replace in update set
     * @param options - update options
     * @return the Uid of the updated object
     */
    public Uid update(ObjectClass objclass, Uid uid, Set<Attribute> replaceAttrs, OperationOptions options) {

        log.info("Update {0}", replaceAttrs);

        if (uid == null) {
            throw new ConnectorException("Uid attribute is missing!");
        }
        if (replaceAttrs == null || replaceAttrs.size() == 0) {
            throw new IllegalArgumentException("Invalid attributes provided to a update operation.");
        }


        if( ObjectClass.ACCOUNT.equals(objclass))
            return userOps.updateUser(uid,replaceAttrs,options);
        else if (ObjectClass.GROUP.equals(objclass))
            return groupOps.updateGroup(uid,replaceAttrs,options);
        else
            throw new IllegalArgumentException("Unsupported objectclass '" + objclass + "'");
        
    }

    public void test() {
        log.info("test connection");
        GoogleAppsClient g = gc.getConnection();
        g.testConnection();
    }

    /**
     * Check the google apps connection is alive
     */
    public void checkAlive() {
        log.info("checkAlive");
        test();
    }

   
}