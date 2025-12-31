import { expect, test, vi } from 'vitest';
import { errorToString } from 'helpers/error-helpers';
import { ApiException } from 'services/api/api-client.types';
import { handleSubmitFormError } from 'helpers/form/useErrorHandler';

// this is what is passed when useQuery hook fails
test('ApiException with urn', () => {
  expect(
    errorToString(
      new ApiException(
        'blablabla',
        503,
        { type: 'urn:mccsoft:external-service-is-unavailable' } as any,
        {},
        null,
      ),
    ),
  ).toBe('External service is unavailable now, please retry later.');
});

// this is what is passed on form submit
test('error with urn', () => {
  expect(
    errorToString({
      status: 503,
      type: 'urn:mccsoft:external-service-is-unavailable',
    }),
  ).toBe('External service is unavailable now, please retry later.');
});

test('401 ApiException with urn', () => {
  expect(
    errorToString(
      new ApiException(
        'blablabla',
        401,
        { type: 'urn:mccsoft:external-service-is-unavailable' } as any,
        {},
        null,
      ),
    ),
  ).toBe('Unauthorized');
});

test('403 ApiException with urn', () => {
  expect(
    errorToString(
      new ApiException(
        'blablabla',
        403,
        { type: 'urn:mccsoft:external-service-is-unavailable' } as any,
        {},
        null,
      ),
    ),
  ).toBe('Access Denied');
});

test('Regular exception with message with urn', () => {
  expect(errorToString(new Error('blablabla'))).toBe('blablabla');
});
test('Submit form error', () => {
  expect(errorToString(new Error('blablabla'))).toBe('blablabla');
});


test('Form validation error', () => {
  expect(
    errorToString({
      type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
      title: 'One or more validation errors occurred.',
      status: 400,
      errors: {
        '$.i': ['The input was not valid.'],
      },
      traceId: '00-95ac9db204f93c0b78ac769c23ff7876-034bbfc5b9a25b1c-00',
    }),
  ).toBe('i: The input was not valid.');
});

test('handleSubmitFormError. field-bound validation errors (system.text.json style)', () => {
  const setFormError = vi.fn();
  handleSubmitFormError(
    {
      type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
      title: 'One or more validation errors occurred.',
      status: 400,
      errors: {
        '$.i': ['The input was not valid.'],
      },
      traceId: '00-95ac9db204f93c0b78ac769c23ff7876-034bbfc5b9a25b1c-00',
    },
    setFormError,
  );
  expect(setFormError).toHaveBeenCalledWith('i', {
    message: 'The input was not valid.',
    type: 'validate',
  });
});
test('handleSubmitFormError. field-bound validation errors (old style)', () => {
  const setFormError = vi.fn();
  handleSubmitFormError(
    {
      type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
      title: 'One or more validation errors occurred.',
      status: 400,
      errors: {
        i: ['The input was not valid.'],
      },
      traceId: '00-95ac9db204f93c0b78ac769c23ff7876-034bbfc5b9a25b1c-00',
    },
    setFormError,
  );
  expect(setFormError).toHaveBeenCalledWith('i', {
    message: 'The input was not valid.',
    type: 'validate',
  });
});
