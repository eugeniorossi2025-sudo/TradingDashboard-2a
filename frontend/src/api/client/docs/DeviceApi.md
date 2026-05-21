# DeviceApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiDeviceGet**](#apideviceget) | **GET** /api/Device | Gets all devices.|
|[**apiDeviceIdDelete**](#apideviceiddelete) | **DELETE** /api/Device/{id} | Deletes a device.|
|[**apiDeviceIdGet**](#apideviceidget) | **GET** /api/Device/{id} | Gets a device by its identifier.|
|[**apiDeviceIdPut**](#apideviceidput) | **PUT** /api/Device/{id} | Updates an existing device.|
|[**apiDevicePost**](#apidevicepost) | **POST** /api/Device | Creates a new device.|

# **apiDeviceGet**
> DeviceIEnumerableApiResponse apiDeviceGet()


### Example

```typescript
import {
    DeviceApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new DeviceApi(configuration);

const { status, data } = await apiInstance.apiDeviceGet();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**DeviceIEnumerableApiResponse**

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

# **apiDeviceIdDelete**
> ObjectApiResponse apiDeviceIdDelete()


### Example

```typescript
import {
    DeviceApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new DeviceApi(configuration);

let id: number; //The device identifier. (default to undefined)

const { status, data } = await apiInstance.apiDeviceIdDelete(
    id
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **id** | [**number**] | The device identifier. | defaults to undefined|


### Return type

**ObjectApiResponse**

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

# **apiDeviceIdGet**
> DeviceApiResponse apiDeviceIdGet()


### Example

```typescript
import {
    DeviceApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new DeviceApi(configuration);

let id: number; //The device identifier (PC name). (default to undefined)

const { status, data } = await apiInstance.apiDeviceIdGet(
    id
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **id** | [**number**] | The device identifier (PC name). | defaults to undefined|


### Return type

**DeviceApiResponse**

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

# **apiDeviceIdPut**
> DeviceApiResponse apiDeviceIdPut()


### Example

```typescript
import {
    DeviceApi,
    Configuration,
    UpdateDeviceRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new DeviceApi(configuration);

let id: number; //The device identifier. (default to undefined)
let updateDeviceRequest: UpdateDeviceRequest; //The update device request. (optional)

const { status, data } = await apiInstance.apiDeviceIdPut(
    id,
    updateDeviceRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **updateDeviceRequest** | **UpdateDeviceRequest**| The update device request. | |
| **id** | [**number**] | The device identifier. | defaults to undefined|


### Return type

**DeviceApiResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |
|**404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiDevicePost**
> DeviceApiResponse apiDevicePost()


### Example

```typescript
import {
    DeviceApi,
    Configuration,
    CreateDeviceRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new DeviceApi(configuration);

let createDeviceRequest: CreateDeviceRequest; //The create device request. (optional)

const { status, data } = await apiInstance.apiDevicePost(
    createDeviceRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **createDeviceRequest** | **CreateDeviceRequest**| The create device request. | |


### Return type

**DeviceApiResponse**

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
|**409** | Conflict |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

