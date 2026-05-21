# UserApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiUserAvailablePermissionsGet**](#apiuseravailablepermissionsget) | **GET** /api/User/available-permissions | Gets all available permissions in the system from database (Admin only).|
|[**apiUserAvailableRolesGet**](#apiuseravailablerolesget) | **GET** /api/User/available-roles | Gets all available roles in the system from database (Admin only).|
|[**apiUserGet**](#apiuserget) | **GET** /api/User | Gets all users (Admin only).|
|[**apiUserIdDelete**](#apiuseriddelete) | **DELETE** /api/User/{id} | Deletes a user (Admin only).|
|[**apiUserIdGet**](#apiuseridget) | **GET** /api/User/{id} | Gets a user by ID (Authenticated users can see their own profile, Admin can see all).|
|[**apiUserIdPermissionsPermissionDelete**](#apiuseridpermissionspermissiondelete) | **DELETE** /api/User/{id}/permissions/{permission} | Removes a permission from a user (Admin only).|
|[**apiUserIdPermissionsPost**](#apiuseridpermissionspost) | **POST** /api/User/{id}/permissions | Assigns a permission to a user (Admin only).|
|[**apiUserIdRolesAndPermissionsGet**](#apiuseridrolesandpermissionsget) | **GET** /api/User/{id}/roles-and-permissions | Gets all roles and permissions for a user (Admin only).|
|[**apiUserIdRolesPost**](#apiuseridrolespost) | **POST** /api/User/{id}/roles | Assigns a role to a user (Admin only).|
|[**apiUserIdRolesRoleNameDelete**](#apiuseridrolesrolenamedelete) | **DELETE** /api/User/{id}/roles/{roleName} | Removes a role from a user (Admin only).|
|[**apiUserPost**](#apiuserpost) | **POST** /api/User | Creates a new user (Admin only).|

# **apiUserAvailablePermissionsGet**
> ObjectApiResponse apiUserAvailablePermissionsGet()


### Example

```typescript
import {
    UserApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

const { status, data } = await apiInstance.apiUserAvailablePermissionsGet();
```

### Parameters
This endpoint does not have any parameters.


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

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiUserAvailableRolesGet**
> StringIEnumerableApiResponse apiUserAvailableRolesGet()


### Example

```typescript
import {
    UserApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

const { status, data } = await apiInstance.apiUserAvailableRolesGet();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**StringIEnumerableApiResponse**

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

# **apiUserGet**
> UserIEnumerableApiResponse apiUserGet()


### Example

```typescript
import {
    UserApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

const { status, data } = await apiInstance.apiUserGet();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**UserIEnumerableApiResponse**

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

# **apiUserIdDelete**
> ObjectApiResponse apiUserIdDelete()


### Example

```typescript
import {
    UserApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let id: string; //The user identifier. (default to undefined)

const { status, data } = await apiInstance.apiUserIdDelete(
    id
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **id** | [**string**] | The user identifier. | defaults to undefined|


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

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiUserIdGet**
> UserApiResponse apiUserIdGet()


### Example

```typescript
import {
    UserApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let id: string; //The user identifier. (default to undefined)

const { status, data } = await apiInstance.apiUserIdGet(
    id
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **id** | [**string**] | The user identifier. | defaults to undefined|


### Return type

**UserApiResponse**

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

# **apiUserIdPermissionsPermissionDelete**
> ObjectApiResponse apiUserIdPermissionsPermissionDelete()


### Example

```typescript
import {
    UserApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let id: string; //The user identifier. (default to undefined)
let permission: string; //The permission to remove. (default to undefined)

const { status, data } = await apiInstance.apiUserIdPermissionsPermissionDelete(
    id,
    permission
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **id** | [**string**] | The user identifier. | defaults to undefined|
| **permission** | [**string**] | The permission to remove. | defaults to undefined|


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

# **apiUserIdPermissionsPost**
> ObjectApiResponse apiUserIdPermissionsPost()


### Example

```typescript
import {
    UserApi,
    Configuration,
    AssignPermissionRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let id: string; //The user identifier. (default to undefined)
let assignPermissionRequest: AssignPermissionRequest; //The permission assignment request. (optional)

const { status, data } = await apiInstance.apiUserIdPermissionsPost(
    id,
    assignPermissionRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **assignPermissionRequest** | **AssignPermissionRequest**| The permission assignment request. | |
| **id** | [**string**] | The user identifier. | defaults to undefined|


### Return type

**ObjectApiResponse**

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
|**400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiUserIdRolesAndPermissionsGet**
> UserRolesAndPermissionsResponseApiResponse apiUserIdRolesAndPermissionsGet()


### Example

```typescript
import {
    UserApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let id: string; //The user identifier. (default to undefined)

const { status, data } = await apiInstance.apiUserIdRolesAndPermissionsGet(
    id
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **id** | [**string**] | The user identifier. | defaults to undefined|


### Return type

**UserRolesAndPermissionsResponseApiResponse**

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

# **apiUserIdRolesPost**
> ObjectApiResponse apiUserIdRolesPost()


### Example

```typescript
import {
    UserApi,
    Configuration,
    AssignRoleRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let id: string; //The user identifier. (default to undefined)
let assignRoleRequest: AssignRoleRequest; //The role assignment request. (optional)

const { status, data } = await apiInstance.apiUserIdRolesPost(
    id,
    assignRoleRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **assignRoleRequest** | **AssignRoleRequest**| The role assignment request. | |
| **id** | [**string**] | The user identifier. | defaults to undefined|


### Return type

**ObjectApiResponse**

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
|**400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiUserIdRolesRoleNameDelete**
> ObjectApiResponse apiUserIdRolesRoleNameDelete()


### Example

```typescript
import {
    UserApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let id: string; //The user identifier. (default to undefined)
let roleName: string; //The role name to remove. (default to undefined)

const { status, data } = await apiInstance.apiUserIdRolesRoleNameDelete(
    id,
    roleName
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **id** | [**string**] | The user identifier. | defaults to undefined|
| **roleName** | [**string**] | The role name to remove. | defaults to undefined|


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

# **apiUserPost**
> UserApiResponse apiUserPost()


### Example

```typescript
import {
    UserApi,
    Configuration,
    CreateUserRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let roleName: string; //Optional role name (Admin, User, BotOperator). Default is based on IsAdmin flag. (optional) (default to undefined)
let createUserRequest: CreateUserRequest; //The create user request. (optional)

const { status, data } = await apiInstance.apiUserPost(
    roleName,
    createUserRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **createUserRequest** | **CreateUserRequest**| The create user request. | |
| **roleName** | [**string**] | Optional role name (Admin, User, BotOperator). Default is based on IsAdmin flag. | (optional) defaults to undefined|


### Return type

**UserApiResponse**

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

