import { Injectable, NgZone } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SystemNotification {
  id: number;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class SystemNotificationService {
  private hubConnection: signalR.HubConnection | undefined;
  private notificationsSubject = new BehaviorSubject<SystemNotification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();
  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  private apiUrl = `${environment.apiUrl}/notifications`;

  constructor(
    private http: HttpClient,
    private ngZone: NgZone
  ) {}

  public startConnection(): void {
    const token = localStorage.getItem('token');
    if (!token) return;

    // Prevent duplicate connections if already connected
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const hubBaseUrl = environment.apiUrl.replace(/\/api\/?$/, '');
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${hubBaseUrl}/hubs/notification`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR connection started for notifications');
        this.addReceiveNotificationListener();
        this.loadInitialNotifications();
      })
      .catch(err => console.error('Error while starting SignalR connection: ', err));
  }

  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = undefined;
    }
  }

  private addReceiveNotificationListener(): void {
    if (!this.hubConnection) return;
    
    this.hubConnection.off('ReceiveNotification');
    this.hubConnection.on('ReceiveNotification', (rawNotification: any) => {
      this.ngZone.run(() => {
        const notification: SystemNotification = {
          id: rawNotification.id ?? rawNotification.Id,
          title: rawNotification.title ?? rawNotification.Title,
          message: rawNotification.message ?? rawNotification.Message,
          isRead: rawNotification.isRead ?? rawNotification.IsRead ?? false,
          createdAt: rawNotification.createdAt ?? rawNotification.CreatedAt ?? new Date().toISOString()
        };

        const currentNotifications = this.notificationsSubject.value;
        const updatedNotifications = [notification, ...currentNotifications];
        this.notificationsSubject.next(updatedNotifications);
        this.updateUnreadCount(updatedNotifications);
      });
    });
  }

  public loadInitialNotifications(): void {
    this.http.get<SystemNotification[]>(this.apiUrl).subscribe(data => {
      this.notificationsSubject.next(data);
      this.updateUnreadCount(data);
    });
  }

  public markAsRead(id: number): Observable<any> {
    const req = this.http.put(`${this.apiUrl}/${id}/read`, {});
    req.subscribe(() => {
      const updated = this.notificationsSubject.value.map(n => 
        n.id === id ? { ...n, isRead: true } : n
      );
      this.notificationsSubject.next(updated);
      this.updateUnreadCount(updated);
    });
    return req;
  }

  public markAllAsRead(): Observable<any> {
    const req = this.http.put(`${this.apiUrl}/read-all`, {});
    req.subscribe(() => {
      const updated = this.notificationsSubject.value.map(n => ({ ...n, isRead: true }));
      this.notificationsSubject.next(updated);
      this.updateUnreadCount(updated);
    });
    return req;
  }

  public deleteNotification(id: number): Observable<any> {
    const req = this.http.delete(`${this.apiUrl}/${id}`);
    req.subscribe(() => {
      const updated = this.notificationsSubject.value.filter(n => n.id !== id);
      this.notificationsSubject.next(updated);
      this.updateUnreadCount(updated);
    });
    return req;
  }

  public clearAll(): Observable<any> {
    const req = this.http.delete(`${this.apiUrl}/clear-all`);
    req.subscribe(() => {
      this.loadInitialNotifications();
    });
    return req;
  }

  private updateUnreadCount(notifications: SystemNotification[]): void {
    const count = notifications.filter(n => !n.isRead).length;
    this.unreadCountSubject.next(count);
  }
}
