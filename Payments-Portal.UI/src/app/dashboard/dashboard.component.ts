import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { PaymentsService } from '../payments.service';
import { PaymentDto } from '../models/payment.model';
import { DecimalPipe, DatePipe, CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, DecimalPipe, DatePipe, CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  payments: PaymentDto[] = [];
  loading = false;
  errorMessage: string | null = null;

  private readonly paymentsService = inject(PaymentsService);
  private readonly route = inject(ActivatedRoute);

  ngOnInit(): void {
    // Get payments from resolver
    const resolvedData = this.route.snapshot.data['payments'];
    
    if (resolvedData) {
      this.payments = resolvedData.payments || [];
      this.errorMessage = resolvedData.error || null;
    }
  }

  loadPayments() {
    this.loading = true;
    this.errorMessage = null;
    this.paymentsService.getPayments().subscribe({
      next: (data) => {
        this.payments = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error fetching payments', err);
        this.errorMessage = 'Failed to load payments. Please try again.';
        this.loading = false;
      }
    });
  }

  deletePayment(id: string) {
    if (confirm('Are you sure you want to delete this payment?')) {
      this.paymentsService.deletePayment(id).subscribe(() => {
        this.loadPayments();
      });
    }
  }
}
