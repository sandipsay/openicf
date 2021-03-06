# DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS HEADER.
#
# Copyright (c) 2014 ForgeRock AS. All Rights Reserved
#
# The contents of this file are subject to the terms
# of the Common Development and Distribution License
# (the License). You may not use this file except in
# compliance with the License.
#
# You can obtain a copy of the License at
# http://forgerock.org/license/CDDLv1.0.html
# See the License for the specific language governing
# permission and limitations under the License.
#
# When distributing Covered Code, include this CDDL
# Header Notice in each file and include the License file
# at http://forgerock.org/license/CDDLv1.0.html
# If applicable, add the following below the CDDL Header,
# with the fields enclosed by brackets [] replaced by
# your own identifying information:
# " Portions Copyrighted [year] [name of copyright owner]"
#
# @author Gael Allioux <gael.allioux@forgerock.com>
#
#REQUIRES -Version 2.0

<#  
.SYNOPSIS  
    This is a sample Schema script for Active Directory 
	
.DESCRIPTION
	The script fetches user and group schema definition from Active Directory Schema
	and then build the proper OpenICF Schema
	
.INPUT VARIABLES
	The connector sends us the following:
	- <prefix>.Configuration : handler to the connector's configuration object
	- <prefix>.Operation: String correponding to the action ("SCHEMA" here)
	- <prefix>.SchemaBuilder: an instance of org.identityconnectors.framework.common.objects.SchemaBuilder 
	that must be used to define the schema.
	
.RETURNS 
	Nothing. Connector will finalize the schema build.
	
.NOTES  
    File Name      : ADSchema.ps1  
    Author         : Gael Allioux (gael.allioux@forgerock.com)
    Prerequisite   : PowerShell V2 - AD module loaded by the connector
    Copyright 2014 - ForgeRock AS    

.LINK  
    Script posted over:  
    http://openicf.forgerock.org
		
	Active Directory Administration with Windows PowerShell
	http://technet.microsoft.com/en-us/library/dd378937(v=ws.10).aspx
#>

try
{
$AttributeInfoBuilder = [Org.IdentityConnectors.Framework.Common.Objects.ConnectorAttributeInfoBuilder]

 if ($Connector.Operation -eq "SCHEMA")
 {
	$schema =[DirectoryServices.ActiveDirectory.ActiveDirectorySchema]::GetCurrentSchema()
	
 	###########################
 	# AD User object class
	###########################
	# Here is an example of AD attribute definition:
	#
	# Name                   : sAMAccountName
	# CommonName             : SAM-Account-Name
	# Oid                    : 1.2.840.113556.1.4.221
	# Syntax                 : DirectoryString
	# Description            :
	# IsSingleValued         : True
	# IsIndexed              : True
	# IsIndexedOverContainer : False
	# IsInAnr                : True
	# IsOnTombstonedObject   : True
	# IsTupleIndexed         : False
	# IsInGlobalCatalog      : True
	# RangeLower             : 0
	# RangeUpper             : 256
	# IsDefunct              : False
	# Link                   :
	# LinkId                 :
	# SchemaGuid             : 3e0abfd0-126a-11d0-a060-00aa006c33ed
	
	$ocib = New-Object Org.IdentityConnectors.Framework.Common.Objects.ObjectClassInfoBuilder
	$ocib.ObjectType = "__ACCOUNT__"
	
	foreach ($attr in $schema.FindClass("user").MandatoryProperties)
	{
		if ($attr.Name.StartsWith("ms","CurrentCultureIgnoreCase")) {continue}
		$caib = New-Object Org.IdentityConnectors.Framework.Common.Objects.ConnectorAttributeInfoBuilder($attr.Name);
		$caib.Required = $TRUE
		if (! $attr.IsSingleValued ) {$caib.MultiValued = $TRUE}
		switch ($attr.Syntax)
		{
			"Int64" {$caib.ValueType = [long] ; break}
            "Int" {$caib.ValueType = [int]; break}
            "Bool" {$caib.ValueType = [bool]; break}
			"Enumeration" {$caib.ValueType = [int]; break}
			default {break} # String is default
		}
		$ocib.AddAttributeInfo($caib.Build())
	}
	
	foreach ($attr in $schema.FindClass("user").OptionalProperties)
	{
		if ($attr.Name.StartsWith("ms","CurrentCultureIgnoreCase")) {continue}
		$caib = New-Object Org.IdentityConnectors.Framework.Common.Objects.ConnectorAttributeInfoBuilder($attr.Name);
		if (! $attr.IsSingleValued ) {$caib.MultiValued = $TRUE}
		switch ($attr.Syntax)
		{
			"Int64" {$caib.ValueType = [long] ; break}
            "Int" {$caib.ValueType = [int]; break}
            "Bool" {$caib.ValueType = [bool]; break}
			"Enumeration" {$caib.ValueType = [int]; break}
			default {break} # String is default
		}
		$ocib.AddAttributeInfo($caib.Build())
	}
	
	# A few custom attributes we want to add here
	$ocib.AddAttributeInfo($AttributeInfoBuilder::Build("accountExpirationDate",[string]))
	$ocib.AddAttributeInfo($AttributeInfoBuilder::Build("allowReversiblePasswordEncryption",[bool]))
	$ocib.AddAttributeInfo($AttributeInfoBuilder::Build("cannotChangePassword",[bool]))
	$ocib.AddAttributeInfo($AttributeInfoBuilder::Build("changePasswordAtLogon",[bool]))
	$ocib.AddAttributeInfo($AttributeInfoBuilder::Build("passwordNeverExpires",[bool]))
	$ocib.AddAttributeInfo($AttributeInfoBuilder::Build("passwordNotRequired",[bool]))
	$ocib.AddAttributeInfo($AttributeInfoBuilder::Build("smartcardLogonRequired",[bool]))
	$ocib.AddAttributeInfo($AttributeInfoBuilder::Build("trustedForDelegation",[bool]))
	
 	# A few operational attributes as well
	$opAttrs = [Org.IdentityConnectors.Framework.Common.Objects.OperationalAttributeInfos]
	$ocib.AddAttributeInfo($opAttrs::ENABLE)
	$ocib.AddAttributeInfo($opAttrs::PASSWORD)
	$ocib.AddAttributeInfo($opAttrs::LOCK_OUT)
	$ocib.AddAttributeInfo($opAttrs::PASSWORD_EXPIRED)
	$ocib.AddAttributeInfo($opAttrs::DISABLE_DATE)
	$ocib.AddAttributeInfo($opAttrs::ENABLE_DATE)
	
	$Connector.SchemaBuilder.DefineObjectClass($ocib.Build())
	
	
	###########################
 	# __GROUP__ object class
	###########################
	$ocib = New-Object Org.IdentityConnectors.Framework.Common.Objects.ObjectClassInfoBuilder
	$ocib.ObjectType = "__GROUP__"
	
	foreach ($attr in $schema.FindClass("group").MandatoryProperties)
	{
		if ($attr.Name.StartsWith("ms","CurrentCultureIgnoreCase")) {continue}
		$caib = New-Object Org.IdentityConnectors.Framework.Common.Objects.ConnectorAttributeInfoBuilder($attr.Name);
		$caib.Required = $TRUE
		if (! $attr.IsSingleValued ) {$caib.MultiValued = $TRUE}
		switch ($attr.Syntax)
		{
			"Int64" {$caib.ValueType = [long] ; break}
            "Int" {$caib.ValueType = [int]; break}
            "Bool" {$caib.ValueType = [bool]; break}
			"Enumeration" {$caib.ValueType = [int]; break}
			default {break} # String is default
		}
		$ocib.AddAttributeInfo($caib.Build())
	}
	
	foreach ($attr in $schema.FindClass("group").OptionalProperties)
	{
		if ($attr.Name.StartsWith("ms","CurrentCultureIgnoreCase")) {continue}
		$caib = New-Object Org.IdentityConnectors.Framework.Common.Objects.ConnectorAttributeInfoBuilder($attr.Name);
		if (! $attr.IsSingleValued ) {$caib.MultiValued = $TRUE}
		switch ($attr.Syntax)
		{
			"Int64" {$caib.ValueType = [long] ; break}
            "Int" {$caib.ValueType = [int]; break}
            "Bool" {$caib.ValueType = [bool]; break}
			"Enumeration" {$caib.ValueType = [int]; break}
			default {break} # String is default
		}
		$ocib.AddAttributeInfo($caib.Build())
	}
	
	# A few custom attributes we want to add here
	# group category is either 'security' or 'distribution'
	$ocib.AddAttributeInfo($AttributeInfoBuilder::Build("groupCategory",[string]))
	# group scope is either 'DomainLocal', 'Universal', 'Global'
	$ocib.AddAttributeInfo($AttributeInfoBuilder::Build("groupScope",[string]))
	
	$Connector.SchemaBuilder.DefineObjectClass($ocib.Build())
 }
 }
 catch #Rethrow the original exception
 {
 	throw
 }
