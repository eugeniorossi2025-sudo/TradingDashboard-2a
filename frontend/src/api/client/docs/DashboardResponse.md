# DashboardResponse

Represents a complete dashboard response with tables, chart data, and statistics.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**tables** | [**Array&lt;DashboardTableRow&gt;**](DashboardTableRow.md) |  | [default to undefined]
**chartData** | [**Array&lt;ChartDataPoint&gt;**](ChartDataPoint.md) |  | [default to undefined]
**statistics** | [**DashboardStatistics**](DashboardStatistics.md) |  | [default to undefined]

## Example

```typescript
import { DashboardResponse } from './api';

const instance: DashboardResponse = {
    tables,
    chartData,
    statistics,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
