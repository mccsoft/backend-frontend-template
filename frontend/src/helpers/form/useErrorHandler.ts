import { History } from 'history';
import { useState } from 'react';
import { FieldValues } from 'react-hook-form';
import {
  UnpackNestedValue,
  UseFormSetError,
} from 'react-hook-form/dist/types/form';
import { DeepPartial } from 'react-hook-form/dist/types/utils';
import { useHistory } from 'react-router';

export type UseSendFormReturn<T> = {
  /*
  Function to be passed to form.handleSubmit
   */
  handler: (data: UnpackNestedValue<T>) => Promise<void>;
  /*
  Server-side error which doesn't belong to any particular field
   */
  overallServerError: string;
  /*
  All server-side errors combined
   */
  serverErrorsCombined: string;
};

export function handleSubmitFormError<T>(
  error: any,
  setError: UseFormSetError<T>,
): {
  // error not related to any field
  overallServerError: string;
  // field-related and overall error in one string
  formErrorsCombined: string;
} {
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
      setError(camelCaseName as any, {
        message: errorString,
        type: 'validate',
      });
      if (key.includes('.')) {
        key = key.substring(key.lastIndexOf('.') + 1);
      }
      formErrorsCombined = formErrorsCombined + `${key}: ${errorString}\n`;
    });
    if (overallError === 'One or more validation errors occurred.') {
      // it doesn't make sense to display this error
      overallError = '';
    }
    if (overallError) {
      formErrorsCombined = overallError + '\n' + formErrorsCombined;
    }

    return {
      formErrorsCombined: formErrorsCombined,
      overallServerError: overallError,
    };
  }

  return {
    overallServerError: overallError,
    formErrorsCombined: overallError,
  };
}

/*
  Helper hook for handling form submit.
  Returns object: {
    handler: (data: TFieldValues) => Promise<void>; // should be passed to handleSubmit function of react-hook-form
    overallServerError // variable containing some general error(not related to any particular field)
                 // that happened during form submitting (e.g. Network error).
                 // Should be rendered somewhere in UI.
  }.

  Usage:
  const submitForm = useErrorHandler(useCallback(async (data: ICreateSubtenantDto) => {
    await ClientFactory.TenantClient.createSubtenant(new CreateSubtenantDto(data));
    history.push(sharedRoutes.Subtenants.root);
  }, []), setError);

  alternatively you could create a submitFormFunction completely outside of the component (to get rid of useCallback):
  async function createTenant (data: ICreateSubtenantDto, history: H.History) {
    await ClientFactory.TenantClient.createSubtenant(new CreateSubtenantDto(data));
    history.push(sharedRoutes.Subtenants.root);
  }
 */
export function useErrorHandler<TFieldValues extends FieldValues = FieldValues>(
  submitFunction: (
    data: UnpackNestedValue<TFieldValues>,
    history: History,
  ) => Promise<void>,
  setError: UseFormSetError<TFieldValues>,
  reset?: (values?: DeepPartial<TFieldValues>) => void,
): UseSendFormReturn<TFieldValues> {
  const [overallServerError, setOverallServerError] = useState('');
  const [formErrorsCombined, setFormErrorsCombined] = useState('');
  const history = useHistory();

  async function submitForm(
    data: UnpackNestedValue<TFieldValues>,
    _event?: React.BaseSyntheticEvent,
  ) {
    try {
      setOverallServerError('');
      await submitFunction(data, history);
      reset?.(undefined);
    } catch (error) {
      const errorDetails = handleSubmitFormError(error, setError);
      if (errorDetails.formErrorsCombined) {
        setFormErrorsCombined(errorDetails.formErrorsCombined);
      }
      if (errorDetails.overallServerError) {
        setOverallServerError(errorDetails.overallServerError);
      }
    }
  }

  return {
    handler: submitForm,
    overallServerError: overallServerError,
    serverErrorsCombined: formErrorsCombined,
  };
}

/*
Converts exception object to readable string.
Handles ASP.NET Core validation errors (ProblemDetails), and other backend errors.
Returns 'Network Error' in case of network errors.
Returns 'Unauthorized' in case of 401
Returns 'Access Denied' in case of 403.
(errors are mentioned here for localization purposes)
 */
export function convertToErrorString(error: any): string {
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
      formErrorsCombined =
        formErrorsCombined + `${camelCaseName}: ${errorString}\n`;
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

function convertToErrorStringInternal(error: any): string {
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
    return 'Network Error';
  }
  if (error.name === 'ChunkLoadError') {
    return 'Network Error';
  }
  if (error.message?.includes("Cannot read property 'status' of undefined")) {
    // nswag generated client throws it when there's no response
    return 'Network Error';
  }
  if (error.message) {
    // e.g. Network Error
    return error.message;
  } else if (error.title) {
    return error.title;
  } else if (error instanceof String || typeof error === 'string') {
    return error as string;
  }
  return error.toString();
}

export function emptyErrorFunction() {
  /*
  Shall be used when errors are handled somewhere else (e.g. inside Loadings)
   */
}
