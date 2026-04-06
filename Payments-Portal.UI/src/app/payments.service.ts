import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PaymentDto, CreatePaymentRequest, UpdatePaymentRequest } from './models/payment.model';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PaymentsService {
  private readonly apiUrl = `${environment.apiBaseUrl}/payments`;

  constructor(private http: HttpClient) { }

  getPayments(): Observable<PaymentDto[]> {
    return this.http.get<PaymentDto[]>(this.apiUrl);
  }

  createPayment(payment: CreatePaymentRequest): Observable<PaymentDto> {
    return this.http.post<PaymentDto>(this.apiUrl, payment);
  }

  updatePayment(id: string, payment: UpdatePaymentRequest): Observable<PaymentDto> {
    return this.http.put<PaymentDto>(`${this.apiUrl}/${id}`, payment);
  }

  deletePayment(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
