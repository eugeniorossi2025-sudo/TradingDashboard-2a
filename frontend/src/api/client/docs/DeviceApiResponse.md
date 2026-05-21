# DeviceApiResponse

Represents a standardized API response.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**success** | **boolean** | Gets or sets a value indicating whether the operation was successful. | [optional] [default to undefined]
**message** | **string** | Gets or sets the message describing the result. | [optional] [default to undefined]
**data** | [**Device**](Device.md) |  | [optional] [default to undefined]
**errors** | **Array&lt;string&gt;** | Gets or sets the list of errors, if any. | [optional] [default to undefined]
**timestamp** | **string** | Gets or sets the timestamp of the response. | [optional] [default to undefined]

## Example

```typescript
import { DeviceApiResponse } from './api';

const instance: DeviceApiResponse = {
    success,
    message,
    data,
    errors,
    timestamp,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
