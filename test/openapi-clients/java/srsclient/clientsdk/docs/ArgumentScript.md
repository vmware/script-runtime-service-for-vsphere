# ArgumentScript

## Properties
Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**templateId** | **String** | Unique identifier for the argument template script. | 
**templatePlaceholderValueList** | [**List&lt;PlaceholderValueList&gt;**](PlaceholderValueList.md) | Placeholder value list which are used to create script from script template.    Single template_placeholder_value_list produces script by the given template replacing placeholder with the given values.  Multiple items for template_placeholder_value_list produce a script of scripts which can produce an array of objects. Each template_placeholder_value_list item is used to produce script from template. Scripts are then combined in a multi-line script where each line produces result object. | 
**script** | **String** | Script result produced by the service based on given template_id and template_placeholder_parameters |  [optional]
