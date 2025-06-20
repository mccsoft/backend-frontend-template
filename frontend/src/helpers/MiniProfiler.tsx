import { AxiosResponse } from 'axios';
import React, { useEffect } from 'react';

/**
 * Intercepts responses and passes request id to mini-profiler
 */
export const miniProfilerInterceptor = async (response: AxiosResponse<any>) => {
  const miniProfilerIds = response.headers['x-miniprofiler-ids'] as string;

  if (miniProfilerIds) {
    const ids = JSON.parse(miniProfilerIds) as string[];
    ((window as any).MiniProfiler as any)?.fetchResults(ids);
  }
  return response;
};
const backendUri = '';
let _miniProfilerAlreadyIncluded = false;
export const MiniProfiler: React.FC = () => {
  useEffect(() => {
    if (_miniProfilerAlreadyIncluded) return;
    _miniProfilerAlreadyIncluded = true;
    const script = document.createElement('script');
    script.id = 'mini-profiler';
    script.src = `${backendUri}/api/profiler/includes.min.js?v=4.2.22+b27bea37e9`;
    script.setAttribute('data-version', '4.2.22+b27bea37e9');
    script.setAttribute('data-path', `${backendUri}/api/profiler/`);
    script.setAttribute('data-position', 'BottomLeft');
    script.setAttribute('data-start-hidden', 'true');
    script.setAttribute('data-scheme', 'Auto');
    script.setAttribute('data-authorized', 'true');
    script.setAttribute('data-max-traces', '15');
    script.setAttribute('data-toggle-shortcut', 'Alt+O');
    script.setAttribute('data-trivial-milliseconds', '2.0');
    script.setAttribute(
      'data-ignored-duplicate-execute-types',
      'Open,OpenAsync,Close,CloseAsync',
    );

    document.head.appendChild(script);
  }, []);
  return null;
};
