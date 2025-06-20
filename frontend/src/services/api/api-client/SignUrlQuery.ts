//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.20.0.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

/* tslint:disable */
/* eslint-disable */
// ReSharper disable InconsistentNaming
import * as Types from '../api-client.types';
import { useQuery, useMutation } from '@tanstack/react-query';
import type { UseQueryResult, QueryFunctionContext, UseQueryOptions, QueryClient, QueryKey, MutationKey, UseMutationOptions, UseMutationResult, QueryMeta, MutationMeta } from '@tanstack/react-query';
import { trimArrayEnd, isParameterObject, getBaseUrl, addMetaToOptions } from './helpers';
import type { QueryMetaContextValue } from 'react-query-swagger';
import { QueryMetaContext } from 'react-query-swagger';
import { useContext } from 'react';
import * as Client from './SignUrlClient'
export { Client };
import type { AxiosRequestConfig } from 'axios';


export function getSignatureUrl(): string {
  let url_ = getBaseUrl() + "/api/sign-url/signature";
  url_ = url_.replace(/[?&]$/, "");
  return url_;
}

let getSignatureDefaultOptions: Omit<UseQueryOptions<string, unknown, string>, 'queryKey'> = {
  queryFn: __getSignature,
};
export function getGetSignatureDefaultOptions() {
  return getSignatureDefaultOptions;
};
export function setGetSignatureDefaultOptions(options: typeof getSignatureDefaultOptions) {
  getSignatureDefaultOptions = options;
}

export function getSignatureQueryKey(): QueryKey;
export function getSignatureQueryKey(...params: any[]): QueryKey {
  return trimArrayEnd([
      'SignUrlClient',
      'getSignature',
    ]);
}
function __getSignature() {
  return Client.getSignature(
    );
}

export function useGetSignatureQuery<TSelectData = string, TError = unknown>(options?: Omit<UseQueryOptions<string, TError, TSelectData>, 'queryKey'>, axiosConfig?: Partial<AxiosRequestConfig>): UseQueryResult<TSelectData, TError>;
export function useGetSignatureQuery<TSelectData = string, TError = unknown>(...params: any []): UseQueryResult<TSelectData, TError> {
  let options: UseQueryOptions<string, TError, TSelectData> | undefined = undefined;
  let axiosConfig: AxiosRequestConfig |undefined;
  

  options = params[0] as any;
  axiosConfig = params[1] as any;

  const metaContext = useContext(QueryMetaContext);
  options = addMetaToOptions(options, metaContext);
  if (axiosConfig) {
    options = options ?? { } as any;
    options!.meta = { ...options!.meta, axiosConfig };
  }

  return useQuery<string, TError, TSelectData>({
    queryFn: __getSignature,
    queryKey: getSignatureQueryKey(),
    ...getSignatureDefaultOptions as unknown as Omit<UseQueryOptions<string, TError, TSelectData>, 'queryKey'>,
    ...options,
  });
}

export function setGetSignatureData(queryClient: QueryClient, updater: (data: string | undefined) => string, ) {
  queryClient.setQueryData(getSignatureQueryKey(),
    updater
  );
}

export function setGetSignatureDataByQueryId(queryClient: QueryClient, queryKey: QueryKey, updater: (data: string | undefined) => string) {
  queryClient.setQueryData(queryKey, updater);
}
    
export function setSignatureCookieUrl(): string {
  let url_ = getBaseUrl() + "/api/sign-url/signature/cookie";
  url_ = url_.replace(/[?&]$/, "");
  return url_;
}

let setSignatureCookieDefaultOptions: Omit<UseQueryOptions<void, unknown, void>, 'queryKey'> = {
  queryFn: __setSignatureCookie,
};
export function getSetSignatureCookieDefaultOptions() {
  return setSignatureCookieDefaultOptions;
};
export function setSetSignatureCookieDefaultOptions(options: typeof setSignatureCookieDefaultOptions) {
  setSignatureCookieDefaultOptions = options;
}

export function setSignatureCookieQueryKey(): QueryKey;
export function setSignatureCookieQueryKey(...params: any[]): QueryKey {
  return trimArrayEnd([
      'SignUrlClient',
      'setSignatureCookie',
    ]);
}
function __setSignatureCookie() {
  return Client.setSignatureCookie(
    );
}

export function useSetSignatureCookieQuery<TSelectData = void, TError = unknown>(options?: Omit<UseQueryOptions<void, TError, TSelectData>, 'queryKey'>, axiosConfig?: Partial<AxiosRequestConfig>): UseQueryResult<TSelectData, TError>;
export function useSetSignatureCookieQuery<TSelectData = void, TError = unknown>(...params: any []): UseQueryResult<TSelectData, TError> {
  let options: UseQueryOptions<void, TError, TSelectData> | undefined = undefined;
  let axiosConfig: AxiosRequestConfig |undefined;
  

  options = params[0] as any;
  axiosConfig = params[1] as any;

  const metaContext = useContext(QueryMetaContext);
  options = addMetaToOptions(options, metaContext);
  if (axiosConfig) {
    options = options ?? { } as any;
    options!.meta = { ...options!.meta, axiosConfig };
  }

  return useQuery<void, TError, TSelectData>({
    queryFn: __setSignatureCookie,
    queryKey: setSignatureCookieQueryKey(),
    ...setSignatureCookieDefaultOptions as unknown as Omit<UseQueryOptions<void, TError, TSelectData>, 'queryKey'>,
    ...options,
  });
}

export function setSetSignatureCookieData(queryClient: QueryClient, updater: (data: void | undefined) => void, ) {
  queryClient.setQueryData(setSignatureCookieQueryKey(),
    updater
  );
}

export function setSetSignatureCookieDataByQueryId(queryClient: QueryClient, queryKey: QueryKey, updater: (data: void | undefined) => void) {
  queryClient.setQueryData(queryKey, updater);
}