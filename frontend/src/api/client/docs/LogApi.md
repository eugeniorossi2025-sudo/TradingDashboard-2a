# LogApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiLogDelete**](#apilogdelete) | **DELETE** /api/Log | Deletes ApiLogs matching the given filters.|
|[**apiLogGet**](#apilogget) | **GET** /api/Log | Gets ApiLogs with optional filters for CreatedAt (range), Category, and Action.|

# **apiLogDelete**
> apiLogDelete()


### Example

```typescript
import {
    LogApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new LogApi(configuration);

let from: string; // (optional) (default to undefined)
let to: string; // (optional) (default to undefined)
let category: string; // (optional) (default to undefined)
let action: number; // (optional) (default to undefined)

const { status, data } = await apiInstance.apiLogDelete(
    from,
    to,
    category,
    action
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **from** | [**string**] |  | (optional) defaults to undefined|
| **to** | [**string**] |  | (optional) defaults to undefined|
| **category** | [**string**] |  | (optional) defaults to undefined|
| **action** | [**number**] |  | (optional) defaults to undefined|


### Return type

void (empty response body)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**204** | No Content |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiLogGet**
> PagedApiLogResult apiLogGet()


### Example

```typescript
import {
    LogApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new LogApi(configuration);

let from: string; //Start of CreatedAt range (inclusive, optional). (optional) (default to undefined)
let to: string; //End of CreatedAt range (inclusive, optional). (optional) (default to undefined)
let category: string; //Category filter (optional). (optional) (default to undefined)
let action: number; //Action filter (optional). (optional) (default to undefined)
let description: string; // (optional) (default to undefined)
let page: number; // (optional) (default to 1)
let pageSize: number; // (optional) (default to 50)

const { status, data } = await apiInstance.apiLogGet(
    from,
    to,
    category,
    action,
    description,
    page,
    pageSize
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **from** | [**string**] | Start of CreatedAt range (inclusive, optional). | (optional) defaults to undefined|
| **to** | [**string**] | End of CreatedAt range (inclusive, optional). | (optional) defaults to undefined|
| **category** | [**string**] | Category filter (optional). | (optional) defaults to undefined|
| **action** | [**number**] | Action filter (optional). | (optional) defaults to undefined|
| **description** | [**string**] |  | (optional) defaults to undefined|
| **page** | [**number**] |  | (optional) defaults to 1|
| **pageSize** | [**number**] |  | (optional) defaults to 50|


### Return type

**PagedApiLogResult**

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

