# BotSessionApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiBotSessionActiveGet**](#apibotsessionactiveget) | **GET** /api/BotSession/active | Get active bot sessions.|
|[**apiBotSessionCleanupInactivePost**](#apibotsessioncleanupinactivepost) | **POST** /api/BotSession/cleanup-inactive | Mark inactive sessions as stopped (background job endpoint).|
|[**apiBotSessionEventPost**](#apibotsessioneventpost) | **POST** /api/BotSession/event | Process a bot session event (START, STOP, HEARTBEAT).|
|[**apiBotSessionPingGet**](#apibotsessionpingget) | **GET** /api/BotSession/ping | Quick ping to verify the service is running.|

# **apiBotSessionActiveGet**
> Array<PcStart> apiBotSessionActiveGet()


### Example

```typescript
import {
    BotSessionApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new BotSessionApi(configuration);

let computer: string; //Optional computer name filter (optional) (default to undefined)

const { status, data } = await apiInstance.apiBotSessionActiveGet(
    computer
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **computer** | [**string**] | Optional computer name filter | (optional) defaults to undefined|


### Return type

**Array<PcStart>**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiBotSessionCleanupInactivePost**
> any apiBotSessionCleanupInactivePost()


### Example

```typescript
import {
    BotSessionApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new BotSessionApi(configuration);

let inactivityMinutes: number; //Minutes of inactivity before marking as stopped (optional) (default to 5)

const { status, data } = await apiInstance.apiBotSessionCleanupInactivePost(
    inactivityMinutes
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **inactivityMinutes** | [**number**] | Minutes of inactivity before marking as stopped | (optional) defaults to 5|


### Return type

**any**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiBotSessionEventPost**
> BotSessionEventResponse apiBotSessionEventPost()

Event Types:  - START: Bot session started (creates PC_Start record)  - STOP: Bot session ended (updates PC_Start, creates Stats and Stats_Margine)  - HEARTBEAT: Periodic update (updates session data, creates Stats_Margine snapshot)    Example START request:  ```json  {    \"eventType\": \"START\",    \"computer\": \"BOT-PC-01\",    \"userId\": 1,    \"botVersion\": \"2.5.3\",    \"balance\": 1000.00,    \"reason\": \"Manual\"  }  ```    Example STOP request:  ```json  {    \"eventType\": \"STOP\",    \"computer\": \"BOT-PC-01\",    \"userId\": 1,    \"balance\": 1250.00,    \"margine\": 250.00,    \"reason\": \"Target reached\",    \"totalHands\": 150,    \"totalWins\": 75,    \"totalLosses\": 70,    \"totalTies\": 5,    \"maxProfit\": 280.00,    \"maxDrawdown\": -50.00  }  ```

### Example

```typescript
import {
    BotSessionApi,
    Configuration,
    BotSessionEventRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new BotSessionApi(configuration);

let botSessionEventRequest: BotSessionEventRequest; //Session event data (optional)

const { status, data } = await apiInstance.apiBotSessionEventPost(
    botSessionEventRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **botSessionEventRequest** | **BotSessionEventRequest**| Session event data | |


### Return type

**BotSessionEventResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |
|**400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiBotSessionPingGet**
> apiBotSessionPingGet()


### Example

```typescript
import {
    BotSessionApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new BotSessionApi(configuration);

const { status, data } = await apiInstance.apiBotSessionPingGet();
```

### Parameters
This endpoint does not have any parameters.


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
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

