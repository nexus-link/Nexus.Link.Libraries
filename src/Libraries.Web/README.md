# Nexus.Link.Libraries.Web

## Error

The `[RestClient](#RestClient)` evaluates the response. If the [HTTP status code](https://en.wikipedia.org/wiki/List_of_HTTP_status_codes) is >= 300, then the response will be converted into a FulcrumError and an appropriate exception will be thrown. This chapter describes how the status codes are mapped into exceptions.

### Maybe create a FulcrumError

If the response body is not formatted as a FulcrumError, then we need to create a FulcrumError based on the status code of the response. The following table describes the rules for that:

| Status code | Description | FulcrumError |
|---|---|---|
| 300 | catch all | Xlent.Fulcrum.ServiceContract |
| 400 | Bad Request | Xlent.Fulcrum.ServiceContract |
| 401 | Unauthorized | Xlent.Fulcrum.Unauthorized |
| 402 | Payment Required | Xlent.Fulcrum.ServiceContract |
| 403 | Forbidden | Xlent.Fulcrum.ForbiddenAccess |
| 404 | Not Found | Xlent.Fulcrum.ServiceContract |
| 405 | Method Not Allowed | Xlent.Fulcrum.ServiceContract |
| 406 | Not Acceptable | Xlent.Fulcrum.ServiceContract |
| 407 | Proxy Authentication Failed | Xlent.Fulcrum.Unauthorized |
| 408 | Request TimeOut | Xlent.Fulcrum.TryAgain |
| 409 | Conflict | Xlent.Fulcrum.Conflict |
| 410 | Gone | Xlent.Fulcrum.NotFound |
| 4xx | catch all | Xlent.Fulcrum.ServiceContract |
| 500 | Internal Server Error | Xlent.Fulcrum.AssertionFailed |
| 501 | Not Implemented | Xlent.Fulcrum.NotImplemented |
| 502 | Bad gateway | Xlent.Fulcrum.Resource |
| 503 | Service Unavailable | Xlent.Fulcrum.TryAgain |
| 504 | Gateway Timeout | Xlent.Fulcrum.TryAgain |
| 505 | HTTP Version Not Supported | Xlent.Fulcrum.NotImplemented |
| 5** | catch all | Xlent.Fulcrum.AssertionFailed |

### Map the FulcrumError to a FulcrumException

The FulcrumError from the response will be mapped to the appropriate FulcrumException, according to the following table

| FulcrumError | FulcrumException | Description |
|---|---|---|
| Xlent.Fulcrum.AssertionFailed | FulcrumResourceException | The resource had an internal error. |
| Xlent.Fulcrum.BusinessRule | FulcrumBusinessRuleException | A business rule error is most probably aimed at the original caller. |
| Xlent.Fulcrum.Conflict | FulcrumConflictException | The conflict most probably origins from the original caller. |
| Xlent.Fulcrum.Contract | FulcrumResourceException | The resource had an internal error. |
| Xlent.Fulcrum.ForbiddenAccess | FulcrumForbiddenAccessException | The provided credentials were insufficient. |
| Xlent.Fulcrum.NotFound | FulcrumNotFoundException | This is probably relevant to the original caller. |
| Xlent.Fulcrum.NotImplemented | FulcrumResourceException | The resource had an internal error. |
| Xlent.Fulcrum.Resource | FulcrumResourceException | The resource had an error in another resource. |
| Xlent.Fulcrum.ServiceContract | FulcrumContractException | The service said that the call did not follow the contract. We see this as an internal error for the code that called the RestClient. |
| Xlent.Fulcrum.TryAgain | FulcrumTryAgainException | The original caller should take the decision on if it should try again or not. |
| Xlent.Fulcrum.Unauthorized | FulcrumUnauthorizedException | The provided credentials could not be accepted. |

