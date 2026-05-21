# LegacyBotApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiLegacyBotSyncGet**](#apilegacybotsyncget) | **GET** /api/LegacyBot/sync | Endpoint /api/legacybot/sync - Replica esatta di index.aspx.cs  Query params: username, password, COMPUTER, TAVOLO, MARGINE, COLPO_MARTINGALA, PBT, MAZZO, TEMPO  Additional params: ACCOUNT, SALDO_INIZIALE, SALDO_ISTANTANEO, VINCITA, VALORE_GIOCATO, AVVIO  Returns: \&quot;0\&quot; (nessuna azione), \&quot;1\&quot; (Stop PC), \&quot;2\&quot; (Azzera Martingala), \&quot;3\&quot; (Start PC), \&quot;9\&quot; (Errore)|

# **apiLegacyBotSyncGet**
> apiLegacyBotSyncGet()


### Example

```typescript
import {
    LegacyBotApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new LegacyBotApi(configuration);

let username: string; // (optional) (default to undefined)
let password: string; // (optional) (default to undefined)
let cOMPUTER: string; // (optional) (default to undefined)
let tAVOLO: string; // (optional) (default to undefined)
let mARGINE: string; // (optional) (default to undefined)
let cOLPOMARTINGALA: string; // (optional) (default to undefined)
let pBT: string; // (optional) (default to undefined)
let mAZZO: string; // (optional) (default to undefined)
let tEMPO: string; // (optional) (default to undefined)
let aCCOUNT: string; // (optional) (default to undefined)
let sALDOINIZIALE: string; // (optional) (default to undefined)
let sALDOISTANTANEO: string; // (optional) (default to undefined)
let vINCITA: string; // (optional) (default to undefined)
let vALOREGIOCATO: string; // (optional) (default to undefined)
let aVVIO: string; // (optional) (default to undefined)

const { status, data } = await apiInstance.apiLegacyBotSyncGet(
    username,
    password,
    cOMPUTER,
    tAVOLO,
    mARGINE,
    cOLPOMARTINGALA,
    pBT,
    mAZZO,
    tEMPO,
    aCCOUNT,
    sALDOINIZIALE,
    sALDOISTANTANEO,
    vINCITA,
    vALOREGIOCATO,
    aVVIO
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **username** | [**string**] |  | (optional) defaults to undefined|
| **password** | [**string**] |  | (optional) defaults to undefined|
| **cOMPUTER** | [**string**] |  | (optional) defaults to undefined|
| **tAVOLO** | [**string**] |  | (optional) defaults to undefined|
| **mARGINE** | [**string**] |  | (optional) defaults to undefined|
| **cOLPOMARTINGALA** | [**string**] |  | (optional) defaults to undefined|
| **pBT** | [**string**] |  | (optional) defaults to undefined|
| **mAZZO** | [**string**] |  | (optional) defaults to undefined|
| **tEMPO** | [**string**] |  | (optional) defaults to undefined|
| **aCCOUNT** | [**string**] |  | (optional) defaults to undefined|
| **sALDOINIZIALE** | [**string**] |  | (optional) defaults to undefined|
| **sALDOISTANTANEO** | [**string**] |  | (optional) defaults to undefined|
| **vINCITA** | [**string**] |  | (optional) defaults to undefined|
| **vALOREGIOCATO** | [**string**] |  | (optional) defaults to undefined|
| **aVVIO** | [**string**] |  | (optional) defaults to undefined|


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

