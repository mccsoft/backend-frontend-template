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
export function serializeProblemDetails(_data: ProblemDetails | undefined) {
  if (_data) {
    _data = prepareSerializeProblemDetails(_data as ProblemDetails);
  }
  return JSON.stringify(_data);
}
export function prepareSerializeProblemDetails(_data: ProblemDetails): ProblemDetails {
  const data: Record<string, any> = { ..._data };
  return data as ProblemDetails;
}
export interface HttpValidationProblemDetails extends ProblemDetails  {
  errors: { [key: string]: string[]; };
  [key: string]: any;
}
export function deserializeHttpValidationProblemDetails(json: string): HttpValidationProblemDetails {
  const data = JSON.parse(json) as HttpValidationProblemDetails;
  initHttpValidationProblemDetails(data);
  return data;
}
export function initHttpValidationProblemDetails(_data: HttpValidationProblemDetails) {
  initProblemDetails(_data);
    return _data;
}
export function serializeHttpValidationProblemDetails(_data: HttpValidationProblemDetails | undefined) {
  if (_data) {
    _data = prepareSerializeHttpValidationProblemDetails(_data as HttpValidationProblemDetails);
  }
  return JSON.stringify(_data);
}
export function prepareSerializeHttpValidationProblemDetails(_data: HttpValidationProblemDetails): HttpValidationProblemDetails {
  const data = prepareSerializeProblemDetails(_data as HttpValidationProblemDetails) as Record<string, any>;
  return data as HttpValidationProblemDetails;
}
/** A ProblemDetails for validation errors. */
export interface ValidationProblemDetails extends HttpValidationProblemDetails  {
  /** Gets or sets the validation errors associated with this instance of ValidationProblemDetails. */
  errors: { [key: string]: string[]; };
  [key: string]: any;
}
export function deserializeValidationProblemDetails(json: string): ValidationProblemDetails {
  const data = JSON.parse(json) as ValidationProblemDetails;
  initValidationProblemDetails(data);
  return data;
}
export function initValidationProblemDetails(_data: ValidationProblemDetails) {
  initHttpValidationProblemDetails(_data);
    return _data;
}
export function serializeValidationProblemDetails(_data: ValidationProblemDetails | undefined) {
  if (_data) {
    _data = prepareSerializeValidationProblemDetails(_data as ValidationProblemDetails);
  }
  return JSON.stringify(_data);
}
export function prepareSerializeValidationProblemDetails(_data: ValidationProblemDetails): ValidationProblemDetails {
  const data = prepareSerializeHttpValidationProblemDetails(_data as ValidationProblemDetails) as Record<string, any>;
  return data as ValidationProblemDetails;
}
/** Webhook subscription DTO */
export interface WebhookSubscriptionDto  {
  id: string;
  /** Human-readable name of integration */
  name: string;
  /** URL for integration */
  url: string;
  eventType: WebHookEventType;
  method: string | null;
  headers: { [key: string]: string; } | null;
}
export function deserializeWebhookSubscriptionDto(json: string): WebhookSubscriptionDto {
  const data = JSON.parse(json) as WebhookSubscriptionDto;
  initWebhookSubscriptionDto(data);
  return data;
}
export function initWebhookSubscriptionDto(_data: WebhookSubscriptionDto) {
  if (_data) {
    _data.eventType = _data["eventType"];
  }
  return _data;
}
export function serializeWebhookSubscriptionDto(_data: WebhookSubscriptionDto | undefined) {
  if (_data) {
    _data = prepareSerializeWebhookSubscriptionDto(_data as WebhookSubscriptionDto);
  }
  return JSON.stringify(_data);
}
export function prepareSerializeWebhookSubscriptionDto(_data: WebhookSubscriptionDto): WebhookSubscriptionDto {
  const data: Record<string, any> = { ..._data };
  return data as WebhookSubscriptionDto;
}
export enum WebHookEventType {
    NewProduct = "NewProduct",
    ProductDeleted = "ProductDeleted",
}
/** Webhook subscription DTO */
export interface CreateWebHookDto  {
  /** Human-readable name of integration */
  name: string;
  /** URL for integration */
  url: string;
  eventType: WebHookEventType;
  method: string | null;
  headers: { [key: string]: string; } | null;
}
export function deserializeCreateWebHookDto(json: string): CreateWebHookDto {
  const data = JSON.parse(json) as CreateWebHookDto;
  initCreateWebHookDto(data);
  return data;
}
export function initCreateWebHookDto(_data: CreateWebHookDto) {
  if (_data) {
    _data.eventType = _data["eventType"];
  }
  return _data;
}
export function serializeCreateWebHookDto(_data: CreateWebHookDto | undefined) {
  if (_data) {
    _data = prepareSerializeCreateWebHookDto(_data as CreateWebHookDto);
  }
  return JSON.stringify(_data);
}
export function prepareSerializeCreateWebHookDto(_data: CreateWebHookDto): CreateWebHookDto {
  const data: Record<string, any> = { ..._data };
  return data as CreateWebHookDto;
}
/** Webhook subscription DTO */
export interface UpdateWebHookSubscriptionDto  {
  /** Webhook name (Human readable name of integration). */
  name?: string;
  /** Webhook URL for integration. */
  url?: string;
  /** HTTP method for accessing URL via specified http method. */
  method?: string;
  eventType?: WebHookEventType;
  headers?: { [key: string]: string; };
}
export function deserializeUpdateWebHookSubscriptionDto(json: string): UpdateWebHookSubscriptionDto {
  const data = JSON.parse(json) as UpdateWebHookSubscriptionDto;
  initUpdateWebHookSubscriptionDto(data);
  return data;
}
export function initUpdateWebHookSubscriptionDto(_data: UpdateWebHookSubscriptionDto) {
  if (_data) {
    _data.eventType = _data["eventType"];
  }
  return _data;
}
export function serializeUpdateWebHookSubscriptionDto(_data: UpdateWebHookSubscriptionDto | undefined) {
  if (_data) {
    _data = prepareSerializeUpdateWebHookSubscriptionDto(_data as UpdateWebHookSubscriptionDto);
  }
  return JSON.stringify(_data);
}
export function prepareSerializeUpdateWebHookSubscriptionDto(_data: UpdateWebHookSubscriptionDto): UpdateWebHookSubscriptionDto {
  const data: Record<string, any> = { ..._data };
  return data as UpdateWebHookSubscriptionDto;
}
export interface CurrentUserDto  {
  id: string;
  username: string;
  nickname: string;
  permissions: string[];
}
export function deserializeCurrentUserDto(json: string): CurrentUserDto {
  const data = JSON.parse(json) as CurrentUserDto;
  initCurrentUserDto(data);
  return data;
}
export function initCurrentUserDto(_data: CurrentUserDto) {
  if (_data) {
    _data.permissions = _data["permissions"];
  }
  return _data;
}
export function serializeCurrentUserDto(_data: CurrentUserDto | undefined) {
  if (_data) {
    _data = prepareSerializeCurrentUserDto(_data as CurrentUserDto);
  }
  return JSON.stringify(_data);
}
export function prepareSerializeCurrentUserDto(_data: CurrentUserDto): CurrentUserDto {
  const data: Record<string, any> = { ..._data };
  return data as CurrentUserDto;
}
export interface ResetPasswordDto  {
  username: string;
  token: string;
  newPassword: string;
}
export function deserializeResetPasswordDto(json: string): ResetPasswordDto {
  const data = JSON.parse(json) as ResetPasswordDto;
  initResetPasswordDto(data);
  return data;
}
export function initResetPasswordDto(_data: ResetPasswordDto) {
    return _data;
}
export function serializeResetPasswordDto(_data: ResetPasswordDto | undefined) {
  if (_data) {
    _data = prepareSerializeResetPasswordDto(_data as ResetPasswordDto);
  }
  return JSON.stringify(_data);
}
export function prepareSerializeResetPasswordDto(_data: ResetPasswordDto): ResetPasswordDto {
  const data: Record<string, any> = { ..._data };
  return data as ResetPasswordDto;
}
export interface ChangePasswordDto  {
  oldPassword: string;
  newPassword: string;
}
export function deserializeChangePasswordDto(json: string): ChangePasswordDto {
  const data = JSON.parse(json) as ChangePasswordDto;
  initChangePasswordDto(data);
  return data;
}
export function initChangePasswordDto(_data: ChangePasswordDto) {
    return _data;
}
export function serializeChangePasswordDto(_data: ChangePasswordDto | undefined) {
  if (_data) {
    _data = prepareSerializeChangePasswordDto(_data as ChangePasswordDto);
  }
  return JSON.stringify(_data);
}
export function prepareSerializeChangePasswordDto(_data: ChangePasswordDto): ChangePasswordDto {
  const data: Record<string, any> = { ..._data };
  return data as ChangePasswordDto;
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
export function serializeCreateTestTenantDto(_data: CreateTestTenantDto | undefined) {
  if (_data) {
    _data = prepareSerializeCreateTestTenantDto(_data as CreateTestTenantDto);
  }
  return JSON.stringify(_data);
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
export function serializeProductDto(_data: ProductDto | undefined) {
  if (_data) {
    _data = prepareSerializeProductDto(_data as ProductDto);
  }
  return JSON.stringify(_data);
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
export function serializeCreateProductDto(_data: CreateProductDto | undefined) {
  if (_data) {
    _data = prepareSerializeCreateProductDto(_data as CreateProductDto);
  }
  return JSON.stringify(_data);
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
export function serializePatchProductDto(_data: PatchProductDto | undefined) {
  if (_data) {
    _data = prepareSerializePatchProductDto(_data as PatchProductDto);
  }
  return JSON.stringify(_data);
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
export function serializePagedResultOfProductListItemDto(_data: PagedResultOfProductListItemDto | undefined) {
  if (_data) {
    _data = prepareSerializePagedResultOfProductListItemDto(_data as PagedResultOfProductListItemDto);
  }
  return JSON.stringify(_data);
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
export function serializeProductListItemDto(_data: ProductListItemDto | undefined) {
  if (_data) {
    _data = prepareSerializeProductListItemDto(_data as ProductListItemDto);
  }
  return JSON.stringify(_data);
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
export interface FileInfoDto  {
  id: string;
  fileName: string;
  size: number;
}
export function deserializeFileInfoDto(json: string): FileInfoDto {
  const data = JSON.parse(json) as FileInfoDto;
  initFileInfoDto(data);
  return data;
}
export function initFileInfoDto(_data: FileInfoDto) {
    return _data;
}
export function serializeFileInfoDto(_data: FileInfoDto | undefined) {
  if (_data) {
    _data = prepareSerializeFileInfoDto(_data as FileInfoDto);
  }
  return JSON.stringify(_data);
}
export function prepareSerializeFileInfoDto(_data: FileInfoDto): FileInfoDto {
  const data: Record<string, any> = { ..._data };
  return data as FileInfoDto;
}
export function formatDate(d: Date) {
    return d.getFullYear() + '-' + 
        (d.getMonth() < 9 ? ('0' + (d.getMonth()+1)) : (d.getMonth()+1)) + '-' +
        (d.getDate() < 10 ? ('0' + d.getDate()) : d.getDate());
}
export function parseDateOnly(s: string) {
    const date = new Date(s);
    return new Date(date.getTime() + 
        date.getTimezoneOffset() * 60000);
}
import type { AxiosError } from 'axios'
export interface FileParameter {
    data: any;
    fileName: string;
}
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