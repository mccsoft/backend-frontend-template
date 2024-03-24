import { AxiosResponse, InternalAxiosRequestConfig } from 'axios';

/**
 * Intercepts error responses for blob requests (e.g. file downloading) and parses the response
 * since it's probably JSON which isn't handled if `request.responseType === 'blob'`
 * See for details: https://stackoverflow.com/questions/56286368/how-can-i-read-http-errors-when-responsetype-is-blob-in-axios-with-vuejs
 * Fixes: #48233: No error message when document is not found
 */
export const blobResponseErrorInterceptor = async (error: {
  response: AxiosResponse<any>;
  request: InternalAxiosRequestConfig;
}) => {
  const response = error.response;
  const request = error.request;
  const data = response.data;

  if (request.responseType === 'blob' && data instanceof Blob && data.type) {
    const text = await data.text();

    response.data = data.type.toLowerCase().includes('json')
      ? JSON.parse(text)
      : text;
  }

  throw error;
};
