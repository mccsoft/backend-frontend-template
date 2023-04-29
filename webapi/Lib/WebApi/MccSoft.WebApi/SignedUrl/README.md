# What's that
This is an infrastructure to set up signed URLs.
Signed URL is a standard way (used by Amazon S3, Google Cloud, etc.) to secure access to files, while allowing them to be downloaded by standard browser tools.
A common example could be downloading large .zip files or images (employing browser cache).

## The problem
The problem we are trying to solve: downloading big files and secured images via standard API requests is tricky.
Normally you have to provide `Authorization` header to call any API (to download a file/image).
This forces us to download files via JS (axios/fetch), because this is the only way to provide `Authorization` header.
This causes the following problems:
1. JS memory consumption (the whole file is downloaded and then manipulated in JS memory), which is inefficient.
1. Inability to use standard browser Download dialog and features like pause/continue/restart/view the progress of a download (especially important for large files)
1. Inability to use standard `<img src='_source_' />'` tag and standard browser caching.

Signed URLs solve this by including a token into URL, Cookie, or Header:
1. Query string: `https://server/product/2/image?sign=__signature__`. Normally used for .zip files (and other files that are downloaded once and do not need browser caching)
2. Cookie. In this case URL will be just `https://server/product/2/image`, and Cookie will contain the value `sign=__signature__`. Best for images, because images are meant to be cached, but when using QueryString the signature will be changed quite often, which will prevent caching (changing Cookie won't prevent caching).
3. Header. You can pass `X-Sign` header with `__signature__` value

# How to use
1. Call `services.AddSignUrl("SOME_SECRET_VALUE_AT_LEAST_16_BYTES")` somewhere from `Startup`.
1. Decorate the actions (e.g. returning images for some of your items) you'd like to secure with `[ValidateSignedUrl]` attribute.
1. Set the `sign` cookie on the Client. You could call `SignUrlTestController.SetSignatureCookie` from the client on a regular basis (e.g. once in 10 minutes) to achieve that.
1. After that the client will be able to download images just by specifying `<img src='_url_' />`
1. You are encouraged to add your own checks regarding whether the user could access the image or not. Check `SignUrlTestController.GetProductImageWithAdvancedUserValidation` for an example.
1. By default signatures expire in 20 minutes. For images you normally should set up on a client something like `setInterval` to regularly update the cookie signature.
1. For zip archives it's preferable to generate a signature per file and put file id into the Claim.

# Additional info
- You could adjust most of the functionality by creating your own class that inherits from `SignUrlHelper` and overrides the needed methods (will be mentioned below). Don't forget to register your implementation instead of a default one!
  `services.AddSingleton<SignUrlHelper, _YOUR_IMPLEMENTATION_OF_SIGN_URL_HELPER>()`
- Signature is a JWT token, containing 'id' claim (with user id) and `exp` claim (with expiration time in seconds since unix epoch). You could add more claims by overriding `CreateClaims` method.
- Signature is JWT (and not just custom signed string) since it's a common and clear exchange format.
- Signature JWT is different from default JWT which is used as bearer token in REST API authorization, because it's signed by different key.
Why these JWT should be different? Because sign url tokens appear in URL (in case of QueryString) or Cookie, and could be logged by proxy servers, so it's not secure to store a 'normal' token there.
- Why use a cookie instead of Query String? To use browser cache when storing the images.
- It's preferable to use `imageGuid` in URL (not the `productId` or something the image refers to), because the image will be cached by the browser.
And when image for Product is updated (by admin), the URL of the image should be changed (to avoid displaying old image from cache).
