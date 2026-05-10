import { environment } from '../../environment/environment';

export const localHost = environment.apiBaseUrl.endsWith('/')
  ? environment.apiBaseUrl
  : `${environment.apiBaseUrl}/`;
