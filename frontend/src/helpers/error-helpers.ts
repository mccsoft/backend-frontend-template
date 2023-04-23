export const NetworkError = 'Network Error';

/*
Converts exception object to readable string.
Handles ASP.NET Core validation errors (ProblemDetails), and other backend errors.
Returns 'Network Error' in case of network errors.
Returns 'Unauthorized' in case of 401
Returns 'Access Denied' in case of 403.
(errors are mentioned here for localization purposes)
 */
export function errorToString(
  error: any,
  options: { removePropertyNames?: boolean },
): string {
  // error could be:
  // - strongly-typed error (if server-side action is decorated with
  // [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
  // - untyped error, in which case `error.response` will be populated with response in JSON.
  const errorResponseData = error.response?.data || error.response || error;
  const errors = errorResponseData?.errors;

  let overallError = convertToErrorStringInternal(error);

  if (errors && Object.keys(errors).length) {
    let formErrorsCombined = '';
    // Some field-bound error (e.g. `user with same Name already exists in DB`)
    Object.keys(errors).forEach((key) => {
      const camelCaseName = key.charAt(0).toLowerCase() + key.slice(1);
      const errorValue = errors[key];
      const errorString = Array.isArray(errorValue)
        ? errorValue.join('; ')
        : errorValue;
      if (key.includes('.')) {
        key = key.substring(key.lastIndexOf('.') + 1);
      }
      if (options.removePropertyNames) {
        formErrorsCombined = formErrorsCombined + `${errorString}\n`;
      } else {
        formErrorsCombined =
          formErrorsCombined + `${camelCaseName}: ${errorString}\n`;
      }
    });

    if (overallError === 'One or more validation errors occurred.') {
      // it doesn't make sense to display this error
      overallError = '';
    }

    if (overallError) {
      overallError = overallError + '\n' + formErrorsCombined;
    } else {
      overallError = formErrorsCombined;
    }
  }
  return overallError;
}

export function convertToErrorStringInternal(error: any): string {
  const errorResponseData = error.response?.data || error;
  const responseDetail = errorResponseData?.detail;
  if (error.status === 401) {
    return 'Unauthorized';
  }
  if (error.status === 403) {
    return 'Access Denied';
  }
  if (responseDetail) {
    // General server-side error not related to certain field (e.g. `Access Denied`)
    return responseDetail;
  }

  if (error.code === 'CSS_CHUNK_LOAD_FAILED') {
    return NetworkError;
  }
  if (error.name === 'ChunkLoadError') {
    return NetworkError;
  }
  if (error.message?.includes('fetch dynamically imported module')) {
    return NetworkError;
  }
  if (error.message?.includes("Cannot read property 'status' of undefined")) {
    // nswag generated client throws it when there's no response
    return NetworkError;
  }
  if (error.message) {
    // e.g. Network Error
    console.log('Error:', error, JSON.stringify(error));
    return error.message;
  } else if (error.title) {
    return error.title;
  } else if (error instanceof String || typeof error === 'string') {
    return error as string;
  }
  console.log('Unknown Error:', error, JSON.stringify(error));
  return error.toString();
}

export function mapServerErrorToFormReadableResult(errorKey: string) {
  return errorKey.charAt(0).toLowerCase() + errorKey.substr(1);
}
