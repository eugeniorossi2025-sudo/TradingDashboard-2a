# ConfigurationApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiConfigurationGet**](#apiconfigurationget) | **GET** /api/Configuration | Gets all configurations.|
|[**apiConfigurationIdDelete**](#apiconfigurationiddelete) | **DELETE** /api/Configuration/{id} | Deletes a configuration.|
|[**apiConfigurationIdPut**](#apiconfigurationidput) | **PUT** /api/Configuration/{id} | Updates an existing configuration.|
|[**apiConfigurationKGet**](#apiconfigurationkget) | **GET** /api/Configuration/{k} | Gets a configuration by its identifier.|
|[**apiConfigurationPost**](#apiconfigurationpost) | **POST** /api/Configuration | Creates a new configuration.|

# **apiConfigurationGet**
> Array<Configuration> apiConfigurationGet()


### Example

```typescript
import {
    ConfigurationApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new ConfigurationApi(configuration);

const { status, data } = await apiInstance.apiConfigurationGet();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**Array<Configuration>**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiConfigurationIdDelete**
> apiConfigurationIdDelete()


### Example

```typescript
import {
    ConfigurationApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new ConfigurationApi(configuration);

let id: string; //The configuration identifier. (default to undefined)

const { status, data } = await apiInstance.apiConfigurationIdDelete(
    id
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **id** | [**string**] | The configuration identifier. | defaults to undefined|


### Return type

void (empty response body)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**204** | No Content |  -  |
|**404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiConfigurationIdPut**
> apiConfigurationIdPut()


### Example

```typescript
import {
    ConfigurationApi,
    Configuration,
    UpdateConfigurationRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ConfigurationApi(configuration);

let id: string; //The configuration identifier. (default to undefined)
let updateConfigurationRequest: UpdateConfigurationRequest; //The update configuration request. (optional)

const { status, data } = await apiInstance.apiConfigurationIdPut(
    id,
    updateConfigurationRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **updateConfigurationRequest** | **UpdateConfigurationRequest**| The update configuration request. | |
| **id** | [**string**] | The configuration identifier. | defaults to undefined|


### Return type

void (empty response body)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**204** | No Content |  -  |
|**404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiConfigurationKGet**
> Configuration apiConfigurationKGet()


### Example

```typescript
import {
    ConfigurationApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new ConfigurationApi(configuration);

let k: string; // (default to undefined)

const { status, data } = await apiInstance.apiConfigurationKGet(
    k
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **k** | [**string**] |  | defaults to undefined|


### Return type

**Configuration**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |
|**404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiConfigurationPost**
> Configuration apiConfigurationPost()


### Example

```typescript
import {
    ConfigurationApi,
    Configuration,
    CreateConfigurationRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ConfigurationApi(configuration);

let createConfigurationRequest: CreateConfigurationRequest; //The create configuration request. (optional)

const { status, data } = await apiInstance.apiConfigurationPost(
    createConfigurationRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **createConfigurationRequest** | **CreateConfigurationRequest**| The create configuration request. | |


### Return type

**Configuration**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**201** | Created |  -  |
|**400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

