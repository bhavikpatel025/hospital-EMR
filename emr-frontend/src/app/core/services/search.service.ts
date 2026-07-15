import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SearchService {
  readonly searchTermSignal = signal<string>('');

  setSearchTerm(term: string): void {
    this.searchTermSignal.set(term);
  }
}
