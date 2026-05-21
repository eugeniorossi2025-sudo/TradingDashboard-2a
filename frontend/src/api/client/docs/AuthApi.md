# AuthApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiAuthLoginPost**](#apiauthloginpost) | **POST** /api/Auth/login | Authenticates a user and returns a JWT token.|
|[**apiAuthLogoutPost**](#apiauthlogoutpost) | **POST** /api/Auth/logout | Logs out the current authenticated user.|
|[**apiAuthResetPasswordConfirmPost**](#apiauthresetpasswordconfirmpost) | **POST** /api/Auth/reset-password-confirm | Step 2: Confirms the password reset with the token and sets a new password.|
|[**apiAuthResetPasswordRequestPost**](#apiauthresetpasswordrequestpost) | **POST** /api/Auth/reset-password-request | Step 1: Generates a password reset token for the specified email.  Sends an email with the token (in production).|
|[**apiAuthTestGet**](#apiauthtestget) | **GET** /api/Auth/test | Test endpoint to verify API is running (no authentication required).|

# **apiAuthLoginPost**
> LoginResponse apiAuthLoginPost()


### Example

```typescript
import {
    AuthApi,
    Configuration,
    LoginRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new AuthApi(configuration);

let loginRequest: LoginRequest; //The login request containing username and password. (optional)

const { status, data } = await apiInstance.apiAuthLoginPost(
    loginRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **loginRequest** | **LoginRequest**| The login request containing username and password. | |


### Return type

**LoginResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |
|**401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiAuthLogoutPost**
> apiAuthLogoutPost()


### Example

```typescript
import {
    AuthApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new AuthApi(configuration);

const { status, data } = await apiInstance.apiAuthLogoutPost();
```

### Parameters
This endpoint does not have any parameters.


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
|**200** | OK |  -  |
|**401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiAuthResetPasswordConfirmPost**
> ObjectApiResponse apiAuthResetPasswordConfirmPost()


### Example

```typescript
import {
    AuthApi,
    Configuration,
    ResetPasswordConfirmRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new AuthApi(configuration);

let resetPasswordConfirmRequest: ResetPasswordConfirmRequest; //The reset password confirmation request containing email, token, and new password. (optional)

const { status, data } = await apiInstance.apiAuthResetPasswordConfirmPost(
    resetPasswordConfirmRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **resetPasswordConfirmRequest** | **ResetPasswordConfirmRequest**| The reset password confirmation request containing email, token, and new password. | |


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
|**400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiAuthResetPasswordRequestPost**
> ObjectApiResponse apiAuthResetPasswordRequestPost()


### Example

```typescript
import {
    AuthApi,
    Configuration,
    ResetPasswordRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new AuthApi(configuration);

let resetPasswordRequest: ResetPasswordRequest; //The reset password request containing the email address. (optional)

const { status, data } = await apiInstance.apiAuthResetPasswordRequestPost(
    resetPasswordRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **resetPasswordRequest** | **ResetPasswordRequest**| The reset password request containing the email address. | |


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

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiAuthTestGet**
> ObjectApiResponse apiAuthTestGet()


### Example

```typescript
import {
    AuthApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new AuthApi(configuration);

const { status, data } = await apiInstance.apiAuthTestGet();
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

