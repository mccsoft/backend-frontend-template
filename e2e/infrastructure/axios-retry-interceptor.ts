import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';

const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));
const maxRetry = 20;
export function addRetryInterceptor(axiosInstance: AxiosInstance) {
  axiosInstance.interceptors.response.use(undefined, async (error) => {
    if (error.config && error.response && error.response.status === 504) {
      await sleep(1000);
      const retriesLeft = error.config.maxRetry ?? maxRetry;
      if (retriesLeft) {
        error.config.maxRetry = retriesLeft - 1;
        return await axios.request(error.config);
      }
    }

    return await Promise.reject(error);
  });
}
