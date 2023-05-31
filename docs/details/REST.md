# Designing REST API

We use 4 standard HTTP methods, and usually each REST controller has at least the following actions:

1. `GET /products?title=bread`
   1. Returns `List<ProductListDto>`, `ProductListDto` usually contains only a few fields (that are required to show the search results)
   2. This method usually accepts some search parameters (e.g. `title`)
   3. This method usually also accepts some sorting/paging parameters (e.g. inherits from `PagedRequestDto`)
2. `GET /products/1` - returns `ProductDto` for Product with given `Id` (1). `ProductDetailsDto` usually contains more fields than `ProductListDto`.
3. `POST /products` - accepts a `CreateProductDto` in request body. Creates a new product and returns a `ProductDto`.
4. `PATCH /products/1` - patches a `Product` with given id, read [details below](#HTTP PATCH implementation).
5. `DELETE /products/1` - deletes the product with given id

# Non-REST API

While we generally prefer the REST, sometimes it makes sense to refrain from it for certain workflows. For example, you might want to implement a `POST /products/1/archive` method to archive a certain product (i.e. make it non-active). While it's possible to implement it via REST (e.g. by adding `isArchived` property to GET and PATCH methods), separate method might make more sense.

You could use the rule of thumb: if it's likely that the method might need a different Permissions (role/claim/access right), then it makes sense to implement it in a separate method (e.g. archiving a product might only be available for Admin user).

# HTTP PATCH implementation

The idea of http [PATCH](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/PATCH) method, is that it changes the object (just like a PUT method), but only changes the values that are actually passed, without setting all others to `null`.

For example, if we have a `Product` with the following values:

```json
{
  "title": "Bread",
  "productType": 1,
  "lastStockUpatedAt": "2022-01-01"
}
```

and we send the following PATCH request:

```json
{
  "lastStockUpdatedAt": "2022-02-01"
}
```

then only `lastStockUpdatedAt` field will actually be changed ([by definition](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/PUT), the `PUT` method completely replace the resource, i.e. it should set `title` and `productType` to `null` in this case).

The behaviour of PATCH method is actually better, agile and backwards compatible. That is, if frontend wants to change a single field only, then the most intuitive way is to issue a PATCH request passing a single field only.

## How to work with HTTP PATCH

The easiest thing is to actually check the sources of [PatchProductDto](../webapi/src/MccSoft.TemplateApp.App/Features/Products/Dto/PatchProductDto.cs) and [Patch method of ProductService](../webapi/src/MccSoft.TemplateApp.App/Features/Products/Dto/PatchProductDto.cs).

In short, you have to inherit your `PatchDto` from `PatchRequest<T>`. Then you define the properties that you would like to be patched via this method (you could define a subset of Entity properties, for example, you probably wouldn't want to change the `Password` field of a `User` via PATCH method).

```csharp
public class PatchProductDto : PatchRequest<Product>
{
    [MinLength(3)]
    public string Title { get; set; }
    public ProductType ProductType { get; set; }

    public DateOnly LastStockUpdatedAt { get; set; }
}
```

After that you can call `product.Update(productPatchDto)`, and it will change those properties of `product` that were passed in `productPatchDto`.

For more details you could check an implementation of `Update` extension method in [PartialUpdateHelper](../webapi/Lib/WebApi/MccSoft.WebApi/Patching/PartialUpdateHelper.cs).

### Rationale

The base PatchRequest class is needed for 2 things:

1. To distinguish when property value is intentionally set to `null` (e.g. `{ "title": null }`) and when property value is not set (e.g. `{}`).
   1. By default in both cases the value of `Title` property will be `null`.
   2. But in case of `{ "title": null }` we want to change the value of `title` to `null`, in second case, we don't want to change it at all.
   3. So, we customize the JSON deserialization procedure, and `PatchRequest` has a special method `IsFieldPresent` that returns `true` if the field was present in a request (first case), and `false` if it wasn't (second case).
2. There's a test that verifies, that PatchDtos only contain the properties that exist in the Entity. That's helpful to avoid issues when you rename entity properties.
   1. To add an exception for certain Dtos/fields just modify the test `PatchRequest_AllFieldsMatch` in [BasicApiTests.cs](../webapi/tests/MccSoft.TemplateApp.ComponentTests/BasicApiTests.cs).

P.S. There's also a [blog post](https://www.arturdr.ru/net/realizacziya-http-patch-v-asp-net-core-3/) which talks about the same thing in russian.
