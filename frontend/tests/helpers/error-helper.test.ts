import { expect, test } from 'vitest';
import { errorToString } from 'helpers/error-helpers';
import { ApiException } from 'services/api/api-client.types';

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
