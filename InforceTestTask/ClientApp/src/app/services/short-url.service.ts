import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ShortUrlTableRequest,
  ShortUrlInfoRequest,
  ShortUrlDeleteRequest,
} from '../models/requests/short-url-table.request';
import { ShortUrlTableResponse } from '../models/responses/short-url-table.response';
import { ShortUrlInfoResponse } from '../models/responses/short-url-info.response';

@Injectable({ providedIn: 'root' })
export class ShortUrlService {
  private apiUrl = 'https://localhost:7033/api/ShortUrl/all';
  private baseUrl = 'https://localhost:7033/api/ShortUrl';

  constructor(private http: HttpClient) {}

  getAllShortUrls(): Observable<ShortUrlTableResponse[]> {
    return this.http.get<ShortUrlTableResponse[]>(this.apiUrl);
  }

  createShortUrl(req: ShortUrlTableRequest): Observable<ShortUrlTableResponse> {
    const token = localStorage.getItem('accessToken');
    return this.http.post<ShortUrlTableResponse>(
      `${this.baseUrl}/create`,
      req,
      { headers: { Authorization: `Bearer ${token}` } }
    );
  }

  getShortUrlInfo(
    request: ShortUrlInfoRequest
  ): Observable<ShortUrlInfoResponse> {
    const token = localStorage.getItem('accessToken');
    return this.http.get<ShortUrlInfoResponse>(
      `${this.baseUrl}/info/${request.urlId}`,
      { headers: { Authorization: `Bearer ${token}` } }
    );
  }

  removeShortUrl(request: ShortUrlDeleteRequest): Observable<boolean> {
    const token = localStorage.getItem('accessToken');
    return this.http.post<boolean>(
      'https://localhost:7033/api/ShortUrl/remove',
      request,
      { headers: { Authorization: `Bearer ${token}` } }
    );
  }
}
