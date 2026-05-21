# DashboardApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiDashboardEmergencyStopPost**](#apidashboardemergencystoppost) | **POST** /api/Dashboard/emergency-stop | POST /api/dashboard/emergency-stop  Resetta le tabelle Value e PcCurrentStatus|
|[**apiDashboardMarginiChartGet**](#apidashboardmarginichartget) | **GET** /api/Dashboard/margini-chart | GET /api/dashboard/margini-chart  Restituisce i punti del grafico dei margini (solo quando cambiano)|
|[**apiDashboardPcCurrentStatusGet**](#apidashboardpccurrentstatusget) | **GET** /api/Dashboard/pc-current-status | GET /api/dashboard/pc-current-status  Restituisce lo stato corrente di tutti i PC|
|[**apiDashboardResetTablesPost**](#apidashboardresettablespost) | **POST** /api/Dashboard/reset-tables | POST /api/dashboard/reset-tables  Resetta le tabelle Value e PcCurrentStatus|

# **apiDashboardEmergencyStopPost**
> apiDashboardEmergencyStopPost()


### Example

```typescript
import {
    DashboardApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new DashboardApi(configuration);

const { status, data } = await apiInstance.apiDashboardEmergencyStopPost();
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

# **apiDashboardMarginiChartGet**
> Array<ChartDataPoint> apiDashboardMarginiChartGet()


### Example

```typescript
import {
    DashboardApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new DashboardApi(configuration);

const { status, data } = await apiInstance.apiDashboardMarginiChartGet();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**Array<ChartDataPoint>**

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

# **apiDashboardPcCurrentStatusGet**
> Array<PcCurrentStatus> apiDashboardPcCurrentStatusGet()


### Example

```typescript
import {
    DashboardApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new DashboardApi(configuration);

const { status, data } = await apiInstance.apiDashboardPcCurrentStatusGet();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**Array<PcCurrentStatus>**

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

# **apiDashboardResetTablesPost**
> apiDashboardResetTablesPost()


### Example

```typescript
import {
    DashboardApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new DashboardApi(configuration);

const { status, data } = await apiInstance.apiDashboardResetTablesPost();
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

