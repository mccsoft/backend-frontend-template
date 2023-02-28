//-----Types.File-----
export interface ProblemDetails  {
  type?: string | null;
  title?: string | null;
  status?: number | null;
  detail?: string | null;
  instance?: string | null;
  [key: string]: any;
}
export function deserializeProblemDetails(json: string): ProblemDetails {
  const data = JSON.parse(json) as ProblemDetails;
  initProblemDetails(data);
  return data;
}
export function initProblemDetails(_data: ProblemDetails) {
    return _data;
}
export function serializeProblemDetails(_data: ProblemDetails) {
  const data = prepareSerializeProblemDetails(_data as ProblemDetails);
  return JSON.stringify(data);
}
export function prepareSerializeProblemDetails(_data: ProblemDetails): ProblemDetails {
  const data: Record<string, any> = { ..._data };
  return data as ProblemDetails;
}
export interface HttpValidationProblemDetails extends ProblemDetails  {
  errors?: { [key: string]: string[]; };
  [key: string]: any;
}
export function deserializeHttpValidationProblemDetails(json: string): HttpValidationProblemDetails {
  const data = JSON.parse(json) as HttpValidationProblemDetails;
  initHttpValidationProblemDetails(data);
  return data;
}
export function initHttpValidationProblemDetails(_data: HttpValidationProblemDetails) {
  initProblemDetails(_data);
  if (_data) {
    _data.errors = _data["errors"];
  }
  return _data;
}
export function serializeHttpValidationProblemDetails(_data: HttpValidationProblemDetails) {
  const data = prepareSerializeHttpValidationProblemDetails(_data as HttpValidationProblemDetails);
  return JSON.stringify(data);
}
export function prepareSerializeHttpValidationProblemDetails(_data: HttpValidationProblemDetails): HttpValidationProblemDetails {
  const data = prepareSerializeProblemDetails(_data as HttpValidationProblemDetails) as Record<string, any>;
  return data as HttpValidationProblemDetails;
}
export interface ValidationProblemDetails extends HttpValidationProblemDetails  {
  errors?: { [key: string]: string[]; };
  [key: string]: any;
}
export function deserializeValidationProblemDetails(json: string): ValidationProblemDetails {
  const data = JSON.parse(json) as ValidationProblemDetails;
  initValidationProblemDetails(data);
  return data;
}
export function initValidationProblemDetails(_data: ValidationProblemDetails) {
  initHttpValidationProblemDetails(_data);
  if (_data) {
    _data.errors = _data["errors"];
  }
  return _data;
}
export function serializeValidationProblemDetails(_data: ValidationProblemDetails) {
  const data = prepareSerializeValidationProblemDetails(_data as ValidationProblemDetails);
  return JSON.stringify(data);
}
export function prepareSerializeValidationProblemDetails(_data: ValidationProblemDetails): ValidationProblemDetails {
  const data = prepareSerializeHttpValidationProblemDetails(_data as ValidationProblemDetails) as Record<string, any>;
  return data as ValidationProblemDetails;
}
export interface CreateTestTenantDto  {
  userEmail: string;
  userPassword: string;
}
export function deserializeCreateTestTenantDto(json: string): CreateTestTenantDto {
  const data = JSON.parse(json) as CreateTestTenantDto;
  initCreateTestTenantDto(data);
  return data;
}
export function initCreateTestTenantDto(_data: CreateTestTenantDto) {
    return _data;
}
export function serializeCreateTestTenantDto(_data: CreateTestTenantDto) {
  const data = prepareSerializeCreateTestTenantDto(_data as CreateTestTenantDto);
  return JSON.stringify(data);
}
export function prepareSerializeCreateTestTenantDto(_data: CreateTestTenantDto): CreateTestTenantDto {
  const data: Record<string, any> = { ..._data };
  return data as CreateTestTenantDto;
}
export interface ProductDto  {
  id: number;
  title: string;
  productType: ProductType;
  lastStockUpdatedAt: Date;
}
export function deserializeProductDto(json: string): ProductDto {
  const data = JSON.parse(json) as ProductDto;
  initProductDto(data);
  return data;
}
export function initProductDto(_data: ProductDto) {
  if (_data) {
    _data.productType = _data["productType"];
    _data.lastStockUpdatedAt = _data["lastStockUpdatedAt"] ? parseDateOnly(_data["lastStockUpdatedAt"].toString()) : <any>null;
  }
  return _data;
}
export function serializeProductDto(_data: ProductDto) {
  const data = prepareSerializeProductDto(_data as ProductDto);
  return JSON.stringify(data);
}
export function prepareSerializeProductDto(_data: ProductDto): ProductDto {
  const data: Record<string, any> = { ..._data };
  data["lastStockUpdatedAt"] = _data.lastStockUpdatedAt && formatDate(_data.lastStockUpdatedAt);
  return data as ProductDto;
}
export enum ProductType {
    Undefined = "Undefined",
    Auto = "Auto",
    Electronic = "Electronic",
    Other = "Other",
}
export interface CreateProductDto  {
  title: string;
  productType: ProductType;
  lastStockUpdatedAt: Date;
}
export function deserializeCreateProductDto(json: string): CreateProductDto {
  const data = JSON.parse(json) as CreateProductDto;
  initCreateProductDto(data);
  return data;
}
export function initCreateProductDto(_data: CreateProductDto) {
  if (_data) {
    _data.productType = _data["productType"];
    _data.lastStockUpdatedAt = _data["lastStockUpdatedAt"] ? parseDateOnly(_data["lastStockUpdatedAt"].toString()) : <any>null;
  }
  return _data;
}
export function serializeCreateProductDto(_data: CreateProductDto) {
  const data = prepareSerializeCreateProductDto(_data as CreateProductDto);
  return JSON.stringify(data);
}
export function prepareSerializeCreateProductDto(_data: CreateProductDto): CreateProductDto {
  const data: Record<string, any> = { ..._data };
  data["lastStockUpdatedAt"] = _data.lastStockUpdatedAt && formatDate(_data.lastStockUpdatedAt);
  return data as CreateProductDto;
}
export interface PatchProductDto  {
  title?: string;
  productType?: ProductType;
  lastStockUpdatedAt?: Date;
}
export function deserializePatchProductDto(json: string): PatchProductDto {
  const data = JSON.parse(json) as PatchProductDto;
  initPatchProductDto(data);
  return data;
}
export function initPatchProductDto(_data: PatchProductDto) {
  if (_data) {
    _data.productType = _data["productType"];
    _data.lastStockUpdatedAt = _data["lastStockUpdatedAt"] ? parseDateOnly(_data["lastStockUpdatedAt"].toString()) : <any>null;
  }
  return _data;
}
export function serializePatchProductDto(_data: PatchProductDto) {
  const data = prepareSerializePatchProductDto(_data as PatchProductDto);
  return JSON.stringify(data);
}
export function prepareSerializePatchProductDto(_data: PatchProductDto): PatchProductDto {
  const data: Record<string, any> = { ..._data };
  data["lastStockUpdatedAt"] = _data.lastStockUpdatedAt && formatDate(_data.lastStockUpdatedAt);
  return data as PatchProductDto;
}
export interface PagedResultOfProductListItemDto  {
  data: ProductListItemDto[];
  totalCount: number;
}
export function deserializePagedResultOfProductListItemDto(json: string): PagedResultOfProductListItemDto {
  const data = JSON.parse(json) as PagedResultOfProductListItemDto;
  initPagedResultOfProductListItemDto(data);
  return data;
}
export function initPagedResultOfProductListItemDto(_data: PagedResultOfProductListItemDto) {
  if (_data) {
    if (Array.isArray(_data["data"])) {
      _data.data = _data["data"].map(item => 
        initProductListItemDto(item)
      );
    }
  }
  return _data;
}
export function serializePagedResultOfProductListItemDto(_data: PagedResultOfProductListItemDto) {
  const data = prepareSerializePagedResultOfProductListItemDto(_data as PagedResultOfProductListItemDto);
  return JSON.stringify(data);
}
export function prepareSerializePagedResultOfProductListItemDto(_data: PagedResultOfProductListItemDto): PagedResultOfProductListItemDto {
  const data: Record<string, any> = { ..._data };
  if (Array.isArray(_data.data)) {
    data["data"] = _data.data.map(item => 
        prepareSerializeProductListItemDto(item)
    );
  }
  return data as PagedResultOfProductListItemDto;
}
export interface ProductListItemDto  {
  id: number;
  title: string;
  productType: ProductType;
  lastStockUpdatedAt: Date;
}
export function deserializeProductListItemDto(json: string): ProductListItemDto {
  const data = JSON.parse(json) as ProductListItemDto;
  initProductListItemDto(data);
  return data;
}
export function initProductListItemDto(_data: ProductListItemDto) {
  if (_data) {
    _data.productType = _data["productType"];
    _data.lastStockUpdatedAt = _data["lastStockUpdatedAt"] ? parseDateOnly(_data["lastStockUpdatedAt"].toString()) : <any>null;
  }
  return _data;
}
export function serializeProductListItemDto(_data: ProductListItemDto) {
  const data = prepareSerializeProductListItemDto(_data as ProductListItemDto);
  return JSON.stringify(data);
}
export function prepareSerializeProductListItemDto(_data: ProductListItemDto): ProductListItemDto {
  const data: Record<string, any> = { ..._data };
  data["lastStockUpdatedAt"] = _data.lastStockUpdatedAt && formatDate(_data.lastStockUpdatedAt);
  return data as ProductListItemDto;
}
export enum SortOrder {
    Asc = "Asc",
    Desc = "Desc",
}
export function formatDate(d: Date) {
    return d.getFullYear() + '-' + 
        (d.getMonth() < 9 ? ('0' + (d.getMonth()+1)) : (d.getMonth()+1)) + '-' +
        (d.getDate() < 10 ? ('0' + d.getDate()) : d.getDate());
}
function parseDateOnly(s: string) {
    const date = new Date(s);
    return new Date(date.getTime() + 
        date.getTimezoneOffset() * 60000);
}
import type { AxiosError } from 'axios'
export class ApiException extends Error {
    message: string;
    status: number;
    response: string;
    headers: { [key: string]: any; };
    result: any;
    constructor(message: string, status: number, response: string, headers: { [key: string]: any; }, result: any) {
        super();
        this.message = message;
        this.status = status;
        this.response = response;
        this.headers = headers;
        this.result = result;
    }
    protected isApiException = true;
    static isApiException(obj: any): obj is ApiException {
        return obj.isApiException === true;
    }
}
export function throwException(message: string, status: number, response: string, headers: { [key: string]: any; }, result?: any): any {
    if (result !== null && result !== undefined)
        throw result;
    else
        throw new ApiException(message, status, response, headers, null);
}
export function isAxiosError(obj: any | undefined): obj is AxiosError {
    return obj && obj.isAxiosError === true;
}
//-----/Types.File-----