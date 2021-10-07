const backendUri = `${window.location.protocol}//${window.location.hostname}:${window.location.port}`;
const getChallengeEndpoint = (provider: string) =>
  `${backendUri}/external-auth/challenge?provider=${provider}`;
let _currentOauthResultsHandler: (event: any) => void;
type OAuthResult = OAuthSuccessResult & OAuthErrorResult;
type OAuthResultEvent = {
  data: {
    type: string;
  } & OAuthResult;
};
type OAuthSuccessResult = {
  provider: string;
  code?: string;
};
type OAuthErrorResult = {
  provider: string;
  error?: string;
  errorDescription?: string;
};

export function getOAuthCode(provider: string): Promise<OAuthResult> {
  function createOAuthMessageHandler(
    resolve: (result: OAuthSuccessResult) => void,
    reject: (result: OAuthErrorResult) => void,
  ) {
    const handler = (event: OAuthResultEvent) => {
      if (event.data && event.data.type === 'oauth-result') {
        const data = event.data;
        if (data.code) {
          resolve({
            provider: data.provider,
            code: data.code,
          });
        } else {
          reject({
            provider: data.provider,
            error: data.error,
            errorDescription: data.errorDescription,
          });
        }
        window.removeEventListener('message', handler, false);
      }
    };
    return handler;
  }

  // @ts-ignore
  return new Promise<OAuthResult>((resolve, reject) => {
    if (_currentOauthResultsHandler) {
      window.removeEventListener('message', _currentOauthResultsHandler, false);
    }
    _currentOauthResultsHandler = createOAuthMessageHandler(resolve, reject);
    window.addEventListener('message', _currentOauthResultsHandler, false);
    window.open(
      getChallengeEndpoint(provider),
      undefined,
      'toolbar=no,menubar=no,directories=no,status=no,width=800,height=600',
    );
  });
}
