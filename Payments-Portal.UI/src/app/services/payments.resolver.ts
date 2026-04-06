import { Injectable, inject } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { PaymentsService } from '../payments.service';
import { PaymentDto } from '../models/payment.model';

export interface PaymentsResolverData {
  payments: PaymentDto[];
  error?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentsResolver implements Resolve<PaymentsResolverData> {
  private readonly paymentsService = inject(PaymentsService);

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<PaymentsResolverData> {
    return this.paymentsService.getPayments().pipe(
      map(payments => ({ payments })),
      catchError((error) => {
        console.error('Error fetching payments in resolver:', error);
        return of({
          payments: [],
          error: 'Failed to load payments. Please try again later.'
        });
      })
    );
  }
}
