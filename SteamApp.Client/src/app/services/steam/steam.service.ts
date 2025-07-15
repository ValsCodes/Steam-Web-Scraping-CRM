import { Injectable } from '@angular/core';
import { HttpClient} from '@angular/common/http';
import { Observable} from 'rxjs';
import { catchError} from 'rxjs/operators';
import { handleError } from '../error-handler';
import * as g from '../general-data'

@Injectable({
  providedIn: 'root',
})
export class SteamService {
  constructor(private http: HttpClient) {}

  private readonly steam: string = `${g.localHost}steam/`;


  getScrapedPage(page: number): Observable<any> {
    const url = `${this.steam}hat/page/${page}`;
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }

    getScrapedPageByPixel(page: number, isGoodPaintsOnly:boolean = false): Observable<any> {
    const url = `${this.steam}hat/check-paint-by-pixel/${page}?isGoodPaintsOnly=${isGoodPaintsOnly}`;
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }

  getBulkPage(page: number): Observable<any> {
    const url = `${this.steam}hat/page/${page}/bulk`;
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }

  getDeepScrapePaintedOnly(page: number): Observable<any> {
    const url = `${this.steam}hat/page/${page}/painted`;
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }

  getIsHatPainted(name: string): Observable<any> {
    const url = `${this.steam}hat/name/${name}/is-painted`;
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }
}
