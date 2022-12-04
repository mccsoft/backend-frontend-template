# Backend logging
We use a standard ASP.Net Core logging library with Serilog backend (which allows to send structured logs to ELK or Loggly).

We encourage structured logging, so that it's easier to search for the logs later.
So, if you want to log some variables, it should look like that:
```csharp
_logger.LogInformation($"Handling some special case for product {Field.Id}", product.id);
```
So, for each variable you are logging there should be a `Field.Something` placeholder within a string.
This results in nice logs texts (e.g. `Handling some special case for product Id: 5`) and also allows to search in the logs by a parameter (e.g. `Id: 5`).

If there's no `Field.Something` that suits you, feel free to create a new predefined field in [Field.partial.cs](../webapi/Lib/Logging/MccSoft.Logging/Field.partial.cs). You could also create a one-time field using `Field.Named("parameterName")`.

If you start to use `Field.Named` too often for the same parameter name, consider creating a predefined Field in [Field.partial.cs](../webapi/Lib/Logging/MccSoft.Logging/Field.partial.cs)

## `[Log]` attribute
You are encouraged to log all meaningful events. But sometimes it becomes tedious to write log messages for entering/leaving methods.
For that matter you could decorate your method with `[Log]` attribute. This will automatically add the logs for Starting and Finishing the method along with method parameters.

You could check out the sources of [LogAttribute](../webapi/Lib/Logging/MccSoft.Logging/LogAttribute.cs) to see how it's done.

You could also apply `[Log]` attribute to a class, which is identical to applying it to all methods.

### Property indexing in `[Log]` attribute
By default Log attribute outputs parameters with the same names as they are named.
Sometimes it makes sense to log them with a different names (e.g. to change the name `userProductId` to just `productId`).

To do that, you could adjust [PostProcessParameterName](../webapi/Lib/Logging/MccSoft.Logging/LogAttributePostProcess.partial.cs) function.

## Adding Properties to all logs within a method
Often it's useful that ALL logs within a method contain a reference to some parameter. For example, if something is done with a certain Product (e.g. Patch operation) it's useful that all logs contain ProductId parameter.

To do this, consider placing the following code at the start of your operation (e.g. in Controller method):
```csharp
using var logContext = LogContext.PushProperty(Field.ProductId, id);
```
It's important to use `using` to control the scope of the Property (so that it's not added anymore after exiting from the method)
