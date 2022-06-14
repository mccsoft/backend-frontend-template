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

## HTTP PATCH implementation
The idea of http PATCH method, is that it changes the object (just like a PUT method), but only changes the values that are actually passed, without setting all others to `null`.

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
then only `lastStockUpdatedAt` field will actually be changed ([by definition](!!!TODO: ADD LINK HERE!!!), the `PUT` method should set `title` and `productType` to `null` in this case).

// ToDo: copy blog post about HTTP PATCH here
