import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class HttpService {

  constructor(private http: HttpClient) { }

  public xhr<T>(settings: HttpSettings): Observable<T> {
    if (!settings.method) {
      settings.method = 'GET';
    }

    switch (settings.method) {
      case 'GET':
        return this.http.get<T>(settings.url, { headers: this.getHeaders(settings.headers) });
      case 'POST':
        return this.http.post<T>(settings.url, JSON.stringify(settings.data), { headers: this.getHeaders(settings.headers) });
      case 'PUT':
        return this.http.put<T>(settings.url, settings.data, { headers: this.getHeaders(settings.headers) });
      case 'DELETE':
        return this.http.delete<T>(settings.url, { headers: this.getHeaders(settings.headers) });
    }
  }

  private getHeaders(headers: any): HttpHeaders {
    let httpHeaders: HttpHeaders = new HttpHeaders();

    // Check if headers are provided, if not, add JSON content type header
    if (!headers) {
      httpHeaders = httpHeaders.append('Content-Type', 'application/json');
    } else {
      Object.keys(headers).forEach(key => {
        httpHeaders = httpHeaders.append(key, headers[key]);
      });
    }
    return httpHeaders;
  }
}

export interface HttpSettings {
  url: string;
  dataType?: string;
  method?: "GET" | "POST" | "PUT" | "DELETE";
  data?: any;
  headers?: any;
}
