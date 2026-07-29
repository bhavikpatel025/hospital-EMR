import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface TranslationRequest {
  texts: string[];
  targetLanguage: string;
}

@Injectable({
  providedIn: 'root'
})
export class TranslationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Translation/translate`;

  translate(texts: string[], targetLanguage: string): Observable<Record<string, string>> {
    const payload: TranslationRequest = { texts, targetLanguage };
    return this.http.post<Record<string, string>>(this.apiUrl, payload);
  }
}
